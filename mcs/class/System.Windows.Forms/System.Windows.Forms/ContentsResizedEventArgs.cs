//
// System.Windows.Forms.ColumnClickEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides data for the ContentsResized event.
	/// </summary>

	public class ContentsResizedEventArgs : EventArgs
	{
		private Rectangle newrectangle;
		/// --- Constructor ---
		public ContentsResizedEventArgs(Rectangle newRectangle) : base() {
			newrectangle = newRectangle;
		}
		
		/// --- Properties ---
		public Rectangle NewRectangle {
			get {
				return newrectangle;
			}
		}

	}
}
