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
		string fileName;
		string message;
		int line;
		int col;
		int sourceErrorLine;

		public ParseException (string fileName, string message, int line, int col)
			: base (message)
		{
			this.fileName = fileName;
			this.message = message;
			this.line = line >= 1 ? line : 1;
			this.col = col;
		}
		
		public ParseException (string fileName, string message, int line, int col, Exception inner)
			: base (message, inner)
		{
			this.fileName = fileName;
			this.message = message;
			this.line = line >= 1 ? line : 1;
			this.col = col;
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
			get { return fileName; }
		}
		
		public override StringReader SourceError {
			get {
				StreamReader input = new StreamReader (File.OpenRead (fileName));
				string result = GetErrorLines (input, line - 1, out sourceErrorLine);
				input.Close ();
				input = null;
				return new StringReader (result);
			}
		}

		public override int SourceErrorLine {
			get { return sourceErrorLine; }
		}

		public override TextReader SourceFile {
			get { return null; }
		}
	}
}

