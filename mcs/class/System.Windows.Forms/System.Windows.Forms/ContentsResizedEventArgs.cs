//
// System.Windows.Forms.ColumnClickEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
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

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the ContentsResized event.
	/// </summary>

	public class ContentsResizedEventArgs : EventArgs {

		#region Fields
		private Rectangle newrectangle;
		#endregion

		/// --- Constructor ---
		public ContentsResizedEventArgs(Rectangle newRectangle) : base() 
		{
			newrectangle = newRectangle;
		}
		
		#region Public Propeties
		public Rectangle NewRectangle 
		{
			get {
				return newrectangle;
			}
		}
		#endregion
	}
}
