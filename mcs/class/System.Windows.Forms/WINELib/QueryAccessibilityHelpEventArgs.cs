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
	//	This is only a template.  Nothing is implemented yet.
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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two QueryAccessibilityHelpEventArgs objects.
		///	The return value is based on the equivalence of
		///	helpkeyword, helpnamespace and helpstring Property
		///	of the two QueryAccessibilityHelpEventArgs.
		/// </remarks>
		public static bool operator == (QueryAccessibilityHelpEventArgs QueryAccessibilityHelpEventArgsA, QueryAccessibilityHelpEventArgs QueryAccessibilityHelpEventArgsB) 
		{
			return ((QueryAccessibilityHelpEventArgsA.HelpKeyword == QueryAccessibilityHelpEventArgsB.HelpKeyword) && (QueryAccessibilityHelpEventArgsA.HelpNamespace == QueryAccessibilityHelpEventArgsB.HelpNamespace) && (QueryAccessibilityHelpEventArgsA.HelpString == QueryAccessibilityHelpEventArgsB.HelpString));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two QueryAccessibilityHelpEventArgs objects.
		///	The return value is based on the equivalence of
		///	helpkeyword, helpnamespace and helpstring Property
		///	of the two QueryAccessibilityHelpEventArgs.
		/// </remarks>
		public static bool operator != (QueryAccessibilityHelpEventArgs QueryAccessibilityHelpEventArgsA, QueryAccessibilityHelpEventArgs QueryAccessibilityHelpEventArgsB) 
		{
			return ((QueryAccessibilityHelpEventArgsA.HelpKeyword != QueryAccessibilityHelpEventArgsB.HelpKeyword) || (QueryAccessibilityHelpEventArgsA.HelpNamespace != QueryAccessibilityHelpEventArgsB.HelpNamespace) || (QueryAccessibilityHelpEventArgsA.HelpString != QueryAccessibilityHelpEventArgsB.HelpString));
		}


		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	QueryAccessibilityHelpEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is QueryAccessibilityHelpEventArgs))return false;
			return (this == (QueryAccessibilityHelpEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the object as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString();
		}


		#endregion

	}
}
