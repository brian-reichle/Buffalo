// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Windows.Controls;
using System.Windows.Input;

namespace Buffalo.Main
{
	partial class CodePageView : UserControl
	{
		public CodePageView()
		{
			InitializeComponent();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			FocusManager.SetFocusedElement(this, textBox);
		}
	}
}
