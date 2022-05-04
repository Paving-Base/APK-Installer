﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls
{
	[ContentProperty(Name = "CustomContent")]
	[TemplatePart(Name = "LayoutRoot", Type = typeof(Grid))]
	[TemplatePart(Name = "TitleText", Type = typeof(TextBlock))]
	[TemplatePart(Name = "CustomContentPresenter", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "DragRegion", Type = typeof(Grid))]
	[TemplatePart(Name = "BackButton", Type = typeof(Button))]
	[TemplatePart(Name = "Icon", Type = typeof(Viewbox))]
	public partial class TitleBar : Control
	{
		private Grid m_layoutRoot;
		private TextBlock m_titleTextBlock;
		private FrameworkElement m_customArea;
		private Viewbox m_icon;

		private bool m_isTitleSquished = false;
		private bool m_isIconSquished = false;

		private double m_titleWidth;
		private double m_iconWidth;

		public TitleBar()
		{
			this.DefaultStyleKey = typeof(TitleBar);

			var window = Window.Current;
			if (window != null)
			{
				window.Activated += OnWindowActivated;
			}
		}

		protected override void OnApplyTemplate()
		{
			m_layoutRoot = (Grid)GetTemplateChild("LayoutRoot");

			m_icon = (Viewbox)GetTemplateChild("Icon");
			m_titleTextBlock = (TextBlock)GetTemplateChild("TitleText");
			m_customArea = (FrameworkElement)GetTemplateChild("CustomContentPresenter");

			var window = Window.Current;
			if (window != null)
			{
				var dragRegion = (Grid)GetTemplateChild("DragRegion");
				if (dragRegion != null)
				{
					window.SetTitleBar(dragRegion);
				}
				else
				{
					window.SetTitleBar(null);
				}
			}

			var backButton = (Button)GetTemplateChild("BackButton");
			if (backButton != null)
			{
				backButton.Click += OnBackButtonClick;
			}

			var refreshButton = (Button)GetTemplateChild("RefreshButton");
			if (refreshButton != null)
			{
				refreshButton.Click += OnRefreshButtonClick;
			}

			UpdateHeight();
			UpdateBackButton();
			UpdateIcon();
			UpdateTitle();
			UpdateRefreshButton();

			base.OnApplyTemplate();
		}

		public void SetProgressValue(double value)
		{
			var templateSettings = TemplateSettings;
			templateSettings.ProgressValue = value;
			templateSettings.IsProgressIndeterminate = false;
		}

		public void ShowProgressRing()
		{
			var templateSettings = TemplateSettings;
			templateSettings.IsProgressActive = true;
			templateSettings.IsProgressIndeterminate = true;
			VisualStateManager.GoToState(this, "ProgressVisible", false);
		}

		public void HideProgressRing()
		{
			var templateSettings = TemplateSettings;
			VisualStateManager.GoToState(this, "ProgressCollapsed", false);
			templateSettings.IsProgressActive = false;
		}

		public void OnBackButtonClick(object sender, RoutedEventArgs args)
		{
			BackRequested?.Invoke(this, null);
		}

		public void OnRefreshButtonClick(object sender, RoutedEventArgs args)
		{
			RefreshRequested?.Invoke(this, null);
		}

		public void OnIconSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			UpdateIcon();
		}

		public void OnIsBackButtonVisiblePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			UpdateBackButton();
		}

		public void OnIsRefreshButtonVisiblePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			UpdateRefreshButton();
		}

		public void OnCustomContentPropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			UpdateHeight();
		}

		public void OnTitlePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			UpdateTitle();
		}

		public void OnWindowActivated(object sender, WindowActivatedEventArgs args)
		{
			VisualStateManager.GoToState(this, (args.WindowActivationState == WindowActivationState.Deactivated) ? "Deactivated" : "Activated", false);
		}

		public void UpdateBackButton()
		{
			VisualStateManager.GoToState(this, IsBackButtonVisible ? "BackButtonVisible" : "BackButtonCollapsed", false);
		}

		public void UpdateRefreshButton()
		{
			VisualStateManager.GoToState(this, IsRefreshButtonVisible ? "RefreshButtonVisible" : "RefreshButtonCollapsed", false);
		}

		public void UpdateHeight()
		{
			VisualStateManager.GoToState(this, (CustomContent == null && AutoSuggestBox == null && PaneFooter == null) ? "CompactHeight" : "ExpandedHeight", false);
		}

		public void UpdateIcon()
		{
			var source = IconSource;
			if (source != null)
			{
				VisualStateManager.GoToState(this, "IconVisible", false);
			}
			else
			{
				VisualStateManager.GoToState(this, "IconCollapsed", false);
			}
		}

		public void UpdateTitle()
		{
			string titleText = Title;
			if (string.IsNullOrEmpty(titleText))
			{
				VisualStateManager.GoToState(this, "TitleTextCollapsed", false);
			}
			else
			{
				VisualStateManager.GoToState(this, "TitleTextVisible", false);
			}
		}
	}
}
