// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Buffalo.Main
{
	sealed class SquiggleAdorner : Adorner
	{
		public SquiggleAdorner(TextBox textbox)
			: base(textbox)
		{
			_squigglePen = new Pen(new SolidColorBrush(Colors.Red) { Opacity = 0.5 }, 2.5);
			_squigglePen.Freeze();

			_map = new RangeMap();

			textbox.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);

			var collection = Squiggle.GetNotifications(textbox);

			if (collection != null)
			{
				collection.CollectionChanged += new NotifyCollectionChangedEventHandler(Notifications_CollectionChanged);
				Notifications_CollectionChanged(textbox, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			Squiggle.AddNotificationsChangedHandler(textbox, TextBox_NotificationsChanged);
			Squiggle.AddPageFilterChangedHandler(textbox, TextBox_PageFilterChanged);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			var box = (TextBox)AdornedElement;

			foreach (var section in _map)
			{
				Underline(box, drawingContext, section.Start, section.Length);
			}
		}

		void Underline(TextBox box, DrawingContext drawingContext, int start, int count)
		{
			var end = start + count - 1;
			var from = start;
			var to = start;

			var text = box.Text;

			for (var i = start; i <= end; i++)
			{
				bool eol;

				if (i >= text.Length)
				{
					eol = true;
				}
				else if (text[i] == '\n')
				{
					eol = true;
				}
				else if (text[i] == '\r')
				{
					if (i + 1 < text.Length && text[i + 1] == '\n')
					{
						i++;
					}

					eol = true;
				}
				else
				{
					eol = false;
				}

				if (eol)
				{
					UnderlineCore(box, text.Length, drawingContext, from, to);

					from = i + 1;
				}
				else
				{
					to = i;
				}
			}

			UnderlineCore(box, text.Length, drawingContext, from, to);
		}

		void UnderlineCore(TextBox box, int len, DrawingContext drawingContext, int from, int to)
		{
			if (len != 0)
			{
				Point p1;
				Point p2;

				if (from >= len)
				{
					p1 = box.GetRectFromCharacterIndex(len - 1, true).BottomRight;
					p2 = new Point(p1.X + 5, p1.Y);
				}
				else
				{
					p1 = box.GetRectFromCharacterIndex(from, false).BottomLeft;

					if (to >= len)
					{
						p2 = box.GetRectFromCharacterIndex(len - 1, true).BottomRight;
					}
					else
					{
						p2 = box.GetRectFromCharacterIndex(to, true).BottomRight;
					}

					if ((p2.X - p1.X) < 5)
					{
						p2 = new Point(p1.X + 5, p1.Y);
					}
				}

				drawingContext.DrawLine(_squigglePen, p1, p2);
			}
		}

		void SignalInvalidate()
		{
			var box = (TextBox)AdornedElement;
			box.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)InvalidateVisual);
		}

		void SignalRebuildMap()
		{
			_rebuildMap = true;

			var box = (TextBox)AdornedElement;

			box.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
			{
				if (_rebuildMap)
				{
					_rebuildMap = false;
					RebuildMap();
				}
			});
		}

		void RebuildMap()
		{
			_map.Clear();

			var textBox = (TextBox)AdornedElement;
			IEnumerable notifications = Squiggle.GetNotifications(textBox);
			var page = Squiggle.GetPageFilter(textBox);

			if (notifications != null)
			{
				foreach (Notification notification in notifications)
				{
					if (notification.Page == page)
					{
						var fromIndex = GetIndex(textBox, notification.FromLineNo - 1, notification.FromCharNo - 1);
						var toIndex = GetIndex(textBox, notification.ToLineNo - 1, notification.ToCharNo - 1);

						_map.Set(fromIndex, toIndex - fromIndex + 1);
					}
				}
			}

			SignalInvalidate();
		}

		static int GetIndex(TextBox box, int lineNo, int charNo)
		{
			if (lineNo < 0)
			{
				return 0;
			}
			else if (lineNo >= box.LineCount)
			{
				return box.Text.Length - 1;
			}
			else
			{
				return box.GetCharacterIndexFromLineIndex(lineNo) + charNo;
			}
		}

		void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			foreach (var change in e.Changes)
			{
				_map.Delete(change.Offset, change.RemovedLength);
				_map.Insert(change.Offset, change.AddedLength);
			}

			SignalInvalidate();
		}

		void TextBox_NotificationsChanged(object sender, RoutedPropertyChangedEventArgs<ObservableCollection<Notification>> e)
		{
			var collection = e.OldValue;

			if (collection != null)
			{
				collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(Notifications_CollectionChanged);
			}

			collection = e.NewValue;

			if (collection != null)
			{
				collection.CollectionChanged += new NotifyCollectionChangedEventHandler(Notifications_CollectionChanged);
			}

			Notifications_CollectionChanged(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		void TextBox_PageFilterChanged(object sender, RoutedPropertyChangedEventArgs<Page> e)
		{
			Notifications_CollectionChanged(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		void Notifications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SignalRebuildMap();
		}

		bool _rebuildMap;
		readonly RangeMap _map;
		readonly Pen _squigglePen;
	}
}
