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
using System.Text;
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
			StringBuilder sb = new StringBuilder ();

			sb.Append (args.File);
			AppendLineNumbers (sb, args.LineNumber, args.ColumnNumber, args.EndLineNumber, args.EndColumnNumber);
			sb.Append (": ");
			sb.Append (args.Subcategory);
			sb.Append (" error ");
			sb.Append (args.Code);
			sb.Append (": ");
			sb.Append (args.Message);

			return sb.ToString ();
		}

		public virtual string FormatWarningEvent (BuildWarningEventArgs args)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (args.File);
			AppendLineNumbers (sb, args.LineNumber, args.ColumnNumber, args.EndLineNumber, args.EndColumnNumber);
			sb.Append (": ");
			sb.Append (args.Subcategory);
			sb.Append (" warning ");
			sb.Append (args.Code);
			sb.Append (": ");
			sb.Append (args.Message);

			return sb.ToString ();
		}

		void AppendLineNumbers (StringBuilder sb, int line, int column, int endLine, int endColumn)
		{
			if (line != 0 && column != 0 && endLine != 0 && endColumn != 0) {
				sb.AppendFormat ("({0},{1},{2},{3})", line, column, endLine, endColumn);
			} else if (line != 0 && column != 0) {
				sb.AppendFormat ("({0},{1})", line, column);
			} else if (line != 0) {
				sb.AppendFormat ("({0})", line);
			} else {
				sb.Append (" ");
			}
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
