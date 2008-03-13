//
// ConditionalExpression.cs
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
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class ConditionalExpression : Expression {

		Expression test;
		Expression if_true;
		Expression if_false;

		public Expression Test {
			get { return test; }
		}

		public Expression IfTrue {
			get { return if_true; }
		}

		public Expression IfFalse {
			get { return if_false; }
		}

		internal ConditionalExpression (Expression test, Expression if_true, Expression if_false)
			: base (ExpressionType.Conditional, if_true.Type)
		{
			this.test = test;
			this.if_true = if_true;
			this.if_false = if_false;
		}

		internal override void Emit (EmitContext ec)
		{
			var ig = ec.ig;
			var false_target = ig.DefineLabel ();
			var end_target = ig.DefineLabel ();

			test.Emit (ec);
			ig.Emit (OpCodes.Brfalse, false_target);

			if_true.Emit (ec);
			ig.Emit (OpCodes.Br, end_target);

			ig.MarkLabel (false_target);
			if_false.Emit (ec);

			ig.MarkLabel (end_target);
		}
	}
}
