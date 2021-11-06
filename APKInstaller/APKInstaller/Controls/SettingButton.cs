using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    public sealed class SettingButton : Button
    {
        public SettingButton()
        {
            DefaultStyleKey = typeof(Button);
            this.Style = (Style)Application.Current.Resources["SettingButtonStyle"];
            this.RegisterPropertyChangedCallback(Button.ContentProperty, OnContentChanged);
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
