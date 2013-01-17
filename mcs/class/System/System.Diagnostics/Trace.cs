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
using System.Diagnostics;
using System.Reflection;

namespace System.Diagnostics {

	public sealed class Trace {

		private Trace () {}

		[MonoNotSupported ("")]
		public static void Refresh ()
		{
			throw new NotImplementedException ();
		}

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

		public static CorrelationManager CorrelationManager {
			get { return TraceImpl.CorrelationManager; }
		}

		public static bool UseGlobalLock {
			get { return TraceImpl.UseGlobalLock; }
			set { TraceImpl.UseGlobalLock = value; }
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

		static void DoTrace (string kind, Assembly report, string message)
		{
			TraceImpl.WriteLine (String.Format ("{0} {1} : 0 : {2}", report.Location, kind, message));
		}
		
		[Conditional("TRACE")]
		public static void TraceError (string message)
		{
			DoTrace ("Error", Assembly.GetCallingAssembly (), message);
		}

		[Conditional("TRACE")]
		public static void TraceError (string message, params object [] args)
		{
			DoTrace ("Error", Assembly.GetCallingAssembly (), String.Format (message, args));
		}

		[Conditional("TRACE")]
		public static void TraceInformation (string message)
		{
			DoTrace ("Information", Assembly.GetCallingAssembly (), message);
		}

		[Conditional("TRACE")]
		public static void TraceInformation (string message, params object [] args)
		{
			DoTrace ("Information", Assembly.GetCallingAssembly (), String.Format (message, args));
		}

		[Conditional("TRACE")]
		public static void TraceWarning (string message)
		{
			DoTrace ("Warning", Assembly.GetCallingAssembly (), message);
		}

		[Conditional("TRACE")]
		public static void TraceWarning (string message, params object [] args)
		{
			DoTrace ("Warning", Assembly.GetCallingAssembly (), String.Format (message, args));
		}
	}
}


