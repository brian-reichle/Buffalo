// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoConfigParser";
TypeName = "ConfigTokenType";
Namespace = "Buffalo.Core.Parser.Configuration";
Visibility = Internal;
CacheTables = true;
TokenType = Terminal;
TypeHandling = Field;
TableCompression = CTB;

using Command = "Buffalo.Core.Parser.Configuration.ConfigCommand";
using Config = "Buffalo.Core.Parser.Configuration.Config";
using EntryPoint = "Buffalo.Core.Parser.Configuration.ConfigEntryPoint";
using Production = "Buffalo.Core.Parser.Configuration.ConfigProduction";
using Rule = "Buffalo.Core.Parser.Configuration.ConfigRule";
using Segment = "Buffalo.Core.Parser.Configuration.ConfigSegment";
using Setting = "Buffalo.Core.Parser.Configuration.ConfigSetting";
using Terminal = "Buffalo.Core.Parser.Configuration.ConfigToken";
using Using = "Buffalo.Core.Parser.Configuration.ConfigUsing";

entry <Config>;

<Config> ( Config )
	::= <Config> <Production>
	| <Config> Error
	| <ConfigSettings> { $$ = $0; }
	;

<ConfigSettings> ( Config )
	::= <ConfigSettings> <Setting>
	| <ConfigSettings> <Using>
	| <ConfigSettings> <EntryPoint>
	| <ConfigSettings> Error
	| Error
	|
	;

<Setting> ( Setting )
	::= Label Assign! <SettingValue> Semicolon!
	| Label Error Semicolon!
	;

<SettingValue> ( Terminal )
	::= String { $$ = $0; }
	| Label { $$ = $0; }
	;

<Using> ( Using )
	::= Using! Label Assign! String Semicolon!
	| Using! Error Semicolon!
	;

<EntryPoint> ( EntryPoint )
	::= Entry! NonTerminal Semicolon!
	| Entry! Error Semicolon!
	;

<Production> ( Production )
	::= NonTerminal <ProductionTypeDef> Becomes! <RuleList> Semicolon!
	| NonTerminal! Error Semicolon!
	;

<ProductionTypeDef> ( Terminal )
	::= OpenParen! Label CloseParen! { $$ = $1; }
	| { $$ = null; }
	;

<RuleList> ( Production )
	::= <RuleList> Pipe! <Rule>
	| <Rule>
	;

<Rule> ( Rule )
	::= <SegmentList> { $$ = $0; }
	| <SegmentList> <Command>
	;

<SegmentList> ( Rule )
	::= <SegmentList> <Segment>
	| Error
	|
	;

<Segment> ( Segment )
	::= <RawSegment>
	| <RawSegment> <SegmentModifier>
	;

<SegmentModifier> (Terminal)
	::= QuestionMark { $$ = $0; }
	| Bang { $$ = $0; }
	;

<RawSegment> ( Terminal )
	::= NonTerminal { $$ = $0; }
	| Label { $$ = $0; }
	;

<Command> ( Command )
	::= OpenBrace TargetValue Assign <CommandExpression> Semicolon CloseBrace { $$ = $3; }
	| OpenBrace! Error CloseBrace!
	;

<CommandExpression> ( Command )
	::= ArgumentValue
	| Null
	;
