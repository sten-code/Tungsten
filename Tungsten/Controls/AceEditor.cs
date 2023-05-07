using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Tungsten.Controls
{
    public class AceEditor : WebView2
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AceEditor), new PropertyMetadata(default(string)));

        public string Text
        {
            set
            {
                SetText(value);
            }
        }

        public async void SetText(string text)
        {
            while (!IsLoaded)
                await Task.Delay(100);
            await CoreWebView2.ExecuteScriptAsync("editor.setValue(\"" + HttpUtility.JavaScriptStringEncode(text) + "\")");
        }

        public async Task<string> GetText()
        {
            while (!IsLoaded)
                await Task.Delay(100);
            return JsonConvert.DeserializeObject<string>(await CoreWebView2.ExecuteScriptAsync("editor.getValue()"));// The string gets returned as "while true do\r\n\r\nend" instead of an already parsed string, json has the exact same rules so a json parser can convert it to a normal string.
        }

        public new bool IsLoaded = false;

        public AceEditor()
        {
            Source = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\bin\\Ace\\Ace.html");
            DefaultBackgroundColor = System.Drawing.Color.FromArgb(25, 27, 33);
            CoreWebView2InitializationCompleted += (s, e) =>
            {
                CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                CoreWebView2.Settings.AreDevToolsEnabled = false;
                CoreWebView2.NewWindowRequested += (sender, args) =>
                {
                    args.Handled = true;
                };
            };
            NavigationCompleted += (s, e) =>
            {
                IsLoaded = true;
            };
            NavigationStarting += (s, e) =>
            {
                IsLoaded = false;
            };
            AllowDrop = false;
        }

    }
}
