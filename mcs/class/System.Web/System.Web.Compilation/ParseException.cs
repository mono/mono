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
		ILocation location;
		string fileText;

		public ParseException (ILocation location, string message)
			: this (location, message, null)
		{
			location = new Location (location);
		}


		public ParseException (ILocation location, string message, Exception inner)
			: base (message, inner)
		{
			this.location = location;
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
			get { return Message; }
		}

		public override string SourceFile {
			get { return FileName; }
		}
		
		public override string FileName {
			get {
				if (location == null)
					return null;

				return location.Filename;
			}
		}

		public override string FileText {
			get {
				if (fileText != null)
					return fileText;

				if (FileName == null)
					return null;

				//FIXME: encoding
				TextReader reader = new StreamReader (FileName);
				fileText = reader.ReadToEnd ();				
				return fileText;
			}
		}

		public override int [] ErrorLines {
			get {
				if (location == null)
					return null;

				return new int [] {location.BeginLine, location.EndLine};
			}
		}

		public override bool ErrorLinesPaired {
			get { return true; }
		}
	}
}

