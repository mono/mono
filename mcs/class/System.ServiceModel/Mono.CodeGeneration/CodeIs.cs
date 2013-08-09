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
using Mono.CodeGeneration;

namespace Mono.CodeGeneration
{
	public class CodeIs: CodeConditionExpression
	{
		Type type; 
		CodeExpression exp;
		
		public CodeIs (Type type, CodeExpression exp)
		{
			this.type = type;		
			this.exp = exp;
		}
		
		public override void Generate (ILGenerator gen)
		{
			Type typeObj = exp.GetResultType ();

			if (type.IsAssignableFrom (typeObj)) { 
				gen.Emit (OpCodes.Ldc_I4_1);
				return;
			}
			else if (!typeObj.IsAssignableFrom (type)) { 
				gen.Emit (OpCodes.Ldc_I4_0);
				return;
			}
			
			exp.Generate (gen);
			gen.Emit (OpCodes.Isinst, type);
			gen.Emit (OpCodes.Ldnull);
			gen.Emit (OpCodes.Cgt_Un);
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			Type typeObj = exp.GetResultType ();

			if (type.IsAssignableFrom (typeObj)) {
				if (branchCase)
					gen.Emit (OpCodes.Br, label);
				return;
			}
			else if (!typeObj.IsAssignableFrom (type)) { 
				if (!branchCase)
					gen.Emit (OpCodes.Br, label);
				return;
			}
			
			exp.Generate (gen);
			gen.Emit (OpCodes.Isinst, type);
			if (branchCase)
				gen.Emit (OpCodes.Brtrue, label);
			else
				gen.Emit (OpCodes.Brfalse, label);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp.PrintCode (cp);
			cp.Write (" is ");
			cp.Write (type.FullName);
		}
		
		public override Type GetResultType ()
		{
			return typeof(bool);
		}
	}
}
#endif
