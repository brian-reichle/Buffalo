// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;

namespace Buffalo.Core.Lexer.Configuration
{
	sealed class ConfigManager : ConfigSettingList
	{
		public ConfigManager()
		{
			AddSetting("Name", _className = new StringConfigSetting() { DefaultValue = "Scanner" });
			AddSetting("Namespace", _classNamespace = new StringConfigSetting() { DefaultValue = "Unspecified" });
			AddSetting("Visibility", _visibility = new EnumConfigSetting<ClassVisibility>("visibility") { DefaultValue = ClassVisibility.Default });
			AddSetting("ElementSize", _elementSize = new EnumConfigSetting<TableElementSize>("element size") { DefaultValue = TableElementSize.Default });
			AddSetting("TableCompression", _tableCompression = new EnumConfigSetting<Compression>("mode") { DefaultValue = Compression.Auto });
			AddSetting("CacheTables", _cacheTables = new BoolConfigSetting());
			AddSetting("SuppressTableEmbedding", _suppressTableEmbedding = new BoolConfigSetting());
			AddSetting("RenderScanGraph", _renderScanGraph = new EnumConfigSetting<GraphStyle>("style") { DefaultValue = GraphStyle.Default });
		}

		public string ClassName => _className.Value;
		public string ClassNamespace => _classNamespace.Value;
		public ClassVisibility Visibility => _visibility.Value;
		public TableElementSize ElementSize => _elementSize.Value;
		public Compression TableCompression => _tableCompression.Value;
		public bool CacheTables => _cacheTables.Value;
		public bool SuppressTableEmbedding => _suppressTableEmbedding.Value;
		public GraphStyle RenderScanGraph => _renderScanGraph.Value;

		readonly StringConfigSetting _className;
		readonly StringConfigSetting _classNamespace;
		readonly EnumConfigSetting<ClassVisibility> _visibility;
		readonly EnumConfigSetting<TableElementSize> _elementSize;
		readonly EnumConfigSetting<Compression> _tableCompression;
		readonly BoolConfigSetting _cacheTables;
		readonly BoolConfigSetting _suppressTableEmbedding;
		readonly EnumConfigSetting<GraphStyle> _renderScanGraph;
	}
}
