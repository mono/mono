//
// Context.cs:
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public class Context {

		internal DocumentContext Document;

		public Context (string filename)
		{
			Document = new DocumentContext (filename);
		}

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