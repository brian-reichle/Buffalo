// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Graph = Buffalo.Core.Common.Graph<string, string>;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class FAGraphTest
	{
		[Test]
		public void NewState()
		{
			var builder = new Graph.Builder();
			var graph = builder.Graph;
			Assert.That(graph.StartStates, Is.EquivalentTo(Array.Empty<Graph.State>()));

			var state1 = builder.NewState(false, null);
			Assert.That(state1.IsStartState, Is.EqualTo(false));
			Assert.That(state1.Label, Is.EqualTo(null));
			Assert.That(graph.StartStates, Is.EquivalentTo(Array.Empty<Graph.State>()));
			Assert.That(graph.States, Is.EquivalentTo(new Graph.State[] { state1 }));

			var state2 = builder.NewState(false, "9");
			Assert.That(state2.IsStartState, Is.EqualTo(false));
			Assert.That(state2.Label, Is.EqualTo("9"));
			Assert.That(graph.StartStates, Is.EquivalentTo(Array.Empty<Graph.State>()));
			Assert.That(graph.States, Is.EquivalentTo(new Graph.State[] { state1, state2 }));

			var state3 = builder.NewState(true, null);
			Assert.That(state3.IsStartState, Is.EqualTo(true));
			Assert.That(state3.Label, Is.EqualTo(null));
			Assert.That(graph.StartStates, Is.EquivalentTo(new Graph.State[] { state3 }));
			Assert.That(graph.States, Is.EquivalentTo(new Graph.State[] { state1, state2, state3 }));
		}

		[Test]
		public void DeleteState()
		{
			var graphBuilder = new Graph.Builder();
			var graph = graphBuilder.Graph;
			var state0 = graphBuilder.NewState(false, null);
			var state1 = graphBuilder.NewState(false, null);
			var state2 = graphBuilder.NewState(true, null);
			var state3 = graphBuilder.NewState(true, "1");
			var state4 = graphBuilder.NewState(false, "2");

			foreach (var state in new Graph.State[] { state1, state2, state3 })
			{
				graphBuilder.AddTransition(state0, state);
				graphBuilder.AddTransition(state, state0);
				graphBuilder.AddTransition(state, state);
			}

			var transitions = new List<Graph.Transition>();
			foreach (var state in graph.States)
			{
				transitions.AddRange(state.ToTransitions);
			}

			graphBuilder.Delete(state1);
			graphBuilder.Delete(state2);
			graphBuilder.Delete(state3);
			graphBuilder.Delete(state4);

			Assert.That(graph.StartStates, Is.EquivalentTo(Array.Empty<Graph.State>()));
			Assert.That(graph.States, Is.EquivalentTo(new Graph.State[] { state0 }));

			foreach (var transition in transitions)
			{
				Assert.That(transition.IsDeleted, Is.EqualTo(true));
			}

			Assert.That(state0.IsDeleted, Is.EqualTo(false));
			Assert.That(state1.IsDeleted, Is.EqualTo(true));
			Assert.That(state2.IsDeleted, Is.EqualTo(true));
			Assert.That(state3.IsDeleted, Is.EqualTo(true));
			Assert.That(state4.IsDeleted, Is.EqualTo(true));

			Assert.That(state0.ToTransitions, Is.EquivalentTo(Array.Empty<Graph.State>()));
			Assert.That(state0.FromTransitions, Is.EquivalentTo(Array.Empty<Graph.State>()));
		}

		[Test]
		public void AddTransition()
		{
			var builder = new Graph.Builder();
			var state1 = builder.NewState(false, null);
			var state2 = builder.NewState(false, null);

			Assert.That(state1.FromTransitions, Is.EquivalentTo(Array.Empty<Graph.State>()), "precondition: state1.FromTransitions");
			Assert.That(state1.ToTransitions, Is.EquivalentTo(Array.Empty<Graph.State>()), "precondition: state1.ToTransitions");
			Assert.That(state2.FromTransitions, Is.EquivalentTo(Array.Empty<Graph.State>()), "precondition: state2.FromTransitions");
			Assert.That(state2.ToTransitions, Is.EquivalentTo(Array.Empty<Graph.State>()), "precondition: state2.ToTransitions");

			builder.AddTransition(state1, state2);

			var transition1 = state1.ToTransitions.ToArray()[0];

			Assert.That(transition1.Label, Is.EqualTo(null));
			Assert.That(state1.FromTransitions, Is.EquivalentTo(Array.Empty<Graph.Transition>()), "state1.FromTransitions");
			Assert.That(state1.ToTransitions, Is.EquivalentTo(new Graph.Transition[] { transition1 }), "state1.ToTransitions");
			Assert.That(state2.FromTransitions, Is.EquivalentTo(new Graph.Transition[] { transition1 }), "state2.FromTransitions");
			Assert.That(state2.ToTransitions, Is.EquivalentTo(Array.Empty<Graph.Transition>()), "state2.ToTransitions");

			builder.AddTransition(state2, state1, "x");

			var transition2 = state2.ToTransitions.ToArray()[0];

			Assert.That(transition2.Label, Is.EqualTo("x"));
			Assert.That(state1.FromTransitions, Is.EquivalentTo(new Graph.Transition[] { transition2 }), "state1.FromTransitions");
			Assert.That(state1.ToTransitions, Is.EquivalentTo(new Graph.Transition[] { transition1 }), "state1.ToTransitions");
			Assert.That(state2.FromTransitions, Is.EquivalentTo(new Graph.Transition[] { transition1 }), "state2.FromTransitions");
			Assert.That(state2.ToTransitions, Is.EquivalentTo(new Graph.Transition[] { transition2 }), "state2.ToTransitions");
		}

		[Test]
		public void DeleteTransition()
		{
			var graph = new Graph.Builder();

			var state0 = graph.NewState(false, null);
			var state1 = graph.NewState(false, null);

			graph.AddTransition(state0, state1);
			graph.AddTransition(state1, state0);

			var transition01 = state0.ToTransitions.ToArray()[0];
			var transition10 = state1.ToTransitions.ToArray()[0];

			Assert.That(state0.FromTransitions, Is.EqualTo(new Graph.Transition[] { transition10 }));
			Assert.That(state0.ToTransitions, Is.EqualTo(new Graph.Transition[] { transition01 }));
			Assert.That(state1.FromTransitions, Is.EqualTo(new Graph.Transition[] { transition01 }));
			Assert.That(state1.ToTransitions, Is.EqualTo(new Graph.Transition[] { transition10 }));

			graph.Delete(transition01);

			Assert.That(transition01.IsDeleted, Is.EqualTo(true));
			Assert.That(transition10.IsDeleted, Is.EqualTo(false));

			Assert.That(state0.IsDeleted, Is.EqualTo(false));
			Assert.That(state1.IsDeleted, Is.EqualTo(false));

			Assert.That(state0.FromTransitions, Is.EqualTo(new Graph.Transition[] { transition10 }));
			Assert.That(state0.ToTransitions, Is.EqualTo(Array.Empty<Graph.Transition>()));
			Assert.That(state1.FromTransitions, Is.EqualTo(Array.Empty<Graph.Transition>()));
			Assert.That(state1.ToTransitions, Is.EqualTo(new Graph.Transition[] { transition10 }));
		}
	}
}
