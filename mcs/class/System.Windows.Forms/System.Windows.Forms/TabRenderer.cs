//
// TabRenderer.cs
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

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
	public sealed class TabRenderer
	{
		#region Private Constructor
		private TabRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawTabItem (Graphics g, Rectangle bounds, TabItemState state)
		{
			DrawTabItem(g, bounds, String.Empty, null, TextFormatFlags.Default, null, Rectangle.Empty, false, state);
		}

		public static void DrawTabItem (Graphics g, Rectangle bounds, bool focused, TabItemState state)
		{
			DrawTabItem (g, bounds, String.Empty, null, TextFormatFlags.Default, null, Rectangle.Empty, focused, state);
		}
		
		public static void DrawTabItem (Graphics g, Rectangle bounds, string tabItemText, Font font, TabItemState state)
		{
			DrawTabItem (g, bounds, tabItemText, font, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, false, state);
		}

		public static void DrawTabItem (Graphics g, Rectangle bounds, Image image, Rectangle imageRectangle, bool focused, TabItemState state)
		{
			DrawTabItem (g, bounds, String.Empty, null, TextFormatFlags.HorizontalCenter, image, imageRectangle, focused, state);
		}

		public static void DrawTabItem (Graphics g, Rectangle bounds, string tabItemText, Font font, bool focused, TabItemState state)
		{
			DrawTabItem (g, bounds, tabItemText, font, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, focused, state);
		}

		public static void DrawTabItem (Graphics g, Rectangle bounds, string tabItemText, Font font, TextFormatFlags flags, bool focused, TabItemState state)
		{
			DrawTabItem (g, bounds, tabItemText, font, flags, null, Rectangle.Empty, focused, state);
		}

		public static void DrawTabItem (Graphics g, Rectangle bounds, string tabItemText, Font font, Image image, Rectangle imageRectangle, bool focused, TabItemState state)
		{
			DrawTabItem (g, bounds, tabItemText, font, TextFormatFlags.HorizontalCenter, image, imageRectangle, focused, state);
		}

		public static void DrawTabItem (Graphics g, Rectangle bounds, string tabItemText, Font font, TextFormatFlags flags, Image image, Rectangle imageRectangle, bool focused, TabItemState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();
				
			VisualStyleRenderer vsr;
			
			switch (state) {
				case TabItemState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.Tab.TabItem.Disabled);
					break;
				case TabItemState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.Tab.TabItem.Hot);
					break;
				case TabItemState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.Tab.TabItem.Normal);
					break;
				case TabItemState.Selected:
					vsr = new VisualStyleRenderer (VisualStyleElement.Tab.TabItem.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
			
			if(image != null)
				vsr.DrawImage(g, imageRectangle, image);
				
			bounds.Offset(3,3);
			bounds.Height -= 6;
			bounds.Width -= 6;
			
			if(tabItemText != String.Empty)
				TextRenderer.DrawText(g, tabItemText, font, bounds, SystemColors.ControlText,flags);
				
			if(focused)
				ControlPaint.DrawFocusRectangle(g, bounds);
		}

		public static void DrawTabPage (Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.Tab.Pane.Normal); ;

			vsr.DrawBackground (g, bounds);
		}
		#endregion
		
		#region Public Static Properties
		public static bool IsSupported {
			get { return VisualStyleInformation.IsEnabledByUser && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled); }
		}
		#endregion
	}
}
