// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;
using System.Text;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.SelfChecks.Test
{
	[TestFixture]
	public class FileTextConstraintTest
	{
		[Test]
		public void Match()
		{
			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadString();

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(false);
			file.Setup(x => x.GenerateFileContent(It.IsNotIn<TextWriter>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(file.Object, new FileTextConstraint(".txt", res));

			file.Verify();
		}

		[Test]
		public void SuffixMismatch()
		{
			const string expectedMessage =
				"Suffix'es do not match.\r\n" +
				"\r\n" +
				"Expected: .bob\r\n" +
				"Actual:   .txt";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadString();
			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(false);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<TextWriter>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileTextConstraint(".bob", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void IsBinaryMismatch()
		{
			const string expectedMessage =
				"IsBinary does not match.\r\n" +
				"\r\n" +
				"Expected: False\r\n" +
				"Actual:   True";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadString();

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);
			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(true);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<TextWriter>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileTextConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void HeadMismatch()
		{
			const string expectedMessage =
				"Resource differs at position 0\r\n" +
				"\r\n" +
				"Expected: Lorem ipsum dolo ...\r\n" +
				"Actual:   Xorem ipsum dolo ...";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = new StringBuilder(res.ReadString());
			buffer[0] = 'X';

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(false);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<TextWriter>()))
				.Callback(WriteBytes(buffer.ToString()))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileTextConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void TailMismatch()
		{
			const string expectedMessage =

				"Resource differs at position 449\r\n" +
				"\r\n" +
				"Expected: ...  id est laborum.\r\n" +
				"Actual:   ...  id est laborumX";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = new StringBuilder(res.ReadString());
			buffer[buffer.Length - 1] = 'X';

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);
			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(false);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<TextWriter>()))
				.Callback(WriteBytes(buffer.ToString()))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileTextConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void BodyMismatch()
		{
			const string expectedMessage =
				"Resource differs at position 225\r\n" +
				"\r\n" +
				"Expected: ... ex ea commodo consequat. Duis a ...\r\n" +
				"Actual:   ... ex ea commodo cXnsequat. Duis a ...";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = new StringBuilder(res.ReadString());
			buffer[buffer.Length >> 1] = 'X';

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);
			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(false);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<TextWriter>()))
				.Callback(WriteBytes(buffer.ToString()))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileTextConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		#region Implementation

		Action<TextWriter> WriteBytes(string value)
		{
			return delegate (TextWriter s)
			{
				s.Write(value);
			};
		}

		#endregion
	}
}
