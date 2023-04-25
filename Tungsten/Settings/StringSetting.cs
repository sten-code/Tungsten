using System.Windows.Controls;
using System.Windows;
using System;

namespace Tungsten.Settings
{
    public class StringSetting : Setting
    {
        public string Value { get; private set; }
        public Action<string> OnChangeEvent { get; private set; }

        public StringSetting(string name, string identifier, string defaultValue = "", Action<string> onChange = null) : base(name, identifier)
        {
            OnChangeEvent = onChange;
            Value = GetValue(defaultValue);
            if (OnChangeEvent != null)
                OnChangeEvent(Value);
        }

        public override Grid GetComponent()
        {
            Grid grid = base.GetComponent();
            TextBox textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5),
                Width = 150,
                Text = Value
            };
            textBox.TextChanged += (s, e) =>
            {
                Value = textBox.Text;
                if (OnChangeEvent != null)
                    OnChangeEvent(Value);

                if (SaveManager.Instance != null)
                    SaveManager.Instance.Save(Identifier, Value);
            };
            grid.Children.Add(textBox);
            return grid;
        }
    }
}
