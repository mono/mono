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
	// Just a template...
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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LinkLabelLinkClickedEventArgs objects.
		///	The return value is based on the equivalence of
		///	Link Property
		///	of the two LinkLabelLinkClickedEventArgs.
		/// </remarks>
		public static bool operator == (LinkLabelLinkClickedEventArgs LinkLabelLinkClickedEventArgsA, LinkLabelLinkClickedEventArgs LinkLabelLinkClickedEventArgsB) 
		{
			return (LinkLabelLinkClickedEventArgsA.Link == LinkLabelLinkClickedEventArgsB.Link);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LinkLabelLinkClickedEventArgs objects.
		///	The return value is based on the equivalence of
		///	Link Property
		///	of the two LinkLabelLinkClickedEventArgs.
		/// </remarks>
		public static bool operator != (LinkLabelLinkClickedEventArgs LinkLabelLinkClickedEventArgsA, LinkLabelLinkClickedEventArgs LinkLabelLinkClickedEventArgsB) 
		{
			return (LinkLabelLinkClickedEventArgsA.Link != LinkLabelLinkClickedEventArgsB.Link);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	LinkLabelLinkClickedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is LinkLabelLinkClickedEventArgs))return false;
			return (this == (LinkLabelLinkClickedEventArgs) obj);
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
