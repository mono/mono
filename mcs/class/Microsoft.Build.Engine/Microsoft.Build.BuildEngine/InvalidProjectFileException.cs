//
// InvalidProjectFileException.cs:
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
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	[Serializable]
	public sealed class InvalidProjectFileException : Exception {
		
		string	baseMessage;
		int	columnNumber;
		int	endColumnNumber;
		string	errorCode;
		string	errorSubcategory;
		string	helpKeyword;
		int	lineNumber;
		int	endLineNumber;
		string	message;
		string	projectFile;
		
		public InvalidProjectFileException ()
			: base ("Invalid project file exception has occured")
		{
			this.message = "Invalid project file exception has occured";
		}

		public InvalidProjectFileException (string message)
			: base (message)
		{
			this.message = message;
		}

		public InvalidProjectFileException (string projectFile,
						    int lineNumber,
						    int columnNumber,
						    int endLineNumber,
						    int endColumnNumber,
						    string message,
						    string errorSubcategory,
						    string errorCode,
						    string helpKeyword)
			: base (message)
		{
			this.projectFile = projectFile;
			this.lineNumber = lineNumber;
			this.columnNumber = columnNumber;
			this.endLineNumber = endLineNumber;
			this.endColumnNumber = endColumnNumber;
			this.message = message;
			this.errorSubcategory = errorSubcategory;
			this.errorCode = errorCode;
			this.helpKeyword = helpKeyword;
		}

		public InvalidProjectFileException (string message,
						    Exception innerException)
			: base (message, innerException)
		{
		}

		public InvalidProjectFileException (XmlNode xmlNode,
						    string message,
						    string errorSubcategory,
						    string errorCode,
						    string helpKeyword)
			: base (message)
		{
			this.message = message;
			this.errorSubcategory = errorSubcategory;
			this.errorCode = errorCode;
			this.helpKeyword = helpKeyword;
		}

		public override void GetObjectData (SerializationInfo info,
						    StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("BaseMessage", baseMessage);
			info.AddValue ("ColumnNumber", columnNumber);
			info.AddValue ("EndColumnNumber", endColumnNumber);
			info.AddValue ("ErrorCode", errorCode);
			info.AddValue ("ErrorSubcategory", errorSubcategory);
			info.AddValue ("HelpKeyword", helpKeyword);
			info.AddValue ("LineNumber", lineNumber);
			info.AddValue ("EndLineNumber", endLineNumber);
			info.AddValue ("ProjectFile", projectFile);
		}

		public string BaseMessage {
			get {
				return baseMessage;
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

		public string ErrorCode {
			get {
				return errorCode;
			}
		}

		public string ErrorSubcategory {
			get {
				return errorSubcategory;
			}
		}

		public string HelpKeyword {
			get {
				return helpKeyword;
			}
		}

		public int LineNumber {
			get {
				return lineNumber;
			}
		}

		public override string Message {
			get {
				return base.Message;
			}
		}

		public string ProjectFile {
			get {
				return projectFile;
			}
		}
	}
}

#endif