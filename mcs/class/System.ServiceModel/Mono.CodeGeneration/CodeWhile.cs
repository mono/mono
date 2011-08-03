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
	public class CodeWhile: CodeStatement
	{
		CodeExpression condition;
		CodeBlock whileBlock;
		
		public CodeWhile (CodeExpression condition)
		{
			this.condition = condition;
			if (condition.GetResultType () != typeof(bool))
				throw new InvalidOperationException ("Condition expression is not boolean"); 
		}
		
		public override void Generate (ILGenerator gen)
		{
			Label startLabel = gen.DefineLabel ();
			Label checkLabel = gen.DefineLabel ();
			
			gen.Emit (OpCodes.Br, checkLabel);
			gen.MarkLabel(startLabel);
			whileBlock.Generate (gen);
			gen.MarkLabel(checkLabel);
			
			if (condition is CodeConditionExpression)
				((CodeConditionExpression)condition).GenerateForBranch (gen, startLabel, true);
			else {
				condition.Generate (gen);
				gen.Emit (OpCodes.Brtrue, startLabel);
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write ("while (");
			condition.PrintCode (cp);
			cp.Write (") {");
			cp.EndLine ();
			cp.Indent ();
			whileBlock.PrintCode (cp);
			cp.Unindent ();
			cp.BeginLine ().Write ("}");
		}
		
		public CodeBlock WhileBlock
		{
			get { return whileBlock; }
			set { whileBlock = value; }
		}
	}
}
#endif
