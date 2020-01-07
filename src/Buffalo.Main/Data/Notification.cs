// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Buffalo.Main
{
	[DebuggerDisplay("({FromLineNo}, {FromCharNo})-({ToLineNo}, {ToCharNo}): {Text}")]
	sealed class Notification : INotifyPropertyChanged
	{
		public bool IsError
		{
			[DebuggerStepThrough]
			get => _isError;
			set
			{
				_isError = value;
				OnPropertyChanged();
			}
		}

		public string Text
		{
			[DebuggerStepThrough]
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}

		public int FromLineNo
		{
			[DebuggerStepThrough]
			get => _fromLineNo;
			set
			{
				_fromLineNo = value;
				OnPropertyChanged();
			}
		}

		public int FromCharNo
		{
			[DebuggerStepThrough]
			get => _fromCharNo;
			set
			{
				_fromCharNo = value;
				OnPropertyChanged();
			}
		}

		public int ToLineNo
		{
			[DebuggerStepThrough]
			get => _toLineNo;
			set
			{
				_toLineNo = value;
				OnPropertyChanged();
			}
		}

		public int ToCharNo
		{
			[DebuggerStepThrough]
			get => _toCharNo;
			set
			{
				_toCharNo = value;
				OnPropertyChanged();
			}
		}

		public ConfigPage Page
		{
			[DebuggerStepThrough]
			get => _page;
			set
			{
				_page = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool _isError;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string _text;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _fromLineNo;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _fromCharNo;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _toLineNo;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _toCharNo;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ConfigPage _page;
	}
}
