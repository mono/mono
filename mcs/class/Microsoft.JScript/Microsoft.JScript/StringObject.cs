//
// StringObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class StringObject : JSObject
	{
		public int length {
			get { throw new NotImplementedException (); }
		}

		public override bool Equals (Object obj)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public new Type GetType ()
		{
			throw new NotImplementedException ();
		}
	}
}