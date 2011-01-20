//
// System.Web.Compilation.CompilationException
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
using System.Collections.Specialized;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Web;

namespace System.Web.Compilation
{
	[Serializable]
	internal class CompilationException : HtmlizedException
	{
		string filename;
		CompilerErrorCollection errors;
		CompilerResults results;
		string fileText;
		string errmsg;
		int [] errorLines;

		CompilationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
                {
			filename = info.GetString ("filename");
			errors = info.GetValue ("errors", typeof (CompilerErrorCollection)) as CompilerErrorCollection;
			results = info.GetValue ("results", typeof (CompilerResults)) as CompilerResults;
			fileText = info.GetString ("fileText");
			errmsg = info.GetString ("errmsg");
			errorLines = info.GetValue ("errorLines", typeof (int[])) as int[];
                }
		
		public CompilationException (string filename, CompilerErrorCollection errors, string fileText)
		{
			this.filename = filename;
			this.errors = errors;
			this.fileText = fileText;
		}

		public CompilationException (string filename, CompilerResults results, string fileText)
			: this (filename, results != null ? results.Errors : null, fileText)
		{
			this.results = results;
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			base.GetObjectData (info, ctx);
			info.AddValue ("filename", filename);
			info.AddValue ("errors", errors);
			info.AddValue ("results", results);
			info.AddValue ("fileText", fileText);
			info.AddValue ("errmsg", errmsg);
			info.AddValue ("errorLines", errorLines);
		}

		public override string Message {
			get { return ErrorMessage; }
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
					CompilerError firstError = null;
					
					foreach (CompilerError err in errors) {
						if (err.IsWarning)
							continue;
						firstError = err;
						break;
					};

					if (firstError != null) {
						errmsg = firstError.ToString ();
						int idx = errmsg.IndexOf (" : error ");
						if (idx > -1)
							errmsg = errmsg.Substring (idx + 9);
					} else
						errmsg = String.Empty;
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
						if (err.IsWarning)
							continue;
						
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

		public StringCollection CompilerOutput {
			get {
				if (results == null)
					return null;

				return results.Output;
			}
		}

		public CompilerResults Results {
			get { return results; }
		}
	}
}

