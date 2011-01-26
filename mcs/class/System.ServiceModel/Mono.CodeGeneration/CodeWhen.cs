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
	public class CodeWhen: CodeExpression
	{
		CodeExpression condition;
		CodeExpression trueBlock;
		CodeExpression falseBlock;
		
		public CodeWhen (CodeExpression condition, CodeExpression trueResult, CodeExpression falseResult)
		{
			this.condition = condition;
			if (condition.GetResultType () != typeof(bool))
				throw new InvalidOperationException ("Condition expression is not boolean");
			if (trueResult.GetResultType() != falseResult.GetResultType())
				throw new InvalidOperationException ("The types of the true and false expressions must be the same");
			trueBlock = trueResult;
			falseBlock = falseResult;
		}
		
		public override void Generate (ILGenerator gen)
		{
			Label falseLabel = gen.DefineLabel ();
			Label endLabel = gen.DefineLabel ();
			
			GenerateCondition (gen, falseLabel);
			trueBlock.Generate (gen);
			gen.Emit (OpCodes.Br, endLabel);
			gen.MarkLabel(falseLabel);
			falseBlock.Generate (gen);
			
			gen.MarkLabel(endLabel);
		}
		
		void GenerateCondition (ILGenerator gen, Label falseLabel)
		{
			if (condition is CodeConditionExpression)
				((CodeConditionExpression)condition).GenerateForBranch (gen, falseLabel, false);
			else {
				condition.Generate (gen);
				gen.Emit (OpCodes.Brfalse, falseLabel);
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write ("(");
			condition.PrintCode (cp);
			cp.Write (") ? ");
			trueBlock.PrintCode (cp);
			cp.Write (" : ");
			falseBlock.PrintCode (cp);
		}
		
		public override Type GetResultType ()
		{
			return trueBlock.GetResultType();
		}
	}
}
#endif
