//
// ElementInit.cs
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

	public sealed class ElementInit {

		MethodInfo add_method;
		ReadOnlyCollection<Expression> arguments;

		public MethodInfo AddMethod {
			get { return add_method; }
		}

		public ReadOnlyCollection<Expression> Arguments {
			get { return arguments; }
		}

		internal ElementInit (MethodInfo add_method, ReadOnlyCollection<Expression> arguments)
		{
			this.add_method = add_method;
			this.arguments = arguments;
		}

		public override string ToString ()
		{
			return ExpressionPrinter.ToString (this);
		}

		void EmitPopIfNeeded (EmitContext ec)
		{
			if (add_method.ReturnType == typeof (void))
				return;

			ec.ig.Emit (OpCodes.Pop);
		}

		internal void Emit (EmitContext ec, LocalBuilder local)
		{
			ec.EmitCall (local, arguments, add_method);
			EmitPopIfNeeded (ec);
		}
	}
}
