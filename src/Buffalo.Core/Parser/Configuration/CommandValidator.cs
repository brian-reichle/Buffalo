// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;
using Buffalo.Core.Parser.Configuration;

namespace Buffalo.Core.Parser
{
	sealed class CommandValidator : IConfigCommandVisitor
	{
		public CommandValidator(IErrorReporter reporter)
		{
			_reporter = reporter;
		}

		public void Validate(ConfigCommand command, Production production)
		{
			var oldProduction = _production;

			try
			{
				_production = production;
				command.Apply(this);
			}
			finally
			{
				_production = oldProduction;
			}
		}

		void IConfigCommandVisitor.Visit(ConfigCommandNull nullValue)
		{
		}

		void IConfigCommandVisitor.Visit(ConfigCommandArg argValue)
		{
			var token = argValue.Value;

			if (!int.TryParse(token.Text, out var argIndex) || argIndex >= _production.Segments.Length)
			{
				ReporterHelper.AddError(_reporter, token, "Invalid argument reference");
			}
		}

		void IConfigCommandVisitor.Visit(ConfigCommandMethod method)
		{
			foreach (var command in method.Arguments)
			{
				command.Apply(this);
			}
		}

		Production _production;
		readonly IErrorReporter _reporter;
	}
}
