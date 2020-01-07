// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class ConfigPage : Page, IConfigPageProvider, IErrorReporter
	{
		public ConfigPage(GenerationManager manager)
			: base(manager)
		{
			_configText = string.Empty;
			_path = string.Empty;
			ResultPages = new List<ResultPage>();
		}

		public override void Close()
		{
			foreach (var page in ResultPages)
			{
				Manager.Pages.Remove(page);
			}

			Manager.Notifications.Clear(this);
			Manager.Pages.Remove(this);
		}

		public string ConfigText
		{
			[DebuggerStepThrough]
			get => _configText;
			set
			{
				_configText = value;
				_dirtySince = DateTime.Now;
				OnPropertyChanged();
				OnPropertyChanged(nameof(DirtySince));
			}
		}

		public string Path
		{
			[DebuggerStepThrough]
			get => _path;
			set => SetField(ref _path, value);
		}

		public GeneratorType GeneratorType
		{
			[DebuggerStepThrough]
			get => _generatorType;
			set
			{
				_generatorType = value;
				_dirtySince = DateTime.Now;
				OnPropertyChanged();
				OnPropertyChanged(nameof(DirtySince));
			}
		}

		public DateTime? DirtySince
		{
			[DebuggerStepThrough]
			get => _dirtySince;
			set
			{
				_dirtySince = value;
				OnPropertyChanged();
			}
		}

		public List<ResultPage> ResultPages { get; }

		ConfigPage IConfigPageProvider.ConfigPage => this;

		void IErrorReporter.AddError(int fromLine, int fromChar, int toLine, int toChar, string text)
			=> Manager.Notifications.AddNotification(this, true, fromLine, fromChar, toLine, toChar, text);

		void IErrorReporter.AddWarning(int fromLine, int fromChar, int toLine, int toChar, string text)
			=> Manager.Notifications.AddNotification(this, false, fromLine, fromChar, toLine, toChar, text);

		string _configText;
		string _path;
		DateTime? _dirtySince;
		GeneratorType _generatorType;
	}
}
