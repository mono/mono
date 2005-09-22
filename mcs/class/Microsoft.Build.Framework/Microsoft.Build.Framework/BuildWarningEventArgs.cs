//
// BuildWarningEventArgs.cs: Provides data for the Microsoft.Build.Framework.
// IEventSource.WariningRaised event.
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

namespace Microsoft.Build.Framework {
	[Serializable]
	public class BuildWarningEventArgs : BuildEventArgs {
	
		string	subcategory;
		string	code;
		string	file;
		int	lineNumber;
		int	columnNumber;
		int	endLineNumber;
		int	endColumnNumber;
		
		protected BuildWarningEventArgs ()
		{
		}

		public BuildWarningEventArgs (string subcategory, string code,
					      string file, int lineNumber,
					      int columnNumber,
					      int endLineNumber,
					      int endColumnNumber,
					      string message,
					      string helpKeyword,
					      string senderName)
			: base (message, helpKeyword, senderName)
		{
			this.subcategory = subcategory;
			this.code = code;
			this.file = file;
			this.lineNumber = lineNumber;
			this.columnNumber = columnNumber;
			this.endLineNumber = endLineNumber;
			this.endColumnNumber = endColumnNumber;
		}

		public string Code {
			get {
				return code;
			}
		}

		public int ColumnNumber {
			get {
				return columnNumber;
			}
		}

		public int EndColumnNumber {
			get {
				return endColumnNumber;
			}
		}

		public int EndLineNumber {
			get {
				return endLineNumber;
			}
		}

		public string File {
			get {
				return file;
			}
		}

		public int LineNumber {
			get {
				return lineNumber;
			}
		}

		public string Subcategory {
			get {
				return subcategory;
			}
		}
	}
}

#endif