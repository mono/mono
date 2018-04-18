//
// PreviewKeyDownEventArgs.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

namespace System.Windows.Forms
{
	public class PreviewKeyDownEventArgs : EventArgs
	{
		private Keys key_data;
		private bool is_input_key;
		
		#region Public Constructors
		public PreviewKeyDownEventArgs (Keys keyData) : base ()
		{
			this.key_data = keyData;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public bool Alt	{
			get { return (this.key_data & Keys.Alt) != 0; }
		}

		public bool Control {
			get { return (this.key_data & Keys.Control) != 0; }
		}

		public bool IsInputKey {
			get { return this.is_input_key; }
			set { this.is_input_key = value; }
		}

		public Keys KeyCode {
			get { return (this.key_data & Keys.KeyCode); }
		}

		public Keys KeyData {
			get { return this.key_data; }
		}

		public int KeyValue {
			get { return (int) (this.key_data & Keys.KeyCode); }
		}

		public Keys Modifiers {
			get { return (this.key_data & Keys.Modifiers); }
		}

		public bool Shift {
			get { return (this.key_data & Keys.Shift) != 0; }
		}
		#endregion	// Public Instance Properties
	}
}
