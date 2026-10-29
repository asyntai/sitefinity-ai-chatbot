using System;
using System.Web;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// The package adds this module to web.config. It hooks the integration into Sitefinity
    /// once per application and adds the Asyntai hosts to the security header of public pages.
    /// </summary>
    public sealed class AsyntaiHttpModule : IHttpModule
    {
        private const string CspHeader = "Content-Security-Policy";

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
            AsyntaiStartup.InstallResolver();
            AsyntaiBootstrap.EnsureAttached();

            // Sitefinity can finish starting before or after this module. Check again on each
            // request until the hook is in place; after that this is one boolean read.
            context.BeginRequest += OnBeginRequest;
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private static void OnBeginRequest(object sender, EventArgs e)
        {
            if (!AsyntaiBootstrap.IsRegistered)
            {
                AsyntaiBootstrap.RegisterIfReady();
            }
        }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            try
            {
                var app = sender as HttpApplication;
                if (app == null || !AsyntaiBootstrap.IsRegistered)
                {
                    return;
                }

                HttpResponse response = app.Context.Response;
                string policy = response.Headers[CspHeader];
                if (string.IsNullOrEmpty(policy) || !IsPublicHtml(app.Context))
                {
                    return;
                }

                AsyntaiChatbot chatbot = AsyntaiBootstrap.ResolveForCurrentSite();
                if (chatbot == null)
                {
                    return;
                }

                string extended = AsyntaiCsp.Extend(policy, chatbot.ScriptUrl);
                if (!string.Equals(extended, policy, StringComparison.Ordinal))
                {
                    response.Headers[CspHeader] = extended;
                }
            }
            catch (Exception)
            {
                // Never break a response because of the chat assistant.
            }
        }

        /// <summary>
        /// True for a public HTML page. The backend, the services and the page editor are skipped.
        /// </summary>
        internal static bool IsPublicHtml(HttpContext context)
        {
            string contentType = context.Response.ContentType ?? string.Empty;
            if (!contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return IsPublicPath(context.Request.Path);
        }

        /// <summary>Path rules, kept separate so they can be tested.</summary>
        public static bool IsPublicPath(string path)
        {
            string p = (path ?? "/").ToLowerInvariant();
            return !(p == "/sitefinity" || p.StartsWith("/sitefinity/") || p.StartsWith("/restapi/")
                || p.Contains("/action/edit") || p.Contains("/action/preview"));
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
