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
using System.IO;
using System.Collections;
using System.Reflection.Emit;
using System.Reflection;

namespace Mono.CodeGeneration
{
	public class CodeBuilder
	{
		CodeBlock mainBlock;
		CodeBlock currentBlock;
		Stack blockStack = new Stack ();
		int varId;
		Label returnLabel;
		ArrayList nestedIfs = new ArrayList();
		int currentIfSerie = -1;
		CodeClass codeClass;

		public CodeBuilder (CodeClass codeClass)
		{
			this.codeClass = codeClass;
			mainBlock = new CodeBlock ();
			currentBlock = mainBlock;
		}
		
		CodeBuilder (CodeBlock block)
		{
			currentBlock = block;
		}
		
		public CodeBlock CurrentBlock
		{
			get {
				return currentBlock;
			}
		}
		
		public CodeClass OwnerClass
		{
			get { return codeClass; }
		}
		
		public void Generate (ILGenerator gen)
		{
//			try {
				mainBlock.Generate (gen);
/*
			}
			catch (Exception ex) {
				string m = ex.Message + "\nCode block:\n";
				m += "-----------------------\n";
				m += PrintCode ();
				m += "-----------------------\n";
				throw new Exception (m, ex);
			}
*/
		}
		
		public string PrintCode ()
		{
			StringWriter sw = new StringWriter ();
			CodeWriter cw = new CodeWriter (sw);
			PrintCode (cw);
			return sw.ToString ();
		}
		
		public void PrintCode (CodeWriter cp)
		{
			mainBlock.PrintCode (cp);
		}
		
		public CodeVariableReference DeclareVariable (Type type)
		{
			return DeclareVariable (type, null);
		}
		
		public CodeVariableReference DeclareVariable (Type type, object ob)
		{
			return DeclareVariable (type, Exp.Literal(ob));
		}
		
		public CodeVariableReference DeclareVariable (CodeExpression initValue)
		{
			return DeclareVariable (initValue.GetResultType(), initValue);
		}
		
		public CodeVariableReference DeclareVariable (Type type, CodeExpression initValue)
		{
			string name = "v" + (varId++);
			CodeVariableDeclaration var = new CodeVariableDeclaration (type, name);
			currentBlock.Add (var);
			if (!object.ReferenceEquals (initValue, null)) 
				Assign (var.Variable, initValue);
			return var.Variable;
		}
		
		public void Assign (CodeValueReference var, CodeExpression val)
		{
			currentBlock.Add (new CodeAssignment (var, val)); 
		}
		
		public void If (CodeExpression condition)
		{
			currentBlock.Add (new CodeIf (condition));
			PushNewBlock ();
			nestedIfs.Add (0);
		}
		
		public void ElseIf (CodeExpression condition)
		{
			if (nestedIfs.Count == 0)
				throw new InvalidOperationException ("'Else' not allowed here");

			Else ();
			currentBlock.Add (new CodeIf (condition));
			PushNewBlock ();
			nestedIfs [nestedIfs.Count-1] = 1 + (int)nestedIfs [nestedIfs.Count-1];
		}
		
		public void Else ()
		{
			CodeBlock block = PopBlock ();
			CodeIf cif = currentBlock.GetLastItem () as CodeIf;
			
			if (cif == null || cif.TrueBlock != null)
				throw new InvalidOperationException ("'Else' not allowed here");
				
			cif.TrueBlock = block;
			PushNewBlock ();
		}
		
		public void EndIf ()
		{
			CodeBlock block = PopBlock ();
			CodeIf cif = currentBlock.GetLastItem () as CodeIf;
			
			if (cif == null || cif.FalseBlock != null || nestedIfs.Count == 0)
				throw new InvalidOperationException ("'EndIf' not allowed here");
			
			if (cif.TrueBlock == null)
				cif.TrueBlock = block;
			else
				cif.FalseBlock = block;
				
			int num = (int) nestedIfs [nestedIfs.Count-1];
			if (num > 0) {
				nestedIfs [nestedIfs.Count-1] = --num;
				EndIf ();
			}
			else {
				nestedIfs.RemoveAt (nestedIfs.Count - 1);
			}
		}
		
		public void Select ()
		{
			currentBlock.Add (new CodeSelect ());
			PushNewBlock ();
		}
		
		public void Case (CodeExpression condition)
		{
			PopBlock ();
			CodeSelect select = currentBlock.GetLastItem () as CodeSelect;
			if (select == null)
				throw new InvalidOperationException ("'Case' not allowed here");

			PushNewBlock ();
			select.AddCase (condition, currentBlock);
		}
		
		public void EndSelect ()
		{
			PopBlock ();
			CodeSelect select = currentBlock.GetLastItem () as CodeSelect;
			if (select == null)
				throw new InvalidOperationException ("'EndSelect' not allowed here");
		}
		
		
		public void While (CodeExpression condition)
		{
			currentBlock.Add (new CodeWhile (condition));
			PushNewBlock ();
		}
		
		public void EndWhile ()
		{
			CodeBlock block = PopBlock ();
			CodeWhile cif = currentBlock.GetLastItem () as CodeWhile;
			
			if (cif == null || cif.WhileBlock != null)
				throw new InvalidOperationException ("'EndWhile' not allowed here");
			
			cif.WhileBlock = block;
		}
		
		public void Foreach (Type type, out CodeExpression item, CodeExpression array)
		{
			CodeForeach cfe = new CodeForeach (array, type);
			item = cfe.ItemExpression;
			currentBlock.Add (cfe);
			PushNewBlock ();
		}
		
		public void EndForeach ()
		{
			CodeBlock block = PopBlock ();
			CodeForeach cif = currentBlock.GetLastItem () as CodeForeach;
			
			if (cif == null || cif.ForBlock != null)
				throw new InvalidOperationException ("'EndForeach' not allowed here");
			
			cif.ForBlock = block;
		}
		
		public void For (CodeExpression initExp, CodeExpression conditionExp, CodeExpression nextExp)
		{
			currentBlock.Add (new CodeFor (initExp, conditionExp, nextExp));
			PushNewBlock ();
		}
		
		public void EndFor ()
		{
			CodeBlock block = PopBlock ();
			CodeFor cif = currentBlock.GetLastItem () as CodeFor;
			
			if (cif == null || cif.ForBlock != null)
				throw new InvalidOperationException ("'EndFor' not allowed here");
			
			cif.ForBlock = block;
		}
		
		
		public void Call (CodeExpression target, string name, params CodeExpression[] parameters)
		{
			if ((object) target == null)
				throw new ArgumentNullException ("target");
			if (name == null)
				throw new ArgumentNullException ("name");
			currentBlock.Add (new CodeMethodCall (target, name, parameters));
		}
		
		public void Call (CodeExpression target, MethodBase method, params CodeExpression[] parameters)
		{
			if ((object) target == null)
				throw new ArgumentNullException ("target");
			if (method == null)
				throw new ArgumentNullException ("method");
			currentBlock.Add (new CodeMethodCall (target, method, parameters));
		}
		
		public void Call (CodeExpression target, CodeMethod method, params CodeExpression[] parameters)
		{
			if ((object) target == null)
				throw new ArgumentNullException ("target");
			if (method == null)
				throw new ArgumentNullException ("method");
			currentBlock.Add (new CodeMethodCall (target, method, parameters));
		}
		
		public void Call (Type type, string name, params CodeExpression[] parameters)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (name == null)
				throw new ArgumentNullException ("name");
			currentBlock.Add (new CodeMethodCall (type, name, parameters));
		}
		
		public void Call (MethodInfo method, params CodeExpression[] parameters)
		{
			if (method == null)
				throw new ArgumentNullException ("method");
			currentBlock.Add (new CodeMethodCall (method, parameters));
		}
		
		public void Call (CodeMethod method, params CodeExpression[] parameters)
		{
			if ((object) method == null)
				throw new ArgumentNullException ("method");
			currentBlock.Add (new CodeMethodCall (method, parameters));
		}
		
		public CodeExpression CallFunc (CodeExpression target, string name, params CodeExpression[] parameters)
		{
			if ((object) target == null)
				throw new ArgumentNullException ("target");
			if (name == null)
				throw new ArgumentNullException ("name");
			return new CodeMethodCall (target, name, parameters);
		}
		
		public CodeExpression CallFunc (CodeExpression target, MethodInfo method, params CodeExpression[] parameters)
		{
			if ((object) target == null)
				throw new ArgumentNullException ("target");
			if (method == null)
				throw new ArgumentNullException ("method");
			return new CodeMethodCall (target, method, parameters);
		}
		
		public CodeExpression CallFunc (CodeExpression target, CodeMethod method, params CodeExpression[] parameters)
		{
			if ((object) target == null)
				throw new ArgumentNullException ("target");
			if (method == null)
				throw new ArgumentNullException ("method");
			return new CodeMethodCall (target, method, parameters);
		}
		
		public CodeExpression CallFunc (Type type, string name, params CodeExpression[] parameters)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (name == null)
				throw new ArgumentNullException ("name");
			return new CodeMethodCall (type, name, parameters);
		}
		
		public CodeExpression CallFunc (MethodInfo method, params CodeExpression[] parameters)
		{
			if (method == null)
				throw new ArgumentNullException ("method");
			return new CodeMethodCall (method, parameters);
		}
		
		public CodeExpression CallFunc (CodeMethod method, params CodeExpression[] parameters)
		{
			if ((object) method == null)
				throw new ArgumentNullException ("method");
			return new CodeMethodCall (method, parameters);
		}
		
		public void Inc (CodeValueReference val)
		{
			Assign (val, new CodeIncrement (val));
		}
		
		public void Dec (CodeValueReference val)
		{
			Assign (val, new CodeDecrement (val));
		}
		
		public CodeExpression When (CodeExpression condition, CodeExpression trueResult, CodeExpression falseResult)
		{
			return new CodeWhen (condition, trueResult, falseResult);
		}
		
		public void ConsoleWriteLine (params CodeExpression[] parameters)
		{
			Call (typeof(Console), "WriteLine", parameters);
		}
		
		public void ConsoleWriteLine (params object[] parameters)
		{
			CodeExpression[] exps = new CodeExpression [parameters.Length];
			for (int n=0; n<exps.Length; n++)
				exps[n] = Exp.Literal (parameters[n]);
				
			ConsoleWriteLine (exps);
		}
		
		public void Return (CodeExpression exp)
		{
			currentBlock.Add (new CodeReturn (this, exp));
		}
		
		public void Return ()
		{
			currentBlock.Add (new CodeReturn (this));
		}
		
		public static CodeBuilder operator+(CodeBuilder cb, CodeItem e)
		{
			cb.currentBlock.Add (e);
			return cb;
		}
		
		internal Label ReturnLabel
		{
			get { return returnLabel; }
			set { returnLabel = value; }
		}
		
		void PushNewBlock ()
		{
			blockStack.Push (currentBlock);
			currentBlock = new CodeBlock ();
		}
		
		CodeBlock PopBlock ()
		{
			CodeBlock block = currentBlock;
			currentBlock = (CodeBlock) blockStack.Pop ();
			return block;
		}
	}
}
#endif
