// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Buffalo.TestResources;

namespace Buffalo.Core.Test
{
	sealed class FileBinaryConstraint : FileConstraint
	{
		public FileBinaryConstraint(string suffix, Resource resource)
			: base(suffix, resource)
		{
		}

		public override bool IsBinary => true;

		protected override void VerifyFileContent(List<ConstraintError> messages, IAdditionalFile file)
		{
			byte[] actualBytes;

			using (var actualStream = new MemoryStream())
			{
				file.GenerateFileContent(actualStream);
				actualBytes = actualStream.ToArray();
			}

			byte[] expectedBytes;

			using (var expectedStream = Resource.CreateStream())
			{
				expectedBytes = new byte[expectedStream.Length];
				expectedStream.Read(expectedBytes, 0, expectedBytes.Length);
			}

			if (actualBytes.Length != expectedBytes.Length)
			{
				messages.Add(new MismatchConstraintError<int>(
					"Resource lengths do not match.",
					actualBytes.Length,
					expectedBytes.Length));
			}
			else
			{
				var firstErrorOrNull = IndexOfFirstError(expectedBytes, actualBytes);

				if (firstErrorOrNull.HasValue)
				{
					var builder = new StringBuilder();
					var firstError = firstErrorOrNull.Value;

					builder.Append("Resource differs at position ");
					builder.Append(firstError);
					builder.AppendLine();
					builder.AppendLine();

					builder.Append("Expected: ");
					AppendBufferSegment(builder, expectedBytes, firstError);

					builder.AppendLine();

					builder.Append("Actual:   ");
					AppendBufferSegment(builder, actualBytes, firstError);

					messages.Add(new SimpleConstraintError(builder.ToString()));
				}
			}
		}

		void AppendBufferSegment(StringBuilder builder, byte[] buffer, int index)
		{
			var min = index - 5;
			var max = index + 6;
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

			if (max < buffer.Length - 1)
			{
				leadout = " ...";
			}
			else
			{
				max = buffer.Length - 1;
				leadout = string.Empty;
			}

			builder.Append(leadin);

			for (var i = min; i < index; i++)
			{
				builder.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
				builder.Append(", ");
			}

			builder.Append('>');
			builder.Append(buffer[index].ToString("X2", CultureInfo.InvariantCulture));
			builder.Append('<');

			for (var i = index + 1; i < max; i++)
			{
				builder.Append(", ");
				builder.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
			}

			builder.Append(leadout);
		}

		int? IndexOfFirstError(byte[] expectedBytes, byte[] actualBytes)
		{
			for (var i = 0; i < expectedBytes.Length; i++)
			{
				if (expectedBytes[i] != actualBytes[i])
				{
					return i;
				}
			}

			return null;
		}
	}
}
