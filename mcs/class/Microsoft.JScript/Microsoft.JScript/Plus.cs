//
// Plus.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public sealed class Plus : BinaryOp {

		public Plus ()
		{
		}

		public  object EvaluatePlus (object v1, object v2)
		{
			if (v1 is String && v2 is String)
				return String.Concat (v1, v2);
			else
				return new object ();
		}

		public static object DoOp (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
