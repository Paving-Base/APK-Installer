using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    public class IsEnabledTextBlock : Control
    {
        public IsEnabledTextBlock()
        {
            DefaultStyleKey = typeof(IsEnabledTextBlock);
        }

        protected override void OnApplyTemplate()
        {
            IsEnabledChanged -= IsEnabledTextBlock_IsEnabledChanged;
            SetEnabledState();
            IsEnabledChanged += IsEnabledTextBlock_IsEnabledChanged;
            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
           "Text",
           typeof(string),
           typeof(IsEnabledTextBlock),
           null);

        [Localizable(true)]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void IsEnabledTextBlock_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetEnabledState();
        }

        private void SetEnabledState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }
    }
}
