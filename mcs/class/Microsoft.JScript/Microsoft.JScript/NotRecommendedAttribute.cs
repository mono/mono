//
// NotRecommended.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class NotRecommended : Attribute
	{
		public NotRecommended (string message)
		{
			throw new NotImplementedException ();
		}


		public Boolean IsError {
			get { throw new NotImplementedException (); }
		}


		public string Message {
			get { throw new NotImplementedException (); }
		}
	}
}