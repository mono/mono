//
// ParameterExpression.cs
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
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class ParameterExpression : Expression {

		string name;

		public string Name {
			get { return name; }
		}

		internal ParameterExpression (Type type, string name)
			: base (ExpressionType.Parameter, type)
		{
			this.name = name;
		}

		void EmitLocalParameter (EmitContext ec, int position)
		{
			ec.ig.Emit (OpCodes.Ldarg, position);
		}

		void EmitHoistedLocal (EmitContext ec, int level, int position)
		{
			ec.EmitScope ();

			for (int i = 0; i < level; i++)
				ec.EmitParentScope ();

			ec.EmitLoadLocals ();

			ec.ig.Emit (OpCodes.Ldc_I4, position);
			ec.ig.Emit (OpCodes.Ldelem, typeof (object));

			ec.EmitLoadStrongBoxValue (Type);
		}

		internal override void Emit (EmitContext ec)
		{
			int position = -1;
			if (ec.IsLocalParameter (this, ref position)) {
				EmitLocalParameter (ec, position);
				return;
			}

			int level = 0;
			if (ec.IsHoistedLocal (this, ref level, ref position)) {
				EmitHoistedLocal (ec, level, position);
				return;
			}

			throw new InvalidOperationException ("Parameter out of scope");
		}
	}
}
