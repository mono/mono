//
// System.Diagnostics.DefaultTraceListener.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
//
using System;

namespace System.Diagnostics
{
	/// <summary>
	/// Provides the default output methods and behavior for tracing.
	/// </summary>
	/// <remarks>
	/// Since there is no debugging API ala Win32 on Mono, <see cref="System.Console.Out">
	/// Console.Out</see> is being used as the default output method.
	/// </remarks>
	public class DefaultTraceListener : TraceListener
	{	
		private string logFileName;
		
		public DefaultTraceListener() : base("Default")
		{
			logFileName = "";
		}
		
		/// <summary>
		/// Gets or sets name of a log file to write trace or debug messages to.
		/// </summary>
		/// <value>
		/// The name of a log file to write trace or debug messages to.
		/// </value>
		public String LogFileName
		{
			get
			{
				return logFileName;
			}
			set
			{
				logFileName = value;
			}
		}
		
		/// <summary>
		/// Emits or displays a message and a stack trace for an assertion that 
		/// always fails.
		/// </summary>
		/// <param name="message">
		/// The message to emit or display.
		/// </param>
		public override void Fail(string message)
		{
			Console.Out.WriteLine(message);
			new StackTrace().ToString();
		}
		
		/// <summary>
		/// Emits or displays detailed messages and a stack trace
		/// for an assertion that always fails.
		/// </summary>
		/// <param name="message">
		/// The message to emit or display
		/// </param>
		/// <param name="detailMessage">
		/// The detailed message to emit or display.
		/// </param>
		public override void Fail(string message, string detailMessage)
		{
			Console.Out.WriteLine(message + ": " + detailMessage);
			new StackTrace().ToString();
		}
		
		/// <summary>
		/// Writes the output to the Console
		/// </summary>
		/// <param name="message">
		/// The message to write
		/// </param>
		public override void Write(string message)
		{
			Console.Out.Write(message);
		}
		
		/// <summary>
		/// Writes the output to the Console, followed by a newline
		/// </summary>
		/// <param name="message">
		/// The message to write
		/// </param>
		public override void WriteLine(string message)
		{
			Console.Out.WriteLine(message);
		}
	}
}
