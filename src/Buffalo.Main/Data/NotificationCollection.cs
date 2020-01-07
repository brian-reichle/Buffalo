// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.ObjectModel;

namespace Buffalo.Main
{
	sealed class NotificationCollection : ObservableCollection<Notification>
	{
		public void Clear(ConfigPage page)
		{
			var write = 0;

			for (var read = 0; read < Count; read++)
			{
				var notification = this[read];

				if (notification.Page != page)
				{
					if (read != write)
					{
						SetItem(write, notification);
					}

					write++;
				}
			}

			while (Count > write)
			{
				RemoveAt(Count - 1);
			}
		}

		public void AddNotification(ConfigPage page, bool isError, int fromLine, int fromChar, int toLine, int toChar, string text)
		{
			Add(new Notification()
			{
				Page = page,
				IsError = isError,
				Text = text,
				FromLineNo = fromLine,
				FromCharNo = fromChar,
				ToLineNo = toLine,
				ToCharNo = toChar,
			});
		}
	}
}
