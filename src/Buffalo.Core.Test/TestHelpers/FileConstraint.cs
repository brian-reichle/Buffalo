// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Diagnostics;
using Buffalo.TestResources;
using NUnit.Framework.Constraints;

namespace Buffalo.Core.Test
{
	abstract class FileConstraint : Constraint
	{
		public FileConstraint(string suffix, Resource resource)
		{
			Suffix = suffix;
			Resource = resource;
			_messages = new List<ConstraintError>();
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			_messages.Clear();
			MatchesCore(_messages, actual);

			if (_messages.Count == 0)
			{
				return new ConstraintResult(this, actual, true);
			}

			return new Result(this, actual, _messages);
		}

		public Resource Resource { get; }
		public string Suffix { get; }
		public abstract bool IsBinary { get; }

		void MatchesCore(List<ConstraintError> messages, object actual)
		{
			if (actual is IAdditionalFile file)
			{
				VerifySuffix(messages, file);
				VerifyIsBinary(messages, file);
				VerifyFileContent(messages, file);
			}
			else
			{
				messages.Add(new SimpleConstraintError("value is null"));
			}
		}

		void VerifySuffix(List<ConstraintError> messages, IAdditionalFile file)
		{
			if (file.Suffix != Suffix)
			{
				messages.Add(new MismatchConstraintError<string>(
					"Suffix'es do not match.",
					Suffix,
					file.Suffix));
			}
		}

		void VerifyIsBinary(List<ConstraintError> messages, IAdditionalFile file)
		{
			if (file.IsBinary != IsBinary)
			{
				messages.Add(new MismatchConstraintError<bool>(
					"IsBinary does not match.",
					IsBinary,
					file.IsBinary));
			}
		}

		protected abstract void VerifyFileContent(List<ConstraintError> messages, IAdditionalFile file);

		[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
		readonly List<ConstraintError> _messages;

		sealed class Result : ConstraintResult
		{
			public Result(IConstraint constraint, object actualValue, List<ConstraintError> messages)
				: base(constraint, actualValue)
			{
				_messages = messages;
			}

			public sealed override void WriteMessageTo(MessageWriter writer)
			{
				foreach (var message in _messages)
				{
					message.WriteDescriptionTo(writer);
				}

				foreach (var message in _messages)
				{
					message.WriteMessageTo(writer);
				}
			}

			readonly List<ConstraintError> _messages;
		}
	}
}
