//
// System.Diagnostics.TraceImpl.cs
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
using System.Diagnostics;
using System.Configuration;

namespace System.Diagnostics {

	internal class TraceImpl {

		private static object lock_ = new object ();

		private static bool autoFlush;

		[ThreadStatic]
		private static int indentLevel = 0;

		[ThreadStatic]
		private static int indentSize;

		static TraceImpl ()
		{
			// defaults
			autoFlush = false;
			indentLevel = 0;
			indentSize = 4;
		}

		private TraceImpl ()
		{
		}

		public static bool AutoFlush {
			get {return autoFlush;}
			set {autoFlush = value;}
		}

		public static int IndentLevel {
			get {return indentLevel;}
			set {
				indentLevel = value;

				// Don't need to lock for threadsafety as 
				// TraceListener.IndentLevel is [ThreadStatic]
				foreach (TraceListener t in Listeners) {
					t.IndentLevel = indentLevel;
				}
			}
		}

		public static int IndentSize {
			get {return indentSize;}
			set {
				indentSize = value;

				// Don't need to lock for threadsafety as 
				// TraceListener.IndentSize is [ThreadStatic]
				foreach (TraceListener t in Listeners) {
					t.IndentSize = indentSize;
				}
			}
		}

		private static TraceListenerCollection listeners;

		public static TraceListenerCollection Listeners {
			get {
				InitOnce ();

				return listeners;
			}
		}

		// Initialize the world.
		//
		// This logically belongs in the static constructor (as it only needs
		// to be done once), except for one thing: if the .config file has a
		// syntax error, .NET throws a ConfigurationException.  If we read the
		// .config file in the static ctor, we throw a ConfigurationException
		// from the static ctor, which results in a TypeLoadException.  Oops.
		// Reading the .config file here will allow the static ctor to
		// complete successfully, allowing us to throw a normal
		// ConfigurationException should the .config file contain an error.
		//
		// There are also some ordering issues.
		//
		// The DiagnosticsConfigurationHandler assumes that the TraceImpl.Listeners
		// collection exists (so it can initialize the DefaultTraceListener and
		// add/remove existing listeners).
		private static void InitOnce ()
		{
			lock (lock_) {
				if (listeners == null) {
					listeners = new TraceListenerCollection ();

					// Read in the .config file and get the ball rolling...
					System.Collections.IDictionary d = DiagnosticsConfiguration.Settings;

					// remove warning about unused temporary
					d = d;
				}
			}
		}

		// FIXME: According to MSDN, this method should display a dialog box
		[MonoTODO]
		public static void Assert (bool condition)
		{
			if (!condition)
				Fail (new StackTrace().ToString());
		}

		// FIXME: According to MSDN, this method should display a dialog box
		[MonoTODO]
		public static void Assert (bool condition, string message)
		{
			if (!condition)
				Fail (message);
		}

		// FIXME: According to MSDN, this method should display a dialog box
		[MonoTODO]
		public static void Assert (bool condition, string message, 
			string detailMessage)
		{
			if (!condition)
				Fail (message, detailMessage);
		}

		public static void Close ()
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Close ();
				}
			}
		}

		// FIXME: From testing .NET, this method should display a dialog
		[MonoTODO]
		public static void Fail (string message)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Fail (message);
				}
			}
		}

		// FIXME: From testing .NET, this method should display a dialog
		[MonoTODO]
		public static void Fail (string message, string detailMessage)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Fail (message, detailMessage);
				}
			}
		}

		public static void Flush ()
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners){
					listener.Flush ();
				}
			}
		}

		public static void Indent ()
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.IndentLevel++;
				}
			}
		}

		public static void Unindent ()
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.IndentLevel--;
				}
			}
		}

		public static void Write (object value)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (value);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void Write (string message)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (message);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void Write (object value, string category)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (value, category);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void Write (string message, string category)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (message, category);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteIf (bool condition, object value)
		{
			if (condition)
				Write (value);
		}

		public static void WriteIf (bool condition, string message)
		{
			if (condition)
				Write (message);
		}

		public static void WriteIf (bool condition, object value, 
			string category)
		{
			if (condition)
				Write (value, category);
		}

		public static void WriteIf (bool condition, string message, 
			string category)
		{
			if (condition)
				Write (message, category);
		}

		public static void WriteLine (object value)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (value);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLine (string message)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (message);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLine (object value, string category)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (value, category);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLine (string message, string category)
		{
			lock (lock_) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (message, category);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLineIf (bool condition, object value)
		{
			if (condition)
				WriteLine (value);
		}

		public static void WriteLineIf (bool condition, string message)
		{
			if (condition)
				WriteLine (message);
		}

		public static void WriteLineIf (bool condition, object value, 
			string category)
		{
			if (condition)
				WriteLine (value, category);
		}

		public static void WriteLineIf (bool condition, string message, 
			string category)
		{
			if (condition)
				WriteLine (message, category);
		}
	}
}

