//
// UnaryExpression.cs
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class UnaryExpression : Expression {

		Expression operand;
		MethodInfo method;

		public Expression Operand {
			get { return operand; }
		}

		public MethodInfo Method {
			get { return method; }
		}

		[MonoTODO]
		public bool IsLifted {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsLiftedToNull {
			get { throw new NotImplementedException (); }
		}

		internal UnaryExpression (ExpressionType node_type, Expression operand)
			: base (node_type, operand.Type)
		{
			this.operand = operand;
		}

		internal UnaryExpression (ExpressionType node_type, Expression operand, Type type)
			: base (node_type, type)
		{
			this.operand = operand;
		}

		internal UnaryExpression (ExpressionType node_type, Expression operand, Type type, MethodInfo method)
			: base (node_type, type)
		{
			this.operand = operand;
			this.method = method;
		}

		void EmitArrayLength (EmitContext ec)
		{
			operand.Emit (ec);
			ec.ig.Emit (OpCodes.Ldlen);
		}

		void EmitTypeAs (EmitContext ec)
		{
			var type = this.Type;

			ec.EmitIsInst (operand, type);

			if (IsNullable (type))
				ec.ig.Emit (OpCodes.Unbox_Any, type);
		}

		internal override void Emit (EmitContext ec)
		{
			switch (this.NodeType) {
			case ExpressionType.ArrayLength:
				EmitArrayLength (ec);
				return;
			case ExpressionType.TypeAs:
				EmitTypeAs (ec);
				return;
			default:
				throw new NotImplementedException (this.NodeType.ToString ());
			}
		}
	}
}
