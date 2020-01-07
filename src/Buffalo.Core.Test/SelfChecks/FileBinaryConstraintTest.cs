// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.SelfChecks.Test
{
	[TestFixture]
	public class FileBinaryConstraintTest
	{
		[Test]
		public void Match()
		{
			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadBytes();

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(true);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<Stream>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(file.Object, new FileBinaryConstraint(".txt", res));

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
			var buffer = res.ReadBytes();

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(true);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<Stream>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileBinaryConstraint(".bob", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void IsBinaryMismatch()
		{
			const string expectedMessage =
				"IsBinary does not match.\r\n" +
				"\r\n" +
				"Expected: True\r\n" +
				"Actual:   False";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadBytes();

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(false);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<Stream>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileBinaryConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void HeadMismatch()
		{
			const string expectedMessage =
				"Resource differs at position 0\r\n" +
				"\r\n" +
				"Expected: >EF<, BB, BF, 4C, 6F, 72 ...\r\n" +
				"Actual:   >42<, BB, BF, 4C, 6F, 72 ...";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadBytes();
			buffer[0] = 0x42;

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(true);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<Stream>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileBinaryConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void TailMismatch()
		{
			const string expectedMessage =
				"Resource differs at position 452\r\n" +
				"\r\n" +
				"Expected: ... 62, 6F, 72, 75, 6D, >2E<\r\n" +
				"Actual:   ... 62, 6F, 72, 75, 6D, >42<";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadBytes();
			buffer[buffer.Length - 1] = 0x42;

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(true);
			file.Setup(x => x.GenerateFileContent(It.IsNotNull<Stream>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileBinaryConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		[Test]
		public void BodyMismatch()
		{
			const string expectedMessage =
				"Resource differs at position 226\r\n" +
				"\r\n" +
				"Expected: ... 6D, 6D, 6F, 64, 6F, >20<, 63, 6F, 6E, 73, 65 ...\r\n" +
				"Actual:   ... 6D, 6D, 6F, 64, 6F, >42<, 63, 6F, 6E, 73, 65 ...";

			var res = MiscTestFiles.GetLoremIpsum();
			var buffer = res.ReadBytes();
			buffer[buffer.Length >> 1] = 0x42;

			var file = new Mock<IAdditionalFile>(MockBehavior.Strict);

			file.Setup(x => x.Suffix).Returns(".txt");
			file.Setup(x => x.IsBinary).Returns(true);
			file.Setup(x => x.GenerateFileContent(It.IsAny<Stream>()))
				.Callback(WriteBytes(buffer))
				.Verifiable();

			Assert.That(
				() => Assert.That(file.Object, new FileBinaryConstraint(".txt", res)),
				Throws.Exception.InstanceOf<AssertionException>().With.Message.EqualTo(expectedMessage));

			file.Verify();
		}

		#region Implementation

		Action<Stream> WriteBytes(byte[] buffer)
		{
			return delegate (Stream s)
			{
				s.Write(buffer, 0, buffer.Length);
			};
		}

		#endregion
	}
}
