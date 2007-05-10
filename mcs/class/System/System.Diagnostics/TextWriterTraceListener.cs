//
// System.Diagnostics.TextWriterTraceListener.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
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
using System.IO;
using System.Diagnostics;

namespace System.Diagnostics {

	public class TextWriterTraceListener : TraceListener {

		private TextWriter writer;

		public TextWriterTraceListener () : base ("TextWriter")
		{
		}

		public TextWriterTraceListener (Stream stream)
			: this (stream, "")
		{
		}

		public TextWriterTraceListener (string fileName)
			: this (fileName, "")
		{
		}

		public TextWriterTraceListener (TextWriter writer)
			: this (writer, "")
		{
		}

		public TextWriterTraceListener (Stream stream, string name) 
			: base (name != null ? name : "")
		{
			if (stream == null) 
				throw new ArgumentNullException ("stream");
			writer = new StreamWriter (stream);
		}

		public TextWriterTraceListener (string fileName, string name) 
			: base (name != null ? name : "")
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			writer = new StreamWriter (new FileStream (fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
		}

		public TextWriterTraceListener (TextWriter writer, string name) 
			: base (name != null ? name : "")
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			this.writer = writer;
		}

		public TextWriter Writer {
			get {return writer;}
			set {writer = value;}
		}

		public override void Close ()
		{
			if (writer != null) {
				writer.Flush ();
				writer.Close ();
				writer = null;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Close ();

			base.Dispose (disposing);
		}

		public override void Flush ()
		{
			if (writer != null)
				writer.Flush ();
		}

		public override void Write (string message)
		{
			if (writer != null) {
				if (NeedIndent)
					WriteIndent ();
				writer.Write (message);
			}
		}

		public override void WriteLine (string message)
		{
			if (writer != null) {
				if (NeedIndent)
					WriteIndent ();
				writer.WriteLine (message);
				NeedIndent = true;
			}
		}
	}
}

