//
// Interpreter.cs
//
// (C) 2008 Mainsoft, Inc. (http://www.mainsoft.com)
// (C) 2008 db4objects, Inc. (http://www.db4o.com)
// (C) 2010 Novell, Inc. (http://www.novell.com)
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

using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq.jvm {

	class Interpreter {

		class VoidTypeMarker {}

		static readonly Type VoidMarker = typeof (VoidTypeMarker);
		static readonly MethodInfo [] delegates = new MethodInfo [5];
		const BindingFlags method_flags = BindingFlags.NonPublic | BindingFlags.Instance;

		readonly LambdaExpression lambda;

		static Interpreter ()
		{
			foreach (var method in typeof (Interpreter).GetMethods (method_flags).Where (m => m.Name == "GetDelegate"))
				delegates [method.GetGenericArguments ().Length - 1] = method;
		}

		public Interpreter (LambdaExpression lambda)
		{
			this.lambda = lambda;
		}

		public Delegate CreateDelegate ()
		{
			var types = GetGenericSignature ();
			var creator = delegates [types.Length - 1].MakeGenericMethod (types);

			return (Delegate) creator.Invoke (this, new object [0]);
		}

		public void Validate ()
		{
			new ExpressionValidator (lambda).Validate ();
		}

		Type [] GetGenericSignature ()
		{
			var count = lambda.Parameters.Count;
			var types = new Type [count + 1];

			var return_type = lambda.GetReturnType ();
			if (return_type == typeof (void))
				return_type = VoidMarker;

			types [count] = return_type;
			for (int i = 0; i < count; i++) {
				types [i] = lambda.Parameters [i].Type;
			}

			return types;
		}

		object Run (object [] arg)
		{
			return ExpressionInterpreter.Interpret (lambda, arg);
		}

		MethodInfo GetActionRunner (params Type [] types)
		{
			return GetRunner ("ActionRunner", types);
		}

		MethodInfo GetFuncRunner (params Type [] types)
		{
			return GetRunner ("FuncRunner", types);
		}

		MethodInfo GetRunner (string name, Type [] type_arguments)
		{
			var method = GetMethod (name, type_arguments.Length);
			if (method == null)
				throw new InvalidOperationException ();

			if (type_arguments.Length == 0)
				return method;

			return method.MakeGenericMethod (type_arguments);
		}

		MethodInfo GetMethod (string name, int parameters)
		{
			foreach (var method in GetType ().GetMethods (method_flags)) {
				if (method.Name != name)
					continue;

				if (method.GetGenericArguments ().Length != parameters)
					continue;

				return method;
			}

			return null;
		}

		Delegate CreateDelegate (MethodInfo runner)
		{
			return Delegate.CreateDelegate (lambda.Type, this, runner);
		}

		// all methods below are called through reflection

		Delegate GetDelegate<TResult> ()
		{
			if (typeof (TResult) == VoidMarker)
				return CreateDelegate (GetActionRunner (Type.EmptyTypes));

			return CreateDelegate (GetFuncRunner (typeof (TResult)));
		}

		TResult FuncRunner<TResult> ()
		{
			return (TResult) Run (new object [0]);
		}

		void ActionRunner ()
		{
			Run (new object [0]);
		}

		Delegate GetDelegate<T, TResult> ()
		{
			if (typeof (TResult) == VoidMarker)
				return CreateDelegate (GetActionRunner (typeof (T)));

			return CreateDelegate (GetFuncRunner (typeof (T), typeof (TResult)));
		}

		TResult FuncRunner<T, TResult> (T arg)
		{
			return (TResult) Run (new object [] { arg });
		}

		void ActionRunner<T> (T arg)
		{
			Run (new object [] { arg });
		}

		Delegate GetDelegate<T1, T2, TResult> ()
		{
			if (typeof (TResult) == VoidMarker)
				return CreateDelegate (GetActionRunner (typeof (T1), typeof (T2)));

			return CreateDelegate (GetFuncRunner (typeof (T1), typeof (T2), typeof (TResult)));
		}

		TResult FuncRunner<T1, T2, TResult> (T1 arg1, T2 arg2)
		{
			return (TResult) Run (new object [] { arg1, arg2 });
		}

		void ActionRunner<T1, T2> (T1 arg1, T2 arg2)
		{
			Run (new object [] { arg1, arg2 });
		}

		Delegate GetDelegate<T1, T2, T3, TResult> ()
		{
			if (typeof (TResult) == VoidMarker)
				return CreateDelegate (GetActionRunner (typeof (T1), typeof (T2), typeof (T3)));

			return CreateDelegate (GetFuncRunner (typeof (T1), typeof (T2), typeof (T3), typeof (TResult)));
		}

		TResult FuncRunner<T1, T2, T3, TResult> (T1 arg1, T2 arg2, T3 arg3)
		{
			return (TResult) Run (new object [] { arg1, arg2, arg3 });
		}

		void ActionRunner<T1, T2, T3> (T1 arg1, T2 arg2, T3 arg3)
		{
			Run (new object [] { arg1, arg2, arg3 });
		}

		Delegate GetDelegate<T1, T2, T3, T4, TResult> ()
		{
			if (typeof (TResult) == VoidMarker)
				return CreateDelegate (GetActionRunner (typeof (T1), typeof (T2), typeof (T3), typeof (T4)));

			return CreateDelegate (GetFuncRunner (typeof (T1), typeof (T2), typeof (T3), typeof (T4), typeof (TResult)));
		}

		TResult FuncRunner<T1, T2, T3, T4, TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			return (TResult) Run (new object [] { arg1, arg2, arg3, arg4 });
		}

		void ActionRunner<T1, T2, T3, T4> (T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			Run (new object [] { arg1, arg2, arg3, arg4 });
		}
	}
}
