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
	public class CodeFor: CodeStatement
	{
		CodeExpression initExp;
		CodeExpression conditionExp;
		CodeExpression nextExp;
		CodeBlock forBlock;
		
		public CodeFor (CodeExpression initExp, CodeExpression conditionExp, CodeExpression nextExp)
		{
			this.initExp = initExp;	
			this.conditionExp = conditionExp;
			this.nextExp = nextExp;
			
			if (conditionExp.GetResultType () != typeof(bool))
				throw new InvalidOperationException ("Condition expression is not boolean"); 
		}
		
		public CodeBlock ForBlock
		{
			get { return forBlock; }
			set { forBlock = value; }
		}
		
		public override void Generate (ILGenerator gen)
		{
			CodeBlock block = new CodeBlock();
			CodeWhile cw;
			
			block.Add (initExp);
			block.Add (cw = new CodeWhile (conditionExp));
			CodeBlock loopBlock = new CodeBlock ();
			loopBlock.Add (forBlock);
			loopBlock.Add (nextExp);
			cw.WhileBlock = loopBlock;
			
			block.Generate (gen);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write ("for (");
			initExp.PrintCode (cp);
			cp.Write (";");
			conditionExp.PrintCode (cp);
			cp.Write (";");
			nextExp.PrintCode (cp);
			cp.Write (") {");
			cp.EndLine ();
			cp.Indent ();
			forBlock.PrintCode (cp);
			cp.Unindent ();
			cp.BeginLine ().Write ("}");
		}
	}
}
#endif
