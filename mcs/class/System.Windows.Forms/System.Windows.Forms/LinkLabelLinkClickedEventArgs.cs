//
// System.Windows.Forms.LinkLabelLinkClickedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class LinkLabelLinkClickedEventArgs : EventArgs {
		private LinkLabel.Link link;
		//
		//  --- Constructor
		//
		public LinkLabelLinkClickedEventArgs(LinkLabel.Link link)
		{
			this.link = link;
		}

		//
		//  --- Public Properties
		//
		//public 
		LinkLabel.Link Link {
			get {
				return link;
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
