// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoConfigParser";
TypeName = "ConfigTokenType";
Namespace = "Buffalo.Core.Lexer.Configuration";
Visibility = Internal;
ElementSize = Short;
CacheTables = true;
TokenType = Terminal;
TypeHandling = Field;
TableCompression = CTB;

using Config = "Buffalo.Core.Lexer.Configuration.Config";
using Rule = "Buffalo.Core.Lexer.Configuration.ConfigRule";
using Setting = "Buffalo.Core.Lexer.Configuration.ConfigSetting";
using State = "Buffalo.Core.Lexer.Configuration.ConfigState";
using Terminal = "Buffalo.Core.Lexer.Configuration.ConfigToken";

entry <Config>;

<Config> ( Config )
	::= <Config> <State>
	| <Config> Error
	| <ConfigSettings> { $$ = $0; }
	;

<ConfigSettings> ( Config )
	::= <ConfigSettings> <Setting>
	| <ConfigSettings> Error
	| Error
	|
	;

<Setting> ( Setting )
	::= Label Assign! <SettingValue> Semicolon!
	| Label! Error Semicolon!
	;

<SettingValue> ( Terminal )
	::= String { $$ = $0; }
	| Label { $$ = $0; }
	;

<State> ( State )
	::= StateKeyword! Label OpenBrace! <RuleList> CloseBrace!
	;

<RuleList> ( State )
	::= <RuleList> <Rule>
	| Error
	|
	;

<Rule> ( Rule )
	::= Regex OpenBrace! TokenKeyword! Label Semicolon! CloseBrace!
	| Regex! OpenBrace! Error CloseBrace!
	;
