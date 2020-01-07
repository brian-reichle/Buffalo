// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class GenerationManager : INotifyPropertyChanged, IPerformanceReporter
	{
		public GenerationManager()
		{
			_timeTaken = TimeSpan.Zero;
			Pages = new ObservableCollection<Page>();
			Notifications = new NotificationCollection();
			ProcessDurations = new ObservableCollection<ProcessDuration>();
		}

		public TimeSpan TimeTaken
		{
			[DebuggerStepThrough]
			get => _timeTaken;
			set
			{
				_timeTaken = value;
				OnPropertyChanged();
			}
		}

		public bool IsGenerating
		{
			[DebuggerStepThrough]
			get => _isGenerating;
			set
			{
				_isGenerating = value;
				OnPropertyChanged();
			}
		}

		public Page CurrentPage
		{
			get => _currentPage;
			set
			{
				_currentPage = value;
				PullGeneratorType();

				OnPropertyChanged();
			}
		}

		public GeneratorType GeneratorType
		{
			[DebuggerStepThrough]
			get => _generatorType;
			set
			{
				_generatorType = value;
				PushGeneratorType();
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Page> Pages { get; }
		public NotificationCollection Notifications { get; }
		public ObservableCollection<ProcessDuration> ProcessDurations { get; }

		public ConfigPage NewPage(GeneratorType generatorType, string fileName)
		{
			var page = new ConfigPage(this);
			page.GeneratorType = generatorType;
			page.FileName = fileName;

			Pages.Add(page);
			CurrentPage = page;

			return page;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void IPerformanceReporter.AddPerformanceMetric(string name, System.TimeSpan span)
		{
			ProcessDurations.Add(new ProcessDuration()
			{
				Name = name,
				Duration = span,
			});
		}

		void PushGeneratorType()
		{
			ConfigPage config;

			if (CurrentPage is IConfigPageProvider provider &&
				(config = provider.ConfigPage) != null)
			{
				config.GeneratorType = _generatorType;
			}
		}

		void PullGeneratorType()
		{
			ConfigPage config;

			if (CurrentPage is IConfigPageProvider provider &&
				(config = provider.ConfigPage) != null)
			{
				var value = config.GeneratorType;

				if (value != _generatorType)
				{
					_generatorType = value;
					OnPropertyChanged(nameof(GeneratorType));
				}
			}
		}

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		bool _isGenerating;
		TimeSpan _timeTaken;
		Page _currentPage;
		GeneratorType _generatorType;
	}
}
