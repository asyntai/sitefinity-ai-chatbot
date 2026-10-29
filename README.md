# Asyntai AI Chatbot for Sitefinity

Put the [Asyntai](https://asyntai.com/) AI chat assistant on every page of your
Progress Sitefinity CMS website.

Asyntai reads your own pages, documents and product feed, and answers visitor
questions on your website in more than 80 languages. It answers questions about
delivery, prices, opening hours and products, it collects an email address when
a visitor wants a person, and it sends the conversation to your inbox.

## What you get

- An **Asyntai** section in Sitefinity's own settings
  (Administration > Settings > Advanced). A marketer pastes the chatbot ID and
  saves. No developer work for each change.
- The chat assistant appears on every public page. It never loads in the
  backend, the page editor, preview or inline editing.
- Multisite support: give each site its own chatbot, or hide the chat
  assistant on one site.
- The loader is part of the rendered page, so Sitefinity's output cache keeps it.
  When you save the Asyntai settings, the add-on clears the page cache, so the
  change shows at once.
- Sitefinity's default Content-Security-Policy blocks outside scripts. On public
  pages, the add-on adds the Asyntai hosts (widget.asyntai.com, asyntai.com,
  flagcdn.com) to that header. Your other security settings stay as they are.

## Requirements

- Sitefinity CMS 14.0 or newer (.NET Framework 4.8). Tested on Sitefinity 15.4.
- Pages rendered by Sitefinity CMS itself (MVC, hybrid or Web Forms templates).
- An Asyntai account. Create one at [asyntai.com](https://asyntai.com/).

## Installation

1. Add the package to your SitefinityWebApp project:

   ```powershell
   Install-Package Asyntai.Sitefinity.AiChatbot
   ```

   The package adds one line to the `modules` list in `web.config`. That line
   starts the add-on.

2. Build and start the site.

3. In the Sitefinity backend, open **Administration > Settings > Advanced**,
   then **Asyntai**.

4. Paste your chatbot ID into **Chatbot ID** and click **Save changes**. The ID
   is on the Install page of your [Asyntai dashboard](https://asyntai.com/dashboard)
   and looks like `asyntai_1a2b3c4d5e6f`. You can also paste the whole embed code.

The chat assistant now shows on your public pages.

### Install without NuGet

1. Copy `Asyntai.Sitefinity.AiChatbot.dll` into the `bin` folder of the site.
2. In `web.config`, add this line inside `<system.webServer><modules>`:

   ```xml
   <add name="AsyntaiAiChatbot" type="Asyntai.Sitefinity.AiChatbot.AsyntaiHttpModule, Asyntai.Sitefinity.AiChatbot" />
   ```

Sitefinity sets `optimizeCompilations="true"`, so ASP.NET does not run the
start-up code of a newly added assembly. The module line makes sure the add-on
starts.

## Settings

| Setting | What it does |
| --- | --- |
| Chatbot ID | Your Asyntai chatbot ID, or the whole embed code. |
| Show the chat assistant | Clear it to hide the chat assistant on every site. |
| Script address | Leave empty. Change it only if Asyntai support asks you to. |
| Sites | Multisite only. Add a row with the site name to give that site its own chatbot ID, or clear its switch to hide the chat assistant on that site. |

### web.config settings (optional)

For build pipelines, three `appSettings` keys override the backend settings:

```xml
<appSettings>
  <add key="Asyntai:WidgetId" value="asyntai_1a2b3c4d5e6f" />
  <add key="Asyntai:Enabled" value="true" />
  <add key="Asyntai:ScriptUrl" value="" />
</appSettings>
```

`Asyntai:Enabled` set to `false` hides the chat assistant everywhere.

## Uninstall

Remove the package. It also removes its line from `web.config`. If you
installed by hand, delete the module line and the DLL. The saved settings stay
in Sitefinity's configuration and do nothing.

## Build from source

You need the .NET SDK and the `bin` folder of a Sitefinity site.

```powershell
dotnet build src/Asyntai.Sitefinity.AiChatbot -c Release -p:SitefinityBin=C:\path\to\SitefinityWebApp\bin
dotnet test tests/Asyntai.Sitefinity.AiChatbot.Tests -p:SitefinityBin=C:\path\to\SitefinityWebApp\bin
```

You can also copy the Sitefinity assemblies into a `sitefinity-bin` folder next to `src`.

## Support

- Documentation: https://asyntai.com/documentation/installation/sitefinity/
- Email: hello@asyntai.com

## License

MIT
