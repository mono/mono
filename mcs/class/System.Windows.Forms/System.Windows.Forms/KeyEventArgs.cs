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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		public virtual bool Shift 
		{
			get {
				return (keydata == Keys.Shift);
			}
		}
		#endregion

	}
}
