//
// System.Windows.Forms.MouseEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)   
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
	//
	// </summary>

    public class MouseEventArgs : EventArgs {

		#region Fields

		private MouseButtons button;
		private int clicks;
		private int x;
		private int y;
		private int delta;
		
		#endregion

		public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
		{
			this.button = button;
			this.clicks = clicks;
			this.x = x;
			this.y = y;
			this.delta = delta;
		}

		#region Public Properties

		[ComVisible(true)]
		public MouseButtons Button 
		{
			get {
				return button;
			}
		}

		[ComVisible(true)]
		public int Clicks {
			get {
				return clicks;
			}
		}

		[ComVisible(true)]
		public int Delta {
			get {
				return delta;
			}
		}

		[ComVisible(true)]
		public int X {
			get {
				return x;
			}
		}

		[ComVisible(true)]
		public int Y {
			get {
				return y;
			}
		}
		#endregion
	 }
}
