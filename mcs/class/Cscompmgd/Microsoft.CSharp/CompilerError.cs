// Microsoft.CSharp.CompilerError
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
//

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
using System.Text;

namespace Microsoft.CSharp {

#if NET_2_0
	[System.Obsolete]
#endif 
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

