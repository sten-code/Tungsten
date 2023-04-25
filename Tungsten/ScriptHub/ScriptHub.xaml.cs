using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tungsten.ScriptHub
{
    public partial class ScriptHub : UserControl
    {
        public int Page = 1;
        public int TotalPages = 1;
        public string CurrentSearch;

        public ScriptHub()
        {
            InitializeComponent();
        }

        private string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private Border CreateTag(string tagName) 
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(33, 35, 43)),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 3, 0),
                Child = new TextBlock
                {
                    Text = tagName,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(197, 199, 211)),
                    Margin = new Thickness(3, 0, 3, 0)
                }
            };
        }

        public void Load(string search, int page)
        {
            string api = $"https://scriptblox.com/api/script/search?q={HttpUtility.JavaScriptStringEncode(search)}&page={page}";
            string response = Get(api);
            ResultObject result = JsonConvert.DeserializeObject<ResultObject>(response);
            foreach (ScriptObject script in result.Result.Scripts)
            {
                ScriptHubResult scriptHubResult = new ScriptHubResult();
                string uri = script.Game.ImageUrl;
                if (!script.Game.ImageUrl.StartsWith("http"))
                    uri = "https://scriptblox.com" + script.Game.ImageUrl;
                scriptHubResult.ThumbnailImage.Source = new BitmapImage(new Uri(uri));
                scriptHubResult.Title.Text = script.Title;
                scriptHubResult.Script = script.Script;

                if (script.IsPatched)
                    scriptHubResult.TagsPanel.Children.Add(CreateTag("Patched"));
                if (script.ScriptType == "paid")
                    scriptHubResult.TagsPanel.Children.Add(CreateTag("Paid"));
                if (script.IsUniversal)
                    scriptHubResult.TagsPanel.Children.Add(CreateTag("Universal"));
                if (script.Verified)
                    scriptHubResult.TagsPanel.Children.Add(CreateTag("Verified"));
                if (script.Key)
                    scriptHubResult.TagsPanel.Children.Add(CreateTag("Key System"));


                TotalPages = result.Result.TotalPages;
                ResultsPanel.Children.Add(scriptHubResult);
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (CurrentSearch == "")
                return;

            ScrollViewer scroll = (ScrollViewer)sender;
            if (scroll.ScrollableHeight - e.VerticalOffset <= 200)
            {
                if (Page < TotalPages)
                {
                    Page++;
                    Load(CurrentSearch, Page);
                }
            }
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                ResultsPanel.Children.Clear();
                Page = 1;
                TotalPages = 1;
                CurrentSearch = SearchBox.Text;
                Load(CurrentSearch, Page);
            }
        }

    }

    public class ResultObject
    {
        [JsonProperty("result")]
        public ResultContent Result { get; set; }
    }

    public class ResultContent
    {
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
        [JsonProperty("scripts")]
        public ScriptObject[] Scripts { get; set; }
    }

    public class ScriptObject
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("Title")]
        public string Title { get; set; }
        [JsonProperty("game")]
        public Game Game { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("verified")]
        public bool Verified { get; set; }
        [JsonProperty("key")]
        public bool Key { get; set; }
        [JsonProperty("views")]
        public int Views { get; set; }
        [JsonProperty("scriptType")]
        public string ScriptType { get; set; }
        [JsonProperty("isUniversal")]
        public bool IsUniversal { get; set; }
        [JsonProperty("isPatched")]
        public bool IsPatched { get; set; }
        [JsonProperty("visibility")]
        public string Visibility { get; set; }
        [JsonProperty("rawCount")]
        public int RawCount { get; set; }
        [JsonProperty("showRawCount")]
        public bool ShowRawCount { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("__v")]
        public int __v { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("matched")]
        public string[] Matched { get; set; }
    }

    public class Game
    {
        [JsonProperty("gameId")]
        public long GameId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

}
