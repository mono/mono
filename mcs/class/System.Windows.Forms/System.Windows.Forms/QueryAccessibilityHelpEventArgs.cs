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
