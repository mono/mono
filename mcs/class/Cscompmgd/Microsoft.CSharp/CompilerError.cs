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

	public class CompilerError {
  
		public ErrorLevel ErrorLevel;
		public string ErrorMessage;
		public string SourceFile;
		public int ErrorNumber;
		public int SourceColumn;
		public int SourceLine;

  		public CompilerError ()
    		{
			ErrorLevel = ErrorLevel.None;
			ErrorMessage = String.Empty;
			SourceFile = String.Empty;
			ErrorNumber = 0;
			SourceColumn = 0;
			SourceLine = 0;
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

