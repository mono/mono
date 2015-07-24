//
// TextBoxRenderer.cs
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
	public sealed class TextBoxRenderer
	{
		#region Private Constructor
		private TextBoxRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawTextBox (Graphics g, Rectangle bounds, TextBoxState state)
		{
			DrawTextBox (g, bounds, String.Empty, null, Rectangle.Empty, TextFormatFlags.Default, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string textBoxText, Font font, TextBoxState state)
		{
			DrawTextBox (g, bounds, textBoxText, font, Rectangle.Empty, TextFormatFlags.Default, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string textBoxText, Font font, Rectangle textBounds, TextBoxState state)
		{
			DrawTextBox (g, bounds, textBoxText, font, textBounds, TextFormatFlags.Default, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string textBoxText, Font font, TextFormatFlags flags, TextBoxState state)
		{
			DrawTextBox (g, bounds, textBoxText, font, Rectangle.Empty, flags, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string textBoxText, Font font, Rectangle textBounds, TextFormatFlags flags, TextBoxState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TextBoxState.Assist:
					vsr = new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Assist);
					break;
				case TextBoxState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Disabled);
					break;
				case TextBoxState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Hot);
					break;
				case TextBoxState.Normal:
				case TextBoxState.Readonly:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Normal);
					break;
				case TextBoxState.Selected:
					vsr = new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Selected);
					break;
			}

			vsr.DrawBackground (g, bounds);

			if (textBounds == Rectangle.Empty)
				textBounds = new Rectangle (bounds.Left + 3, bounds.Top + 3, bounds.Width - 6, bounds.Height - 6);

			if (textBoxText != String.Empty)
				if (state == TextBoxState.Disabled)
					TextRenderer.DrawText (g, textBoxText, font, textBounds, SystemColors.GrayText, flags);
				else
					TextRenderer.DrawText (g, textBoxText, font, textBounds, SystemColors.ControlText, flags);
		}
		#endregion

		#region Public Static Properties
		public static bool IsSupported {
			get { return VisualStyleInformation.IsEnabledByUser && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled); }
		}
		#endregion
	}
}
