// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Windows;

namespace Buffalo.Main
{
	sealed class NotificationEventArgs : RoutedEventArgs
	{
		public NotificationEventArgs(Notification notification)
		{
			Notification = notification;
		}

		public Notification Notification { get; }
	}
}
