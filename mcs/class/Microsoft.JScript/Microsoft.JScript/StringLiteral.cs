//
// StringLiteral.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	internal class StringLiteral : Exp {

		internal string str;

		internal StringLiteral (AST parent, string s)
		{
			this.parent = parent;
			str = s;
		}

		public override string ToString ()
		{
			return str;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			ig.Emit (OpCodes.Ldstr, str);
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}
}
