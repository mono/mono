//
// System.Diagnostics.TraceImpl.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
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

		private static TraceListenerCollection listeners = 
			new TraceListenerCollection ();

		public static TraceListenerCollection Listeners {
			get {return listeners;}
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

