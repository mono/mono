//
// System.Web.Compilation.CompilationException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Web;
using System.IO;

namespace System.Web.Compilation
{
	internal class CompilationException : HtmlizedException
	{
		CompilationResult result;

		public CompilationException (CompilationResult result)
			: base ("Compilation Error")
		{
			this.result = result;
		}

		public CompilationException (string msg, CompilationResult result)
			: base (msg)
		{
			this.result = result;
		}

		public CompilationException (CompilationCacheItem item)
		{
			this.result = item.Result;
		}

		public override string FileName {
			get { return result.FileName; }
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
				//FIXME: it should be a one line message.
				return result.CompilerOutput;
			}
		}

		//TODO: get lines from compiler output.
		public override StringReader SourceError { get {return null;}}
		public override int SourceErrorLine { get { return -1; } }

		public override TextReader SourceFile {
			get {
				StreamReader input = new StreamReader (File.OpenRead (FileName));
				int sourceErrorLine;
				string result = GetErrorLines (input, 0, out sourceErrorLine);
				input.Close ();
				input = null;
				return new StringReader (result);
			}
		}
	}
}

