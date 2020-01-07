// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Buffalo.Main
{
	sealed partial class NotificationsGrid : UserControl
	{
		static readonly RoutedEvent GotoNotificationEvent = EventManager.RegisterRoutedEvent(
			nameof(GotoNotification),
			RoutingStrategy.Bubble,
			typeof(EventHandler<NotificationEventArgs>),
			typeof(NotificationsGrid));

		public NotificationsGrid()
		{
			InitializeComponent();
		}

		public event EventHandler<NotificationEventArgs> GotoNotification
		{
			add => AddHandler(GotoNotificationEvent, value);
			remove => RemoveHandler(GotoNotificationEvent, value);
		}

		void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			var item = (ListViewItem)sender;

			RaiseEvent(new NotificationEventArgs((Notification)item.Content)
			{
				RoutedEvent = GotoNotificationEvent,
				Source = this,
			});
		}
	}
}
