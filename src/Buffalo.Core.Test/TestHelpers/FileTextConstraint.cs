// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.IO;
using System.Text;
using Buffalo.TestResources;

namespace Buffalo.Core.Test
{
	sealed class FileTextConstraint : FileConstraint
	{
		public FileTextConstraint(string suffix, Resource resource)
			: base(suffix, resource)
		{
		}

		public override bool IsBinary => false;

		protected override void VerifyFileContent(List<ConstraintError> messages, IAdditionalFile file)
		{
			string actualValue;

			using (var actualStream = new MemoryStream())
			using (var actualWriter = new StreamWriter(actualStream))
			{
				file.GenerateFileContent(actualWriter);
				actualWriter.Flush();
				actualValue = actualWriter.Encoding.GetString(actualStream.ToArray());
			}

			var expectedValue = Resource.ReadString();

			if (actualValue.Length != expectedValue.Length)
			{
				messages.Add(new MismatchConstraintError<int>(
					"Resource lengths do not match.",
					actualValue.Length,
					expectedValue.Length));
			}
			else
			{
				var firstErrorOrNull = IndexOfFirstError(expectedValue, actualValue);

				if (firstErrorOrNull.HasValue)
				{
					var builder = new StringBuilder();
					var firstError = firstErrorOrNull.Value;

					builder.Append("Resource differs at position ");
					builder.Append(firstError);
					builder.AppendLine();
					builder.AppendLine();

					builder.Append("Expected: ");
					AppendBufferSegment(builder, expectedValue, firstError);

					builder.AppendLine();

					builder.Append("Actual:   ");
					AppendBufferSegment(builder, actualValue, firstError);

					messages.Add(new SimpleConstraintError(builder.ToString()));
				}
			}
		}

		void AppendBufferSegment(StringBuilder builder, string value, int index)
		{
			var min = index - 15;
			var max = index + 16;
			string leadin;
			string leadout;

			if (min > 0)
			{
				leadin = "... ";
			}
			else
			{
				min = 0;
				leadin = string.Empty;
			}

			if (max < value.Length - 1)
			{
				leadout = " ...";
			}
			else
			{
				max = value.Length;
				leadout = string.Empty;
			}

			builder.Append(leadin);

			for (var i = min; i < max; i++)
			{
				builder.Append(value[i]);
			}

			builder.Append(leadout);
		}

		int? IndexOfFirstError(string expectedValue, string actualValue)
		{
			for (var i = 0; i < expectedValue.Length; i++)
			{
				if (expectedValue[i] != actualValue[i])
				{
					return i;
				}
			}

			return null;
		}
	}
}
