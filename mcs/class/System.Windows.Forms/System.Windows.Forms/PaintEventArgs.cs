//
// System.Windows.Forms.PaintEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
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
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public class PaintEventArgs : EventArgs, IDisposable {

		#region Fields

			private Graphics mgraphics;
			private Rectangle mclipRect;

		#endregion

		public PaintEventArgs(Graphics graphics, Rectangle clipRect )
		{
				this.mgraphics = graphics;
				this.mclipRect = clipRect;
		}

		#region Public Properties
		public Rectangle ClipRectangle 
		{
			get {
				return mclipRect;
			}
		}
		
		public Graphics Graphics {
			get {
				return mgraphics;
			}
		}
		#endregion

		#region Public Methods

		public void Dispose()
		{
			//
			mgraphics.Dispose();
		}

		#endregion

		#region Protected Methods

		protected virtual void Dispose(bool disposing)
		{
			//
		}
		#endregion
	 }
}
