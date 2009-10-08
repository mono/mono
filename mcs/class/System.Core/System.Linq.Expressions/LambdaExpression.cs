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

		internal LambdaExpression (Type delegateType, Expression body, ReadOnlyCollection<ParameterExpression> parameters)
			: base (ExpressionType.Lambda, delegateType)
		{
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
			ec.EmitCreateDelegate (this);
		}

		internal void EmitBody (EmitContext ec)
		{
			body.Emit (ec);
			EmitPopIfNeeded (ec);
			ec.ig.Emit (OpCodes.Ret);
		}

		internal Type GetReturnType ()
		{
			return this.Type.GetInvokeMethod ().ReturnType;
		}

		public Delegate Compile ()
		{
#if TARGET_JVM || MONOTOUCH
			System.Linq.jvm.Interpreter inter =
				new System.Linq.jvm.Interpreter (this);
			inter.Validate ();
			return inter.CreateDelegate ();
#else
			var context = new CompilationContext ();
			context.AddCompilationUnit (this);
			return context.CreateDelegate ();
#endif
		}
	}
}
