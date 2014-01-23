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

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	[Serializable]
	public sealed class InvalidProjectFileException : Exception {
		
		int	columnNumber;
		int	endColumnNumber;
		string	errorCode;
		string	errorSubcategory;
		string	helpKeyword;
		int	lineNumber;
		int	endLineNumber;
		string	projectFile;
		
		public InvalidProjectFileException ()
			: base ("Invalid project file exception has occured")
		{
		}

		public InvalidProjectFileException (string message)
			: base (message)
		{
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
			this.errorSubcategory = errorSubcategory;
			this.errorCode = errorCode;
			this.helpKeyword = helpKeyword;
		}

		public InvalidProjectFileException (string message,
						    Exception innerException)
			: base (message, innerException)
		{
		}

		// FIXME: set line/column numbers?
		[MonoTODO]
		public InvalidProjectFileException (XmlNode xmlNode,
						    string message,
						    string errorSubcategory,
						    string errorCode,
						    string helpKeyword)
			: base (message)
		{
			this.errorSubcategory = errorSubcategory;
			this.errorCode = errorCode;
			this.helpKeyword = helpKeyword;
		}

		// FIXME: private temporarily
		private InvalidProjectFileException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.columnNumber = info.GetInt32 ("columnNumber");
			this.endColumnNumber = info.GetInt32 ("endColumnNumber");
			this.errorCode = info.GetString ("errorCode");
			this.errorSubcategory = info.GetString ("errorSubcategory");
			this.helpKeyword = info.GetString ("helpKeyword");
			this.lineNumber = info.GetInt32 ("lineNumber");
			this.endLineNumber = info.GetInt32 ("endLineNumber");
			this.projectFile = info.GetString ("projectFile");
		}

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info,
						    StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("columnNumber", columnNumber);
			info.AddValue ("endColumnNumber", endColumnNumber);
			info.AddValue ("errorCode", errorCode);
			info.AddValue ("errorSubcategory", errorSubcategory);
			info.AddValue ("helpKeyword", helpKeyword);
			info.AddValue ("lineNumber", lineNumber);
			info.AddValue ("endLineNumber", endLineNumber);
			info.AddValue ("projectFile", projectFile);
		}

		public string BaseMessage {
			get {
				return base.Message;
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
				if (projectFile == null || projectFile == String.Empty) {
					return BaseMessage;
				} else {
					return BaseMessage + "  " + ProjectFile;
				}
			}
		}

		public string ProjectFile {
			get {
				return projectFile;
			}
		}
	}
}
