// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Buffalo.Core.Parser.Configuration;

namespace Buffalo.Core.Parser
{
	sealed class CommandWriter : IConfigCommandVisitor
	{
		public CommandWriter(TextWriter writer, Config config)
		{
			_writer = writer;
			_config = config;
		}

		public void Write(ConfigCommand command, Production production)
		{
			_production = production;
			command.Apply(this);
		}

		void IConfigCommandVisitor.Visit(ConfigCommandNull nullValue)
		{
			_writer.Write("null");
		}

		void IConfigCommandVisitor.Visit(ConfigCommandArg argValue)
		{
			var argumentIndex = int.Parse(argValue.Value.Text, CultureInfo.InvariantCulture);
			var cUsing = argValue.Using;

			if (_config.Manager.TypeHandling == TypeHandling.Cast)
			{
				_writer.Write("(Type_");
				_writer.Write(cUsing.Label.Text);
				_writer.Write(')');
			}

			_writer.Write("stack[stack.Count - ");
			_writer.Write(_production.Segments.Length - argumentIndex);

			_writer.Write("].value");

			if (_config.Manager.TypeHandling == TypeHandling.Field)
			{
				_writer.Write('_');
				_writer.Write(cUsing.Label.Text);
			}
		}

		void IConfigCommandVisitor.Visit(ConfigCommandMethod method)
		{
			_writer.Write(method.Name.Text);
			_writer.Write("(");

			using (IEnumerator<ConfigCommand> enumerator = method.Arguments.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					enumerator.Current.Apply(this);

					while (enumerator.MoveNext())
					{
						_writer.Write(", ");
						enumerator.Current.Apply(this);
					}
				}
			}

			_writer.Write(")");
		}

		Production _production;
		readonly TextWriter _writer;
		readonly Config _config;
	}
}
