// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Globalization;
using System.Windows.Data;

namespace Buffalo.Main
{
	[ValueConversion(typeof(object), typeof(bool))]
	sealed class EnumBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> parameter.Equals(value);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// The radio button only calls this for the check box being set to true.
			return parameter;
		}
	}
}
