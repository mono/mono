//
// LogExtensions.cs: Extension methods for logging on Engine
//
// Author:
//	Ankit Jain (jankit@novell.com)
//
// Copyright 2010 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine
{
	static class LogExtensions
	{
		public static string FormatString (string unformatted,
						   params object[] args)
		{
			if (unformatted == null)
				throw new ArgumentNullException ("unformatted");
		
			if (args == null || args.Length == 0)
				return unformatted;
			else
				return String.Format (unformatted, args);
		}

		public static void LogError (this Engine engine, string message,
				     params object[] messageArgs)
		{
			engine.LogError (null, message, messageArgs);
		}

		public static void LogError (this Engine engine, string filename, string message,
				     params object[] messageArgs)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
				
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, filename, 0, 0, 0, 0, FormatString (message, messageArgs),
				null, null);
			engine.EventSource.FireErrorRaised (engine, beea);
		}

		public static void LogError (this Engine engine, string subcategory, string errorCode,
				      string helpKeyword, string file,
				      int lineNumber, int columnNumber,
				      int endLineNumber, int endColumnNumber,
				      string message,
				      params object[] messageArgs)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, errorCode, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				FormatString (message, messageArgs), helpKeyword /*it's helpKeyword*/,
				null /*it's senderName*/);

			engine.EventSource.FireErrorRaised (engine, beea);
		}

		public static void LogErrorFromException (this Engine engine, Exception e)
		{
			LogErrorFromException (engine, e, true);
		}

		public static void LogErrorFromException (this Engine engine, Exception e,
						   bool showStackTrace)
		{
			LogErrorFromException (engine, e, showStackTrace, true, String.Empty);
		}

		[MonoTODO ("Arguments @showDetail and @file are not honored")]
		public static void LogErrorFromException (this Engine engine, Exception e,
						   bool showStackTrace, bool showDetail, string file)
		{
			if (e == null)
				throw new ArgumentNullException ("e");
		
			StringBuilder sb = new StringBuilder ();
			sb.Append (e.Message);
			if (showStackTrace == true)
				sb.Append (e.StackTrace);
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, null, 0, 0, 0, 0, sb.ToString (),
				e.HelpLink, e.Source);
			engine.EventSource.FireErrorRaised (engine, beea);
		}

		public static void LogMessage (this Engine engine, string message,
				       params object[] messageArgs)
		{
			LogMessage (engine, MessageImportance.Normal, message, messageArgs); 
		}

		public static void LogMessage (this Engine engine, MessageImportance importance,
					string message,
					params object[] messageArgs)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
		
			LogMessageFromText (engine, FormatString (message, messageArgs), importance);
		}

		public static bool LogMessageFromText (this Engine engine, string lineOfText,
						MessageImportance importance)
		{
			if (lineOfText == null)
				throw new ArgumentNullException ("lineOfText");

			BuildMessageEventArgs bmea = new BuildMessageEventArgs (
				lineOfText, null,
				null, importance);
			
			engine.EventSource.FireMessageRaised (engine, bmea);

			return true;
		}

		public static void LogWarning (this Engine engine, string message,
				       params object[] messageArgs)
		{
			// FIXME: what about all the parameters?
			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				null, null, null, 0, 0, 0, 0, FormatString (message, messageArgs),
				null, null);
			engine.EventSource.FireWarningRaised (engine, bwea);
		}

		public static void LogWarning (this Engine engine, string subcategory, string warningCode,
					string helpKeyword, string file,
					int lineNumber, int columnNumber,
					int endLineNumber, int endColumnNumber,
					string message,
					params object[] messageArgs)
		{
			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				subcategory, warningCode, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				FormatString (message, messageArgs), helpKeyword, null);
			engine.EventSource.FireWarningRaised (engine, bwea);
		}

		public static void LogWarningFromException (this Engine engine, Exception e)
		{
			LogWarningFromException (engine, e, false);
		}

		public static void LogWarningFromException (this Engine engine, Exception e,
						     bool showStackTrace)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (e.Message);
			if (showStackTrace)
				sb.Append (e.StackTrace);
			LogWarning (engine, null, null, null, null, 0, 0, 0, 0,
				sb.ToString (), null);
		}
	}
}

#endif
