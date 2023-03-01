using Microsoft.UI.Xaml;

namespace APKInstaller.Controls
{
    public partial class TitleBarTemplateSettings
    {
        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(
           "ProgressValue",
           typeof(double),
           typeof(TitleBarTemplateSettings),
           new PropertyMetadata(0d));

        public static readonly DependencyProperty IsProgressActiveProperty = DependencyProperty.Register(
           "IsProgressActive",
           typeof(bool),
           typeof(TitleBarTemplateSettings),
           new PropertyMetadata(false));

        public static readonly DependencyProperty IsProgressIndeterminateProperty = DependencyProperty.Register(
           "IsProgressIndeterminate",
           typeof(bool),
           typeof(TitleBarTemplateSettings),
           new PropertyMetadata(true));

        public static readonly DependencyProperty TopPaddingColumnGridLengthProperty = DependencyProperty.Register(
           "TopPaddingColumnGridLength",
           typeof(GridLength),
           typeof(TitleBarTemplateSettings),
           new PropertyMetadata(new GridLength(0)));

        public static readonly DependencyProperty LeftPaddingColumnGridLengthProperty = DependencyProperty.Register(
           "LeftPaddingColumnGridLength",
           typeof(GridLength),
           typeof(TitleBarTemplateSettings),
           new PropertyMetadata(new GridLength(0)));

        public static readonly DependencyProperty RightPaddingColumnGridLengthProperty = DependencyProperty.Register(
           "RightPaddingColumnGridLength",
           typeof(GridLength),
           typeof(TitleBarTemplateSettings),
           new PropertyMetadata(new GridLength(0)));

        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public bool IsProgressActive
        {
            get => (bool)GetValue(IsProgressActiveProperty);
            set => SetValue(IsProgressActiveProperty, value);
        }

        public bool IsProgressIndeterminate
        {
            get => (bool)GetValue(IsProgressIndeterminateProperty);
            set => SetValue(IsProgressIndeterminateProperty, value);
        }

        public GridLength TopPaddingColumnGridLength
        {
            get => (GridLength)GetValue(TopPaddingColumnGridLengthProperty);
            set => SetValue(TopPaddingColumnGridLengthProperty, value);
        }

        public GridLength LeftPaddingColumnGridLength
        {
            get => (GridLength)GetValue(LeftPaddingColumnGridLengthProperty);
            set => SetValue(LeftPaddingColumnGridLengthProperty, value);
        }

        public GridLength RightPaddingColumnGridLength
        {
            get => (GridLength)GetValue(RightPaddingColumnGridLengthProperty);
            set => SetValue(RightPaddingColumnGridLengthProperty, value);
        }
    }
}
