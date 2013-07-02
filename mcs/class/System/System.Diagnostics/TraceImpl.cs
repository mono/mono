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

#if !MOBILE
	internal class TraceImplSettings {
		public const string Key = ".__TraceInfoSettingsKey__.";

		public bool AutoFlush;
		//public int IndentLevel;
		public int IndentSize = 4;
		public TraceListenerCollection Listeners = new TraceListenerCollection (false);

		public TraceImplSettings ()
		{
			Listeners.Add (new DefaultTraceListener (), this);
		}
	}
#endif

	internal class TraceImpl {

#if !MOBILE
		private static object initLock = new object ();
#endif

		private static bool autoFlush;

#if TARGET_JVM
		static readonly LocalDataStoreSlot _indentLevelStore = System.Threading.Thread.AllocateDataSlot ();
		static readonly LocalDataStoreSlot _indentSizeStore = System.Threading.Thread.AllocateDataSlot ();

		private static int indentLevel {
			get {
				object o = System.Threading.Thread.GetData (_indentLevelStore);
				if (o == null)
					return 0;
				return (int) o;
			}
			set { System.Threading.Thread.SetData (_indentLevelStore, value); }
		}

		private static int indentSize {
			get {
				object o = System.Threading.Thread.GetData (_indentSizeStore);
				if (o == null)
					return 0;
				return (int) o;
			}
			set { System.Threading.Thread.SetData (_indentSizeStore, value); }
		}
#else
		[ThreadStatic]
		private static int indentLevel = 0;

		[ThreadStatic]
		private static int indentSize;
#endif

		private TraceImpl ()
		{
		}

#if MOBILE
		static TraceImpl ()
		{
			listeners = new TraceListenerCollection (true);
		}
#endif

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

		private static TraceListenerCollection listeners;

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

		static bool use_global_lock;
		static CorrelationManager correlation_manager = new CorrelationManager ();

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

		// FIXME: According to MSDN, this method should display a dialog box
		[MonoTODO]
		public static void Assert (bool condition)
		{
			if (!condition)
				Fail (new StackTrace(true).ToString());
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
}

