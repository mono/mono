// Microsoft.CSharp.CompilerError
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
//

using System;
using System.Text;

namespace Microsoft.CSharp {

	public enum ErrorLevel {
		None,
		Warning
		Error,
		FatalError,
	}

	public class CompilerError {
  
		private ErrorLevel level = ErrorLevel.None;
		private string message = null;
		private string file = null;
		private int number = 0;
		private int column = 0;
		private int line = 0;

  		public CompilerError()
    		{
    		}

		~CompilerError()
		{
			message = null;
			file = null;
		}


		//
		// Public Properties
		//

		public ErrorLevel ErrorLevel {
			get { return level; }
			set { level = value; }
		}

		public string ErrorMessage {
			get {
				return (null == message ? String.Empty : message);
			}
			set {
				message = value;
			}
		}

		public int ErrorNumber {
			get { return number; }
			set { number = value; }
		}

		public int SourceColumn {
			get { return column; }
			set { column = value; }      
		}

		public string SourceFile {
			get { 
				return (null == file ? String.Empty : file);
			}
			set { file = value; }
  		}

		public int SourceLine {
			get { return line; }
			set { line = value; }
		}

		//
		// Public Methods
		//

		/// <summary>
		///   Error message in form: 
		///   filename(line,column): AAAAA CSXXX: message
  		/// </summary>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder ();

			if (String.Empty != SourceFile)
				builder.AppendFormat ("{0}({1},{2}) ", 
					SourceFile, SourceLine, SourceColumn);
			builder.AppendFormat ("{0} CS{1}: {2}", 
				ErrorLevelString, ErrorNumber, ErrorMessage);
			
			return builder.ToString ();
		}

		//
		// Private Properties
		//

		private string ErrorLevelString {
			get {
				if (ErrorLevel.FatalError == ErrorLevel)
					return "Error Fatal";
				return ErrorLevel.ToString ().ToLower ();
			}
		}
	}
}

