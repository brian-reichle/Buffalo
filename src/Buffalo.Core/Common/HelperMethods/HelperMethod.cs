// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	abstract class HelperMethod
	{
		public static readonly HelperMethod[] Empty = Array.Empty<HelperMethod>();

		public static HelperMethod[] GetDecompressionMethods(Compression method, ElementSizeStrategy sizeStrategy, bool fromResource)
		{
			switch (method)
			{
				case Compression.Simple:
					return new HelperMethod[]
					{
						new MethodExpandSimple(sizeStrategy, fromResource),
					};

				case Compression.CTB:
					return new HelperMethod[]
					{
						new MethodExpandCTB(sizeStrategy, fromResource),
					};

				case Compression.None:
					if (fromResource)
					{
						return new HelperMethod[]
						{
							new MethodExtract(sizeStrategy),
						};
					}
					else
					{
						return Empty;
					}

				default: return Empty;
			}
		}

		public abstract void Write(TextWriter writer, int indent);

		public abstract bool Equals(HelperMethod other);
	}
}
