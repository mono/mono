//
// Context.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class Context 
	{
		public int EndColumn {
			get { throw new NotImplementedException (); }
		}

		public int EndLine {
			get { throw new NotImplementedException (); }
		}

		public int EndPosition {
			get { throw new NotImplementedException (); }
		}

		public string GetCode ()
		{
			throw new NotImplementedException ();
		}

		public JSToken GetToken ()
		{
			throw new NotImplementedException ();
		}

		public int StartColumn {
			get { throw new NotImplementedException (); }
		}

		public int StartLine {
			get { throw new NotImplementedException (); }
		}

		public int StartPosition {
			get { throw new NotImplementedException (); }
		}
	}
}			