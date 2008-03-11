//
// MethodCallExpression.cs
//
// Author:
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class MethodCallExpression : Expression {

		Expression obj;
		MethodInfo method;
		ReadOnlyCollection<Expression> arguments;

		public Expression Object {
			get { return obj; }
		}

		public MethodInfo Method {
			get { return method; }
		}

		public ReadOnlyCollection<Expression> Arguments {
			get { return arguments; }
		}

		internal MethodCallExpression (MethodInfo method, ReadOnlyCollection<Expression> arguments)
			: base (ExpressionType.Call, method.ReturnType)
		{
			this.method = method;
			this.arguments = arguments;
		}

		internal MethodCallExpression (Expression obj, MethodInfo method, ReadOnlyCollection<Expression> arguments)
			: base (ExpressionType.Call, method.ReturnType)
		{
			this.obj = obj;
			this.method = method;
			this.arguments = arguments;
		}

		internal override void Emit (EmitContext ec)
		{
			ec.EmitCall (obj, arguments, method);
		}
	}
}
