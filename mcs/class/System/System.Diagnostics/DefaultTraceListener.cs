//
// System.Diagnostics.DefaultTraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original
// implementation.
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.IO;
using System.Runtime.InteropServices;

using System.Diagnostics;

namespace System.Diagnostics {

 	/// <summary>
 	/// Provides the default output methods and behavior for tracing.
 	/// </summary>
 	/// <remarks>
 	/// Since there is no debugging API ala Win32 on Mono, 
  /// <see cref="System.Console.Out">
 	/// Console.Out</see> is being used as the default output method.
	/// 
  /// <para>This needs help, as MSDN specifies that GUI widgets be used 
  /// for certain features.  The short-term solution is to just send output to
  /// OutputDebugString.</para>
 	/// </remarks>
	[ComVisible(false)]
	public class DefaultTraceListener : TraceListener {

		public DefaultTraceListener () : base ("Default")
		{
		}

		[MonoTODO]
		public bool AssertUiEnabled {
			get {return false;}
			set {/* ignore */}
		}

 		/// <summary>
 		/// Gets or sets name of a log file to write trace or debug messages to.
 		/// </summary>
 		/// <value>
 		/// The name of a log file to write trace or debug messages to.
 		/// </value>
		[MonoTODO]
		public string LogFileName {
			get {return "";}
			set {/* ignore */}
		}

 		/// <summary>
 		/// Emits or displays a message and a stack trace for an assertion that 
 		/// always fails.
 		/// </summary>
 		/// <param name="message">
 		/// The message to emit or display.
 		/// </param>
    public override void Fail (string message)
    {
      base.Fail (message);
      WriteLine (new StackTrace().ToString());
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
      base.Fail (message, detailMessage);
      WriteLine (new StackTrace().ToString());
 		}

		#if USE_NATIVE_WIN32_OUTPUT_DEBUG_STRING

      [DllImport ("kernel32.dll")]
      private extern static void OutputDebugString (string message);

		#else

      private static void OutputDebugString (string message)
      {
        Console.Write ("**ods** " + message);
      }

		#endif

 		/// <summary>
 		/// Writes the output to the Console
 		/// </summary>
 		/// <param name="message">
 		/// The message to write
 		/// </param>
		public override void Write (string message)
		{
			if (NeedIndent)
				WriteIndent ();
			OutputDebugString (message);
		}

 		/// <summary>
 		/// Writes the output to the Console, followed by a newline
 		/// </summary>
 		/// <param name="message">
 		/// The message to write
 		/// </param>
		public override void WriteLine (string message)
		{
			if (NeedIndent)
				WriteIndent ();
			OutputDebugString (message + Environment.NewLine);
			NeedIndent = true;
		}
	}
}

