//
// System.Windows.Forms.LinkClickedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class LinkClickedEventArgs : EventArgs {

		#region Fields

		private string linktext;
		
		#endregion
		//
		//  --- Constructor
		//
		public LinkClickedEventArgs(string linkText) 
		{
			linktext = linkText;
		}

		#region Public Properties

		[ComVisible(true)]
		public string LinkText 
		{
			get {
				return linktext;
			}
		}

		#endregion
	}
}
