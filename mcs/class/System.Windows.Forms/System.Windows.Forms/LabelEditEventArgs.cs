//
// System.Windows.Forms.LabelEditEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class LabelEditEventArgs : EventArgs {

		#region Fields

		private int item;
		private string label = "";			//Gian : Initialized string to empty...
		private bool canceledit = false;	
		
		#endregion
		//
		//  --- Constructor
		//
		public LabelEditEventArgs (int item) 
		{
			this.item = item;
		}

		public LabelEditEventArgs (int item, string label) {
			this.item = item;
			this.label = label;
		}

		#region Public Properties
		public bool CancelEdit 
		{
			get {
				return canceledit;
			}
			set {
				canceledit = value;
			}
		}
		public int Item {
			get {
				return item;
			}
		}
		public string Label {
			get {
				return label;
			}
		}
		#endregion

	}
}
