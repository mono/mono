//
// System.Web.Compilation.CompilationException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.CodeDom.Compiler;
using System.Web;

namespace System.Web.Compilation
{
	internal class CompilationException : HtmlizedException
	{
		string filename;
		string errors;
		string file;

		public CompilationException (string filename, string errors, string file)
		{
			this.filename = filename;
			this.errors = errors;
			this.file = file;
		}

		public override string FileName {
			get { return filename; }
		}
		
		public override string Title {
			get { return "Compilation Error"; }
		}

		public override string Description {
			get {
				return "Error compiling a resource required to service this request. " +
				       "Review your source file and modify it to fix this error.";
			}
		}

		public override string ErrorMessage {
			get { return errors; }
		}

		public string File {
			get { return file; }
		}
	}
}

