// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using NUnit.Framework.Constraints;

namespace Buffalo.Core.Test
{
	abstract class ConstraintError
	{
		public abstract void WriteDescriptionTo(MessageWriter writer);
		public abstract void WriteMessageTo(MessageWriter writer);
	}
}
