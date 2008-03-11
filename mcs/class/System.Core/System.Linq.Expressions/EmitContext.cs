//
// EmitContext.cs
//
// Author:
//   Miguel de Icaza (miguel@novell.com)
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	abstract class EmitContext {

		protected LambdaExpression owner;
		protected Type [] param_types;
		protected Type return_type;

		public ILGenerator ig;

		protected EmitContext (LambdaExpression lambda)
		{
			this.owner = lambda;

			param_types = owner.Parameters.Select (p => p.Type).ToArray ();
			return_type = owner.GetReturnType ();
		}

		public static EmitContext Create (LambdaExpression lambda)
		{
			if (Environment.GetEnvironmentVariable ("LINQ_DBG") != null)
				return new DebugEmitContext (lambda);

			return new DynamicEmitContext (lambda);
		}

		public int GetParameterPosition (ParameterExpression p)
		{
			int position = owner.Parameters.IndexOf (p);
			if (position == -1)
				throw new InvalidOperationException ("Parameter not in scope");

			return position;
		}

		public abstract Delegate CreateDelegate ();

		public void Emit (Expression expression)
		{
			expression.Emit (this);
		}

		public LocalBuilder EmitStored (Expression expression)
		{
			var local = ig.DeclareLocal (expression.Type);
			expression.Emit (this);
			ig.Emit (OpCodes.Stloc, local);

			return local;
		}

		public void EmitLoad (Expression expression)
		{
			if (expression.Type.IsValueType) {
				var local = EmitStored (expression);
				ig.Emit (OpCodes.Ldloca, local);
			} else
				expression.Emit (this);
		}

		public void EmitLoad (LocalBuilder local)
		{
			ig.Emit (OpCodes.Ldloc, local);
		}

		public void EmitCall (LocalBuilder local, IEnumerable<Expression> arguments, MethodInfo method)
		{
			EmitLoad (local);
			EmitCollection (arguments);
			EmitCall (method);
		}

		public void EmitCall (Expression expression, IEnumerable<Expression> arguments, MethodInfo method)
		{
			if (expression != null)
				EmitLoad (expression);

			EmitCollection (arguments);
			EmitCall (method);
		}

		public void EmitCall (MethodInfo method)
		{
			ig.Emit (
				method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
				method);
		}

		public void EmitCollection<T> (IEnumerable<T> collection) where T : Expression
		{
			foreach (var expression in collection)
				expression.Emit (this);
		}

		public void EmitCollection (IEnumerable<ElementInit> initializers, LocalBuilder local)
		{
			foreach (var initializer in initializers)
				initializer.Emit (this, local);
		}

		public void EmitCollection (IEnumerable<MemberBinding> bindings, LocalBuilder local)
		{
			foreach (var binding in bindings)
				binding.Emit (this, local);
		}

		public void EmitIsInst (Expression expression, Type candidate)
		{
			expression.Emit (this);

			if (expression.Type.IsValueType)
				ig.Emit (OpCodes.Box, expression.Type);

			ig.Emit (OpCodes.Isinst, candidate);
		}
	}

	class DynamicEmitContext : EmitContext {

		DynamicMethod method;

		static object mlock = new object ();
		static int method_count;

		public DynamicMethod Method {
			get { return method; }
		}

		public DynamicEmitContext (LambdaExpression lambda)
			: base (lambda)
		{
			// FIXME: Need to force this to be verifiable, see:
			// https://bugzilla.novell.com/show_bug.cgi?id=355005
			method = new DynamicMethod (GenerateName (), return_type, param_types, typeof (EmitContext), true);
			ig = method.GetILGenerator ();

			owner.Emit (this);
		}

		public override Delegate CreateDelegate ()
		{
			return method.CreateDelegate (owner.Type);
		}

		static string GenerateName ()
		{
			lock (mlock) {
				return "lambda_method-" + (method_count++);
			}
		}
	}

	class DebugEmitContext : EmitContext {

		DynamicEmitContext dynamic_context;

		public DebugEmitContext (LambdaExpression lambda)
			: base (lambda)
		{
			dynamic_context = new DynamicEmitContext (lambda);

			var name = dynamic_context.Method.Name;
			var file_name = name + ".dll";

			var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
				new AssemblyName (name), AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

			var type = assembly.DefineDynamicModule (file_name, file_name).DefineType ("Linq", TypeAttributes.Public);

			var method = type.DefineMethod (name, MethodAttributes.Public | MethodAttributes.Static, return_type, param_types);
			ig = method.GetILGenerator ();

			owner.Emit (this);

			type.CreateType ();
			assembly.Save (file_name);
		}

		public override Delegate CreateDelegate ()
		{
			return dynamic_context.CreateDelegate ();
		}
	}
}
