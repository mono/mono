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
#if NET_2_0

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

		protected override string [] GetSupportedAttributes ()
		{
			return attributes;
		}

		[MonoTODO]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, object data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, params object [] data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string message)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string format, params object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
