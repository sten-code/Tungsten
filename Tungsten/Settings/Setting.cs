using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tungsten.Settings
{
    public class Setting : UIElement
    {
        public string Name { get; private set; }
        public string Identifier { get; private set; }

        public Setting(string name, string identifier = "")
        {
            Name = name;
            Identifier = identifier;
        }

        public T GetValue<T>(T defaultValue)
        {
            T returnValue;
            if (SaveManager.Instance == null)
            {
                returnValue = defaultValue;
            }
            else
            {
                returnValue = SaveManager.Instance.Load(Identifier, defaultValue);
            }

            if (SaveManager.Instance != null && returnValue != null)
                SaveManager.Instance.Save(Identifier, returnValue);

            return returnValue;
        }

        public virtual Grid GetComponent()
        {
            Grid grid = new Grid();
            grid.Children.Add(new TextBlock
            {
                Text = Name,
                Foreground = new SolidColorBrush(Color.FromRgb(197, 199, 211)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(7),
                FontSize = 13
            });
            return grid;
        }

    }
}
