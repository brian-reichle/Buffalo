// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Buffalo.Main
{
	abstract class Page : INotifyPropertyChanged
	{
		protected Page(GenerationManager manager)
		{
			if (manager == null) throw new ArgumentNullException(nameof(manager));
			Manager = manager;
			_filename = string.Empty;
		}

		public abstract void Close();

		public string FileName
		{
			[DebuggerStepThrough]
			get => _filename;
			set => SetField(ref _filename, value);
		}

		public GenerationManager Manager { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
			where T : IEquatable<T>
		{
			if (!field.Equals(value))
			{
				field = value;
				OnPropertyChanged(propertyName);
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		string _filename;
	}
}
