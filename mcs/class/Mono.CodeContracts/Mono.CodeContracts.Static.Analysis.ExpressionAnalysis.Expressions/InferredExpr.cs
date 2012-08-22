using System;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions
{
	internal struct InferredExpr
	{
		public BoxedExpression[] PreConditions;
		public BoxedExpression[] PostConditions;
		public BoxedExpression[] ObjectInvariants;

		public override string ToString ()
		{
			return string.Format ("Preconditions:\n{0}\nPostconditions:\n{1}\nObjectInvariants:\n{2}\n", (object)string.Join<BoxedExpression> ("\n", (IEnumerable<BoxedExpression>)this.PreConditions), (object)string.Join<BoxedExpression> ("\n", (IEnumerable<BoxedExpression>)this.PostConditions), (object)string.Join<BoxedExpression> ("\n", (IEnumerable<BoxedExpression>)this.ObjectInvariants));
		}
	}
}

