// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.TestResources
{
	public static class MiscTestFiles
	{
		public const string Namespace = "Buffalo.TestResources.MiscTestFiles.";

		public static Resource GetLoremIpsum()
		{
			return new Resource(Namespace + "LoremIpsum.txt");
		}
	}
}
