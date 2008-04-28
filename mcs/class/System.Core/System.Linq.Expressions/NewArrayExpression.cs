//
// NewArrayExpression.cs
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class NewArrayExpression : Expression {

		ReadOnlyCollection<Expression> expressions;

		public ReadOnlyCollection<Expression> Expressions {
			get { return expressions; }
		}

		internal NewArrayExpression (ExpressionType et, Type type, ReadOnlyCollection<Expression> expressions)
			: base (et, type)
		{
			this.expressions = expressions;
		}

		void EmitNewArrayInit (EmitContext ec, Type type)
		{
			var size = expressions.Count;

			ec.ig.Emit (OpCodes.Ldc_I4, size);
			ec.ig.Emit (OpCodes.Newarr, type);

			for (int i = 0; i < size; i++) {
				ec.ig.Emit (OpCodes.Dup);
				ec.ig.Emit (OpCodes.Ldc_I4, i);
				expressions [i].Emit (ec);
				ec.ig.Emit (OpCodes.Stelem, type);
			}
		}

		void EmitNewArrayBounds (EmitContext ec, Type type)
		{
			int rank = expressions.Count;

			ec.EmitCollection (expressions);

			if (rank == 1) {
				ec.ig.Emit (OpCodes.Newarr, type);
				return;
			}

			ec.ig.Emit(OpCodes.Newobj, GetArrayConstructor (type, rank));
		}

		static ConstructorInfo GetArrayConstructor (Type type, int rank)
		{
			return CreateArray (type, rank).GetConstructor (CreateTypeParameters (rank));
		}

		static Type [] CreateTypeParameters (int rank)
		{
			return Enumerable.Repeat (typeof (int), rank).ToArray ();
		}

		static Type CreateArray (Type type, int rank)
		{
			return type.MakeArrayType (rank);
		}

		internal override void Emit (EmitContext ec)
		{
			var type = this.Type.GetElementType ();

			switch (this.NodeType) {
			case ExpressionType.NewArrayInit:
				EmitNewArrayInit (ec, type);
				return;
			case ExpressionType.NewArrayBounds:
				EmitNewArrayBounds (ec, type);
				return;
			default:
				throw new NotSupportedException ();
			}
		}
	}
}
