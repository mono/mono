//
// System.Diagnostics.Debug.cs
//
// Author: John R. Hicks <angryjohn69@nc.rr.com>
//
// (C) 2002
//
using System;

namespace System.Diagnostics
{
	
	/// <summary>
	/// Provides a set of methods to help debug code
	/// </summary>
	public sealed class Debug
	{
		private static bool autoFlush;
		private static int indentLevel;
		private static int indentSize;
		private static TraceListenerCollection listeners;
		
		static Debug()
		{
			autoFlush = false;
			indentLevel = 0;
			indentSize = 4;
			listeners = new TraceListenerCollection();
		}
		
		/// <summary>
		/// Gets or sets value indicating whether Flush should
		/// be called on the listeners.
		/// </summary>
		public static bool AutoFlush
		{
			get
			{
				return autoFlush;
			}
			set
			{
				autoFlush = value;
			}
		}
		
		/// <summary>
		/// Gets or sets indent level
		/// </summary>
		public static int IndentLevel
		{
			get
			{
				return indentLevel;
			}
			set
			{
				indentLevel = value;
			}
		}
		
		/// <summary>
		/// The number of spaces in an indent.
		/// </summary>
		public static int IndentSize
		{
			get
			{
				return indentSize;
			}
			set
			{
				indentSize = value;
			}
		}
		
		/// <summary>
		/// Returns the listeners collection
		/// </summary>
		public static TraceListenerCollection Listeners
		{
			get
			{
				return listeners;
			}
		}
		
		/// <summary>
		/// Checks for a condition, and prints a stack trace
		/// if the condition is false.
		/// </summary>
		public static void Assert(bool condition)
		{
			if(!condition) {
				WriteLine(new StackTrace().ToString());		
			}
			
		}
		
		/// <summary>
		/// Checks for a condition, and displays a message if the condition
		/// is false.
		/// </summary>
		public static void Assert(bool condition, string message)
		{
			if(!condition) {
				WriteLine(message);		
				
			}
			
		}
		
		/// <summary>
		/// Checks for a condtion, and displays a message and a detailed message
		/// string if the condition is false.
		/// </summary>
		public static void Assert(bool condition, string message, string detailMessage)
		{
			if(!condition) {
				WriteLine(message);
				Indent();
				WriteLine(detailMessage);
				Unindent();
				
			}
		}
		
		/// <summary>
		/// Closes the Debug buffer
		/// </summary>
		public static void Close()
		{
			foreach(TraceListener l in listeners)
			{
				l.Close();
			}
		}
		
		/// <summary>
		/// Emits the specified error message.
		/// </summary>
		public static void Fail(string message)
		{
			WriteLine(message);
			
		}
		
		/// <summary>
		/// Emits the specified error message and detailed error message.
		/// </summary>
		public static void Fail(string message, string detailMessage)
		{
			WriteLine(message);
			Indent();
			WriteLine(detailMessage);
			Unindent();
			
		}
		
		/// <summary>
		/// Flushes the listeners
		/// </summary>
		public static void Flush()
		{
			foreach(TraceListener l in listeners)
			{
				l.Flush();
			}
		}
		
		/// <summary>
		/// Increments the indent level
		/// </summary>
		public static void Indent()
		{
			indentLevel++;	
		}
		
		/// <summary>
		/// Decrements the indent level
		/// </summary>
		public static void Unindent()
		{
			if(indentLevel == 0)
				return;
			else
				indentLevel--;
		}
		
		/// <summary>
		/// Writes the value of the specified object's ToString method
		/// to the listeners.
		/// </summary>
		public static void Write(object value)
		{
			foreach(TraceListener l in listeners)
			{
				l.Write(value.ToString());
			}
		}
		
		/// <summary>
		/// Writes the specified message to each listener in the Listeners collection.
		/// </summary>
		public static void Write(string message)
		{
			foreach(TraceListener l in listeners)
			{
				l.Write(message);
			}
		}
		
		/// <summary>
		/// Writes the category name and value of the specified object's
		/// ToString method to each listener in the Listeners collection.
		/// </summary>
		public static void Write(object value, string category)
		{
			foreach(TraceListener l in listeners)
			{
				l.Write("[" + category + "] " + value.ToString());
			}
		}
		
		/// <summary>
		/// Writes the category name and the specified message
		/// to each listener in the Listeners collection.
		/// </summary>
		public static void Write(string message, string category)
		{
			foreach(TraceListener l in listeners)
			{
				l.Write("[" + category + "] " + message);
			}
		}
		
		/// <summary>
		/// Writes the value of the specified object's ToString method
		/// to each of the listeners if the condition is true.
		/// </summary>
		public static void WriteIf(bool condition, object value)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.Write(value.ToString());
				}
			}
		}
		
		/// <summary>
		/// Writes the specified message to each of the listeners
		/// if the specified condition is true.
		/// </summary>
		public static void WriteIf(bool condition, string message)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.Write(message);
				}
			}
		}
		
		/// <summary>
		/// Writes the value of the specified object's ToString message
		/// and category to each of the listeners if the condition is true.
		/// </summary>
		public static void WriteIf(bool condition, object value, string category)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.Write("[" + category + "] " + value.ToString());
				}
			}
		}
		
		/// <summary>
		/// Writes the category and specified message to each listener
		/// if the specified condition is true.
		/// </summary>
		public static void WriteIf(bool condition, string message, string category)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.Write("[" + category + "] " + message);
				}
			}
			
		}
		
		/// <summary>
		/// Writes the value of the object's ToString method,
		/// followed by a line terminator, to each listener.
		/// </summary>
		public static void WriteLine(object value)
		{
			foreach(TraceListener l in listeners)
			{
				l.WriteLine(value.ToString());
			}
		}
		
		/// <summary>
		/// Writes the specified message, followed by a line terminator,
		/// to each listener.
		/// </summary>
		public static void WriteLine(string message)
		{
			foreach(TraceListener l in listeners)
			{
				l.WriteLine(message);
			}
		}
		
		/// <summary>
		/// Writes the value of the specified object's ToString method,
		/// along with a category, followed by a line terminator, to each listener.
		/// </summary>
		public static void WriteLine(object value, string category)
		{
			foreach(TraceListener l in listeners)
			{
				l.WriteLine("[" + category + "] " + value.ToString());
			}
		}
		
		/// <summary>
		/// Writes the specified category and message, followed by a line terminator,
		/// to each listener.
		/// </summary>
		public static void WriteLine(string message, string category)
		{
			foreach(TraceListener l in listeners)
			{
				l.WriteLine("[" + category + "] " + message);
			}
		}
		
		/// <summary>
		/// Writes the value of the object's ToString method
		/// to each listener if the specified condition is true.
		/// </summary>
		public static void WriteLineIf(bool condition, object value)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.WriteLine(value.ToString());
				}
			}
		}
		
		/// <summary>
		/// Writes the specified message to each listener
		/// if the specified condition is true.
		/// </summary>
		public static void WriteLineIf(bool condition, string message)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.WriteLine(message);
				}
			}
		}
		
		/// <summary>
		/// Writes the value of the object's ToString method, and a category
		/// to each listener if the specified condition is true.
		/// </summary>
		public static void WriteLineIf(bool condition, object value, string category)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.WriteLine("[" + category + "] " + value.ToString());
				}
			}
		}
		
		/// <summary>
		/// Writes the specified category and message to each listener, followed by a line
		/// terminator, if the specified condition is true.
		/// </summary>
		public static void WriteLineIf(bool condition, string message, string category)
		{
			if(condition)
			{
				foreach(TraceListener l in listeners)
				{
					l.WriteLine("[" + category + "] " + message);
				}
			}
			
		}
	}
}
