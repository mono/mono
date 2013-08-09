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
	public class CodeAssignment: CodeExpression
	{
		new CodeValueReference var;
		CodeExpression exp;
		
		public CodeAssignment (CodeValueReference var, CodeExpression exp)
		{
			if (var == null)
				throw new ArgumentNullException ("var");
			if (exp == null)
				throw new ArgumentNullException ("exp");
			this.exp = exp;
			this.var = var;
		}
		
		public override void Generate (ILGenerator gen)
		{
			var.GenerateSet (gen, exp);
			exp.Generate (gen);
		}
		
		public override void GenerateAsStatement (ILGenerator gen)
		{
			CodeExpression val = exp;
			if (var.GetResultType () == typeof(object) && exp.GetResultType ().IsValueType)
				var.GenerateSet (gen, new CodeCast (typeof(object), exp));
			else
				var.GenerateSet (gen, exp);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			var.PrintCode (cp);
			cp.Write (" = ");
			exp.PrintCode (cp);
		}
		
		public override Type GetResultType ()
		{
			return var.GetResultType ();
		}
	}
}
#endif
