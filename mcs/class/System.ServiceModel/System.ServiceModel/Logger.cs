//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
using System.Xml;

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
#if !NET_2_1
		static readonly TraceSource source = new TraceSource ("System.ServiceModel");
		static readonly TraceSource message_source = new TraceSource ("System.ServiceModel.MessageLogging");

		static Logger ()
		{
			message_source.Switch.Level = SourceLevels.Information;
		}
#endif

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
#if NET_2_1
			Console.Error.Write ("[{0}]", event_id++);
			Console.Error.WriteLine (message, args);
#else
			source.TraceEvent (eventType, event_id++, message, args);
#endif
		}
		
		#endregion
		
		#region message logging
		
		static readonly XmlWriterSettings xws = new XmlWriterSettings () { OmitXmlDeclaration = true };
		
		public static void LogMessage (MessageLogSourceKind sourceKind, ref Message msg, int maxMessageSize)
		{
			var mb = msg.CreateBufferedCopy (maxMessageSize);
			msg = mb.CreateMessage ();
			LogMessage (new MessageLogTraceRecord (sourceKind, msg.GetType (), mb));
		}
		
		public static void LogMessage (MessageLogTraceRecord log)
		{
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
			log.Message.CreateMessage ().WriteMessage (xw);
			xw.WriteEndElement ();
			xw.Close ();
#if NET_2_1
			Console.Error.Write ("[{0}]", event_id++);
			Console.Error.WriteLine (sw);
#else
			message_source.TraceData (TraceEventType.Information, event_id++, doc.CreateNavigator ());
#endif
		}

		#endregion
	}
}
