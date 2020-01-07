// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Buffalo.Main
{
	partial class ConfigPageView : UserControl
	{
		public ConfigPageView()
		{
			InitializeComponent();
		}

		void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			_adorner = new SquiggleAdorner(configTextBox);

			var layer = AdornerLayer.GetAdornerLayer(configTextBox);
			layer.Add(_adorner);

			_checker = new ConfigAutoChecker(Dispatcher, (ConfigPage)DataContext);
			_checker.Start();

			FocusManager.SetFocusedElement(this, configTextBox);
		}

		void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			if (_checker != null)
			{
				_checker.Stop();
				_checker = null;
			}
		}

		public void GoTo(int fromLineNo, int fromCharNo, int toLineNo, int toCharNo)
		{
			var fromIndex = GetIndex(fromLineNo, fromCharNo);
			var toIndex = GetIndex(toLineNo, toCharNo);

			configTextBox.Focus();
			configTextBox.Select(fromIndex, toIndex - fromIndex + 1);
		}

		int GetIndex(int lineNo, int charNo)
		{
			if (lineNo < 0)
			{
				return 0;
			}
			else if (lineNo >= configTextBox.LineCount)
			{
				return configTextBox.Text.Length - 1;
			}
			else
			{
				return configTextBox.GetCharacterIndexFromLineIndex(lineNo) + charNo;
			}
		}

		ConfigAutoChecker _checker;
		SquiggleAdorner _adorner;
	}
}
