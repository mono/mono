//
// Logger.cs: Logs warning and errors.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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
using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities
{
	public abstract class Logger : ILogger
	{
		string		parameters;
		LoggerVerbosity	verbosity;
	
		protected Logger ()
		{
		}

		public virtual string Parameters {
			get {
				return parameters;
			}
			set {
				parameters = value;
			}
		}

		public virtual LoggerVerbosity Verbosity {
			get {
				return verbosity;
			}
			set {
				verbosity = value;
			}
		}

		public virtual string FormatErrorEvent (BuildErrorEventArgs args)
		{
			return String.Format ("{0}({1},{2},{3},{4}): {5} error {6}: {7}",
				args.File, args.LineNumber, args.ColumnNumber, args.EndLineNumber, args.EndColumnNumber,
				args.Subcategory, args.Code, args.Message);
		}

		public virtual string FormatWarningEvent (BuildWarningEventArgs args)
		{
			return String.Format ("{0}({1},{2},{3},{4}): {5} warning {6}: {7}",
				args.File, args.LineNumber, args.ColumnNumber, args.EndLineNumber, args.EndColumnNumber,
				args.Subcategory, args.Code, args.Message);
		}

		public abstract void Initialize (IEventSource eventSource);

		public virtual void Shutdown ()
		{
		}
		
		[MonoTODO]
		public bool IsVerbosityAtLeast (LoggerVerbosity verbosity)
		{
			return (this.verbosity >= verbosity) ? true : false;
		}
	}
}

#endif