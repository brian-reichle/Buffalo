// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Globalization;
using System.Windows.Data;

namespace Buffalo.Main
{
	[ValueConversion(typeof(TimeSpan), typeof(string))]
	sealed class TimeSpanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> ((TimeSpan)value).TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture);

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException("Oneway only");
	}
}
