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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {

	abstract class EmitContext {

		protected LambdaExpression owner;
		protected Type [] param_types;
		protected Type return_type;

		protected List<object> globals = new List<object> ();

		public ILGenerator ig;

		protected EmitContext (LambdaExpression lambda)
		{
			this.owner = lambda;

			param_types = CreateParameterTypes (owner.Parameters);
			return_type = owner.GetReturnType ();
		}

		static Type [] CreateParameterTypes (ReadOnlyCollection<ParameterExpression> parameters)
		{
			var types = new Type [parameters.Count + 1];
			types [0] = typeof (ExecutionScope);

			for (int i = 0; i < parameters.Count; i++)
				types [i + 1] = parameters [i].Type;

			return types;
		}

		public static EmitContext Create (LambdaExpression lambda)
		{
#if !NET_2_1
			if (Environment.GetEnvironmentVariable ("LINQ_DBG") != null)
				return new DebugEmitContext (lambda);
#endif
			return new DynamicEmitContext (lambda);
		}

		public int GetParameterPosition (ParameterExpression p)
		{
			int position = owner.Parameters.IndexOf (p);
			if (position == -1)
				throw new InvalidOperationException ("Parameter not in scope");

			return position + 1; // + 1 because 0 is the ExecutionScope
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

		public void EmitCall (LocalBuilder local, ReadOnlyCollection<Expression> arguments, MethodInfo method)
		{
			EmitLoad (local);
			EmitArguments (method, arguments);
			EmitCall (method);
		}

		public void EmitCall (Expression expression, ReadOnlyCollection<Expression> arguments, MethodInfo method)
		{
			if (!method.IsStatic)
				EmitLoad (expression);

			EmitArguments (method, arguments);
			EmitCall (method);
		}

		void EmitArguments (MethodInfo method, ReadOnlyCollection<Expression> arguments)
		{
			var parameters = method.GetParameters ();

			for (int i = 0; i < parameters.Length; i++) {
				var parameter = parameters [i];
				var argument = arguments [i];

				if (parameter.ParameterType.IsByRef) {
					ig.Emit (OpCodes.Ldloca, EmitStored (argument));
					return;
				}

				Emit (arguments [i]);
			}
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

		public void EmitScope ()
		{
			ig.Emit (OpCodes.Ldarg_0);
		}

		public void EmitReadGlobal (object global)
		{
			EmitReadGlobal (global, global.GetType ());
		}

		public void EmitReadGlobal (object global, Type type)
		{
			EmitScope ();

			ig.Emit (OpCodes.Ldfld, typeof (ExecutionScope).GetField ("Globals"));

			ig.Emit (OpCodes.Ldc_I4, AddGlobal (global, type));
			ig.Emit (OpCodes.Ldelem, typeof (object));

			var strongbox = type.MakeStrongBoxType ();

			ig.Emit (OpCodes.Isinst, strongbox);
			ig.Emit (OpCodes.Ldfld, strongbox.GetField ("Value"));
		}

		int AddGlobal (object value, Type type)
		{
			globals.Add (CreateStrongBox (value, type));
			return globals.Count - 1;
		}

		static object CreateStrongBox (object value, Type type)
		{
			return Activator.CreateInstance (
				type.MakeStrongBoxType (), value);
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
			method = new DynamicMethod (GenerateName (), return_type, param_types, typeof (ExecutionScope), true);
			ig = method.GetILGenerator ();

			owner.EmitBody (this);
		}

		public override Delegate CreateDelegate ()
		{
			return method.CreateDelegate (owner.Type, new ExecutionScope (globals.ToArray ()));
		}

		static string GenerateName ()
		{
			lock (mlock) {
				return "lambda_method-" + (method_count++);
			}
		}
	}

#if !NET_2_1
	class DebugEmitContext : DynamicEmitContext {

		public DebugEmitContext (LambdaExpression lambda)
			: base (lambda)
		{
			var name = Method.Name;
			var file_name = name + ".dll";

			var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
				new AssemblyName (name), AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

			var type = assembly.DefineDynamicModule (file_name, file_name).DefineType ("Linq", TypeAttributes.Public);

			var method = type.DefineMethod (name, MethodAttributes.Public | MethodAttributes.Static, return_type, param_types);
			ig = method.GetILGenerator ();

			owner.EmitBody (this);

			type.CreateType ();
			assembly.Save (file_name);
		}
	}
#endif
}
