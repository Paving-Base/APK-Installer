using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.UI;

namespace APKInstaller.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (window.Content is FrameworkElement rootElement)
                    {
                        if (rootElement.RequestedTheme != ElementTheme.Default)
                        {
                            return rootElement.RequestedTheme;
                        }
                    }
                }

                return SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (window.Content is FrameworkElement rootElement)
                    {
                        return rootElement.RequestedTheme;
                    }
                }

                return ElementTheme.Default;
            }
            set
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (window.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = value;
                    }
                }

                SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
                UpdateSystemCaptionButtonColors();
            }
        }

        public static void Initialize()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = UIHelper.MainWindow;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
        }

        public static bool IsDarkTheme()
        {
            return RootTheme == ElementTheme.Default
                ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                : RootTheme == ElementTheme.Dark;
        }

        public static void UpdateSystemCaptionButtonColors()
        {
            if (!UIHelper.HasTitleBar)
            {
                AppWindowTitleBar TitleBar = WindowHelper.GetAppWindowForCurrentWindow().TitleBar;

                ResourceDictionary ResourceDictionary = new()
                {
                    Source = new Uri("ms-appx:///Controls/TitleBar/TitleBar_themeresources.xaml")
                };

                Color titleBarBackgroundColor = (Color)ResourceDictionary["TitleBarBackgroudColor"];
                TitleBar.BackgroundColor = titleBarBackgroundColor;

                // rest colors
                Color buttonForegroundColor = (Color)ResourceDictionary["TitleBarButtonForegroundColor"];
                TitleBar.ButtonForegroundColor = buttonForegroundColor;

                Color buttonBackgroundColor = (Color)ResourceDictionary["TitleBarButtonBackgroundColor"];
                TitleBar.ButtonBackgroundColor = UIHelper.TitleBarExtended ? buttonBackgroundColor : titleBarBackgroundColor;
                TitleBar.ButtonInactiveBackgroundColor = buttonBackgroundColor;

                // hover colors
                Color buttonHoverForegroundColor = (Color)ResourceDictionary["TitleBarButtonHoverForegroundColor"];
                TitleBar.ButtonHoverForegroundColor = buttonHoverForegroundColor;

                Color buttonHoverBackgroundColor = (Color)ResourceDictionary["TitleBarButtonHoverBackgroundColor"];
                TitleBar.ButtonHoverBackgroundColor = UIHelper.TitleBarExtended ? buttonHoverBackgroundColor : null;

                // pressed colors
                Color buttonPressedForegroundColor = (Color)ResourceDictionary["TitleBarButtonPressedForegroundColor"];
                TitleBar.ButtonPressedForegroundColor = buttonPressedForegroundColor;

                Color buttonPressedBackgroundColor = (Color)ResourceDictionary["TitleBarButtonPressedBackgroundColor"];
                TitleBar.ButtonPressedBackgroundColor = UIHelper.TitleBarExtended ? buttonPressedBackgroundColor : null;

                // inactive foreground
                Color buttonInactiveForegroundColor = (Color)ResourceDictionary["TitleBarButtonInactiveForegroundColor"];
                TitleBar.ButtonInactiveForegroundColor = buttonInactiveForegroundColor;
            }
        }
    }
}
