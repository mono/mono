//
// System.Windows.Forms.KeyPressEventArgs.cs
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

    public class KeyPressEventArgs : EventArgs {

		#region Fields

		private char keychar;
		private bool handled = false;	//Gian : Initialize?
		
		#endregion

		//
		//  --- Constructor
		//
		public KeyPressEventArgs (char keyChar)
		{
			this.keychar = keyChar;
		}

		#region Public Properties
		[ComVisible(true)]
		public bool Handled {
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}

		[ComVisible(true)]
		public char KeyChar {
			get {
				return keychar;
			}
		}
		#endregion
	}
}
