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
			writer = File.AppendText (fileName);
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
			writer.Flush ();
		}

		public override void Write (string message)
		{
			if (NeedIndent)
				WriteIndent ();
			writer.Write (message);
		}

		public override void WriteLine (string message)
		{
			if (NeedIndent)
				WriteIndent ();
			writer.WriteLine (message);
			NeedIndent = true;
		}
	}
}

