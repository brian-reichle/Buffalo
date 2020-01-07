// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Buffalo.Core;
using Microsoft.Win32;

namespace Buffalo.Main
{
	partial class MainForm : Window
	{
		public static readonly RoutedCommand ToggleMaximise = new RoutedCommand(nameof(ToggleMaximise), typeof(MainForm));
		public static readonly RoutedCommand FocusOnCurrent = new RoutedCommand(nameof(FocusOnCurrent), typeof(MainForm));
		public static readonly RoutedCommand ClosePage = new RoutedCommand(nameof(ClosePage), typeof(MainForm));
		public static readonly RoutedCommand Generate = new RoutedCommand(nameof(Generate), typeof(MainForm));

		public MainForm()
		{
			InitializeComponent();
			DataContext = new GenerationManager();
		}

		void ExitMenu_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		void OnToggleMaximiseExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
		}

		void OnFocusOnCurrentExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;

			DoFocusOnCurrent((GenerationManager)DataContext);
		}

		void OnSaveFileExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var manager = (GenerationManager)DataContext;

			if (manager.CurrentPage is ConfigPage page)
			{
				var dialog = new SaveFileDialog();
				FileDialogHelper.SetupFileDialog(page.GeneratorType, dialog);

				if (!string.IsNullOrEmpty(page.FileName))
				{
					dialog.InitialDirectory = page.Path;
					dialog.FileName = page.FileName;
				}

				if (dialog.ShowDialog() == true)
				{
					ManagerFileOperations.Save(page, dialog.FileName);
				}
			}
		}

		void OnOpenFileExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			FileDialogHelper.SetupFileDialog(null, dialog);

			if (dialog.ShowDialog() == true)
			{
				var manager = (GenerationManager)DataContext;
				var type = FileDialogHelper.GetSelectedGeneratorType(dialog);
				ManagerFileOperations.Open(manager, type, dialog.FileName);
			}
		}

		void OnNewFileExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var viewModel = new NewFileManager();
			viewModel.FileName = "NewConfig.l";
			viewModel.GeneratorType = GeneratorType.Lexer;

			var dialog = new NewFileDialog();
			dialog.Owner = this;
			dialog.DataContext = viewModel;

			if (dialog.ShowDialog() == true)
			{
				var manager = (GenerationManager)DataContext;
				ManagerFileOperations.New(manager, viewModel.GeneratorType, viewModel.FileName);
			}
		}

		void OnCloseFileExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var manager = (GenerationManager)DataContext;
			var page = manager.CurrentPage;

			if (page != null && QueryClosePage(page))
			{
				page.Close();
			}
		}

		void OnClosePageExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var page = (Page)e.Parameter;

			if (!page.Manager.IsGenerating)
			{
				page.Close();
			}
		}

		void NotificationsGrid_GotoNotification(object sender, NotificationEventArgs e)
		{
			e.Handled = true;

			var notification = e.Notification;
			var page = notification.Page;

			if (page != null)
			{
				page.Manager.CurrentPage = page;

				Dispatcher.BeginInvoke(
					DispatcherPriority.Loaded,
					new DoSelectDelegate(DoSelect),
					page,
					notification.FromLineNo - 1,
					notification.FromCharNo - 1,
					notification.ToLineNo - 1,
					notification.ToCharNo - 1);
			}
		}

		async void OnGenerateExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is IConfigPageProvider pageProvider &&
				pageProvider.ConfigPage is var page &&
				page != null)
			{
				await GeneratorLauncher.GenerateAsync(Dispatcher, page);
			}
		}

		delegate void DoSelectDelegate(ConfigPage page, int fromLineNo, int fromCharNo, int toLineNo, int toCharNo);

		void DoSelect(ConfigPage page, int fromLineNo, int fromCharNo, int toLineNo, int toCharNo)
		{
			if (page.Manager.CurrentPage == page)
			{
				var view = (ConfigPageView)ExtractPageControl(page);
				view.GoTo(fromLineNo, fromCharNo, toLineNo, toCharNo);
			}
		}

		void DoFocusOnCurrent(GenerationManager manager)
		{
			Page page;

			if (manager != null && (page = manager.CurrentPage) != null)
			{
				var control = (FrameworkElement)ExtractPageControl(page);
				control.Focus();
			}
		}

		object ExtractPageControl(Page page)
		{
			var presenter = tabControl.Template.FindName("PART_SelectedContentHost", tabControl) as ContentPresenter;
			var template = tabControl.ContentTemplateSelector.SelectTemplate(page, tabControl);
			return template.FindName("View", presenter);
		}

		bool QueryClosePage(Page page)
		{
			if (!(page is ConfigPage)) return true;

			return MessageBox.Show(
				this,
				"Are you sure you want to close this document?",
				"Close Document",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Warning,
				MessageBoxResult.Cancel) == MessageBoxResult.OK;
		}
	}
}
