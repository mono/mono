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
