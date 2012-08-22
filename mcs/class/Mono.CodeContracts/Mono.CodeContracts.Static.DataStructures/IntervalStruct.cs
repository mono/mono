using System;
using Mono.CodeContracts.Static.AST;

namespace Mono.CodeContracts.Static.DataStructures
{
	internal struct IntervalStruct
  	{
	    public readonly bool IsValid;
	    public readonly BoxedExpression MinValue;
	    public readonly BoxedExpression MaxValue;

	    public static IntervalStruct None
	    {
	      get
	      {
	        return new IntervalStruct();
	      }
	    }

	    public IntervalStruct(Decimal MinValue, Decimal MaxValue, Func<Decimal, BoxedExpression> ToBoxedExpression)
	    {
	      this.IsValid = true;
	      this.MinValue = ToBoxedExpression(MinValue);
	      this.MaxValue = ToBoxedExpression(MaxValue);
	    }

	    public override string ToString()
	    {
	      if (!this.IsValid)
	        return "None";
	      else
	        return string.Format("({0}, {1})", (object) this.MinValue, (object) this.MaxValue);
	    }
  	}
}

