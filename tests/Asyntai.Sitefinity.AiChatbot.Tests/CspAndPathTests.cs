using System.Linq;
using Xunit;

namespace Asyntai.Sitefinity.AiChatbot.Tests
{
    public class CspAndPathTests
    {
        private const string Script = AsyntaiSettings.DefaultScriptUrl;

        private static string Directive(string policy, string name)
        {
            return policy.Split(';').Select(p => p.Trim()).FirstOrDefault(p => p.StartsWith(name + " ") || p == name);
        }

        [Fact]
        public void SitefinityDefaultPolicyGetsHosts()
        {
            string policy = "default-src 'self'; script-src 'self' *.googleapis.com 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; frame-src 'self'; connect-src 'self' data:; media-src 'self' data: blob:; child-src 'self'";
            string result = AsyntaiCsp.Extend(policy, Script);

            Assert.Contains("https://widget.asyntai.com", Directive(result, "script-src"));
            Assert.Contains("https://asyntai.com", Directive(result, "script-src"));
            Assert.Contains("wss://asyntai.com", Directive(result, "connect-src"));
            Assert.Contains("https://flagcdn.com", Directive(result, "img-src"));
            Assert.Contains("https://widget.asyntai.com", Directive(result, "style-src"));
            Assert.Contains("https://flagcdn.com", Directive(result, "default-src"));
            Assert.Contains("*.googleapis.com", Directive(result, "script-src"));
        }

        [Fact]
        public void ExtendIsIdempotent()
        {
            string once = AsyntaiCsp.Extend("default-src 'self'; script-src 'self'", Script);
            Assert.Equal(once, AsyntaiCsp.Extend(once, Script));
        }

        [Fact]
        public void NoneIsLeftAlone()
        {
            string result = AsyntaiCsp.Extend("default-src 'self'; frame-src 'none'", Script);
            Assert.Equal("frame-src 'none'", Directive(result, "frame-src"));
        }

        [Fact]
        public void OtherDirectivesAreKept()
        {
            string result = AsyntaiCsp.Extend("default-src 'self'; upgrade-insecure-requests; report-uri /csp", Script);
            Assert.Contains("upgrade-insecure-requests", result);
            Assert.Contains("report-uri /csp", result);
        }

        [Fact]
        public void MissingDirectivesAreNotCreated()
        {
            string result = AsyntaiCsp.Extend("default-src 'self'", Script);
            Assert.Null(Directive(result, "script-src"));
            Assert.Contains("https://widget.asyntai.com", Directive(result, "default-src"));
        }

        [Fact]
        public void CustomScriptOriginIsAdded()
        {
            string result = AsyntaiCsp.Extend("script-src 'self'", "https://cdn.example.com:8443/w.js");
            Assert.Contains("https://cdn.example.com:8443", Directive(result, "script-src"));
        }

        [Fact]
        public void EmptyPolicyStaysEmpty()
        {
            Assert.Null(AsyntaiCsp.Extend(null, Script));
            Assert.Equal("", AsyntaiCsp.Extend("", Script));
        }

        [Theory]
        [InlineData("/", true)]
        [InlineData("/home", true)]
        [InlineData("/about/team", true)]
        [InlineData("/sitefinity-guide", true)]
        [InlineData("/Sitefinity", false)]
        [InlineData("/Sitefinity/Pages", false)]
        [InlineData("/sitefinity/adminapp/dashboard", false)]
        [InlineData("/RestApi/sitefinity/pages", false)]
        [InlineData("/home/Action/Edit", false)]
        [InlineData("/home/Action/Preview", false)]
        [InlineData(null, true)]
        public void PublicPaths(string path, bool expected)
        {
            Assert.Equal(expected, AsyntaiHttpModule.IsPublicPath(path));
        }
    }
}
