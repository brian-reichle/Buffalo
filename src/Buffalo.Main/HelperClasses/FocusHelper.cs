// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Windows;

namespace Buffalo.Main
{
	static class FocusHelper
	{
		public static readonly DependencyProperty IsInitialFocusProperty =
			DependencyProperty.RegisterAttached(
			"IsInitialFocus",
			typeof(bool),
			typeof(FocusHelper),
			new PropertyMetadata(false, OnIsInitialFocusChanged));

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetIsInitialFocus(DependencyObject d) => (bool)d.GetValue(IsInitialFocusProperty);
		public static void SetIsInitialFocus(DependencyObject d, bool value) => d.SetValue(IsInitialFocusProperty, value);

		static void OnIsInitialFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FrameworkElement element && !element.IsLoaded)
			{
				if (e.NewValue.Equals(true))
				{
					element.Loaded += ElementLoaded;
				}
				else
				{
					element.Loaded -= ElementLoaded;
				}
			}
		}

		static void ElementLoaded(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement element)
			{
				element.Loaded -= ElementLoaded;
				element.Focus();
			}
		}
	}
}
