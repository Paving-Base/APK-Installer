﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
    [ContentProperty(Name = "ActionContent")]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplatePart(Name = PartIconPresenter, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PartDescriptionPresenter, Type = typeof(ContentPresenter))]
    public class Setting : ContentControl
    {
        private const string PartIconPresenter = "IconPresenter";
        private const string PartDescriptionPresenter = "DescriptionPresenter";
        private ContentPresenter _iconPresenter;
        private ContentPresenter _descriptionPresenter;
        private Setting _setting;

        public Setting()
        {
            DefaultStyleKey = typeof(Setting);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
           "Header",
           typeof(string),
           typeof(Setting),
           new PropertyMetadata(default(string), OnHeaderChanged));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description",
            typeof(object),
            typeof(Setting),
            new PropertyMetadata(null, OnDescriptionChanged));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(object),
            typeof(Setting),
            new PropertyMetadata(default(string), OnIconChanged));

        public static readonly DependencyProperty RightIconProperty = DependencyProperty.Register(
            "RightIcon",
            typeof(string),
            typeof(Setting),
            new PropertyMetadata(null, OnIconChanged));

        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            "ActionContent",
            typeof(object),
            typeof(Setting),
            null);

        public static readonly DependencyProperty RightIconVisibilityProperty = DependencyProperty.Register(
            "RightIconVisibility",
            typeof(Visibility),
            typeof(Setting),
            new PropertyMetadata(null, OnIconChanged));

        [Localizable(true)]
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        [Localizable(true)]
        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string RightIcon
        {
            get => (string)GetValue(RightIconProperty);
            set => SetValue(RightIconProperty, value);
        }

        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        public Visibility RightIconVisibility
        {
            get => (Visibility)GetValue(RightIconVisibilityProperty);
            set => SetValue(RightIconVisibilityProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            IsEnabledChanged -= Setting_IsEnabledChanged;
            _setting = this;
            _iconPresenter = (ContentPresenter)_setting.GetTemplateChild(PartIconPresenter);
            _descriptionPresenter = (ContentPresenter)_setting.GetTemplateChild(PartDescriptionPresenter);
            Update();
            SetEnabledState();
            IsEnabledChanged += Setting_IsEnabledChanged;
            base.OnApplyTemplate();
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Setting)d).Update();
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Setting)d).Update();
        }

        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Setting)d).Update();
        }

        private void Setting_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetEnabledState();
        }

        private void SetEnabledState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        private void Update()
        {
            if (_setting == null)
            {
                return;
            }

            if (_setting.ActionContent != null)
            {
                if (_setting.ActionContent.GetType() != typeof(Button))
                {
                    // We do not want to override the default AutomationProperties.Name of a button. Its Content property already describes what it does.
                    if (!string.IsNullOrEmpty(_setting.Header))
                    {
                        AutomationProperties.SetName((UIElement)_setting.ActionContent, _setting.Header);
                    }
                }
            }

            if (_setting._iconPresenter != null)
            {
                _setting._iconPresenter.Visibility = _setting.Icon == null ? Visibility.Collapsed : Visibility.Visible;
            }

            _setting._descriptionPresenter.Visibility = _setting.Description == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
