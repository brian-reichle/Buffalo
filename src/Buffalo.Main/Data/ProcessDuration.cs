// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Buffalo.Main
{
	sealed class ProcessDuration : INotifyPropertyChanged
	{
		public string Name
		{
			[DebuggerStepThrough]
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public TimeSpan Duration
		{
			[DebuggerStepThrough]
			get => _duration;
			set
			{
				_duration = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string _name;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		TimeSpan _duration;
	}
}
