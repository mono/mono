//
// NewExpression.cs
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

	public sealed class NewExpression : Expression {

		ConstructorInfo constructor;
		ReadOnlyCollection<Expression> arguments;
		ReadOnlyCollection<MemberInfo> members;

		public ConstructorInfo Constructor {
			get { return constructor; }
		}

		public ReadOnlyCollection<Expression> Arguments {
			get { return arguments; }
		}

		public ReadOnlyCollection<MemberInfo> Members {
			get { return members; }
		}

		internal NewExpression (Type type, ReadOnlyCollection<Expression> arguments)
			: base (ExpressionType.New, type)
		{
			this.arguments = arguments;
		}

		internal NewExpression (ConstructorInfo constructor, ReadOnlyCollection<Expression> arguments, ReadOnlyCollection<MemberInfo> members)
			: base (ExpressionType.New, constructor.DeclaringType)
		{
			this.constructor = constructor;
			this.arguments = arguments;
			this.members = members;
		}

		internal override void Emit (EmitContext ec)
		{
			var ig = ec.ig;
			var type = this.Type;

			LocalBuilder local = null;
			if (type.IsValueType) {
				local = ig.DeclareLocal (type);
				ig.Emit (OpCodes.Ldloca, local);

				if (constructor == null) {
					ig.Emit (OpCodes.Initobj, type);
					ig.Emit (OpCodes.Ldloc, local);
					return;
				}
			}

			ec.EmitCollection (arguments);

			if (type.IsValueType) {
				ig.Emit (OpCodes.Call, constructor);
				ig.Emit (OpCodes.Ldloc, local);
			} else
				ig.Emit (OpCodes.Newobj, constructor ?? GetDefaultConstructor (type));
		}

		static ConstructorInfo GetDefaultConstructor (Type type)
		{
			return type.GetConstructor (Type.EmptyTypes);
		}
	}
}
