//
// System.CodeDom.Compiler.CompilerError
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.CodeDom.Compiler
{
	public class CompilerError
	{
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
			return String.Format ("{0}({1},{2}) : {3} {4}: {5}", fileName, line, column, type,
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

