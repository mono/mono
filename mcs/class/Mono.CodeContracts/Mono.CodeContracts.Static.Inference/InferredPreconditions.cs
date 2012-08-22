using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions;

namespace Mono.CodeContracts.Static.Inference
{
	public class InferredPreconditions : List<IInferredPrecond>
	{
		public InferredPreconditions(IEnumerable<IInferredPrecond> collection) : 
			base(collection == null ? (IEnumerable<IInferredPrecond>) null : Enumerable.Where<IInferredPrecond>(collection, (Func<IInferredPrecond, bool>) (pre => pre != null)))
    	{
    	}

    	public InferredPreconditions(IEnumerable<BoxedExpression> collection, ExpressionInPreStateKind kind, bool isSufficient) : 
			this(collection == null ? (IEnumerable<IInferredPrecond>) null : (IEnumerable<IInferredPrecond>) Enumerable.Select<BoxedExpression, SimpleInferredPrecondition>(Enumerable.Where<BoxedExpression>(collection, (Func<BoxedExpression, bool>) (be => be != null)), (Func<BoxedExpression, SimpleInferredPrecondition>) (be => new SimpleInferredPrecondition(be, kind, isSufficient))))
	    {
	    }

		public bool IsSufficient(BoxedExpression exp)
	    {
	      if (exp != null)
	        return Enumerable.Any<IInferredPrecond>(Enumerable.Where<IInferredPrecond>((IEnumerable<IInferredPrecond>) this, (Func<IInferredPrecond, bool>) (pre => pre.expr.Equals((object) exp))), (Func<IInferredPrecond, bool>) (pre => pre.IsSufficientForTheWarning));
	      else
	        return false;
	    }

		public void Split(out IEnumerable<BoxedExpression> suggestedPreconditions, out IEnumerable<BoxedExpression> objectInvariants, out IEnumerable<BoxedExpression> assumes)
		{
			suggestedPreconditions = Enumerable.Select<IInferredPrecond, BoxedExpression>(Enumerable.Where<IInferredPrecond>((IEnumerable<IInferredPrecond>) this, (Func<IInferredPrecond, bool>) (pre =>
	      	{
	        	if (pre.kind != ExpressionInPreStateKind.Any)
	          	return pre.kind == ExpressionInPreStateKind.MethodPrecondition;
	        	else
	          	return true;
	      	})), (Func<IInferredPrecond, BoxedExpression>) (pre => pre.expr));

			objectInvariants = Enumerable.Select<IInferredPrecond, BoxedExpression>(Enumerable.Where<IInferredPrecond>((IEnumerable<IInferredPrecond>) this, (Func<IInferredPrecond, bool>) (pre => pre.kind == ExpressionInPreStateKind.ObjectInvariant)), (Func<IInferredPrecond, BoxedExpression>) (pre => pre.expr));
      		assumes = Enumerable.Select<IInferredPrecond, BoxedExpression>(Enumerable.Where<IInferredPrecond>((IEnumerable<IInferredPrecond>) this, (Func<IInferredPrecond, bool>) (pre => pre.kind == ExpressionInPreStateKind.Assume)), (Func<IInferredPrecond, BoxedExpression>) (pre => pre.expr));
    
		}

		private void ObjectInvariant()
	    {
	    }
	}
}

