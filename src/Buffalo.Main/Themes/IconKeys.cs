// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Windows;

namespace Buffalo.Main
{
	static class IconKeys
	{
		public static ResourceKey CloseKey { get; } = new ComponentResourceKey(typeof(IconKeys), nameof(CloseKey));
		public static ResourceKey GenerateKey { get; } = new ComponentResourceKey(typeof(IconKeys), nameof(GenerateKey));
		public static ResourceKey LoadKey { get; } = new ComponentResourceKey(typeof(IconKeys), nameof(LoadKey));
		public static ResourceKey NewKey { get; } = new ComponentResourceKey(typeof(IconKeys), nameof(NewKey));
		public static ResourceKey SaveKey { get; } = new ComponentResourceKey(typeof(IconKeys), nameof(SaveKey));
	}
}
