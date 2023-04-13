using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;

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

        public static bool IsDarkTheme(ElementTheme ElementTheme)
        {
            return ElementTheme == ElementTheme.Default
                ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                : ElementTheme == ElementTheme.Dark;
        }

        public static void UpdateSystemCaptionButtonColors()
        {
            if (!UIHelper.HasTitleBar)
            {
                bool IsHighContrast = new AccessibilitySettings().HighContrast;
                AppWindowTitleBar TitleBar = UIHelper.MainWindow.AppWindow.TitleBar;

                Color ForegroundColor = IsDarkTheme() || IsHighContrast ? Colors.White : Colors.Black;
                Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDarkTheme() ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

                TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = UIHelper.TitleBarExtended ? Colors.Transparent : BackgroundColor;
            }

            ResourceDictionary resources = Application.Current.Resources;
            resources["WindowCaptionForeground"] = IsDarkTheme() ? Colors.White : Colors.Black;

            if (UIHelper.HasTitleBar && (UIHelper.MainPage?.IsLoaded).Equals(true))
            {
                TitleBarHelper.TriggerTitleBarRepaint();
            }

            if (IsDarkTheme())
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    window?.ApplyWindowDarkMode();
                }
            }
            else
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    window?.RemoveWindowDarkMode();
                }
            }
        }
    }
}
