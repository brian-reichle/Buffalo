// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Globalization;
using System.Reflection;

namespace Buffalo.TestResources
{
	public static class ParserTestFiles
	{
		public const string Namespace = "Buffalo.TestResources.ParserTestFiles.";

		public static ResourceSet ParserBase()
		{
			return new ResourceSet(
				new Resource(Namespace + "Base.AutoParserBase.y"),
				new Resource(Namespace + "Base.AutoParserBase.g.cs"),
				new TypedResource(Namespace + "Base.AutoParserBase.table", true));
		}

		public static ResourceSet ParserByte()
		{
			return new ResourceSet(
				new Resource(Namespace + "ElementSize.AutoParserByte.y"),
				new Resource(Namespace + "ElementSize.AutoParserByte.g.cs"),
				new TypedResource(Namespace + "ElementSize.AutoParserByte.table", true));
		}

		public static ResourceSet ParserInt()
		{
			return new ResourceSet(
				new Resource(Namespace + "ElementSize.AutoParserInt.y"),
				new Resource(Namespace + "ElementSize.AutoParserInt.g.cs"),
				new TypedResource(Namespace + "ElementSize.AutoParserInt.table", true));
		}

		public static ResourceSet ParserShort()
		{
			return new ResourceSet(
				new Resource(Namespace + "ElementSize.AutoParserShort.y"),
				new Resource(Namespace + "ElementSize.AutoParserShort.g.cs"),
				new TypedResource(Namespace + "ElementSize.AutoParserShort.table", true));
		}

		public static ResourceSet ParserCacheTables()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.AutoParserCacheTables.y"),
				new Resource(Namespace + "Flags.AutoParserCacheTables.g.cs"),
				new TypedResource(Namespace + "Flags.AutoParserCacheTables.table", true));
		}

		public static ResourceSet ParserShowTable()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.AutoParserShowTable.y"),
				new Resource(Namespace + "Flags.AutoParserShowTable.g.cs"),
				new TypedResource(Namespace + "Flags.AutoParserShowTable.table", true),
				new TypedResource(Namespace + "Flags.AutoParserShowTable.txt", false));
		}

		public static ResourceSet ParserSuppressEmbedding()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.AutoParserSuppressEmbedding.y"),
				new Resource(Namespace + "Flags.AutoParserSuppressEmbedding.g.cs"));
		}

		public static ResourceSet ParserSVG()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.AutoParserSVG.y"),
				new Resource(Namespace + "Flags.AutoParserSVG.g.cs"),
				new TypedResource(Namespace + "Flags.AutoParserSVG.table", true),
				new TypedResource(Namespace + "Flags.AutoParserSVG.svg", false));
		}

		public static ResourceSet ParserTrace()
		{
			return new ResourceSet(
				new Resource(Namespace + "Flags.AutoParserTrace.y"),
				new Resource(Namespace + "Flags.AutoParserTrace.g.cs"),
				new TypedResource(Namespace + "Flags.AutoParserTrace.table", true));
		}

		public static ResourceSet ParserCTB()
		{
			return new ResourceSet(
				new Resource(Namespace + "TableCompression.AutoParserCTB.y"),
				new Resource(Namespace + "TableCompression.AutoParserCTB.g.cs"),
				new TypedResource(Namespace + "TableCompression.AutoParserCTB.table", true));
		}

		public static ResourceSet ParserNone()
		{
			return new ResourceSet(
				new Resource(Namespace + "TableCompression.AutoParserNone.y"),
				new Resource(Namespace + "TableCompression.AutoParserNone.g.cs"),
				new TypedResource(Namespace + "TableCompression.AutoParserNone.table", true));
		}

		public static ResourceSet ParserSimple()
		{
			return new ResourceSet(
				new Resource(Namespace + "TableCompression.AutoParserSimple.y"),
				new Resource(Namespace + "TableCompression.AutoParserSimple.g.cs"),
				new TypedResource(Namespace + "TableCompression.AutoParserSimple.table", true));
		}

		public static ResourceSet ParserCast()
		{
			return new ResourceSet(
				new Resource(Namespace + "TypeHandling.AutoParserCast.y"),
				new Resource(Namespace + "TypeHandling.AutoParserCast.g.cs"),
				new TypedResource(Namespace + "TypeHandling.AutoParserCast.table", true));
		}

		public static ResourceSet ParserField()
		{
			return new ResourceSet(
				new Resource(Namespace + "TypeHandling.AutoParserField.y"),
				new Resource(Namespace + "TypeHandling.AutoParserField.g.cs"),
				new TypedResource(Namespace + "TypeHandling.AutoParserField.table", true));
		}

		public static ResourceSet ParserInternal()
		{
			return new ResourceSet(
				new Resource(Namespace + "Visibility.AutoParserInternal.y"),
				new Resource(Namespace + "Visibility.AutoParserInternal.g.cs"),
				new TypedResource(Namespace + "Visibility.AutoParserInternal.table", true));
		}

		public static ResourceSet ParserPublic()
		{
			return new ResourceSet(
				new Resource(Namespace + "Visibility.AutoParserPublic.y"),
				new Resource(Namespace + "Visibility.AutoParserPublic.g.cs"),
				new TypedResource(Namespace + "Visibility.AutoParserPublic.table", true));
		}

		public static ResourceSet ParserTypeVisibility()
		{
			return new ResourceSet(
				new Resource(Namespace + "Visibility.AutoParserTypeVisibility.y"),
				new Resource(Namespace + "Visibility.AutoParserTypeVisibility.g.cs"),
				new TypedResource(Namespace + "Visibility.AutoParserTypeVisibility.table", true));
		}

		public static ResourceSet ParserErrorWithoutEOF()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserErrorWithoutEOF.y"),
				new Resource(Namespace + "Gramour.AutoParserErrorWithoutEOF.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserErrorWithoutEOF.table", true));
		}

		public static ResourceSet ParserGotoOnlyStates()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserGotoOnlyStates.y"),
				new Resource(Namespace + "Gramour.AutoParserGotoOnlyStates.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserGotoOnlyStates.table", true));
		}

		public static ResourceSet ParserIgnoredSegments()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserIgnoredSegments.y"),
				new Resource(Namespace + "Gramour.AutoParserIgnoredSegments.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserIgnoredSegments.table", true));
		}

		public static ResourceSet ParserMerge()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserMerge.y"),
				new Resource(Namespace + "Gramour.AutoParserMerge.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserMerge.table", true),
				new TypedResource(Namespace + "Gramour.AutoParserMerge.txt", false));
		}

		public static ResourceSet ParserMergeNonTerminals()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserMergeNonTerminals.y"),
				new Resource(Namespace + "Gramour.AutoParserMergeNonTerminals.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserMergeNonTerminals.table", true));
		}

		public static ResourceSet ParserMultipleEntry()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserMultipleEntry.y"),
				new Resource(Namespace + "Gramour.AutoParserMultipleEntry.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserMultipleEntry.table", true));
		}

		public static ResourceSet ParserNoMerge()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserNoMerge.y"),
				new Resource(Namespace + "Gramour.AutoParserNoMerge.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserNoMerge.table", true),
				new TypedResource(Namespace + "Gramour.AutoParserNoMerge.txt", false));
		}

		public static ResourceSet ParserOptionalSegments()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserOptionalSegments.y"),
				new Resource(Namespace + "Gramour.AutoParserOptionalSegments.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserOptionalSegments.table", true));
		}

		public static ResourceSet ParserReductionDuringError()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserReductionDuringError.y"),
				new Resource(Namespace + "Gramour.AutoParserReductionDuringError.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserReductionDuringError.table", true));
		}

		public static ResourceSet ParserShiftReduceConflict()
		{
			return new ResourceSet(
				new Resource(Namespace + "Gramour.AutoParserShiftReduceConflict.y"),
				new Resource(Namespace + "Gramour.AutoParserShiftReduceConflict.g.cs"),
				new TypedResource(Namespace + "Gramour.AutoParserShiftReduceConflict.table", true));
		}

		public static Resource DuplicateEntryPoints()
		{
			return new Resource(Namespace + "ErrorConfigs.DuplicateEntryPoint.y");
		}

		public static Resource DuplicateOption()
		{
			return new Resource(Namespace + "ErrorConfigs.DuplicateOption.y");
		}

		public static Resource DuplicateProduction()
		{
			return new Resource(Namespace + "ErrorConfigs.DuplicateProduction.y");
		}

		public static Resource DuplicateReduction()
		{
			return new Resource(Namespace + "ErrorConfigs.DuplicateReduction.y");
		}

		public static Resource InvalidProductionArg()
		{
			return new Resource(Namespace + "ErrorConfigs.InvalidProductionArg.y");
		}

		public static Resource NoProductions()
		{
			return new Resource(Namespace + "ErrorConfigs.NoProductions.y");
		}

		public static Resource ParserPseudoReachable()
		{
			return new Resource(Namespace + "ErrorConfigs.PseudoReachable.y");
		}

		public static Resource ReDefinedDifferentType()
		{
			return new Resource(Namespace + "ErrorConfigs.ReDefinedDifferentType.y");
		}

		public static Resource ReDefinedSameType()
		{
			return new Resource(Namespace + "ErrorConfigs.ReDefinedSameType.y");
		}

		public static Resource ReduceAcceptConflict()
		{
			return new Resource(Namespace + "ErrorConfigs.ReduceAcceptConflict.y");
		}

		public static Resource ReduceReduceConflict()
		{
			return new Resource(Namespace + "ErrorConfigs.ReduceReduceConflict.y");
		}

		public static Resource UndefinedEntry()
		{
			return new Resource(Namespace + "ErrorConfigs.UndefinedEntry.y");
		}

		public static Resource UndefinedProduction()
		{
			return new Resource(Namespace + "ErrorConfigs.UndefinedProduction.y");
		}

		public static Resource UnknownOption()
		{
			return new Resource(Namespace + "ErrorConfigs.UnknownOption.y");
		}

		public static Resource UnreachableNonTerminal()
		{
			return new Resource(Namespace + "ErrorConfigs.UnreachableNonTerminal.y");
		}

		public static ResourceSet Parser()
		{
			return new ResourceSet(
				new Resource(Namespace + "ParserConfig.AutoConfigParser.y"),
				new Resource(Namespace + "ParserConfig.AutoConfigParser.g.cs"),
				new TypedResource(Namespace + "ParserConfig.AutoConfigParser.table", true));
		}

		public static ResourceSet Scanner()
		{
			return new ResourceSet(
				new Resource(Namespace + "ScannerConfig.AutoConfigParser.y"),
				new Resource(Namespace + "ScannerConfig.AutoConfigParser.g.cs"),
				new TypedResource(Namespace + "ScannerConfig.AutoConfigParser.table", true));
		}

		public static ResourceSet GetNamedResourceSet(string name)
		{
			return (ResourceSet)typeof(ParserTestFiles).InvokeMember(
				name,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
				null,
				null,
				null,
				CultureInfo.InvariantCulture);
		}
	}
}
