using System;
using System.Web.UI;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.Events;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Adds the loader to every public page. The loader becomes part of the rendered page, so
    /// Sitefinity's output cache keeps it too.
    /// </summary>
    internal static class AsyntaiPageInjector
    {
        private const string PageItemKey = "Asyntai.Sitefinity.AiChatbot.Added";

        /// <summary>
        /// Sitefinity raises this for every page it renders, public and backend.
        /// </summary>
        public static void OnPagePreRenderComplete(IPagePreRenderCompleteEvent ev)
        {
            try
            {
                Page page = ev == null ? null : ev.Page;
                if (page == null || ev.PageSiteNode == null || ev.PageSiteNode.IsBackend)
                {
                    return;
                }

                // The page editor, preview and inline editing are for editors, not visitors.
                if (SystemManager.IsDesignMode || SystemManager.IsPreviewMode || SystemManager.IsInlineEditingMode)
                {
                    return;
                }

                if (page.Items.Contains(PageItemKey))
                {
                    return;
                }

                AsyntaiChatbot chatbot = ResolveForCurrentSite();
                if (chatbot == null)
                {
                    return;
                }

                page.Items[PageItemKey] = true;
                Control target = (Control)page.Header ?? page.Form ?? (Control)page;
                target.Controls.Add(new LiteralControl(AsyntaiScriptBuilder.Build(chatbot)));
            }
            catch (Exception)
            {
                // Never break a visitor's page because of the chat assistant.
            }
        }

        internal static AsyntaiChatbot ResolveForCurrentSite()
        {
            AsyntaiConfig config = Config.Get<AsyntaiConfig>();
            string siteName = null;
            try
            {
                var site = SystemManager.CurrentContext == null ? null : SystemManager.CurrentContext.CurrentSite;
                siteName = site == null ? null : site.Name;
            }
            catch (Exception)
            {
                siteName = null;
            }

            return AsyntaiSettings.Resolve(
                siteName,
                config.WidgetId,
                config.Enabled,
                config.ScriptUrl,
                name =>
                {
                    AsyntaiSiteElement element;
                    return config.Sites.TryGetValue(name, out element) && element != null
                        ? new AsyntaiSiteElementValues(element.WidgetId, element.Enabled)
                        : null;
                },
                AsyntaiSettings.ReadAppSetting);
        }
    }
}
