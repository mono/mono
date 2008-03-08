//
// MemberAssignement.cs
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
using System.Text;

namespace System.Linq.Expressions {

	public sealed class MemberAssignment : MemberBinding {

		Expression expression;

		public Expression Expression {
			get { return expression; }
		}

		internal MemberAssignment (MemberInfo member, Expression expression)
			: base (MemberBindingType.Assignment, member)
		{
			this.expression = expression;
		}

		internal override void Emit (EmitContext ec, LocalBuilder local)
		{
			var member = this.Member;

			switch (member.MemberType) {
			case MemberTypes.Field:
				EmitFieldAssignment (ec, (FieldInfo) member, local);
				return;
			case MemberTypes.Property:
				EmitPropertyAssignment (ec, (PropertyInfo) member, local);
				return;
			default:
				throw new NotSupportedException (member.MemberType.ToString ());
			}
		}

		void EmitFieldAssignment (EmitContext ec, FieldInfo field, LocalBuilder local)
		{
			Expression.EmitLoad (ec, local);
			expression.Emit (ec);
			ec.ig.Emit (OpCodes.Stfld, field);
		}

		void EmitPropertyAssignment (EmitContext ec, PropertyInfo property, LocalBuilder local)
		{
			var setter = property.GetSetMethod (true);
			if (setter == null)
				throw new InvalidOperationException ();

			Expression.EmitLoad (ec, local);
			expression.Emit (ec);
			Expression.EmitCall (ec, setter);
		}
	}
}
