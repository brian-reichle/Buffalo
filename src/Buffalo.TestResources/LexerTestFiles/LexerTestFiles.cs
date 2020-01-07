// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Globalization;
using System.Reflection;

namespace Buffalo.TestResources
{
	public static class LexerTestFiles
	{
		public const string Namespace = "Buffalo.TestResources.LexerTestFiles.";

		public static ResourceSet LexerBase()
		{
			return new ResourceSet(
				new Resource(Namespace + "Base.Base.l"),
				new Resource(Namespace + "Base.Base.g.cs"),
				new TypedResource(Namespace + "Base.Base.0.table", true));
		}

		public static ResourceSet LexerByte()
		{
			return new ResourceSet(
				new Resource(Namespace + "ElementSize.Byte.l"),
				new Resource(Namespace + "ElementSize.Byte.g.cs"),
				new TypedResource(Namespace + "ElementSize.Byte.0.table", true));
		}

		public static ResourceSet LexerInt()
		{
			return new ResourceSet(
				new Resource(Namespace + "ElementSize.Int.l"),
				new Resource(Namespace + "ElementSize.Int.g.cs"),
				new TypedResource(Namespace + "ElementSize.Int.0.table", true));
		}

		public static ResourceSet LexerShort()
		{
			return new ResourceSet(
				new Resource(Namespace + "ElementSize.Short.l"),
				new Resource(Namespace + "ElementSize.Short.g.cs"),
				new TypedResource(Namespace + "ElementSize.Short.0.table", true));
		}

		public static ResourceSet LexerCacheTables()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.CacheTables.l"),
				new Resource(Namespace + "Flags.CacheTables.g.cs"),
				new TypedResource(Namespace + "Flags.CacheTables.0.table", true));
		}

		public static ResourceSet LexerRederGraph()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.RenderGraph.l"),
				new Resource(Namespace + "Flags.RenderGraph.g.cs"),
				new TypedResource(Namespace + "Flags.RenderGraph.0.table", true),
				new TypedResource(Namespace + "Flags.RenderGraph.0.svg", false));
		}

		public static ResourceSet LexerSuppressEmbedding()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.SuppressEmbedding.l"),
				new Resource(Namespace + "Flags.SuppressEmbedding.g.cs"));
		}

		public static ResourceSet LexerCacheMultiEntry()
		{
			return new ResourceSet(
				new Resource(Namespace + "Multi.CacheMultiEntry.l"),
				new Resource(Namespace + "Multi.CacheMultiEntry.g.cs"),
				new TypedResource(Namespace + "Multi.CacheMultiEntry.0.table", true));
		}

		public static ResourceSet LexerCacheMultiTable()
		{
			return new ResourceSet(
				new Resource(Namespace + "Multi.CacheMultiTable.l"),
				new Resource(Namespace + "Multi.CacheMultiTable.g.cs"),
				new TypedResource(Namespace + "Multi.CacheMultiTable.0.table", true),
				new TypedResource(Namespace + "Multi.CacheMultiTable.1.table", true));
		}

		public static ResourceSet LexerMultiEntry()
		{
			return new ResourceSet(
				new Resource(Namespace + "Multi.MultiEntry.l"),
				new Resource(Namespace + "Multi.MultiEntry.g.cs"),
				new TypedResource(Namespace + "Multi.MultiEntry.0.table", true));
		}

		public static ResourceSet LexerMultiTable()
		{
			return new ResourceSet(
				new Resource(Namespace + "Multi.MultiTable.l"),
				new Resource(Namespace + "Multi.MultiTable.g.cs"),
				new TypedResource(Namespace + "Multi.MultiTable.0.table", true),
				new TypedResource(Namespace + "Multi.MultiTable.1.table", true));
		}

		public static ResourceSet LexerCTB()
		{
			return new ResourceSet(
				new Resource(Namespace + "TableCompression.CTB.l"),
				new Resource(Namespace + "TableCompression.CTB.g.cs"),
				new TypedResource(Namespace + "TableCompression.CTB.0.table", true));
		}

		public static ResourceSet LexerNone()
		{
			return new ResourceSet(
				new Resource(Namespace + "TableCompression.None.l"),
				new Resource(Namespace + "TableCompression.None.g.cs"),
				new TypedResource(Namespace + "TableCompression.None.0.table", true));
		}

		public static ResourceSet LexerSimple()
		{
			return new ResourceSet(
				new Resource(Namespace + "TableCompression.Simple.l"),
				new Resource(Namespace + "TableCompression.Simple.g.cs"),
				new TypedResource(Namespace + "TableCompression.Simple.0.table", true));
		}

		public static ResourceSet LexerInternal()
		{
			return new ResourceSet(
				new Resource(Namespace + "Visibility.Internal.l"),
				new Resource(Namespace + "Visibility.Internal.g.cs"),
				new TypedResource(Namespace + "Visibility.Internal.0.table", true));
		}

		public static ResourceSet LexerPublic()
		{
			return new ResourceSet(
				new Resource(Namespace + "Visibility.Public.l"),
				new Resource(Namespace + "Visibility.Public.g.cs"),
				new TypedResource(Namespace + "Visibility.Public.0.table", true));
		}

		public static Resource DuplicatedSetting()
		{
			return new Resource(Namespace + "ErrorConfigs.DuplicatedSetting.l");
		}

		public static Resource MatchEmptyString()
		{
			return new Resource(Namespace + "ErrorConfigs.MatchEmptyString.l");
		}

		public static Resource NoRules()
		{
			return new Resource(Namespace + "ErrorConfigs.NoRules.l");
		}

		public static Resource NoStates()
		{
			return new Resource(Namespace + "ErrorConfigs.NoStates.l");
		}

		public static Resource ReErrors()
		{
			return new Resource(Namespace + "ErrorConfigs.ReErrors.l");
		}

		public static Resource UnknownSetting()
		{
			return new Resource(Namespace + "ErrorConfigs.UnknownSetting.l");
		}

		public static Resource UnknownVisibility()
		{
			return new Resource(Namespace + "ErrorConfigs.UnknownVisibility.l");
		}

		public static ResourceSet Parser()
		{
			return new ResourceSet(
				new Resource(Namespace + "ParserConfig.AutoConfigScanner.l"),
				new Resource(Namespace + "ParserConfig.AutoConfigScanner.g.cs"),
				new TypedResource(Namespace + "ParserConfig.AutoConfigScanner.0.table", true));
		}

		public static ResourceSet Scanner()
		{
			return new ResourceSet(
				new Resource(Namespace + "ScannerConfig.AutoConfigScanner.l"),
				new Resource(Namespace + "ScannerConfig.AutoConfigScanner.g.cs"),
				new TypedResource(Namespace + "ScannerConfig.AutoConfigScanner.0.table", true));
		}

		public static ResourceSet GetNamedResourceSet(string name)
		{
			return (ResourceSet)typeof(LexerTestFiles).InvokeMember(
				name,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
				null,
				null,
				null,
				CultureInfo.InvariantCulture);
		}
	}
}
