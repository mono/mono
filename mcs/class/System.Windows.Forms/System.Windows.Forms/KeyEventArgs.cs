//
// System.Windows.Forms.KeyEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// Complete
	// </summary>

    public class KeyEventArgs : EventArgs {

		#region Fields
		
		private Keys keydata;
		private bool handled = false;

		#endregion
		//
		//  --- Constructor
		//
		public KeyEventArgs (Keys keyData)
		{
			this.keydata = keyData;
		}

		#region Public Properties

		[ComVisible(true)]
		public virtual bool Alt 
		{
			get {
				return (keydata == Keys.Alt);
			}
		}
		
		[ComVisible(true)]
		public bool Control 
		{
			get {
				return (keydata == Keys.Control);
			}
		}
		
		[ComVisible(true)]
		public bool Handled 
		{
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}
		
		[ComVisible(true)]
		public Keys KeyCode 
		{
			get {
				return keydata & Keys.KeyCode;
			}
		}
		
		[ComVisible(true)]
		public Keys KeyData 
		{
			get {
				return keydata;
			}
		}
		
		[ComVisible(true)]
		public int KeyValue 
		{
			get {
				return Convert.ToInt32(keydata);
			}
		}
		
		[ComVisible(true)]
		public Keys Modifiers 
		{
			get {
				Keys returnKeys = new Keys();
				if(keydata == Keys.Alt)returnKeys = Keys.Alt;
				if(keydata == Keys.Control)returnKeys = returnKeys | Keys.Control;
				if(keydata == Keys.Shift)returnKeys = returnKeys | Keys.Shift;
				return returnKeys;
			}
		}
		
		[ComVisible(true)]
		public bool Shift 
		{
			get {
				return (keydata == Keys.Shift);
			}
		}
		#endregion

	}
}
