using System;
using System.Collections.Generic;
using System.Linq;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Adds the Asyntai hosts to a Content-Security-Policy header. Sitefinity sends a strict
    /// policy by default, and without this the browser blocks the chat widget.
    /// </summary>
    public static class AsyntaiCsp
    {
        private static readonly string[] BaseHosts = { "https://widget.asyntai.com", "https://asyntai.com" };

        /// <summary>
        /// Returns the policy with the Asyntai hosts added to the directives that the widget
        /// needs. Directives that the site did not set stay unset, because the browser then
        /// falls back to default-src, which also gets the hosts. A directive set to 'none' is
        /// left alone: the site owner blocked it on purpose.
        /// </summary>
        public static string Extend(string policy, string scriptUrl)
        {
            if (string.IsNullOrWhiteSpace(policy))
            {
                return policy;
            }

            var hosts = new List<string>(BaseHosts);
            string scriptOrigin = OriginOf(scriptUrl);
            if (scriptOrigin != null && !hosts.Contains(scriptOrigin, StringComparer.OrdinalIgnoreCase))
            {
                hosts.Add(scriptOrigin);
            }

            var sockets = hosts.Select(h => "wss://" + h.Substring("https://".Length)).ToList();
            var images = new List<string>(hosts) { "https://flagcdn.com" };

            var wanted = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "default-src", hosts.Concat(sockets).Concat(new[] { "https://flagcdn.com" }).ToList() },
                { "script-src", hosts },
                { "script-src-elem", hosts },
                { "connect-src", hosts.Concat(sockets).ToList() },
                { "img-src", images },
                { "style-src", hosts },
                { "style-src-elem", hosts },
                { "font-src", hosts },
                { "frame-src", hosts },
                { "child-src", hosts },
                { "media-src", hosts },
            };

            string[] parts = policy.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                string[] tokens = part.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> add;
                if (!wanted.TryGetValue(tokens[0], out add))
                {
                    continue;
                }

                if (tokens.Any(t => string.Equals(t, "'none'", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var present = new HashSet<string>(tokens.Skip(1), StringComparer.OrdinalIgnoreCase);
                var extra = add.Where(h => !present.Contains(h)).ToList();
                if (extra.Count > 0)
                {
                    parts[i] = " " + part + " " + string.Join(" ", extra);
                }
            }

            return string.Join(";", parts).Trim();
        }

        private static string OriginOf(string url)
        {
            Uri uri;
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                return null;
            }

            return uri.IsDefaultPort ? "https://" + uri.Host : "https://" + uri.Host + ":" + uri.Port;
        }
    }
}
