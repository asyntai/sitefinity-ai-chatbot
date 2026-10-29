using System.Collections.Generic;
using Xunit;

namespace Asyntai.Sitefinity.AiChatbot.Tests
{
    public class SettingsTests
    {
        private const string Id = "asyntai_1a2b3c4d5e6f";

        private static AsyntaiChatbot Resolve(
            string site = null,
            string mainId = Id,
            bool mainEnabled = true,
            string mainScript = "",
            Dictionary<string, AsyntaiSiteElementValues> sites = null,
            Dictionary<string, string> app = null)
        {
            return AsyntaiSettings.Resolve(
                site,
                mainId,
                mainEnabled,
                mainScript,
                name =>
                {
                    AsyntaiSiteElementValues v;
                    return sites != null && sites.TryGetValue(name, out v) ? v : null;
                },
                key =>
                {
                    string v;
                    return app != null && app.TryGetValue(key, out v) ? v : null;
                });
        }

        [Theory]
        [InlineData("asyntai_1a2b3c4d5e6f", "asyntai_1a2b3c4d5e6f")]
        [InlineData("  asyntai_1a2b3c4d5e6f  ", "asyntai_1a2b3c4d5e6f")]
        [InlineData("<script async src=\"https://widget.asyntai.com/static/js/chat-widget.js\" data-asyntai-id=\"asyntai_1a2b3c4d5e6f\"></script>", "asyntai_1a2b3c4d5e6f")]
        [InlineData("<script data-asyntai-id='asyntai_abcdef'></script>", "asyntai_abcdef")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("1a2b3c4d5e6f", "")]
        [InlineData("asyntai_12", "")]
        [InlineData("asyntai_1a2b3c\"onload=alert(1)", "")]
        [InlineData("asyntai_1a2b3c4d5e6f</script>", "")]
        public void CleanWidgetId(string raw, string expected)
        {
            Assert.Equal(expected, AsyntaiSettings.CleanWidgetId(raw));
        }

        [Theory]
        [InlineData("", AsyntaiSettings.DefaultScriptUrl)]
        [InlineData(null, AsyntaiSettings.DefaultScriptUrl)]
        [InlineData("https://cdn.example.com/w.js", "https://cdn.example.com/w.js")]
        [InlineData("http://cdn.example.com/w.js", "")]
        [InlineData("javascript:alert(1)", "")]
        [InlineData("https://x.com/\"><script>", "")]
        public void CleanScriptUrl(string raw, string expected)
        {
            Assert.Equal(expected, AsyntaiSettings.CleanScriptUrl(raw));
        }

        [Fact]
        public void MainIdIsUsed()
        {
            AsyntaiChatbot c = Resolve();
            Assert.Equal(Id, c.WidgetId);
            Assert.Equal(AsyntaiSettings.DefaultScriptUrl, c.ScriptUrl);
        }

        [Fact]
        public void EmptyMainIdMeansOff()
        {
            Assert.Null(Resolve(mainId: ""));
        }

        [Fact]
        public void BadMainIdMeansOff()
        {
            Assert.Null(Resolve(mainId: "not-an-id"));
        }

        [Fact]
        public void MainSwitchOffHidesEverySite()
        {
            var sites = new Dictionary<string, AsyntaiSiteElementValues> { { "Shop", new AsyntaiSiteElementValues("asyntai_shop123", true) } };
            Assert.Null(Resolve(mainEnabled: false));
            Assert.Null(Resolve(site: "Shop", mainEnabled: false, sites: sites));
        }

        [Fact]
        public void SiteRowGivesOwnId()
        {
            var sites = new Dictionary<string, AsyntaiSiteElementValues> { { "Shop", new AsyntaiSiteElementValues("asyntai_shop123", true) } };
            Assert.Equal("asyntai_shop123", Resolve(site: "Shop", sites: sites).WidgetId);
            Assert.Equal(Id, Resolve(site: "Blog", sites: sites).WidgetId);
        }

        [Fact]
        public void SiteRowWithoutIdUsesMainId()
        {
            var sites = new Dictionary<string, AsyntaiSiteElementValues> { { "Shop", new AsyntaiSiteElementValues("", true) } };
            Assert.Equal(Id, Resolve(site: "Shop", sites: sites).WidgetId);
        }

        [Fact]
        public void SiteRowSwitchedOffHidesOnlyThatSite()
        {
            var sites = new Dictionary<string, AsyntaiSiteElementValues> { { "Shop", new AsyntaiSiteElementValues("asyntai_shop123", false) } };
            Assert.Null(Resolve(site: "Shop", sites: sites));
            Assert.Equal(Id, Resolve(site: "Blog", sites: sites).WidgetId);
        }

        [Fact]
        public void AppSettingIdWins()
        {
            var app = new Dictionary<string, string> { { AsyntaiSettings.WidgetIdAppSetting, "asyntai_fromconfig" } };
            Assert.Equal("asyntai_fromconfig", Resolve(app: app).WidgetId);
            Assert.Equal("asyntai_fromconfig", Resolve(mainId: "", app: app).WidgetId);
        }

        [Fact]
        public void BadAppSettingIdMeansOff()
        {
            var app = new Dictionary<string, string> { { AsyntaiSettings.WidgetIdAppSetting, "bad value" } };
            Assert.Null(Resolve(app: app));
        }

        [Fact]
        public void AppSettingEnabledFalseHidesEverything()
        {
            var app = new Dictionary<string, string> { { AsyntaiSettings.EnabledAppSetting, " FALSE " }, { AsyntaiSettings.WidgetIdAppSetting, "asyntai_fromconfig" } };
            Assert.Null(Resolve(app: app));
        }

        [Fact]
        public void BadScriptUrlMeansOff()
        {
            Assert.Null(Resolve(mainScript: "javascript:alert(1)"));
            var app = new Dictionary<string, string> { { AsyntaiSettings.ScriptUrlAppSetting, "http://insecure.example/w.js" } };
            Assert.Null(Resolve(app: app));
        }

        [Fact]
        public void CustomScriptUrlIsUsed()
        {
            Assert.Equal("https://cdn.example.com/w.js", Resolve(mainScript: "https://cdn.example.com/w.js").ScriptUrl);
        }

        [Fact]
        public void LoaderHasIdScriptAndMarker()
        {
            string html = AsyntaiScriptBuilder.Build(new AsyntaiChatbot(Id, AsyntaiSettings.DefaultScriptUrl));
            Assert.StartsWith("<script", html);
            Assert.EndsWith("</script>", html);
            Assert.Contains("data-asyntai-loader=\"sitefinity\"", html);
            Assert.Contains("\"" + Id + "\"", html);
            Assert.Contains("\"" + AsyntaiSettings.DefaultScriptUrl + "\"", html);
            Assert.Contains("script[data-asyntai-id]", html);
            Assert.Contains("addEventListener(\"load\"", html);
        }

        [Theory]
        [InlineData("a\"b", "\"a\\\"b\"")]
        [InlineData("</script>", "\"\\u003c/script\\u003e\"")]
        [InlineData("a\\b", "\"a\\\\b\"")]
        [InlineData("x'y&z", "\"x\\u0027y\\u0026z\"")]
        public void JsStringEscapes(string raw, string expected)
        {
            Assert.Equal(expected, AsyntaiScriptBuilder.JsString(raw));
        }

        [Fact]
        public void JsStringEscapesLineSeparators()
        {
            Assert.Equal("\"a\\u2028b\\u2029c\"", AsyntaiScriptBuilder.JsString("a" + (char)0x2028 + "b" + (char)0x2029 + "c"));
        }
    }
}
