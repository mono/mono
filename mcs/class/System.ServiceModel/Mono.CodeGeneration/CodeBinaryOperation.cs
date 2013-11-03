// created on 28/08/2004 at 17:30

#if !FULL_AOT_RUNTIME
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public abstract class CodeBinaryOperation: CodeConditionExpression
	{
		protected CodeExpression exp1;
		protected CodeExpression exp2;
		protected Type t1;
		protected Type t2;
		string symbol;
		
		public CodeBinaryOperation (CodeExpression exp1, CodeExpression exp2, string symbol)
		{
			this.symbol = symbol;
			this.exp1 = exp1;
			this.exp2 = exp2;
			
			t1 = exp1.GetResultType ();
			t2 = exp2.GetResultType ();
			
			if (!t1.IsPrimitive || !t2.IsPrimitive || (t1 != t2)) {
				throw new InvalidOperationException ("Operator " + GetType().Name + " cannot be applied to operands of type '" + t1.Name + " and " + t2.Name);
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp1.PrintCode (cp);
			cp.Write (" " + symbol + " ");
			exp2.PrintCode (cp);
		}
	}
}
#endif
