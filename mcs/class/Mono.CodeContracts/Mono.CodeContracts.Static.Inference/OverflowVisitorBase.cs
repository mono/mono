using System;
using Mono.CodeContracts.Inference.Interface;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Inference
{
	public abstract class OverflowVisitorBase : IBoxedExpressionController
	{
		protected readonly APC pc;
      	protected readonly IFactQuery<BoxedExpression, Variable> facts;
		protected bool Overflow { get; set; }
		
		
		public OverflowVisitorBase(APC pc, IFactQuery<BoxedExpression, Variable> facts)
      	{
	        this.Overflow = true;
	        this.pc = pc;
	        this.Facts = facts;
      	}
		
		public abstract void Binary(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression parent);

      	public abstract void Unary(UnaryOperator unaryOp, BoxedExpression argument, BoxedExpression parent);

      	public void Variable(object var, PathElement[] path, BoxedExpression parent)
      	{
        	this.Overflow = false;
      	}
		
		public void Constant<Type>(Type type, object value, BoxedExpression parent)
        {
        	this.Overflow = false;
        }
		
		public void SizeOf<Type>(Type type, int sizeAsConstant, BoxedExpression parent)
      	{
        	this.Overflow = false;
      	}

      	public void IsInst<Type>(Type type, BoxedExpression argument, BoxedExpression parent)
      	{
        	argument.Dispatch((IBoxedExpressionController) this);
      	}

      	public void ArrayIndex<Type>(Type type, BoxedExpression array, BoxedExpression index, BoxedExpression parent)
      	{
        	index.Dispatch((IBoxedExpressionController) this);
      	}
		
		public void Result<Type>(Type type, BoxedExpression parent)
       	{
       	 	this.Overflow = false;
      	}

      	public void Old<Type>(Type type, BoxedExpression expression, BoxedExpression parent)
      	{
        	expression.Dispatch((IBoxedExpressionController) this);
      	}

      	public void ValueAtReturn<Type>(Type type, BoxedExpression expression, BoxedExpression parent)
      	{
        	expression.Dispatch((IBoxedExpressionVisitor) this);
      	}

      	public void Assert(BoxedExpression condition, BoxedExpression parent)
      	{
        	condition.Dispatch((IBoxedExpressionController) this);
      	}

      	public void Assume(BoxedExpression condition, BoxedExpression parent)
      	{
        	condition.Dispatch((IBoxedExpressionController) this);
      	}

      	public void StatementSequence(IIndexable<BoxedExpression> statements, BoxedExpression parent)
      	{
        	for (int index = 0; index < statements.Count; ++index)
        	{
          		statements[index].Dispatch((IBoxedExpressionController) this);
          		if (this.Overflow)
            	break;
        	}
      	}

      	public void ForAll(BoxedExpression boundVariable, BoxedExpression lower, BoxedExpression upper, BoxedExpression body, BoxedExpression parent)
      	{
        	boundVariable.Dispatch((IBoxedExpressionController) this);
        	if (this.Overflow)
          	return;
        	lower.Dispatch((IBoxedExpressionVisitor) this);
	        if (this.Overflow)
	          return;
	        upper.Dispatch((IBoxedExpressionController) this);
	        if (this.Overflow)
	          return;
	        body.Dispatch((IBoxedExpressionController) this);
	        int num = this.Overflow ? 1 : 0;
      	}
	}
}

