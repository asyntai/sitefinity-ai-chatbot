using System;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Services.Events;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.Events;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Registers the settings section and the page hook when Sitefinity has finished starting.
    /// </summary>
    internal static class AsyntaiBootstrap
    {
        private static readonly object Sync = new object();
        private static bool attached;
        private static volatile bool registered;

        /// <summary>True when the settings section and the page hook are in place.</summary>
        public static bool IsRegistered
        {
            get { return registered; }
        }

        /// <summary>
        /// Subscribes to the Sitefinity start-up event, once per application.
        /// </summary>
        public static void EnsureAttached()
        {
            lock (Sync)
            {
                if (attached)
                {
                    return;
                }

                attached = true;
                Bootstrapper.Initialized += OnInitialized;
            }

            RegisterIfReady();
        }

        /// <summary>
        /// Registers now when Sitefinity is ready. Safe to call many times.
        /// </summary>
        public static void RegisterIfReady()
        {
            bool ready;
            try
            {
                ready = Bootstrapper.IsReady;
            }
            catch (Exception)
            {
                ready = false;
            }

            if (ready)
            {
                Register();
            }
        }

        /// <summary>The chatbot for the site of the current request, or null.</summary>
        internal static AsyntaiChatbot ResolveForCurrentSite()
        {
            return AsyntaiPageInjector.ResolveForCurrentSite();
        }

        private static void OnInitialized(object sender, ExecutedEventArgs e)
        {
            if (e != null && e.CommandName == "Bootstrapped")
            {
                Register();
            }
        }

        private static void Register()
        {
            lock (Sync)
            {
                if (registered)
                {
                    return;
                }

                Config.RegisterSection<AsyntaiConfig>();
                EventHub.Subscribe<IPagePreRenderCompleteEvent>(AsyntaiPageInjector.OnPagePreRenderComplete);
                EventHub.Subscribe<IEvent>(AsyntaiCacheRefresher.OnEvent);
                registered = true;
            }
        }
    }
}
