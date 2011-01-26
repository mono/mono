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
	public class CodeMethodCall: CodeExpression
	{
		CodeExpression target;
		CodeExpression[] parameters;
		MethodBase method;
		CodeMethod codeMethod;
		
		public CodeMethodCall (CodeExpression target, string name, params CodeExpression[] parameters)
		{
			this.target = target;
			this.parameters = parameters;
			Type[] types = GetParameterTypes (parameters);
			method = target.GetResultType().GetMethod (name, types);
			if (method == null) {
				throw new InvalidOperationException ("Method " + GetSignature(target.GetResultType(), name, parameters) + " not found");
			}
		}
		
		public CodeMethodCall (CodeExpression target, MethodBase method, params CodeExpression[] parameters)
		{
			this.target = target;
			this.parameters = parameters;
			this.method = method;
		}
		
		public CodeMethodCall (CodeExpression target, CodeMethod method, params CodeExpression[] parameters)
		{
			this.target = target;
			this.parameters = parameters;
			this.codeMethod = method;
		}
		
		public CodeMethodCall (Type type, string name, params CodeExpression[] parameters)
		{
			this.parameters = parameters;
			method = type.GetMethod (name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, GetParameterTypes (parameters), null);
			if (method == null) throw new InvalidOperationException ("Method " + GetSignature(type, name, parameters) + " not found");
		}
		
		public CodeMethodCall (MethodInfo method, params CodeExpression[] parameters)
		{
			this.parameters = parameters;
			this.method = method;
		}
		
		public CodeMethodCall (CodeMethod method, params CodeExpression[] parameters)
		{
			this.parameters = parameters;
			this.codeMethod = method;
		}
		
		Type[] GetParameterTypes (CodeExpression[] parameters)
		{
			Type[] ts = new Type [parameters.Length];
			for (int n=0; n<ts.Length; n++)
				ts [n] = parameters[n].GetResultType ();
			return ts;
		}
		
		string GetSignature (Type type, string name, params CodeExpression[] parameters)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			sb.Append (type.FullName).Append(".").Append(name);
			
			Type[] types = GetParameterTypes (parameters);
			sb.Append ("(");
			for (int n=0; n<types.Length; n++)
			{
				if (n > 0) sb.Append (", ");
				sb.Append (types[n].FullName);
			}
			sb.Append (")");
			return sb.ToString ();
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (codeMethod != null)
				CodeGenerationHelper.GenerateMethodCall (gen, target, codeMethod, parameters);
			else
				CodeGenerationHelper.GenerateMethodCall (gen, target, method, parameters);
		}
		
		public override void GenerateAsStatement (ILGenerator gen)
		{
			Generate (gen);
			if (GetResultType () != typeof(void)) gen.Emit (OpCodes.Pop);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			MethodBase met = method != null ? method : codeMethod.MethodInfo;
			if (!object.ReferenceEquals (target, null))
				target.PrintCode (cp);
			else
				cp.Write (met.DeclaringType.FullName);
			
			cp.Write (".");
			cp.Write (met.Name).Write (" (");
			for (int n=0; n<parameters.Length; n++) {
				if (n > 0) cp.Write (", ");
				parameters[n].PrintCode (cp);
			}
			cp.Write (")");
		}
		
		public override Type GetResultType ()
		{
			if (codeMethod != null) return codeMethod.ReturnType;
			else if (method is MethodInfo) return ((MethodInfo) method).ReturnType;
			else return typeof (void);
		}
	}
}

#endif
