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
// Copyright (C) Lluis Sanchez Gual, 2004
//

#if !MONOTOUCH
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public abstract class CodeArithmeticOperation: CodeExpression
	{
		protected CodeExpression exp1;
		protected CodeExpression exp2;
		protected Type t1;
		protected Type t2;
		protected string symbol;
		
		protected CodeArithmeticOperation ()
		{
		}
		
		public CodeArithmeticOperation (CodeExpression exp1, CodeExpression exp2, string symbol)
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
		
		public override Type GetResultType ()
		{
			return exp1.GetResultType();
		}
	}
	
	public class CodeAdd: CodeArithmeticOperation
	{
		public CodeAdd (CodeExpression exp1, CodeExpression exp2)
		{
			this.symbol = "+";
			this.exp1 = exp1;
			this.exp2 = exp2;
			
			t1 = exp1.GetResultType ();
			t2 = exp2.GetResultType ();
			
			if ((!t1.IsPrimitive || !t2.IsPrimitive || (t1 != t2)) && (t1 != typeof(string) || t2 != typeof(string))) {
				throw new InvalidOperationException ("Operator " + GetType().Name + " cannot be applied to operands of type '" + t1.Name + " and " + t2.Name);
			}
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (exp1.GetResultType () == typeof(string)) {
				MethodInfo m = typeof(string).GetMethod ("Concat", new Type[] { typeof(string), typeof(string) });
				CodeGenerationHelper.GenerateMethodCall (gen, null, m, exp1, exp2);
			}
			else {
				exp1.Generate (gen);
				exp2.Generate (gen);
				gen.Emit (OpCodes.Add);
			}
		}
	}
	
	public class CodeMul: CodeArithmeticOperation
	{
		public CodeMul (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, "*")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Mul);
		}
	}
	
	public class CodeDiv: CodeArithmeticOperation
	{
		public CodeDiv (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, "*")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Div);
		}
	}
	
	public class CodeSub: CodeArithmeticOperation
	{
		public CodeSub (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, "-")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Sub);
		}
	}	
}
#endif
