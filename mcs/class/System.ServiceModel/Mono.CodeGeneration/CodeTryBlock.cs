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
// Copyright (C) 2009 Novell, Inc
//

#if !FULL_AOT_RUNTIME
using System;
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#else
using System.Collections.Specialized;
#endif
using System.Reflection;
using System.Reflection.Emit;

#if NET_2_0
using DictionaryEntry = System.Collections.Generic.KeyValuePair<
		Mono.CodeGeneration.CodeVariableDeclaration,
		Mono.CodeGeneration.CodeBlock>;
using ArrayList = System.Collections.Generic.List<
	System.Collections.Generic.KeyValuePair<
		Mono.CodeGeneration.CodeVariableDeclaration,
		Mono.CodeGeneration.CodeBlock>>;
#endif

namespace Mono.CodeGeneration
{
	public class CodeTry: CodeStatement
	{
		CodeExpression condition;
		CodeBlock tryBlock, finallyBlock;
		ArrayList catchBlocks = new ArrayList ();
		
		public CodeTry ()
		{
			tryBlock = new CodeBlock ();
			catchBlocks = new ArrayList ();
			finallyBlock = new CodeBlock ();
		}
		
		public override void Generate (ILGenerator gen)
		{
			gen.BeginExceptionBlock ();
			tryBlock.Generate (gen);
			foreach (DictionaryEntry de in catchBlocks) {
				CodeVariableDeclaration vd = (CodeVariableDeclaration) de.Key;
				gen.BeginCatchBlock (vd.Variable.Type);
				if (vd.Variable.Name.Length > 0) {
					vd.Generate (gen);
					// FIXME: assign exception to this local declaration
				}
				((CodeBlock) de.Value).Generate (gen);
			}
			if (!finallyBlock.IsEmpty) {
				gen.BeginFinallyBlock ();
				finallyBlock.Generate (gen);
			}
			gen.EndExceptionBlock ();
		}
				
		public override void PrintCode (CodeWriter cp)
		{
			if (tryBlock == null) return;
			
			cp.Write ("try {");
			cp.Indent ();
			condition.PrintCode (cp);
			cp.Unindent ();
			foreach (DictionaryEntry de in catchBlocks) {
				CodeVariableDeclaration vd = (CodeVariableDeclaration) de.Key;
				cp.Write ("} catch (");
				if (vd.Variable.Name.Length > 0)
					vd.PrintCode (cp);
				else
					cp.Write (vd.Variable.Type.FullName);
				cp.Write (") {");
				cp.Indent ();
				((CodeBlock) de.Value).PrintCode (cp);
				cp.Unindent ();
			}
			if (!finallyBlock.IsEmpty) {
				cp.Write ("} finally {");
				cp.Indent ();
				finallyBlock.PrintCode (cp);
				cp.Unindent ();
			}
			cp.Write ("}");
		}
		
		public CodeBlock TryBlock
		{
			get { return tryBlock; }
			set { tryBlock = value; }
		}

		public ArrayList CatchBlocks
		{
			get { return catchBlocks; }
		}
		
		public CodeBlock FinallyBlock
		{
			get { return finallyBlock; }
			set { finallyBlock = value; }
		}
	}
}
#endif
