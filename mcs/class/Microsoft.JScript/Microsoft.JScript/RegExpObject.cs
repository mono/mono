//
// RegExpObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class RegExpObject : JSObject
	{
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public string Source {
			get { throw new NotImplementedException (); }
		}

		public bool ignoreCase {
			get { throw new NotImplementedException (); }
		}

		public bool global {
			get { throw new NotImplementedException (); }
		}
		
		public bool multiline {
			get { throw new NotImplementedException (); }
		}

		public Object lastIndex {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}