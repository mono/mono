//
// System.Windows.Forms.ScrollEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gterzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class ScrollEventArgs : EventArgs {

		#region Fields
			
		private int newvalue;
		private ScrollEventType type;

		#endregion

		//
		//  --- Constructor
		//
		public ScrollEventArgs(ScrollEventType type, int newVal)
		{
			this.newvalue = newvalue;
			this.type = type;
		}
		
		#region Public Properties

		[ComVisible(true)]
		public int NewValue 
		{
			get {
				return newvalue;
			}
			set {
				newvalue = value;
			}
		}
		
		[ComVisible(true)]
		public ScrollEventType Type {
			get {
				return type;
			}
		}

		#endregion
	}
}
