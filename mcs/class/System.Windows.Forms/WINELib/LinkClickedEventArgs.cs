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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LinkClickedEventArgs objects.
		///	The return value is based on the equivalence of
		///	LinkText Property
		///	of the two LinkClickedEventArgs.
		/// </remarks>
		public static bool operator == (LinkClickedEventArgs LinkClickedEventArgsA, LinkClickedEventArgs LinkClickedEventArgsB) 
		{
			return (LinkClickedEventArgsA.LinkText == LinkClickedEventArgsB.LinkText);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LinkClickedEventArgs objects.
		///	The return value is based on the equivalence of
		///	LinkText Property
		///	of the two LinkClickedEventArgs.
		/// </remarks>
		public static bool operator != (LinkClickedEventArgs LinkClickedEventArgsA, LinkClickedEventArgs LinkClickedEventArgsB) 
		{
			return (LinkClickedEventArgsA.LinkText != LinkClickedEventArgsB.LinkText);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	LinkClickedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is LinkClickedEventArgs))return false;
			return (this == (LinkClickedEventArgs) obj);
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
			return base.ToString() + " LinkClickedEventArgs";
		}


		#endregion
	}
}
