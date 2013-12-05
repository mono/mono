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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeForeach: CodeStatement
	{
		Type itemType;
		CodeExpression array;
		CodeBlock forBlock;
		
		CodeVariableDeclaration itemDec;
		
		public CodeForeach (CodeExpression array, Type itemType)
		{
			this.array = array;
			this.itemType = itemType;
			
			Type t = array.GetResultType ();
			if (!t.IsArray && t.GetMethod ("GetEnumerator", Type.EmptyTypes) == null)
				throw new InvalidOperationException ("foreach statement cannot operate on variables of type `" + t + "' because that class does not provide a GetEnumerator method or it is inaccessible");
			
			itemDec = new CodeVariableDeclaration (itemType, "item");
		}
		
		public CodeValueReference ItemExpression
		{
			get { return itemDec.Variable; }
		}
		
		public CodeBlock ForBlock
		{
			get { return forBlock; }
			set { forBlock = value; }
		}
		
		public override void Generate (ILGenerator gen)
		{
			Type t = array.GetResultType ();
			if (t.IsArray)
			{
				CodeBlock block = new CodeBlock();
				CodeVariableDeclaration indexDec;
				CodeWhile cw;
				CodeValueReference index;
				CodeValueReference item;
				
				block.Add (itemDec);
				item = itemDec.Variable;
				block.Add (indexDec = new CodeVariableDeclaration (typeof(int), "n"));
				index = indexDec.Variable;
				block.Add (new CodeAssignment (index, new CodeLiteral (0)));
				
				block.Add (cw = new CodeWhile (CodeExpression.IsSmallerThan (index, array.ArrayLength)));
				CodeBlock loopBlock = new CodeBlock ();
				loopBlock.Add (new CodeAssignment (item, array[index]));
				loopBlock.Add (new CodeIncrement(index));
				loopBlock.Add (forBlock);
				cw.WhileBlock = loopBlock;
				
				block.Generate (gen);
			}
			else
			{
				CodeBlock block = new CodeBlock();
				CodeVariableDeclaration dec;
				CodeWhile cw;
				CodeValueReference enumerator;
				CodeValueReference item;
				
				block.Add (itemDec);
				item = itemDec.Variable;
				block.Add (dec = new CodeVariableDeclaration (typeof(IEnumerator), "e"));
				enumerator = dec.Variable;
				block.Add (new CodeAssignment (enumerator, array.Call("GetEnumerator")));
				
				block.Add (cw = new CodeWhile (enumerator.Call ("MoveNext")));
				CodeBlock loopBlock = new CodeBlock ();
				loopBlock.Add (new CodeAssignment (item, enumerator["Current"]));
				loopBlock.Add (forBlock);
				cw.WhileBlock = loopBlock;
				
				block.Generate (gen);
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write ("foreach (" + itemType + " item in ");
			array.PrintCode (cp);
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
