//
// ErrorObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript {

	public class ErrorObject : JSObject {

		public Object message;
		public Object number;
		public Object description;
	
		public static explicit operator Exception (ErrorObject errObj)
		{
			throw new NotImplementedException ();
		}
	}
}