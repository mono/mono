//
// System.CodeDom.Compiler.CompilerError
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security.Permissions;

namespace System.CodeDom.Compiler {

	[Serializable]
	public class CompilerError {
		string fileName;
		int line;
		int column;
		string errorNumber;
		string errorText;
		bool isWarning = false;

		public CompilerError () :
			this (String.Empty, 0, 0, String.Empty, String.Empty)
		{
		}

		public CompilerError (string fileName, int line, int column, string errorNumber, string errorText)
		{
			this.fileName = fileName;
			this.line = line;
			this.column = column;
			this.errorNumber = errorNumber;
			this.errorText = errorText;
		}

		public override string ToString ()
		{
			string type = isWarning ? "warning" : "error";
			return String.Format (System.Globalization.CultureInfo.InvariantCulture,
					"{0}({1},{2}) : {3} {4}: {5}", fileName, line, column, type,
					errorNumber, errorText);
		}

		public int Line
		{
			get { return line; }
			set { line = value; }
		}

		public int Column
		{
			get { return column; }
			set { column = value; }
		}

		public string ErrorNumber
		{
			get { return errorNumber; }
			set { errorNumber = value; }
		}

		public string ErrorText
		{
			get { return errorText; }
			set { errorText = value; }
		}

		public bool IsWarning
		{
			get { return isWarning; }
			set { isWarning = value; }
		}

		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}
	}
}

