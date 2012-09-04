//
// DelimitedListTraceFilter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2007 Novell, Inc.
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
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Diagnostics
{
	public class DelimitedListTraceListener : TextWriterTraceListener
	{
		public DelimitedListTraceListener (string fileName)
			: base (fileName)
		{
		}

		public DelimitedListTraceListener (string fileName, string name)
			: base (fileName, name)
		{
		}

		public DelimitedListTraceListener (Stream stream)
			: base (stream)
		{
		}

		public DelimitedListTraceListener (Stream stream, string name)
			: base (stream, name)
		{
		}

		public DelimitedListTraceListener (TextWriter writer)
			: base (writer)
		{
		}

		public DelimitedListTraceListener (TextWriter writer, string name)
			: base (writer, name)
		{
		}

		static readonly string [] attributes = new string [] {"delimiter"};
		string delimiter = ";";

		public string Delimiter {
			get { return delimiter; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				delimiter = value;
			}
		}

		protected internal override string [] GetSupportedAttributes ()
		{
			return attributes;
		}

		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, object data)
		{
			TraceCore (eventCache, source, eventType, id, null, data);
		}

		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, params object [] data)
		{
			TraceCore (eventCache, source, eventType, id, null, data);
		}

		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string message)
		{
			TraceCore (eventCache, source, eventType, id, message);
		}

		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string format, params object [] args)
		{
			TraceCore (eventCache, source, eventType, id, String.Format (format, args));
		}

		void TraceCore (TraceEventCache c, string source, TraceEventType eventType, int id, string message, params object [] data)
		{
			// source, eventType, id, message?, data-comma-separated
			Write (String.Format ("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{12}",
			       delimiter,
			       source != null ? "\"" + source.Replace ("\"", "\"\"") + "\"": null,
			       eventType,
			       id,
			       message != null ? "\"" + message.Replace ("\"", "\"\"") + "\"" : null,
			       FormatData (data),
			       IsTarget (c, TraceOptions.ProcessId) ? c.ProcessId.ToString () : null,
			       IsTarget (c, TraceOptions.LogicalOperationStack) ? FormatArray (c.LogicalOperationStack, ", ") : null,
			       IsTarget (c, TraceOptions.ThreadId) ? c.ThreadId : null,
			       IsTarget (c, TraceOptions.DateTime) ? c.DateTime.ToString ("o") : null,
			       IsTarget (c, TraceOptions.Timestamp) ? c.Timestamp.ToString () : null,
			       IsTarget (c, TraceOptions.Callstack) ? c.Callstack : null,
			       Environment.NewLine));
		}

		bool IsTarget (TraceEventCache c, TraceOptions opt)
		{
			return c != null && (TraceOutputOptions & opt) != 0;
		}

		string FormatData (object [] data)
		{
			if (data == null || data.Length == 0)
				return null;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < data.Length; i++) {
				if (data [i] != null)
					sb.Append ('"').Append (data [i].ToString ().Replace ("\"", "\"\"")).Append ('"');
				if (i + 1 < data.Length)
					sb.Append (',');
			}
			return sb.ToString ();
		}
	}
}
