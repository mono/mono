//
// System.Windows.Forms.ColumnClickEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the ContentsResized event.
	/// </summary>

	public class ContentsResizedEventArgs : EventArgs {

		#region Fields
		private Rectangle newrectangle;
		#endregion

		/// --- Constructor ---
		public ContentsResizedEventArgs(Rectangle newRectangle) : base() 
		{
			newrectangle = newRectangle;
		}
		
		#region Public Propeties
		public Rectangle NewRectangle 
		{
			get {
				return newrectangle;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ContentsResizedEventArgs objects.
		///	The return value is based on the equivalence of
		///	newRectangle Property
		///	of the two ContentsResizedEventArgs.
		/// </remarks>
		public static bool operator == (ContentsResizedEventArgs ContentsResizedEventArgsA, ContentsResizedEventArgs ContentsResizedEventArgsB) 
		{
			return (ContentsResizedEventArgsA.NewRectangle == ContentsResizedEventArgsB.NewRectangle);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ContentsResizedEventArgs objects.
		///	The return value is based on the equivalence of
		///	newRectangle Property
		///	of the two ContentsResizedEventArgs.
		/// </remarks>
		public static bool operator != (ContentsResizedEventArgs ContentsResizedEventArgsA, ContentsResizedEventArgs ContentsResizedEventArgsB) 
		{
			return (ContentsResizedEventArgsA.NewRectangle != ContentsResizedEventArgsB.NewRectangle);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ContentsResizedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ContentsResizedEventArgs))return false;
			return (this == (ContentsResizedEventArgs) obj);
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
			return base.ToString() + " ContentsResizedEventArgs";
		}


		#endregion

	}
}
