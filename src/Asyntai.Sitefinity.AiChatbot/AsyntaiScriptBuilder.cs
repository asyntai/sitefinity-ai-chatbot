using System.Globalization;
using System.Text;

namespace Asyntai.Sitefinity.AiChatbot
{
    /// <summary>
    /// Builds the small loader script that starts the Asyntai chat assistant.
    /// </summary>
    public static class AsyntaiScriptBuilder
    {
        /// <summary>Attribute that marks the loader, so a page never gets two copies.</summary>
        public const string LoaderMarker = "data-asyntai-loader";

        /// <summary>
        /// Returns the loader script tag. The loader waits for the page load event, so a slow
        /// network never delays the page itself.
        /// </summary>
        public static string Build(AsyntaiChatbot chatbot)
        {
            string id = JsString(chatbot.WidgetId);
            string src = JsString(chatbot.ScriptUrl);

            return "<script type=\"text/javascript\" " + LoaderMarker + "=\"sitefinity\">(function(){var l=function(){"
                + "if(document.querySelector('script[data-asyntai-id]')){return}"
                + "var s=document.createElement(\"script\");s.src=" + src + ";s.async=true;"
                + "s.setAttribute(\"data-asyntai-id\"," + id + ");document.head.appendChild(s)};"
                + "if(document.readyState===\"complete\"){l()}else{window.addEventListener(\"load\",l)}})();</script>";
        }

        /// <summary>
        /// Quotes a value as a JavaScript string that is also safe inside an HTML script block.
        /// </summary>
        public static string JsString(string value)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in value ?? string.Empty)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '<': sb.Append("\\u003c"); break;
                    case '>': sb.Append("\\u003e"); break;
                    case '&': sb.Append("\\u0026"); break;
                    case '\'': sb.Append("\\u0027"); break;
                    case (char)0x2028: sb.Append("\\u2028"); break;
                    case (char)0x2029: sb.Append("\\u2029"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            return sb.Append('"').ToString();
        }
    }
}
