// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	abstract class BaseSetting<T> : IConfigSetting
	{
		public T Value { get; protected set; }
		public bool ValueSet { get; protected set; }
		public T DefaultValue { get; set; }

		public void Reset()
		{
			Value = DefaultValue;
			ValueSet = false;
		}

		public bool Set(IErrorReporter reporter, IToken valueToken)
		{
			var success = SetCore(reporter, valueToken);

			if (success)
			{
				ValueSet = true;
			}

			return success;
		}

		protected abstract bool SetCore(IErrorReporter reporter, IToken valueToken);
	}
}
