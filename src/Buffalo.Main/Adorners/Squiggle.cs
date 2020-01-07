// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Buffalo.Main
{
	static class Squiggle
	{
		public static readonly DependencyProperty NotificationsProperty = DependencyProperty.RegisterAttached(
			"Notifications",
			typeof(ObservableCollection<Notification>),
			typeof(Squiggle),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnNotificationsChanged));

		public static readonly DependencyProperty PageFilterProperty = DependencyProperty.RegisterAttached(
			"PageFilter",
			typeof(Page),
			typeof(Squiggle),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPageFilterChanged));

		public static readonly RoutedEvent NotificationsChangedEvent = EventManager.RegisterRoutedEvent(
			"NotificationsChanged",
			RoutingStrategy.Direct,
			typeof(RoutedPropertyChangedEventHandler<ObservableCollection<Notification>>),
			typeof(Squiggle));

		public static readonly RoutedEvent PageFilterChangedEvent = EventManager.RegisterRoutedEvent(
			"PageFilterChanged",
			RoutingStrategy.Direct,
			typeof(RoutedPropertyChangedEventHandler<Page>),
			typeof(Squiggle));

		public static void SetNotifications(TextBox textBox, ObservableCollection<Notification> value)
			=> textBox.SetValue(NotificationsProperty, value);

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static ObservableCollection<Notification> GetNotifications(TextBox textBox)
			=> (ObservableCollection<Notification>)textBox.GetValue(NotificationsProperty);

		public static void SetPageFilter(TextBox textBox, Page value)
			=> textBox.SetValue(PageFilterProperty, value);

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static Page GetPageFilter(TextBox textBox)
			=> (Page)textBox.GetValue(PageFilterProperty);

		public static void AddNotificationsChangedHandler(FrameworkElement dobj, RoutedPropertyChangedEventHandler<ObservableCollection<Notification>> handler)
			=> dobj.AddHandler(NotificationsChangedEvent, handler);

		public static void RemoveNotificationsChangedHandler(FrameworkElement dobj, RoutedPropertyChangedEventHandler<ObservableCollection<Notification>> handler)
			=> dobj.RemoveHandler(NotificationsChangedEvent, handler);

		public static void AddPageFilterChangedHandler(FrameworkElement dobj, RoutedPropertyChangedEventHandler<Page> handler)
			=> dobj.AddHandler(PageFilterChangedEvent, handler);

		public static void RemovePageFilterChangedHandler(FrameworkElement dobj, RoutedPropertyChangedEventHandler<Page> handler)
			=> dobj.RemoveHandler(PageFilterChangedEvent, handler);

		static void OnNotificationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var textBox = (TextBox)d;
			textBox.RaiseEvent(new RoutedPropertyChangedEventArgs<ObservableCollection<Notification>>(
				(ObservableCollection<Notification>)e.OldValue,
				(ObservableCollection<Notification>)e.NewValue,
				NotificationsChangedEvent));
		}

		static void OnPageFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var textBox = (TextBox)d;
			textBox.RaiseEvent(new RoutedPropertyChangedEventArgs<Page>(
				(Page)e.OldValue,
				(Page)e.NewValue,
				PageFilterChangedEvent));
		}
	}
}
