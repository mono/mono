//
// System.Web.Compilation.ParseException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;

namespace System.Web.Compilation
{
	internal class ParseException : HtmlizedException
	{
		string message;
		ILocation location;

		public ParseException (ILocation location, string message)
		{
			if (location == null)
				throw new Exception ();
			this.location = location;
			this.message = message;
		}

		public override string Title {
			get { return "Parser Error"; }
		}

		public override string Description {
			get {
				return "Error parsing a resource required to service this request. " +
				       "Review your source file and modify it to fix this error.";
			}
		}

		public override string ErrorMessage {
			get { return message; }
		}

		public override string FileName {
			get { return location.Filename; }
		}
	}
}

