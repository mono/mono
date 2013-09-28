// ConsoleLogger.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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
using System.Globalization;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Logging
{
	public class ConsoleLogger : INodeLogger
	{
		public ConsoleLogger ()
			: this (LoggerVerbosity.Normal)
		{
		}

		public ConsoleLogger (LoggerVerbosity verbosity)
			: this (verbosity, message => Console.WriteLine (message), color => Console.ForegroundColor = color, Console.ResetColor)
		{
		}

		public ConsoleLogger (LoggerVerbosity verbosity, WriteHandler write, ColorSetter colorSet, ColorResetter colorReset)
		{
			if (write == null)
				throw new ArgumentNullException ("write");
			if (colorSet == null)
				throw new ArgumentNullException ("colorSet");
			if (colorReset == null)
				throw new ArgumentNullException ("colorReset");
			Verbosity = verbosity;
			this.write = write;
			set_color = colorSet;
			reset_color = colorReset;
		}

		WriteHandler write;
		ColorSetter set_color;
		ColorResetter reset_color;

		#region INodeLogger implementation

		public virtual void Initialize (IEventSource eventSource, int nodeCount)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region ILogger implementation

		public virtual void Initialize (IEventSource eventSource)
		{
			throw new NotImplementedException ();
		}

		public virtual void Shutdown ()
		{
			throw new NotImplementedException ();
		}

		public string Parameters { get; set; }

		public LoggerVerbosity Verbosity { get; set; }

		public bool ShowSummary { get; set; }

		public bool SkipProjectStartedText { get; set; }
		
		public WriteHandler WriteHandler {
			get { return write; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				write = value;
			}
		}

		#endregion

		public void ApplyParameter (string parameterName, string parameterValue)
		{
			throw new NotImplementedException ();
		}

		DateTime build_started;

		public void BuildFinishedHandler (object sender, BuildFinishedEventArgs e)
		{
			if (Verbosity == LoggerVerbosity.Quiet || Verbosity == LoggerVerbosity.Minimal)
				return;

			set_color (ConsoleColor.White);
			write (e.Message);
			write ("");
			write ("");
			write ("");
			// .NET doesn't care if BuildStarted is actually invoked.
			write (string.Format ("Time Elapsed {0}", (e.Timestamp - build_started).ToString ("hh\\:mm\\:ss\\.ff")));
			write ("");
			reset_color ();
		}

		public void BuildStartedHandler (object sender, BuildStartedEventArgs e)
		{
			if (Verbosity == LoggerVerbosity.Quiet || Verbosity == LoggerVerbosity.Minimal)
				return;

			build_started = e.Timestamp;
			set_color (ConsoleColor.White);
			write (string.Format ("Build started {0}.", e.Timestamp.ToString ("yyyy/MM/dd HH:mm:ss")));
			write ("");
			reset_color ();
		}

		public void CustomEventHandler (object sender, CustomBuildEventArgs e)
		{
			// nothing happens.
		}

		string GetLocation (int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber)
		{
			string line = null, col = null;
			if (lineNumber > 0) {
				if (endLineNumber > 0)
					line = string.Format ("{0}-{1}", lineNumber, endLineNumber);
				else
					line = lineNumber.ToString ();
			}
			if (columnNumber > 0) {
				if (endColumnNumber > 0)
					col = string.Format ("{0}-{1}", columnNumber, endColumnNumber);
				else
					col = columnNumber.ToString ();
			}
			string loc = line != null ? line + (col != null ? "," + col : null) : null;
			return string.IsNullOrEmpty (loc) ? string.Empty : '(' + loc + ')';
		}
		
		public void ErrorHandler (object sender, BuildErrorEventArgs e)
		{
			if (Verbosity == LoggerVerbosity.Quiet)
				return;

			set_color (ConsoleColor.Red);
			string loc = GetLocation (e.LineNumber, e.ColumnNumber, e.EndLineNumber, e.EndColumnNumber);
			write (string.Format ("{0}{1} : {2} error {3}: {4}", e.File, loc, e.Subcategory, e.Code, e.Message));
			write ("");
			reset_color ();
		}
		
		public void MessageHandler (object sender, BuildMessageEventArgs e)
		{
			switch (e.Importance) {
			case MessageImportance.High:
				set_color (ConsoleColor.White);
				break;
			case MessageImportance.Low:
			case MessageImportance.Normal:
				set_color (ConsoleColor.Gray);
				break;
			}
			write (e.Message);
			reset_color ();
		}

		public void ProjectFinishedHandler (object sender, ProjectFinishedEventArgs e)
		{
			if (Verbosity == LoggerVerbosity.Quiet || Verbosity == LoggerVerbosity.Minimal)
				return;

			set_color (ConsoleColor.Cyan);
			write (e.Message);
			write ("");
			write ("");
			write ("");
			reset_color ();
		}
		
		public void ProjectStartedHandler (object sender, ProjectStartedEventArgs e)
		{
			if (Verbosity == LoggerVerbosity.Quiet || Verbosity == LoggerVerbosity.Minimal)
				return;

			set_color (ConsoleColor.Cyan);
			write ("__________________________________________________");
			write ("");
			write (string.Format ("Project \"{0}\" ({1} target(s)):", e.ProjectFile, e.TargetNames));
			write ("");
			write ("");
			write ("");
			reset_color ();
		}
		
		public void TargetFinishedHandler (object sender, TargetFinishedEventArgs e)
		{
			if (Verbosity != LoggerVerbosity.Detailed && Verbosity != LoggerVerbosity.Diagnostic)
				return;

			set_color (ConsoleColor.Cyan);
			write (e.Message);
			write ("");
			write ("");
			write ("");
			reset_color ();
		}

		public void TargetStartedHandler (object sender, TargetStartedEventArgs e)
		{
			if (Verbosity != LoggerVerbosity.Detailed && Verbosity != LoggerVerbosity.Diagnostic)
				return;

			string message = Verbosity == LoggerVerbosity.Detailed ?
				string.Format ("Target \"{0}\":", e.TargetName) :
				string.Format ("Target \"{0}\" in file \"{1}\":", e.TargetName, e.TargetFile);

			set_color (ConsoleColor.Cyan);
			write (message);
			write ("");
			reset_color ();
		}
		
		public void TaskFinishedHandler (object sender, TaskFinishedEventArgs e)
		{
			if (Verbosity != LoggerVerbosity.Detailed && Verbosity != LoggerVerbosity.Diagnostic)
				return;

			set_color (ConsoleColor.Cyan);
			write ("  " + e.Message);
			write ("");
			reset_color ();
		}

		public void TaskStartedHandler (object sender, TaskStartedEventArgs e)
		{
			if (Verbosity != LoggerVerbosity.Detailed && Verbosity != LoggerVerbosity.Diagnostic)
				return;

			set_color (ConsoleColor.Cyan);
			write ("  " + e.Message);
			write ("");
			reset_color ();
		}
		
		public void WarningHandler (object sender, BuildWarningEventArgs e)
		{
			if (Verbosity == LoggerVerbosity.Quiet)
				return;

			set_color (ConsoleColor.Yellow);
			string loc = GetLocation (e.LineNumber, e.ColumnNumber, e.EndLineNumber, e.EndColumnNumber);
			write (string.Format ("{0}{1} : {2} warning {3}: {4}", e.File, loc, e.Subcategory, e.Code, e.Message));
			write ("");
			reset_color ();
		}
	}
}

