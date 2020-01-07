// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Buffalo.Main
{
	[ContentProperty(nameof(Templates))]
	sealed class PageTemplateSelector : DataTemplateSelector
	{
		public PageTemplateSelector()
		{
			Templates = new DataTemplateCollection();
		}

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null) return null;

			var actualType = item.GetType();

			foreach (var template in Templates)
			{
				var t = template.DataType as Type;

				if (t.IsAssignableFrom(actualType))
				{
					return template;
				}

				var s = template.DataType as string;

				if (t.FullName == s || t.Name == s)
				{
					return template;
				}
			}

			return null;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DataTemplateCollection Templates { get; }
	}
}
