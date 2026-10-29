using System;
using System.Reflection;
using Telerik.Sitefinity.Services.Events;
using Telerik.Sitefinity.Data;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Drops the cached copies of the public pages, so the next visitor gets a page rendered
    /// with the current chatbot settings.
    /// </summary>
    internal static class AsyntaiCacheRefresher
    {
        private const string ConfigEventTypeName = "Telerik.Sitefinity.Configuration.Events.ConfigEvent";
        private static PropertyInfo configNameProperty;

        /// <summary>
        /// Sitefinity raises a ConfigEvent when a settings section is saved. The type is internal,
        /// so it is matched by name and its ConfigName read once by reflection.
        /// </summary>
        public static void OnEvent(IEvent ev)
        {
            if (ev == null)
            {
                return;
            }

            Type type = ev.GetType();
            if (!string.Equals(type.FullName, ConfigEventTypeName, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                PropertyInfo property = configNameProperty ?? (configNameProperty = type.GetProperty("ConfigName"));
                string name = property == null ? null : property.GetValue(ev, null) as string;
                if (string.Equals(name, "asyntaiConfig", StringComparison.OrdinalIgnoreCase))
                {
                    PagesChanged();
                }
            }
            catch (Exception)
            {
                // A failed refresh only means the change shows when the page cache expires.
            }
        }

        /// <summary>
        /// Clears Sitefinity's caches, which includes the page output cache. A settings save is
        /// rare, so a full refresh is fine; clearing only page entries did not drop MVC pages.
        /// </summary>
        public static void PagesChanged()
        {
            try
            {
                CacheDependency.NotifyAll();
            }
            catch (Exception)
            {
                // A failed refresh only means the change shows when the page cache expires.
            }
        }
    }
}
