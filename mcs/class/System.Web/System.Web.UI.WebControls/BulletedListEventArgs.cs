//
// System.Web.UI.WebControls.BulletedListEventArgs.cs
//
// Authors:
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//
#if NET_1_2
namespace System.Web.UI.WebControls {
	public class BulletedListEventArgs : EventArgs {
		int index;
		
		public BulletedListEventArgs (int index) {
			this.index = index;
		}
		
		public int Index { get { return index; } }
	}
}
#endif