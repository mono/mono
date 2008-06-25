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

	class CompilationContext {

		List<object> globals = new List<object> ();
		List<EmitContext> units = new List<EmitContext> ();

		public int AddGlobal (object global)
		{
			return AddItemToList (global, globals);
		}

		public object [] GetGlobals ()
		{
			return globals.ToArray ();
		}

		static int AddItemToList<T> (T item, IList<T> list)
		{
			list.Add (item);
			return list.Count - 1;
		}

		public int AddCompilationUnit (LambdaExpression lambda)
		{
			var context = new EmitContext (this, lambda);
			var unit = AddItemToList (context, units);
			context.Emit ();
			return unit;
		}

		public Delegate CreateDelegate ()
		{
			return CreateDelegate (0, new ExecutionScope (this));
		}

		public Delegate CreateDelegate (int unit, ExecutionScope scope)
		{
			return units [unit].CreateDelegate (scope);
		}
	}

	class EmitContext {

		LambdaExpression owner;
		CompilationContext context;
		DynamicMethod method;

		public ILGenerator ig;

		public EmitContext (CompilationContext context, LambdaExpression lambda)
		{
			this.context = context;
			this.owner = lambda;

			method = new DynamicMethod ("lambda_method", owner.GetReturnType (),
				CreateParameterTypes (owner.Parameters), typeof (ExecutionScope), true);

			ig = method.GetILGenerator ();
		}

		public void Emit ()
		{
			owner.EmitBody (this);
		}

		static Type [] CreateParameterTypes (ReadOnlyCollection<ParameterExpression> parameters)
		{
			var types = new Type [parameters.Count + 1];
			types [0] = typeof (ExecutionScope);

			for (int i = 0; i < parameters.Count; i++)
				types [i + 1] = parameters [i].Type;

			return types;
		}

		public int GetParameterPosition (ParameterExpression p)
		{
			int position = owner.Parameters.IndexOf (p);
			if (position == -1)
				throw new InvalidOperationException ("Parameter not in scope");

			return position + 1; // + 1 because 0 is the ExecutionScope
		}

		public Delegate CreateDelegate (ExecutionScope scope)
		{
			return method.CreateDelegate (owner.Type, scope);
		}

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

		public void EmitLoadAddress (Expression expression)
		{
			ig.Emit (OpCodes.Ldloca, EmitStored (expression));
		}

		public void EmitLoadSubject (Expression expression)
		{
			if (expression.Type.IsValueType) {
				EmitLoadAddress (expression);
				return;
			}

			Emit (expression);
		}

		public void EmitLoadSubject (LocalBuilder local)
		{
			if (local.LocalType.IsValueType) {
				EmitLoadAddress (local);
				return;
			}

			EmitLoad (local);
		}

		public void EmitLoadAddress (LocalBuilder local)
		{
			ig.Emit (OpCodes.Ldloca, local);
		}

		public void EmitLoad (LocalBuilder local)
		{
			ig.Emit (OpCodes.Ldloc, local);
		}

		public void EmitCall (LocalBuilder local, ReadOnlyCollection<Expression> arguments, MethodInfo method)
		{
			EmitLoadSubject (local);
			EmitArguments (method, arguments);
			EmitCall (method);
		}

		public void EmitCall (LocalBuilder local, MethodInfo method)
		{
			EmitLoadSubject (local);
			EmitCall (method);
		}

		public void EmitCall (Expression expression, MethodInfo method)
		{
			if (!method.IsStatic)
				EmitLoadSubject (expression);

			EmitCall (method);
		}

		public void EmitCall (Expression expression, ReadOnlyCollection<Expression> arguments, MethodInfo method)
		{
			if (!method.IsStatic)
				EmitLoadSubject (expression);

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
					continue;
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

		public void EmitNullableHasValue (LocalBuilder local)
		{
			EmitCall (local, "get_HasValue");
		}

		public void EmitNullableInitialize (LocalBuilder local)
		{
			ig.Emit (OpCodes.Ldloca, local);
			ig.Emit (OpCodes.Initobj, local.LocalType);
			ig.Emit (OpCodes.Ldloc, local);
		}

		public void EmitNullableGetValue (LocalBuilder local)
		{
			EmitCall (local, "get_Value");
		}

		public void EmitNullableGetValueOrDefault (LocalBuilder local)
		{
			EmitCall (local, "GetValueOrDefault");
		}

		void EmitCall (LocalBuilder local, string method_name)
		{
			EmitCall (local, local.LocalType.GetMethod (method_name, Type.EmptyTypes));
		}

		public void EmitNullableNew (Type of)
		{
			ig.Emit (OpCodes.Newobj, of.GetConstructor (new [] { of.GetFirstGenericArgument () }));
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

			var type = expression.Type;

			if (type.IsValueType)
				ig.Emit (OpCodes.Box, type);

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
			return context.AddGlobal (CreateStrongBox (value, type));
		}

		public void EmitCreateDelegate (LambdaExpression lambda)
		{
			EmitScope ();

			ig.Emit (OpCodes.Ldc_I4, AddChildContext (lambda));
			ig.Emit (OpCodes.Ldnull);

			ig.Emit (OpCodes.Callvirt, typeof (ExecutionScope).GetMethod ("CreateDelegate"));

			ig.Emit (OpCodes.Castclass, lambda.Type);
		}

		int AddChildContext (LambdaExpression lambda)
		{
			return context.AddCompilationUnit (lambda);
		}

		static object CreateStrongBox (object value, Type type)
		{
			return Activator.CreateInstance (
				type.MakeStrongBoxType (), value);
		}
	}
}
