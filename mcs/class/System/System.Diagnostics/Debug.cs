//
// System.Diagnostics.Debug.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original
// implementation.
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	/// <summary>
	/// Provides a set of methods to help debug code
	/// </summary>
	public sealed class Debug {

		private Debug () {}

		/// <summary>
		/// Gets or sets value indicating whether Flush should
		/// be called on the listeners.
		/// </summary>
		public static bool AutoFlush {
			get {return TraceImpl.AutoFlush;}
			set {TraceImpl.AutoFlush = value;}
		}

		/// <summary>
		/// Gets or sets indent level
		/// </summary>
		public static int IndentLevel {
			get {return TraceImpl.IndentLevel;}
			set {TraceImpl.IndentLevel = value;}
		}

		/// <summary>
		/// The number of spaces in an indent.
		/// </summary>
		public static int IndentSize {
			get {return TraceImpl.IndentSize;}
			set {TraceImpl.IndentSize = value;}
		}

		/// <summary>
		/// Returns the listeners collection
		/// </summary>
		public static TraceListenerCollection Listeners {
			get {return TraceImpl.Listeners;}
		}

		/// <summary>
		/// Checks for a condition, and prints a stack trace
		/// if the condition is false.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Assert (bool condition)
		{
			TraceImpl.Assert (condition);
		}

		/// <summary>
		/// Checks for a condition, and displays a message if the condition
		/// is false.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Assert (bool condition, string message)
		{
			TraceImpl.Assert (condition, message);
		}

		/// <summary>
		/// Checks for a condtion, and displays a message and a detailed message
		/// string if the condition is false.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Assert (bool condition, string message, 
			string detailMessage)
		{
			TraceImpl.Assert (condition, message, detailMessage);
		}

		/// <summary>
		/// Closes the Debug buffer
		/// </summary>
		[Conditional("DEBUG")]
		public static void Close ()
		{
			TraceImpl.Close ();
		}

		/// <summary>
		/// Emits the specified error message.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Fail (string message)
		{
			TraceImpl.Fail (message);
		}

		/// <summary>
		/// Emits the specified error message and detailed error message.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Fail (string message, string detailMessage)
		{
			TraceImpl.Fail (message, detailMessage);
		}

		/// <summary>
		/// Flushes the listeners
		/// </summary>
		[Conditional("DEBUG")]
		public static void Flush ()
		{
			TraceImpl.Flush ();
		}

		/// <summary>
		/// Increments the indent level
		/// </summary>
		[Conditional("DEBUG")]
		public static void Indent ()
		{
			TraceImpl.Indent ();
		}

		/// <summary>
		/// Decrements the indent level
		/// </summary>
		[Conditional("DEBUG")]
		public static void Unindent ()
		{
			TraceImpl.Unindent ();
		}

		/// <summary>
		/// Writes the value of the specified object's ToString method
		/// to the listeners.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Write (object value)
		{
			TraceImpl.Write (value);
		}

		/// <summary>
		/// Writes the specified message to each listener in the Listeners 
		/// collection.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Write (string message)
		{
			TraceImpl.Write (message);
		}

		/// <summary>
		/// Writes the category name and value of the specified object's
		/// ToString method to each listener in the Listeners collection.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Write (object value, string category)
		{
			TraceImpl.Write (value, category);
		}

		/// <summary>
		/// Writes the category name and the specified message
		/// to each listener in the Listeners collection.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Write (string message, string category)
		{
			TraceImpl.Write (message, category);
		}

		/// <summary>
		/// Writes the value of the specified object's ToString method
		/// to each of the listeners if the condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, object value)
		{
			TraceImpl.WriteIf (condition, value);
		}

		/// <summary>
		/// Writes the specified message to each of the listeners
		/// if the specified condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, string message)
		{
			TraceImpl.WriteIf (condition, message);
		}

		/// <summary>
		/// Writes the value of the specified object's ToString message
		/// and category to each of the listeners if the condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, object value, 
			string category)
		{
			TraceImpl.WriteIf (condition, value, category);
		}

		/// <summary>
		/// Writes the category and specified message to each listener
		/// if the specified condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, string message, 
			string category)
		{
			TraceImpl.WriteIf (condition, message, category);
		}

		/// <summary>
		/// Writes the value of the object's ToString method,
		/// followed by a line terminator, to each listener.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLine (object value)
		{
			TraceImpl.WriteLine (value);
		}

		/// <summary>
		/// Writes the specified message, followed by a line terminator,
		/// to each listener.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLine (string message)
		{
			TraceImpl.WriteLine (message);
		}

		/// <summary>
		/// Writes the value of the specified object's ToString method,
		/// along with a category, followed by a line terminator, to each listener.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLine (object value, string category)
		{
			TraceImpl.WriteLine (value, category);
		}

		/// <summary>
		/// Writes the specified category and message, followed by a line 
		/// terminator, to each listener.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLine (string message, string category)
		{
			TraceImpl.WriteLine (message, category);
		}

		/// <summary>
		/// Writes the value of the object's ToString method
		/// to each listener if the specified condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, object value)
		{
			TraceImpl.WriteLineIf (condition, value);
		}

		/// <summary>
		/// Writes the specified message to each listener
		/// if the specified condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, string message)
		{
			TraceImpl.WriteLineIf (condition, message);
		}

		/// <summary>
		/// Writes the value of the object's ToString method, and a category
		/// to each listener if the specified condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, object value, 
			string category)
		{
			TraceImpl.WriteLineIf (condition, value, category);
		}

		/// <summary>
		/// Writes the specified category and message to each listener, followed 
		/// by a line terminator, if the specified condition is true.
		/// </summary>
		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, string message, 
			string category)
		{
			TraceImpl.WriteLineIf (condition, message, category);
		}
	}
}

