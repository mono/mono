//
// System.Windows.Forms.StatusBarPanelClickEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
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

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the PanelClick event.
	/// </summary>
	public class StatusBarPanelClickEventArgs : MouseEventArgs {

		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarPanelClickEventArgs(StatusBarPanel panel, MouseButtons button, int clicks, int x, int y) : base(button, clicks, x, y, 0) 
		{
			this.panel = panel;
		}

		#region Public Properties
		
		/// <summary>
		///	StatusBarPanel Property
		/// </summary>
		///
		/// <remarks>
		///	Gets the StatusBarPanel to draw.
		/// </remarks>
		public StatusBarPanel StatusBarPanel 
		{
			get 
			{
				return panel;
			}
		}

		#endregion
	}
}
