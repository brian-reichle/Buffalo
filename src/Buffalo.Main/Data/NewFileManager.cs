// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class NewFileManager : INotifyPropertyChanged
	{
		public GeneratorType GeneratorType
		{
			[DebuggerStepThrough]
			get => _generatorType;
			set
			{
				if (_generatorType != value)
				{
					_generatorType = value;

					switch (value)
					{
						case GeneratorType.Lexer:
							FileName = Path.ChangeExtension(FileName, "l");
							break;

						case GeneratorType.Parser:
							FileName = Path.ChangeExtension(FileName, "y");
							break;
					}
				}

				OnPropertyChanged();
			}
		}

		public string FileName
		{
			[DebuggerStepThrough]
			get => _fileName;
			set
			{
				_fileName = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		GeneratorType _generatorType;
		string _fileName;
	}
}
