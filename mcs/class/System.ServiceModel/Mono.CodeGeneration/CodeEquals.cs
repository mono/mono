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

#if !FULL_AOT_RUNTIME
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeEquals: CodeConditionExpression
	{
		CodeExpression exp1;
		CodeExpression exp2;
		Type t1;
		Type t2;
		
		public CodeEquals (CodeExpression exp1, CodeExpression exp2)
		{
			this.exp1 = exp1;
			this.exp2 = exp2;
			
			t1 = exp1.GetResultType ();
			t2 = exp2.GetResultType ();
			
			if (t1.IsValueType && t2.IsValueType) {
				if (t1 != t2)
					throw new InvalidOperationException ("Can't compare values of different primitive types");
			}
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (t1.IsPrimitive)
			{
				exp1.Generate (gen);
				exp2.Generate (gen);
				gen.Emit (OpCodes.Ceq);
			}
			else
			{
				exp1.Generate (gen);
				exp2.Generate (gen);
//				gen.Emit (OpCodes.Ceq);
				gen.EmitCall (OpCodes.Callvirt, t1.GetMethod ("Equals", new Type[] {t2}), null);
			}
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			if (t1.IsPrimitive)
			{
				exp1.Generate (gen);
				exp2.Generate (gen);
				if (branchCase)
					gen.Emit (OpCodes.Beq, label);
				else
					gen.Emit (OpCodes.Bne_Un, label);
			}
			else {
				Generate (gen);
				if (branchCase)
					gen.Emit (OpCodes.Brtrue, label);
				else
					gen.Emit (OpCodes.Brfalse, label);
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp1.PrintCode (cp);
			cp.Write (" == ");
			exp2.PrintCode (cp);
		}
		
		public override Type GetResultType ()
		{
			return typeof (bool);
		}
	}
}
#endif
