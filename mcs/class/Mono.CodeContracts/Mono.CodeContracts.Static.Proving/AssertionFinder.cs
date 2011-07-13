// 
// AssertionFinder.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Proving {
	static class AssertionFinder {
		public static void ValidateAssertions<TExpression, TVariable> (IFactQuery<BoxedExpression, TVariable> facts, IMethodDriver<TExpression, TVariable> driver, List<string> proofResults)
			where TExpression : IEquatable<TExpression>
			where TVariable : IEquatable<TVariable>
		{
			Bind<TExpression, TVariable>.ValidateAssertions (facts, driver, proofResults);
		}

		#region Nested type: Bind
		private static class Bind<TExpression, TVariable>
			where TExpression : IEquatable<TExpression>
			where TVariable : IEquatable<TVariable> {
			public static void ValidateAssertions (IFactQuery<BoxedExpression, TVariable> facts, IMethodDriver<TExpression, TVariable> driver, List<string> proofResults)
			{
				APC entryAfterRequires = driver.ContextProvider.MethodContext.CFG.EntryAfterRequires;
				if (facts.IsUnreachable (entryAfterRequires)) {
					proofResults.Add ("Method precondition is unsatisfiable");
					return;
				}

				object assertStats;
				foreach (AssertionObligation obl in GetAssertions (driver, out assertStats)) {
					ProofOutcome outcome = facts.IsTrue (obl.Apc, BoxedExpression.For (driver.ContextProvider.ExpressionContext.Refine (obl.Apc, obl.Condition), driver.ExpressionDecoder));

					string pc = obl.Apc.ToString ();
					switch (outcome) {
					case ProofOutcome.Top:
						proofResults.Add ("Assertion at point " + pc + " is unproven");
						break;
					case ProofOutcome.True:
						proofResults.Add ("Assertion at point " + pc + " is true");
						break;
					case ProofOutcome.False:
						proofResults.Add ("Assertion at point " + pc + " is false");
						break;
					case ProofOutcome.Bottom:
						proofResults.Add ("Assertion at point " + pc + " is unreachable");
						break;
					}
				}
			}

			private static IEnumerable<AssertionObligation> GetAssertions (IMethodDriver<TExpression, TVariable> driver, out object assertStats)
			{
				var analysis = new AssertionCrawlerAnalysis ();
				List<AssertionObligation> obligations = analysis.Gather (driver);

				assertStats = null;
				return obligations;
			}

			#region Nested type: AssertionCrawlerAnalysis
			private class AssertionCrawlerAnalysis : ValueCodeVisitor<TVariable> {
				private readonly List<AssertionObligation> Obligations = new List<AssertionObligation> ();

				public List<AssertionObligation> Gather (IMethodDriver<TExpression, TVariable> driver)
				{
					Run (driver.ValueLayer);
					return this.Obligations;
				}

				public override bool Assert (APC pc, EdgeTag tag, TVariable condition, bool data)
				{
					if (pc.InsideRequiresAtCallInsideContract)
						return data;

					this.Obligations.Add (new AssertionObligation (pc, tag, condition));
					return data;
				}

				public override bool Assume (APC pc, EdgeTag tag, TVariable condition, bool data)
				{
					if (!pc.InsideRequiresAtCallInsideContract && tag == EdgeTag.Assume)
						this.Obligations.Add (new AssertionObligation (pc, tag, condition, true));

					return data;
				}
			}
			#endregion

			#region Nested type: AssertionObligation
			private class AssertionObligation {
				public readonly APC Apc;
				public readonly TVariable Condition;
				public readonly bool IsAssume;
				public readonly EdgeTag Tag;

				public AssertionObligation (APC pc, EdgeTag tag, TVariable cond)
					: this (pc, tag, cond, false)
				{
				}

				public AssertionObligation (APC pc, EdgeTag tag, TVariable cond, bool isAssume)
				{
					this.Apc = pc;
					this.Tag = tag;
					this.Condition = cond;
					this.IsAssume = isAssume;
				}
			}
			#endregion
		}
		#endregion
	}
}
