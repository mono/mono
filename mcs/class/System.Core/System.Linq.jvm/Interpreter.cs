//
// Interpreter.cs
//
// (C) 2008 Mainsoft, Inc. (http://www.mainsoft.com)
// (C) 2008 db4objects, Inc. (http://www.db4o.com)
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq.jvm {

	class Interpreter {

		class VoidTypeMarker {
		}

		static readonly Type VoidMarker = typeof (VoidTypeMarker);
		static readonly MethodInfo [] delegates = new MethodInfo [5];

		LambdaExpression lambda;

		static Interpreter ()
		{
			var methods = from method in typeof (Interpreter).GetMethods (
							  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
						  where method.Name == "GetDelegate"
						  select method;

			foreach (var method in methods)
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

		Delegate GetDelegate<TResult> ()
		{
			if (typeof (TResult) == VoidMarker)
				return new Action (ActionRunner);

			return new Func<TResult> (FuncRunner<TResult>);
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
				return new Action<T> (ActionRunner<T>);

			return new Func<T, TResult> (FuncRunner<T, TResult>);
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
				return new Action<T1, T2> (ActionRunner<T1, T2>);

			return new Func<T1, T2, TResult> (FuncRunner<T1, T2, TResult>);
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
				return new Action<T1, T2, T3> (ActionRunner<T1, T2, T3>);

			return new Func<T1, T2, T3, TResult> (FuncRunner<T1, T2, T3, TResult>);
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
				return new Action<T1, T2, T3, T4> (ActionRunner<T1, T2, T3, T4>);

			return new Func<T1, T2, T3, T4, TResult> (FuncRunner<T1, T2, T3, T4, TResult>);
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
