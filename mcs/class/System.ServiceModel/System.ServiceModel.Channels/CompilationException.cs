//
// CompilationException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.CodeDom.Compiler;
using System.Text;
using System.Web;

namespace System.ServiceModel.Channels
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

