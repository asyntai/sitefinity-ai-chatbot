using System;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(Asyntai.Sitefinity.AiChatbot.AsyntaiStartup), "Start")]

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Runs before the web application starts. Nothing in this class may touch a Sitefinity type,
    /// because the version resolver below must be in place before the first Sitefinity type loads.
    /// </summary>
    public static class AsyntaiStartup
    {
        private const string SitefinityPublicKeyToken = "b28c218413bdf563";
        private static readonly object ResolverSync = new object();
        private static bool started;
        private static bool resolverInstalled;

        /// <summary>
        /// Called by ASP.NET through <see cref="PreApplicationStartMethodAttribute"/>.
        /// </summary>
        public static void Start()
        {
            if (started)
            {
                return;
            }

            started = true;
            InstallResolver();
            DynamicModuleUtility.RegisterModule(typeof(AsyntaiHttpModule));
        }

        /// <summary>
        /// Installs the Sitefinity version resolver, once. Called from <see cref="Start"/> and
        /// from the module, because ASP.NET skips start methods of new assemblies when the site
        /// sets optimizeCompilations="true" (Sitefinity does by default). Then only the module
        /// line that the package adds to web.config starts the integration.
        /// </summary>
        public static void InstallResolver()
        {
            lock (ResolverSync)
            {
                if (resolverInstalled)
                {
                    return;
                }

                resolverInstalled = true;
                AppDomain.CurrentDomain.AssemblyResolve += ResolveSitefinityAssembly;
            }
        }

        /// <summary>
        /// Sitefinity assemblies are strong named and change version with every release. This
        /// package is compiled against one release, so a site that runs another release asks for
        /// a version it does not have. Answer with the Sitefinity assembly the site really has.
        /// </summary>
        private static Assembly ResolveSitefinityAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName requested;
            try
            {
                requested = new AssemblyName(args.Name);
            }
            catch (ArgumentException)
            {
                return null;
            }

            string name = requested.Name ?? string.Empty;
            if (!name.StartsWith("Telerik.", StringComparison.OrdinalIgnoreCase)
                && !name.StartsWith("Progress.", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            byte[] token = requested.GetPublicKeyToken();
            if (token == null || token.Length == 0
                || !string.Equals(BitConverter.ToString(token).Replace("-", string.Empty), SitefinityPublicKeyToken, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Assembly loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
            if (loaded != null)
            {
                return loaded;
            }

            try
            {
                // Load by simple name: ASP.NET probes the bin folder and finds the site's own copy.
                return Assembly.Load(name);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
