//
// System.Diagnostics.TraceImpl.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002, 2005 Jonathan Pryor
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
using System.Collections;
using System.Diagnostics;
using System.Configuration;
using System.Threading;

namespace System.Diagnostics {

#if !MOBILE || MOBILE_DESKTOP_HOST
	internal class TraceImplSettings {
		public const string Key = ".__TraceInfoSettingsKey__.";

	// Disable warning that AutoFlush is not used
#pragma warning disable 649
		public bool AutoFlush;
#pragma warning restore
		//public int IndentLevel;
		public int IndentSize = 4;
		public TraceListenerCollection Listeners = new TraceListenerCollection ();

		public TraceImplSettings ()
		{
			Listeners.Add (new DefaultTraceListener () { IndentSize = this.IndentSize });
		}
	}
#endif

/*
	static class TraceImpl {

#if !MOBILE
		private static object initLock = new object ();
#endif

		private static bool autoFlush;

		[ThreadStatic]
		private static int indentLevel;

		[ThreadStatic]
		private static int indentSize;

#if MOBILE
		static TraceListenerCollection listeners = new TraceListenerCollection (true);
#else
		static TraceListenerCollection listeners;
#endif

		static bool use_global_lock;
		static CorrelationManager correlation_manager = new CorrelationManager ();

		public static bool AutoFlush {
			get {
				InitOnce ();
				return autoFlush;
			}
			set {
				InitOnce ();
				autoFlush = value;
			}
		}

		public static int IndentLevel {
			get {
				InitOnce ();
				return indentLevel;
			}
			set {
				lock (ListenersSyncRoot) {
					indentLevel = value;

					foreach (TraceListener t in Listeners) {
						t.IndentLevel = indentLevel;
					}
				}
			}
		}

		public static int IndentSize {
			get {
				InitOnce ();
				return indentSize;
			}
			set {
				lock (ListenersSyncRoot) {
					indentSize = value;

					foreach (TraceListener t in Listeners) {
						t.IndentSize = indentSize;
					}
				}
			}
		}

		public static TraceListenerCollection Listeners {
			get {
				InitOnce ();

				return listeners;
			}
		}

		private static object ListenersSyncRoot {
			get {
				return ((ICollection) Listeners).SyncRoot;
			}
		}

		public static CorrelationManager CorrelationManager {
			get {
				InitOnce ();
		 		return correlation_manager;
			}
		}

		[MonoLimitation ("the property exists but it does nothing.")]
		public static bool UseGlobalLock {
			get {
				InitOnce ();
				return use_global_lock;
			}
			set {
				InitOnce ();
				use_global_lock = value;
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
		// DiagnosticsConfigurationHandler doesn't store values within TraceImpl,
		// but instead stores values it reads from the .config file within a
		// TraceImplSettings object (accessible via the TraceImplSettings.Key key
		// in the IDictionary returned).
		private static void InitOnce ()
		{
#if !MOBILE
			if (initLock != null) {
				lock (initLock) {
					if (listeners == null) {
						IDictionary       d = DiagnosticsConfiguration.Settings;
						TraceImplSettings s = (TraceImplSettings) d [TraceImplSettings.Key];

						d.Remove (TraceImplSettings.Key);

						autoFlush   = s.AutoFlush;
//						indentLevel = s.IndentLevel;
						indentSize  = s.IndentSize;
						listeners   = s.Listeners;
					}
				}
				initLock = null;
			}
#endif
		}

		public static void Assert (bool condition)
		{
			if (!condition)
				Fail ("");
		}

		public static void Assert (bool condition, string message)
		{
			if (!condition)
				Fail (message);
		}

		public static void Assert (bool condition, string message, 
			string detailMessage)
		{
			if (!condition)
				Fail (message, detailMessage);
		}

		public static void Close ()
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.Close ();
				}
			}
		}

		// FIXME: From testing .NET, this method should display a dialog
		//(it probably depends on the listener)p
		[MonoTODO]
		public static void Fail (string message)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.Fail (message);
				}
			}
		}

		// FIXME: From testing .NET, this method should display a dialog
		// (it probably depends on the listener)p
		[MonoTODO]
		public static void Fail (string message, string detailMessage)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.Fail (message, detailMessage);
				}
			}
		}

		public static void Flush ()
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners){
					listener.Flush ();
				}
			}
		}

		public static void Indent ()
		{
			IndentLevel ++;
		}

		public static void Unindent ()
		{
			IndentLevel --;
		}

		public static void Write (object value)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (value);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void Write (string message)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (message);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void Write (object value, string category)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.Write (value, category);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void Write (string message, string category)
		{
			lock (ListenersSyncRoot) {
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
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (value);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLine (string message)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (message);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLine (object value, string category)
		{
			lock (ListenersSyncRoot) {
				foreach (TraceListener listener in Listeners) {
					listener.WriteLine (value, category);

					if (AutoFlush)
						listener.Flush ();
				}
			}
		}

		public static void WriteLine (string message, string category)
		{
			lock (ListenersSyncRoot) {
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
*/
}

