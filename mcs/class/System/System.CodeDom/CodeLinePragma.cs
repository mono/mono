//
// System.CodeDom CodeLinePragma Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
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

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	// <summary>
	//    Use objects of this class to keep track of locations where
	//    statements are defined
	// </summary>
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeLinePragma 
	{
		private string fileName;
		private int lineNumber;
		
		//
		// Constructors
		//
		public CodeLinePragma ()
		{
		}

		public CodeLinePragma (string fileName, int lineNumber)
		{
			this.fileName = fileName;
			this.lineNumber = lineNumber;
		}
		
		//
		// Properties
		//
		public string FileName {
			get {
				if (fileName == null) {
					return string.Empty;
				}
				return fileName;
			}
			set {
				fileName = value;
			}
		}
		
		public int LineNumber {
			get {
				return lineNumber;
			}
			set {
				lineNumber = value;
			}
		}
	}
}
