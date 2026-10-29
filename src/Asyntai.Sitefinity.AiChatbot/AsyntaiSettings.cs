using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// The chatbot that one page should show: its ID and the script address.
    /// </summary>
    public sealed class AsyntaiChatbot
    {
        /// <summary>Creates a resolved chatbot.</summary>
        public AsyntaiChatbot(string widgetId, string scriptUrl)
        {
            WidgetId = widgetId;
            ScriptUrl = scriptUrl;
        }

        /// <summary>Clean chatbot ID, for example asyntai_1a2b3c4d5e6f.</summary>
        public string WidgetId { get; private set; }

        /// <summary>Https address of the widget script.</summary>
        public string ScriptUrl { get; private set; }
    }

    /// <summary>
    /// Reads and checks the stored values. Every decision about a stored value lives here.
    /// </summary>
    public static class AsyntaiSettings
    {
        /// <summary>Default location of the Asyntai chat widget script.</summary>
        public const string DefaultScriptUrl = "https://widget.asyntai.com/static/js/chat-widget.js";

        /// <summary>web.config appSettings key that overrides the chatbot ID for every site.</summary>
        public const string WidgetIdAppSetting = "Asyntai:WidgetId";

        /// <summary>web.config appSettings key that switches the chat assistant off ("false").</summary>
        public const string EnabledAppSetting = "Asyntai:Enabled";

        /// <summary>web.config appSettings key that overrides the script address.</summary>
        public const string ScriptUrlAppSetting = "Asyntai:ScriptUrl";

        private static readonly Regex IdPattern = new Regex("^asyntai_[A-Za-z0-9]{6,64}$", RegexOptions.CultureInvariant);
        private static readonly Regex SnippetPattern = new Regex("data-asyntai-id\\s*=\\s*[\"']([^\"']+)[\"']", RegexOptions.CultureInvariant);
        private static readonly Regex HttpsPattern = new Regex("^https://[^\\s\"'<>]+$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Returns a usable chatbot ID, or an empty string. Accepts the bare ID or the whole
        /// embed code from the Asyntai dashboard, because people paste the embed code.
        /// </summary>
        public static string CleanWidgetId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string value = raw.Trim();
            Match match = SnippetPattern.Match(value);
            if (match.Success)
            {
                value = match.Groups[1].Value.Trim();
            }

            return IdPattern.IsMatch(value) ? value : string.Empty;
        }

        /// <summary>
        /// Returns the script address, or an empty string when the value is bad. Only https is
        /// accepted, so a bad value can never put a javascript: address on every page.
        /// </summary>
        public static string CleanScriptUrl(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return DefaultScriptUrl;
            }

            string value = raw.Trim();
            return HttpsPattern.IsMatch(value) ? value : string.Empty;
        }

        /// <summary>
        /// Picks the chatbot for one site. Order: web.config appSettings, then the site's own
        /// row in the settings, then the main settings. Returns null when the chat stays off.
        /// </summary>
        public static AsyntaiChatbot Resolve(
            string siteName,
            string mainWidgetId,
            bool mainEnabled,
            string mainScriptUrl,
            Func<string, AsyntaiSiteElementValues> siteLookup,
            Func<string, string> appSetting)
        {
            string appEnabled = appSetting == null ? null : appSetting(EnabledAppSetting);
            if (appEnabled != null && string.Equals(appEnabled.Trim(), "false", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string appScriptUrl = appSetting == null ? null : appSetting(ScriptUrlAppSetting);
            string scriptUrl = CleanScriptUrl(string.IsNullOrWhiteSpace(appScriptUrl) ? mainScriptUrl : appScriptUrl);
            if (scriptUrl.Length == 0)
            {
                return null;
            }

            string appWidgetId = appSetting == null ? null : appSetting(WidgetIdAppSetting);
            if (!string.IsNullOrWhiteSpace(appWidgetId))
            {
                string fromApp = CleanWidgetId(appWidgetId);
                return fromApp.Length == 0 ? null : new AsyntaiChatbot(fromApp, scriptUrl);
            }

            // The main switch hides the chat assistant on every site.
            if (!mainEnabled)
            {
                return null;
            }

            AsyntaiSiteElementValues site = (siteLookup == null || string.IsNullOrEmpty(siteName)) ? null : siteLookup(siteName);
            if (site != null)
            {
                if (!site.Enabled)
                {
                    return null;
                }

                string siteId = CleanWidgetId(site.WidgetId);
                if (siteId.Length > 0)
                {
                    return new AsyntaiChatbot(siteId, scriptUrl);
                }
            }

            string mainId = CleanWidgetId(mainWidgetId);
            return mainId.Length == 0 ? null : new AsyntaiChatbot(mainId, scriptUrl);
        }

        /// <summary>Reads one appSettings value from web.config.</summary>
        public static string ReadAppSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Plain copy of one site's row, so the rules above can be tested without Sitefinity.
    /// </summary>
    public sealed class AsyntaiSiteElementValues
    {
        /// <summary>Creates the copy.</summary>
        public AsyntaiSiteElementValues(string widgetId, bool enabled)
        {
            WidgetId = widgetId;
            Enabled = enabled;
        }

        /// <summary>Chatbot ID for the site, may be empty.</summary>
        public string WidgetId { get; private set; }

        /// <summary>False hides the chat assistant on the site.</summary>
        public bool Enabled { get; private set; }
    }
}
