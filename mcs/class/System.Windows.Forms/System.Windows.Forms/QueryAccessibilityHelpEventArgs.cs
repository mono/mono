//
// System.Windows.Forms.QueryAccessibilityHelpEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class QueryAccessibilityHelpEventArgs : EventArgs {
		string helpnamespace;
		string helpstring;
		string helpkeyword;
		//
		//  --- Constructor
		//
		public QueryAccessibilityHelpEventArgs()
		{
			//
		}
		public QueryAccessibilityHelpEventArgs(string helpNamespace, string helpString, string helpKeyword)
		{
			helpkeyword = helpKeyword;
			helpnamespace = helpNamespace;
			helpstring =helpString;
		}

		//
		//  --- Public Properties
		//
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

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
