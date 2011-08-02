//
// GroupBoxRenderer.cs
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
	public sealed class GroupBoxRenderer
	{
		private static bool always_use_visual_styles = false;
		
		#region Private Constructor
		private GroupBoxRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawGroupBox (Graphics g, Rectangle bounds, GroupBoxState state)
		{
			DrawGroupBox (g, bounds, String.Empty, null, Color.Empty, TextFormatFlags.Default, state);
		}

		public static void DrawGroupBox (Graphics g, Rectangle bounds, string groupBoxText, Font font, GroupBoxState state)
		{
			DrawGroupBox (g, bounds, groupBoxText, font, Color.Empty, TextFormatFlags.Default, state);
		}

		public static void DrawGroupBox (Graphics g, Rectangle bounds, string groupBoxText, Font font, Color textColor, GroupBoxState state)
		{
			DrawGroupBox (g, bounds, groupBoxText, font, textColor, TextFormatFlags.Default, state);
		}

		public static void DrawGroupBox (Graphics g, Rectangle bounds, string groupBoxText, Font font, TextFormatFlags flags, GroupBoxState state)
		{
			DrawGroupBox (g, bounds, groupBoxText, font, Color.Empty, flags, state);
		}

		public static void DrawGroupBox (Graphics g, Rectangle bounds, string groupBoxText, Font font, Color textColor, TextFormatFlags flags, GroupBoxState state)
		{
			Size font_size = TextRenderer.MeasureText (groupBoxText, font);

			if (Application.RenderWithVisualStyles || always_use_visual_styles == true) {
				VisualStyleRenderer vsr;
				Rectangle new_bounds;

				switch (state) {
					case GroupBoxState.Normal:
					default:
						vsr = new VisualStyleRenderer (VisualStyleElement.Button.GroupBox.Normal);
						new_bounds = new Rectangle (bounds.Left, bounds.Top + (int)(font_size.Height / 2) - 1, bounds.Width, bounds.Height - (int)(font_size.Height / 2) + 1);
						break;
					case GroupBoxState.Disabled:
						vsr = new VisualStyleRenderer (VisualStyleElement.Button.GroupBox.Disabled);
						new_bounds = new Rectangle (bounds.Left, bounds.Top + (int)(font_size.Height / 2) - 2, bounds.Width, bounds.Height - (int)(font_size.Height / 2) + 2);
						break;
				}

				if (groupBoxText == String.Empty)
					vsr.DrawBackground (g, bounds);
				else
					vsr.DrawBackgroundExcludingArea (g, new_bounds, new Rectangle (bounds.Left + 9, bounds.Top, font_size.Width - 3, font_size.Height));

				if (textColor == Color.Empty)
					textColor = vsr.GetColor (ColorProperty.TextColor);

				if (groupBoxText != String.Empty)
					TextRenderer.DrawText (g, groupBoxText, font, new Point (bounds.Left + 8, bounds.Top), textColor, flags);
			}
			else {
				// MS has a pretty big bug when rendering the non-visual styles group box.  Instead of using the height
				// part of the bounds as height, they use it as the bottom, so the boxes are drawn in completely different
				// places.  Rather than emulate this bug, we do it correctly.  After googling for a while, I don't think
				// anyone has ever actually used this class for anything, so it should be fine.  :)
				Rectangle new_bounds = new Rectangle (bounds.Left, bounds.Top + (int)(font_size.Height / 2), bounds.Width, bounds.Height - (int)(font_size.Height / 2));
				
				// Don't paint over the background where we are going to put the text
				Region old_clip = g.Clip;
				g.SetClip (new Rectangle (bounds.Left + 9, bounds.Top, font_size.Width - 3, font_size.Height), System.Drawing.Drawing2D.CombineMode.Exclude);
				
				ControlPaint.DrawBorder3D (g, new_bounds, Border3DStyle.Etched);
				
				g.Clip = old_clip;

				if (groupBoxText != String.Empty) {
					if (textColor == Color.Empty)
						textColor = state == GroupBoxState.Normal ? SystemColors.ControlText :
							SystemColors.GrayText;
					TextRenderer.DrawText (g, groupBoxText, font, new Point (bounds.Left + 8, bounds.Top), textColor, flags);
				}
			}
		}

		public static bool IsBackgroundPartiallyTransparent (GroupBoxState state)
		{
			if (!VisualStyleRenderer.IsSupported)
				return false;

			VisualStyleRenderer vsr;

			switch (state) {
				case GroupBoxState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.Button.GroupBox.Normal);
					break;
				case GroupBoxState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.Button.GroupBox.Disabled);
					break;
			}

			return vsr.IsBackgroundPartiallyTransparent ();
		}

		public static void DrawParentBackground (Graphics g, Rectangle bounds, Control childControl)
		{
			if (!VisualStyleRenderer.IsSupported)
				return;
			
			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.Button.GroupBox.Normal);

			vsr.DrawParentBackground (g, bounds, childControl);
		}
		#endregion

		#region Public Static Properties
		public static bool RenderMatchingApplicationState {
			get { return !always_use_visual_styles; }
			set { always_use_visual_styles = !value; }
		}
		#endregion
	}
}
