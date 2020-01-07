// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class ConfigAutoChecker
	{
		public ConfigAutoChecker(Dispatcher dispatcher, ConfigPage page)
		{
			if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
			if (page == null) throw new ArgumentNullException(nameof(page));

			_dispatcher = dispatcher;
			_page = page;
			_timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, Tick, _dispatcher);
		}

		public void Start() => _timer.Start();
		public void Stop() => _timer.Stop();

		async void Tick(object sender, EventArgs e)
		{
			var dirtySince = _page.DirtySince;

			if (!_calculating && dirtySince.HasValue && dirtySince.Value.AddSeconds(1) < DateTime.Now)
			{
				var token = new Token(dirtySince.Value, _page.ConfigText, _page.GeneratorType);

				_calculating = true;
				try
				{
					await ParallelTickAsync(token);
				}
				finally
				{
					Update(token);
					_calculating = false;
				}
			}
		}

		Task ParallelTickAsync(Token token)
		{
			return Task.Run(() => ParallelTick(token));
		}

		static void ParallelTick(Token token)
		{
			try
			{
				if (!string.IsNullOrEmpty(token.Text))
				{
					var generator = GeneratorFactory.NewGenerator(token.GeneratorType);

					using (var reader = new StringReader(token.Text))
					{
						if (generator.Load(reader, token, NullEnvironment.Instance))
						{
							generator.Write(TextWriter.Null);
						}
					}
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception)
			{
				token.AddError(1, 1, 1, 1, "KA-BOOM!!!");
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		void Update(Token token)
		{
			try
			{
				if (_page.DirtySince == token.Timestamp)
				{
					var notifications = _page.Manager.Notifications;

					notifications.Clear(_page);

					foreach (var notification in token)
					{
						notification.Page = _page;
						notifications.Add(notification);
					}
				}
			}
			finally
			{
				_page.DirtySince = null;
			}
		}

		bool _calculating;

		readonly Dispatcher _dispatcher;
		readonly ConfigPage _page;
		readonly DispatcherTimer _timer;

		sealed class Token : List<Notification>, IErrorReporter
		{
			public Token(DateTime timestamp, string text, GeneratorType generatorType)
			{
				Timestamp = timestamp;
				Text = text;
				GeneratorType = generatorType;
			}

			#region IErrorReporter Members

			public void AddError(int fromLine, int fromChar, int toLine, int toChar, string text)
			{
				Add(new Notification()
				{
					IsError = true,
					FromLineNo = fromLine,
					FromCharNo = fromChar,
					ToLineNo = toLine,
					ToCharNo = toChar,
					Text = text,
				});
			}

			public void AddWarning(int fromLine, int fromChar, int toLine, int toChar, string text)
			{
				Add(new Notification()
				{
					IsError = false,
					FromLineNo = fromLine,
					FromCharNo = fromChar,
					ToLineNo = toLine,
					ToCharNo = toChar,
					Text = text,
				});
			}

			#endregion

			public DateTime Timestamp { get; private set; }
			public string Text { get; private set; }
			public GeneratorType GeneratorType { get; private set; }
		}
	}
}
