//
// System.Windows.Forms.ColumnClickEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Implmented by Dennis Hayes <dennish@raytek.com>
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the ColumnClick event.
	/// </summary>
	public class ColumnClickEventArgs : EventArgs {

		private int column;
		/// --- Constructor ---
		public ColumnClickEventArgs(int Column) : base() 
		{
			column = Column;
		}

		/// --- Properties ---
		public int Column {
			get { 
				return column;
			}
		}
	}
}
