//
// System.Web.Compilation.CompilationException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.CodeDom.Compiler;
using System.Text;
using System.Web;

namespace System.Web.Compilation
{
	internal class CompilationException : HtmlizedException
	{
		string filename;
		CompilerErrorCollection errors;
		string fileText;
		string errmsg;
		int [] errorLines;

		public CompilationException (string filename, CompilerErrorCollection errors, string fileText)
		{
			this.filename = filename;
			this.errors = errors;
			this.fileText = fileText;
		}

		public override string SourceFile {
			get {
				if (errors == null || errors.Count == 0)
					return filename;

				return errors [0].FileName;
			}
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
			get {
				if (errmsg == null && errors != null) {
					StringBuilder sb = new StringBuilder ();
					foreach (CompilerError err in errors) {
						sb.Append (err);
						sb.Append ("\n");
					}
					errmsg = sb.ToString ();
				}

				return errmsg;
			}
		}

		public override string FileText {
			get { return fileText; }
		}

		public override int [] ErrorLines {
			get {
				if (errorLines == null && errors != null) {
					ArrayList list = new ArrayList ();
					foreach (CompilerError err in errors) {
						if (err.Line != 0 && !list.Contains (err.Line))
							list.Add (err.Line);
					}
					errorLines = (int []) list.ToArray (typeof (int));
					Array.Sort (errorLines);
				}

				return errorLines;
			}
		}

		public override bool ErrorLinesPaired {
			get { return false; }
		}
	}
}

