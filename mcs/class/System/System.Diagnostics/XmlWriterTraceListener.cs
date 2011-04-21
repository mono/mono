//
// XmlWriterTraceListener.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc  http://www.novell.com
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

#if NET_2_0 && XML_DEP

using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace System.Diagnostics
{
	public class XmlWriterTraceListener : TextWriterTraceListener
	{
		static readonly string e2e_ns = "http://schemas.microsoft.com/2004/06/E2ETraceEvent";
		static readonly string sys_ns = "http://schemas.microsoft.com/2004/06/windows/eventlog/system";
		static readonly string default_name = "XmlWriter";
		XmlWriter w;

		public XmlWriterTraceListener (string filename)
			: this (filename, default_name)
		{
		}

		public XmlWriterTraceListener (string filename, string name)
			: this (new StreamWriter (new FileStream (filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)), name)
		{
		}

		public XmlWriterTraceListener (Stream stream)
			: this (stream, default_name)
		{
		}

		public XmlWriterTraceListener (Stream writer, string name)
			: this (new StreamWriter (writer), name)
		{
		}

		public XmlWriterTraceListener (TextWriter writer)
			: this (writer, default_name)
		{
		}

		public XmlWriterTraceListener (TextWriter writer, string name)
			: base (name)
		{
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			w = XmlWriter.Create (writer, settings);
		}

		public override void Close ()
		{
			w.Close ();
		}

		public override void Fail (string message, string detailMessage)
		{
			TraceEvent (null, null, TraceEventType.Error,
				    0, String.Concat (message, " ", detailMessage));
		}

		public override void TraceData (TraceEventCache eventCache,
			string source, TraceEventType eventType, int id,
			object data)
		{
			TraceCore (eventCache, source, eventType, id, false,
				   Guid.Empty, 2, true, data);
		}

		[MonoLimitation ("level is not always correct")]
		public override void TraceData (TraceEventCache eventCache,
			string source, TraceEventType eventType, int id,
			params object [] data)
		{
			TraceCore (eventCache, source, eventType, id, false,
				   Guid.Empty, 2, true, data);
		}

		[MonoLimitation ("level is not always correct")]
		public override void TraceEvent (TraceEventCache eventCache,
			string source, TraceEventType eventType, int id,
			string message)
		{
			TraceCore (eventCache, source, eventType,
				   id, false, Guid.Empty, 2, true, message);
		}

		[MonoLimitation ("level is not always correct")]
		public override void TraceEvent (TraceEventCache eventCache,
			string source, TraceEventType eventType, int id,
			string format, params object [] args)
		{
			TraceCore (eventCache, source, eventType,
				   id, false, Guid.Empty, 2, true, String.Format (format, args));
		}

		public override void TraceTransfer (TraceEventCache eventCache,
			string source, int id, string message,
			Guid relatedActivityId)
		{
			TraceCore (eventCache, source, TraceEventType.Transfer,
				   id, true, relatedActivityId, 255, true, message);
		}

		public override void Write (string message)
		{
			WriteLine (message);
		}

		[MonoLimitation ("level is not always correct")]
		public override void WriteLine (string message)
		{
			// FIXME: what is the correct level?
			TraceCore (null, "Trace", TraceEventType.Information,
				   0, false, Guid.Empty, 8, false, message);
		}

		void TraceCore (TraceEventCache eventCache,
			string source, TraceEventType eventType, int id,
			bool hasRelatedActivity, Guid relatedActivity,
			int level, bool wrapData, params object [] data)
		{
			Process p = eventCache != null ?
				Process.GetProcessById (eventCache.ProcessId) :
				Process.GetCurrentProcess ();

			w.WriteStartElement ("E2ETraceEvent", e2e_ns);

			// <System>
			w.WriteStartElement ("System", sys_ns);
			w.WriteStartElement ("EventID", sys_ns);
			w.WriteString (XmlConvert.ToString (id));
			w.WriteEndElement ();
			// FIXME: find out what should be written
			w.WriteStartElement ("Type", sys_ns);
			w.WriteString ("3");
			w.WriteEndElement ();
			w.WriteStartElement ("SubType", sys_ns);
			// FIXME: it does not seem always to match eventType value ...
			w.WriteAttributeString ("Name", eventType.ToString ());
			// FIXME: find out what should be written
			w.WriteString ("0");
			w.WriteEndElement ();
			// FIXME: find out what should be written
			w.WriteStartElement ("Level", sys_ns);
			w.WriteString (level.ToString ());
			w.WriteEndElement ();
			w.WriteStartElement ("TimeCreated", sys_ns);
			w.WriteAttributeString ("SystemTime", XmlConvert.ToString (eventCache != null ? eventCache.DateTime : DateTime.Now));
			w.WriteEndElement ();
			w.WriteStartElement ("Source", sys_ns);
			w.WriteAttributeString ("Name", source);
			w.WriteEndElement ();
			w.WriteStartElement ("Correlation", sys_ns);
			w.WriteAttributeString ("ActivityID", String.Concat ("{", Guid.Empty, "}"));
			w.WriteEndElement ();
			w.WriteStartElement ("Execution", sys_ns);
			// FIXME: which should I use here?
			//w.WriteAttributeString ("ProcessName", p.ProcessName);
			w.WriteAttributeString ("ProcessName", p.MainModule.ModuleName);
			w.WriteAttributeString ("ProcessID", p.Id.ToString ());
			w.WriteAttributeString ("ThreadID", eventCache != null ? eventCache.ThreadId : Thread.CurrentThread.ManagedThreadId.ToString ());
			w.WriteEndElement ();
			w.WriteStartElement ("Channel", sys_ns);
			// FIXME: find out what should be written.
			w.WriteEndElement ();
			w.WriteStartElement ("Computer");
			w.WriteString (p.MachineName);
			w.WriteEndElement ();

			w.WriteEndElement ();

			// <ApplicationData>
			w.WriteStartElement ("ApplicationData", e2e_ns);
			foreach (object o in data) {
				if (wrapData)
					w.WriteStartElement ("TraceData", e2e_ns);
				if (o is XPathNavigator)
					// the output ignores xmlns difference between the parent (E2ETraceEvent and the content node).
					// To clone such behavior, I took this approach.
					w.WriteRaw (XPathNavigatorToString ((XPathNavigator) o));
				else if (o != null)
					w.WriteString (o.ToString ());
				if (wrapData)
					w.WriteEndElement ();
			}
			w.WriteEndElement ();

			w.WriteEndElement ();

			w.Flush (); // for XmlWriter
			Flush (); // for TextWriter
		}

		static readonly XmlWriterSettings xml_writer_settings = new XmlWriterSettings () { OmitXmlDeclaration = true };

		// I avoided OuterXml which includes indentation.
		string XPathNavigatorToString (XPathNavigator nav)
		{
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, xml_writer_settings))
				nav.WriteSubtree (xw);
			return sw.ToString ();
		}
	}
}

#endif
