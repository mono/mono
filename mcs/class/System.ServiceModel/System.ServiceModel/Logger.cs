//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Diagnostics;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace System.ServiceModel
{
	internal enum MessageLogSourceKind
	{
		TransportSend,
		TransportReceive,
		ServiceLevelReceiveDatagram,
		ServiceLevelSendDatagram,
		// more, maybe for clients?
	}

	internal static class Logger
	{
#if NET_2_1
		enum TraceEventType // dummy
		{
			Critical,
			Error,
			Warning,
			Information,
			Verbose,
			Start,
			Stop,
			Suspend,
			Resume,
			Transfer
		}
#endif

		const string xmlns = "http://schemas.microsoft.com/2004/06/ServiceModel/Management/MessageTrace";
		static MessageLoggingSettings settings = new MessageLoggingSettings ();
		static int event_id;
		static TextWriter log_writer;
		static XmlWriter xml_writer;
#if !NET_2_1
		static readonly TraceSource source = new TraceSource ("System.ServiceModel");
		static readonly TraceSource message_source = new TraceSource ("System.ServiceModel.MessageLogging");
#endif

		static Logger ()
		{
			var env =
				Environment.GetEnvironmentVariable ("MOON_WCF_TRACE") ??
				Environment.GetEnvironmentVariable ("MONO_WCF_TRACE");

			switch (env) {
			case "stdout":
				log_writer = Console.Out;
				break;
			case "stderr":
				log_writer = Console.Error;
				break;
#if !NET_2_1
			default:
				try {
					if (!String.IsNullOrEmpty (env))
						log_writer = File.CreateText (env);
				} catch (Exception ex) {
					Console.Error.WriteLine ("WARNING: WCF trace environment variable points to non-creatable file name: " + env);
				}
				break;
#endif
			}

			if (log_writer != null)
				xml_writer = XmlWriter.Create (log_writer, new XmlWriterSettings () { OmitXmlDeclaration = true });

#if !NET_2_1
			message_source.Switch.Level = SourceLevels.Information;
#endif
		}

		#region logger methods

		public static void Critical (string message, params object [] args)
		{
			Log (TraceEventType.Critical, message, args);
		}

		public static void Error (string message, params object [] args)
		{
			Log (TraceEventType.Error, message, args);
		}
		
		public static void Warning (string message, params object [] args)
		{
			Log (TraceEventType.Warning, message, args);
		}
		
		public static void Info (string message, params object [] args)
		{
			Log (TraceEventType.Information, message, args);
		}
		
		public static void Verbose (string message, params object [] args)
		{
			Log (TraceEventType.Verbose, message, args);
		}
		
		// FIXME: do we need more?

		static void Log (TraceEventType eventType, string message, params object [] args)
		{
			if (log_writer != null) {
				lock (log_writer){
					event_id++;
#if NET_2_1
					log_writer.Write ("[{0}] ", event_id);
#endif
					TraceCore (TraceEventType.Information, event_id,
						false, Guid.Empty, // FIXME
						message, args);
					log_writer.WriteLine (message, args);
					log_writer.Flush ();
#if !NET_2_1
					source.TraceEvent (eventType, event_id, message, args);
#endif
				}
			}
		}
		
		#endregion
		
		#region message logging
		
		static readonly XmlWriterSettings xws = new XmlWriterSettings () { OmitXmlDeclaration = true };
		
		public static void LogMessage (MessageLogSourceKind sourceKind, ref Message msg, long maxMessageSize)
		{
			if (log_writer != null) {
				if (maxMessageSize > int.MaxValue)
					throw new ArgumentOutOfRangeException ("maxMessageSize");
				var mb = msg.CreateBufferedCopy ((int) maxMessageSize);
				msg = mb.CreateMessage ();
				LogMessage (new MessageLogTraceRecord (sourceKind, msg.GetType (), mb));
			}
		}
		
		public static void LogMessage (MessageLogTraceRecord log)
		{
			if (log_writer != null) {
				var sw = new StringWriter ();
#if NET_2_1
				var xw = XmlWriter.Create (sw, xws);
#else
				var doc = new XmlDocument ();
				var xw = doc.CreateNavigator ().AppendChild ();
#endif
				xw.WriteStartElement ("MessageLogTraceRecord", xmlns);
				xw.WriteStartAttribute ("Time");
				xw.WriteValue (log.Time);
				xw.WriteEndAttribute ();
				xw.WriteAttributeString ("Source", log.Source.ToString ());
				xw.WriteAttributeString ("Type", log.Type.FullName);
				var msg = log.Message.CreateMessage ();
				if (!msg.IsEmpty)
					msg.WriteMessage (xw);
				xw.WriteEndElement ();
				xw.Close ();

				event_id++;
				lock (log_writer){
#if NET_2_1
					log_writer.Write ("[{0}] ", event_id);

					TraceCore (TraceEventType.Information, event_id, /*FIXME*/false, /*FIXME*/Guid.Empty, sw);
#else
					TraceCore (TraceEventType.Information, event_id, /*FIXME*/false, /*FIXME*/Guid.Empty, doc.CreateNavigator ());

					message_source.TraceData (TraceEventType.Information, event_id, doc.CreateNavigator ());
#endif
					log_writer.Flush ();
				}
			}
		}

		#endregion

		#region XmlWriterTraceListener compatibility
		static void TraceCore (//TraceEventCache eventCache,
			/*string source,*/ TraceEventType eventType, int id,
			bool hasRelatedActivity, Guid relatedActivity,
			/*int level, bool wrapData, */params object [] data)
		{
			string source = "mono(dummy)";
			int level = 2;
			bool wrapData = true;
			var w = xml_writer;

			w.WriteStartElement ("E2ETraceEvent", e2e_ns);

			// <System>
			w.WriteStartElement ("System", sys_ns);
			w.WriteStartElement ("EventID", sys_ns);
			w.WriteString (XmlConvert.ToString (id));
			w.WriteEndElement ();
			w.WriteStartElement ("Type", sys_ns);
			// ...what to write here?
			w.WriteString ("3");
			w.WriteEndElement ();
			w.WriteStartElement ("SubType", sys_ns);
			// FIXME: it does not seem always to match eventType value ...
			w.WriteAttributeString ("Name", eventType.ToString ());
			// ...what to write here?
			w.WriteString ("0");
			w.WriteEndElement ();
			// ...what to write here?
			w.WriteStartElement ("Level", sys_ns);
			w.WriteString (level.ToString ());
			w.WriteEndElement ();
			w.WriteStartElement ("TimeCreated", sys_ns);
			w.WriteAttributeString ("SystemTime", XmlConvert.ToString (DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
			w.WriteEndElement ();
			w.WriteStartElement ("Source", sys_ns);
			w.WriteAttributeString ("Name", source);
			w.WriteEndElement ();
			w.WriteStartElement ("Correlation", sys_ns);
			w.WriteAttributeString ("ActivityID", String.Concat ("{", Guid.Empty, "}"));
			w.WriteEndElement ();
			w.WriteStartElement ("Execution", sys_ns);
			w.WriteAttributeString ("ProcessName", "mono (dummy)");
			w.WriteAttributeString ("ProcessID", "0");
			w.WriteAttributeString ("ThreadID", Thread.CurrentThread.ManagedThreadId.ToString ());
			w.WriteEndElement ();
			w.WriteStartElement ("Channel", sys_ns);
			// ...what to write here?
			w.WriteEndElement ();
			w.WriteStartElement ("Computer");
			w.WriteString ("localhost(dummy)");
			w.WriteEndElement ();

			w.WriteEndElement ();

			// <ApplicationData>
			w.WriteStartElement ("ApplicationData", e2e_ns);
			w.WriteStartElement ("TraceData", e2e_ns);
			foreach (object o in data) {
				if (wrapData)
					w.WriteStartElement ("DataItem", e2e_ns);
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

			w.WriteEndElement ();

			w.Flush (); // for XmlWriter
			log_writer.WriteLine ();
			log_writer.Flush (); // for TextWriter
		}

		static readonly XmlWriterSettings xml_writer_settings = new XmlWriterSettings () { OmitXmlDeclaration = true };

		// I avoided OuterXml which includes indentation.
		static string XPathNavigatorToString (XPathNavigator nav)
		{
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, xml_writer_settings))
				nav.WriteSubtree (xw);
			return sw.ToString ();
		}

		static readonly string e2e_ns = "http://schemas.microsoft.com/2004/06/E2ETraceEvent";
		static readonly string sys_ns = "http://schemas.microsoft.com/2004/06/windows/eventlog/system";
		#endregion
	}
}
