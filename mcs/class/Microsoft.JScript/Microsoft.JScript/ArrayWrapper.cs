//
// ArrayWrapper.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class ArrayWrapper : ArrayObject
	{
		public new Type GetType ()
		{
			throw new NotImplementedException ();
		}

		
		public override object length {
			get { throw new NotImplementedException (); }
			set {}
		}


		public int Compare (object x, object y)
		{
			throw new NotImplementedException ();
		}
	}
}