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
	public abstract class CodeBinaryComparison: CodeConditionExpression
	{
		protected CodeExpression exp1;
		protected CodeExpression exp2;
		protected Type t1;
		protected Type t2;
		string symbol;
		
		public CodeBinaryComparison (CodeExpression exp1, CodeExpression exp2, string symbol)
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
			return typeof(bool);
		}
	}
	
	public class CodeGreaterThan: CodeBinaryComparison
	{
		public CodeGreaterThan (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, ">")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Cgt);
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			if (branchCase)
				gen.Emit (OpCodes.Bgt, label);
			else
				gen.Emit (OpCodes.Ble, label);
		}
	}
	
	public class CodeGreaterEqualThan: CodeBinaryComparison
	{
		public CodeGreaterEqualThan (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, ">=")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Clt);
			gen.Emit (OpCodes.Ldc_I4_0);
			gen.Emit (OpCodes.Ceq);
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			if (branchCase)
				gen.Emit (OpCodes.Bge, label);
			else
				gen.Emit (OpCodes.Blt, label);
		}
	}
	
	public class CodeLessThan: CodeBinaryComparison
	{
		public CodeLessThan (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, "<")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Clt);
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			if (branchCase)
				gen.Emit (OpCodes.Blt, label);
			else
				gen.Emit (OpCodes.Bge, label);
		}
	}
	
	public class CodeLessEqualThan: CodeBinaryComparison
	{
		public CodeLessEqualThan (CodeExpression exp1, CodeExpression exp2)
		: base (exp1, exp2, "<=")
		{
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			gen.Emit (OpCodes.Cgt);
			gen.Emit (OpCodes.Ldc_I4_0);
			gen.Emit (OpCodes.Ceq);
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			exp1.Generate (gen);
			exp2.Generate (gen);
			if (branchCase)
				gen.Emit (OpCodes.Ble, label);
			else
				gen.Emit (OpCodes.Bgt, label);
		}
	}
}
#endif
