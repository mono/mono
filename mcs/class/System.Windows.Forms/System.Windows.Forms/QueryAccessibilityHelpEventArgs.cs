//
// System.Windows.Forms.QueryAccessibilityHelpEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gterzi@lario.com)
//
// (C) 2002 Ximian, Inc
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

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public class QueryAccessibilityHelpEventArgs : EventArgs {

		#region Fields

		string helpnamespace;
		string helpstring;
		string helpkeyword;
		
		#endregion
		//
		//  --- Constructor
		//

		public QueryAccessibilityHelpEventArgs() 
		{
			this.helpkeyword = "";
			this.helpnamespace = "";
			this.helpstring = "";
		}

		public QueryAccessibilityHelpEventArgs(string helpNamespace, string helpString, string helpKeyword) {
			this.helpkeyword = helpKeyword;
			this.helpnamespace = helpNamespace;
			this.helpstring =helpString;
		}


		#region Public Properties

		[ComVisible(true)] 
		public string HelpKeyword {
			get {
				return helpkeyword;
			}
			set {
				helpkeyword = value;
			}
		}

		[ComVisible(true)] 
		public string HelpNamespace {
			get {
				return helpnamespace;
			}
			set {
				helpnamespace = value;
			}
		}

		[ComVisible(true)] 
		public string HelpString {
			get {
				return helpstring;
			}
			set {
				helpstring = value;
			}
		}
		#endregion
	}
}
