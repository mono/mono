//
// System.Windows.Forms.LinkLabelLinkClickedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//	Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class LinkLabelLinkClickedEventArgs : EventArgs {

		#region Fields

		private LinkLabel.Link link;
		
		#endregion
		//
		//  --- Constructor
		//

		public LinkLabelLinkClickedEventArgs(LinkLabel.Link link)
		{
			this.link = link;
		}

		#region Public Properties

		[ComVisible(true)]
		public LinkLabel.Link Link{
			get {
				return link;
			}
		}
		#endregion
	}
}
