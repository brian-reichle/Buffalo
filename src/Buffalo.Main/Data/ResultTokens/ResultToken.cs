// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;

namespace Buffalo.Main
{
	abstract class ResultToken<T> : IResultToken
		where T : ResultPage
	{
		protected ResultToken(string suffix)
		{
			Suffix = suffix;
		}

		public string Suffix { get; }

		public void AddOrUpdatePage(ConfigPage page, ResultPage[] resultPages)
		{
			var filename = Path.ChangeExtension(page.FileName, Suffix);
			var resultPage = ExtractPage(resultPages, filename);

			if (resultPage == null && !IsEmpty)
			{
				resultPage = NewResultPage(page);
				page.Manager.Pages.Add(resultPage);
				page.ResultPages.Add(resultPage);
			}

			if (resultPage != null)
			{
				resultPage.FileName = filename;
				UpdatePage(resultPage);
			}
		}

		protected abstract bool IsEmpty { get; }
		protected abstract T NewResultPage(ConfigPage page);
		protected abstract void UpdatePage(T resultPage);

		static T ExtractPage(ResultPage[] resultPages, string filename)
		{
			for (var i = 0; i < resultPages.Length; i++)
			{
				var resultPage = resultPages[i];

				if (resultPage != null &&
					resultPage.FileName == filename &&
					resultPage is T result)
				{
					resultPages[i] = null;
					return result;
				}
			}

			return null;
		}
	}
}
