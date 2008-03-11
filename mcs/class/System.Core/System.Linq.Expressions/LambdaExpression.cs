//
// LambdaExpression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public class LambdaExpression : Expression {

		Expression body;
		ReadOnlyCollection<ParameterExpression> parameters;

		public Expression Body {
			get { return body; }
		}

		public ReadOnlyCollection<ParameterExpression> Parameters {
			get { return parameters; }
		}

		static bool CanAssign (Type target, Type source)
		{
			// This catches object and value type mixage, type compatibility is handled later
			if (target.IsValueType ^ source.IsValueType)
				return false;

			return target.IsAssignableFrom (source);
		}

		internal LambdaExpression (Type delegateType, Expression body, ReadOnlyCollection<ParameterExpression> parameters)
			: base (ExpressionType.Lambda, delegateType)
		{
			if (!delegateType.IsSubclassOf (typeof (System.Delegate)))
				throw new ArgumentException ("delegateType");

			var invoke = delegateType.GetMethod ("Invoke", BindingFlags.Instance | BindingFlags.Public);
			if (invoke == null)
				throw new ArgumentException ("delegate must contain an Invoke method", "delegateType");

			var invoke_parameters = invoke.GetParameters ();
			if (invoke_parameters.Length != parameters.Count)
				throw new ArgumentException (string.Format ("Different number of arguments in delegate {0}", delegateType), "delegateType");

			for (int i = 0; i < invoke_parameters.Length; i++){
				if (!CanAssign (parameters [i].Type, invoke_parameters [i].ParameterType))
					throw new ArgumentException (String.Format ("Can not assign a {0} to a {1}", invoke_parameters [i].ParameterType, parameters [i].Type));
			}

			if (invoke.ReturnType != typeof (void) && !CanAssign (invoke.ReturnType, body.Type))
				throw new ArgumentException (String.Format ("body type {0} can not be assigned to {1}", body.Type, invoke.ReturnType));

			this.body = body;
			this.parameters = parameters;
		}

		void EmitPopIfNeeded (EmitContext ec)
		{
			if (GetReturnType () == typeof (void) && body.Type != typeof (void))
				ec.ig.Emit (OpCodes.Pop);
		}

		internal override void Emit (EmitContext ec)
		{
			body.Emit (ec);
			EmitPopIfNeeded (ec);
			ec.ig.Emit (OpCodes.Ret);
		}

		internal Type GetReturnType ()
		{
			return this.Type.GetMethod ("Invoke").ReturnType;
		}

		public Delegate Compile ()
		{
			var context = EmitContext.Create (this);
			return context.CreateDelegate ();
		}
	}
}
