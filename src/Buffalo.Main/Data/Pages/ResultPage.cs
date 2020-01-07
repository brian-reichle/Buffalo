// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Main
{
	abstract class ResultPage : Page, IConfigPageProvider
	{
		protected ResultPage(GenerationManager manager, ConfigPage config)
			: base(manager)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));

			ConfigPage = config;
		}

		public override void Close()
		{
			ConfigPage.ResultPages.Remove(this);
			Manager.Pages.Remove(this);
		}

		public ConfigPage ConfigPage { get; }
	}
}
