//
// MemberBinding.cs
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

	public abstract class MemberBinding {

		MemberBindingType binding_type;
		MemberInfo member;

		public MemberBindingType BindingType {
			get { return binding_type; }
		}

		public MemberInfo Member {
			get { return member; }
		}

		protected MemberBinding (MemberBindingType binding_type, MemberInfo member)
		{
			this.binding_type = binding_type;
			this.member = member;
		}

		public override string ToString ()
		{
			return ExpressionPrinter.ToString (this);
		}

		internal abstract void Emit (EmitContext ec, LocalBuilder local);

		internal LocalBuilder EmitLoadMember (EmitContext ec, LocalBuilder local)
		{
			ec.EmitLoadSubject (local);

			return member.OnFieldOrProperty<LocalBuilder> (
				field => EmitLoadField (ec, field),
				prop => EmitLoadProperty (ec, prop));
		}

		LocalBuilder EmitLoadProperty (EmitContext ec, PropertyInfo property)
		{
			var getter = property.GetGetMethod (true);
			if (getter == null)
				throw new NotSupportedException ();

			var store = ec.ig.DeclareLocal (property.PropertyType);
			ec.EmitCall (getter);
			ec.ig.Emit (OpCodes.Stloc, store);
			return store;
		}

		LocalBuilder EmitLoadField (EmitContext ec, FieldInfo field)
		{
			var store = ec.ig.DeclareLocal (field.FieldType);
			ec.ig.Emit (OpCodes.Ldfld, field);
			ec.ig.Emit (OpCodes.Stloc, store);
			return store;
		}
	}
}
