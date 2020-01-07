// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigManager : ConfigSettingList
	{
		public ConfigManager()
		{
			AddSetting("Name", _parserName = new StringConfigSetting() { DefaultValue = "Parser" });
			AddSetting("TypeName", _typeName = new StringConfigSetting());
			AddSetting("Namespace", _classNamespace = new StringConfigSetting() { DefaultValue = "Unspecified" });
			AddSetting("Visibility", _visibility = new EnumConfigSetting<ClassVisibility>("visibility") { DefaultValue = ClassVisibility.Default });
			AddSetting("TypeVisibility", _typeVisibility = new EnumConfigSetting<ClassVisibility>("visibility") { DefaultValue = ClassVisibility.Default });
			AddSetting("ElementSize", _elementSize = new EnumConfigSetting<TableElementSize>("element size") { DefaultValue = TableElementSize.Default });
			AddSetting("TableCompression", _tableCompression = new EnumConfigSetting<Compression>("mode") { DefaultValue = Compression.Auto });
			AddSetting("TypeHandling", _typeHandling = new EnumConfigSetting<TypeHandling>("mode") { DefaultValue = TypeHandling.Default });
			AddSetting("RenderParseTable", _renderParseTable = new BoolConfigSetting());
			AddSetting("RenderParseGraph", _renderParseGraph = new EnumConfigSetting<GraphStyle>("style") { DefaultValue = GraphStyle.Default });
			AddSetting("Trace", _trace = new BoolConfigSetting());
			AddSetting("CacheTables", _cacheTables = new BoolConfigSetting());
			AddSetting("SuppressTableEmbedding", _suppressTableEmbedding = new BoolConfigSetting());
			AddSetting("TrimParseGraph", _trimParseGraph = new BoolConfigSetting() { DefaultValue = true });
			AddSetting("TokenType", _tokenType = new LabelConfigSetting());
		}

		public string ParserName => _parserName.Value;
		public string TypeName => _typeName.Value ?? (_parserName.Value + "TokenType");
		public string ClassNamespace => _classNamespace.Value;
		public bool RenderParseTable => _renderParseTable.Value;
		public bool Trace => _trace.Value;
		public bool CacheTables => _cacheTables.Value;
		public bool SuppressTableEmbedding => _suppressTableEmbedding.Value;
		public bool TrimParseGraph => _trimParseGraph.Value;
		public string TokenType => _tokenType.Value;
		public ClassVisibility Visibility => _visibility.Value;

		public ClassVisibility TypeVisibility
		{
			get
			{
				if (_typeVisibility.ValueSet)
				{
					return _typeVisibility.Value;
				}
				else
				{
					return _visibility.Value;
				}
			}
		}

		public TableElementSize ElementSize => _elementSize.Value;
		public GraphStyle RenderParseGraph => _renderParseGraph.Value;
		public Compression TableCompression => _tableCompression.Value;
		public TypeHandling TypeHandling => _typeHandling.Value;

		readonly StringConfigSetting _parserName;
		readonly StringConfigSetting _typeName;
		readonly StringConfigSetting _classNamespace;
		readonly BoolConfigSetting _renderParseTable;
		readonly BoolConfigSetting _trace;
		readonly BoolConfigSetting _cacheTables;
		readonly BoolConfigSetting _suppressTableEmbedding;
		readonly BoolConfigSetting _trimParseGraph;
		readonly LabelConfigSetting _tokenType;
		readonly EnumConfigSetting<ClassVisibility> _visibility;
		readonly EnumConfigSetting<ClassVisibility> _typeVisibility;
		readonly EnumConfigSetting<TableElementSize> _elementSize;
		readonly EnumConfigSetting<GraphStyle> _renderParseGraph;
		readonly EnumConfigSetting<Compression> _tableCompression;
		readonly EnumConfigSetting<TypeHandling> _typeHandling;
	}
}
