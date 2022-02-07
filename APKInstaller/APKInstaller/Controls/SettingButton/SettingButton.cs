using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    public sealed class SettingButton : Button
    {
        public SettingButton()
        {
            DefaultStyleKey = typeof(Button);
            Style = (Style)Application.Current.Resources["SettingButtonStyle"];
            RegisterPropertyChangedCallback(Button.ContentProperty, OnContentChanged);
        }

        private static void OnContentChanged(DependencyObject d, DependencyProperty dp)
        {
            SettingButton self = (SettingButton)d;
            if (self.Content != null)
            {
                if (self.Content.GetType() == typeof(Setting))
                {
                    Setting selfSetting = (Setting)self.Content;
                    selfSetting.Style = (Style)Application.Current.Resources["ButtonContentSettingStyle"];

                    if (!string.IsNullOrEmpty(selfSetting.Header))
                    {
                        AutomationProperties.SetName(self, selfSetting.Header);
                    }
                }
            }
        }
    }
}
