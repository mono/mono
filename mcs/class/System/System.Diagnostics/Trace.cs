//
// System.Diagnostics.Trace.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	public sealed class Trace {

		private Trace () {}

		public static bool AutoFlush {
			get {return TraceImpl.AutoFlush;}
			set {TraceImpl.AutoFlush = value;}
		}

		public static int IndentLevel {
			get {return TraceImpl.IndentLevel;}
			set {TraceImpl.IndentLevel = value;}
		}

		public static int IndentSize {
			get {return TraceImpl.IndentSize;}
			set {TraceImpl.IndentSize = value;}
		}

		public static TraceListenerCollection Listeners {
			get {return TraceImpl.Listeners;}
		}

		[Conditional("TRACE")]
		public static void Assert (bool condition)
		{
			TraceImpl.Assert (condition);
		}

		[Conditional("TRACE")]
		public static void Assert (bool condition, string message)
		{
			TraceImpl.Assert (condition, message);
		}

		[Conditional("TRACE")]
		public static void Assert (bool condition, string message, 
			string detailMessage)
		{
			TraceImpl.Assert (condition, message, detailMessage);
		}

		[Conditional("TRACE")]
		public static void Close ()
		{
			TraceImpl.Close ();
		}

		[Conditional("TRACE")]
		public static void Fail (string message)
		{
			TraceImpl.Fail (message);
		}

		[Conditional("TRACE")]
		public static void Fail (string message, string detailMessage)
		{
			TraceImpl.Fail (message, detailMessage);
		}

		[Conditional("TRACE")]
		public static void Flush ()
		{
			TraceImpl.Flush ();
		}

		[Conditional("TRACE")]
		public static void Indent ()
		{
			TraceImpl.Indent ();
		}

		[Conditional("TRACE")]
		public static void Unindent ()
		{
			TraceImpl.Unindent ();
		}

		[Conditional("TRACE")]
		public static void Write (object value)
		{
			TraceImpl.Write (value);
		}

		[Conditional("TRACE")]
		public static void Write (string message)
		{
			TraceImpl.Write (message);
		}

		[Conditional("TRACE")]
		public static void Write (object value, string category)
		{
			TraceImpl.Write (value, category);
		}

		[Conditional("TRACE")]
		public static void Write (string message, string category)
		{
			TraceImpl.Write (message, category);
		}

		[Conditional("TRACE")]
		public static void WriteIf (bool condition, object value)
		{
			TraceImpl.WriteIf (condition, value);
		}

		[Conditional("TRACE")]
		public static void WriteIf (bool condition, string message)
		{
			TraceImpl.WriteIf (condition, message);
		}

		[Conditional("TRACE")]
		public static void WriteIf (bool condition, object value, 
			string category)
		{
			TraceImpl.WriteIf (condition, value, category);
		}

		[Conditional("TRACE")]
		public static void WriteIf (bool condition, string message, 
			string category)
		{
			TraceImpl.WriteIf (condition, message, category);
		}

		[Conditional("TRACE")]
		public static void WriteLine (object value)
		{
			TraceImpl.WriteLine (value);
		}

		[Conditional("TRACE")]
		public static void WriteLine (string message)
		{
			TraceImpl.WriteLine (message);
		}

		[Conditional("TRACE")]
		public static void WriteLine (object value, string category)
		{
			TraceImpl.WriteLine (value, category);
		}

		[Conditional("TRACE")]
		public static void WriteLine (string message, string category)
		{
			TraceImpl.WriteLine (message, category);
		}

		[Conditional("TRACE")]
		public static void WriteLineIf (bool condition, object value)
		{
			TraceImpl.WriteLineIf (condition, value);
		}

		[Conditional("TRACE")]
		public static void WriteLineIf (bool condition, string message)
		{
			TraceImpl.WriteLineIf (condition, message);
		}

		[Conditional("TRACE")]
		public static void WriteLineIf (bool condition, object value, 
			string category)
		{
			TraceImpl.WriteLineIf (condition, value, category);
		}

		[Conditional("TRACE")]
		public static void WriteLineIf (bool condition, string message, 
			string category)
		{
			TraceImpl.WriteLineIf (condition, message, category);
		}
	}
}

