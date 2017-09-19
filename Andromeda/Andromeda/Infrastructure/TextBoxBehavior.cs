using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Andromeda.Infrastructure
{
    public class TextBoxBehavior
    {
        static readonly Dictionary<TextBox, Capture> Associations = new Dictionary<TextBox, Capture>();

        public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
        {
            var value = dependencyObject.GetValue(ScrollOnTextChangedProperty);
            return value != null && (bool)value;
        }

        public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(ScrollOnTextChangedProperty, value);
        }

        public static readonly DependencyProperty ScrollOnTextChangedProperty =
            DependencyProperty.RegisterAttached("ScrollOnTextChanged", typeof(bool), typeof(TextBoxBehavior),
            new UIPropertyMetadata(false, OnScrollOnTextChanged));

        private static void OnScrollOnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            if (textBox == null)
            {
                return;
            }
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue)
            {
                return;
            }
            if (newValue)
            {
                textBox.Loaded += TextBoxLoaded;
                textBox.Unloaded += TextBoxUnloaded;
            }
            else
            {
                textBox.Loaded -= TextBoxLoaded;
                textBox.Unloaded -= TextBoxUnloaded;
                if (Associations.ContainsKey(textBox))
                {
                    Associations[textBox].Dispose();
                }
            }
        }

        private static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            Associations[textBox].Dispose();
            textBox.Unloaded -= TextBoxUnloaded;
        }

        private static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            textBox.Loaded -= TextBoxLoaded;
            Associations[textBox] = new Capture(textBox);
        }

        private class Capture : IDisposable
        {
            private TextBox TextBox { get; set; }

            public Capture(TextBox textBox)
            {
                TextBox = textBox;
                TextBox.TextChanged += OnTextBoxOnTextChanged;
            }

            private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
            {
                TextBox.ScrollToEnd();
            }

            public void Dispose()
            {
                TextBox.TextChanged -= OnTextBoxOnTextChanged;
            }
        }
    }
}