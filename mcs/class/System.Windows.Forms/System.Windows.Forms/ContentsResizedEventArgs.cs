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
	}
}
