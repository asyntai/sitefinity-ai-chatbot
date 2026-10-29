using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Settings shown in Administration > Settings > Advanced > AsyntaiAiChatbot.
    /// </summary>
    [ObjectInfo(Title = "Asyntai AI Chatbot", Description = "Adds the Asyntai AI chat assistant to every page of this website.")]
    public class AsyntaiConfig : ConfigSection
    {
        /// <summary>Default location of the Asyntai chat widget script.</summary>
        public const string DefaultScriptUrl = "https://asyntai.com/static/js/chat-widget.js";

        /// <summary>
        /// Your chatbot ID from the Install page of the Asyntai dashboard, for example asyntai_1a2b3c4d5e6f.
        /// </summary>
        [ConfigurationProperty("widgetId", DefaultValue = "")]
        [ObjectInfo(Title = "Chatbot ID", Description = "Your chatbot ID from the Install page of your Asyntai dashboard (asyntai.com/dashboard), for example asyntai_1a2b3c4d5e6f. You can also paste the whole embed code.")]
        public string WidgetId
        {
            get { return (string)this["widgetId"]; }
            set { this["widgetId"] = value; }
        }

        /// <summary>Shows or hides the chat assistant on all sites.</summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        [ObjectInfo(Title = "Show the chat assistant", Description = "Clear this box to hide the chat assistant on every site without deleting the chatbot ID.")]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>Optional custom address of the widget script.</summary>
        [ConfigurationProperty("scriptUrl", DefaultValue = "")]
        [ObjectInfo(Title = "Script address (optional)", Description = "Leave empty. Only change this if Asyntai support asks you to.")]
        public string ScriptUrl
        {
            get { return (string)this["scriptUrl"]; }
            set { this["scriptUrl"] = value; }
        }

        /// <summary>Optional chatbot per site, for multisite installations.</summary>
        [ConfigurationProperty("sites")]
        [ObjectInfo(Title = "Sites", Description = "Only for multisite: give a site its own chatbot ID, or hide the chat assistant on one site. The site name must match the name in Administration > Sites.")]
        public ConfigElementDictionary<string, AsyntaiSiteElement> Sites
        {
            get { return (ConfigElementDictionary<string, AsyntaiSiteElement>)this["sites"]; }
        }
    }

    /// <summary>
    /// Chatbot settings for one site of a multisite installation.
    /// </summary>
    public class AsyntaiSiteElement : ConfigElement
    {
        /// <summary>Creates the element inside its parent collection.</summary>
        public AsyntaiSiteElement(ConfigElement parent)
            : base(parent)
        {
        }

        /// <summary>Name of the site, as shown in Administration > Sites.</summary>
        [ConfigurationProperty("siteName", IsKey = true, IsRequired = true)]
        [ObjectInfo(Title = "Site name", Description = "The site name exactly as shown in Administration > Sites.")]
        public string SiteName
        {
            get { return (string)this["siteName"]; }
            set { this["siteName"] = value; }
        }

        /// <summary>Chatbot ID for this site. Empty means the main chatbot ID.</summary>
        [ConfigurationProperty("widgetId", DefaultValue = "")]
        [ObjectInfo(Title = "Chatbot ID", Description = "The chatbot ID for this site. Leave empty to use the main chatbot ID.")]
        public string WidgetId
        {
            get { return (string)this["widgetId"]; }
            set { this["widgetId"] = value; }
        }

        /// <summary>Shows or hides the chat assistant on this site.</summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        [ObjectInfo(Title = "Show the chat assistant on this site", Description = "Clear this box to hide the chat assistant on this site only.")]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }
    }
}
