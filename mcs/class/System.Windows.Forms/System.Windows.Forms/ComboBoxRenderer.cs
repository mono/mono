//
// ComboBoxRenderer.cs
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
	public sealed class ComboBoxRenderer
	{
		#region Private Constructor
		private ComboBoxRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawDropDownButton (Graphics g, Rectangle bounds, ComboBoxState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			GetComboRenderer (state).DrawBackground (g, bounds);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string comboBoxText, Font font, Rectangle textBounds, TextFormatFlags flags, ComboBoxState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			GetTextBoxRenderer (state).DrawBackground (g, bounds);

			if (textBounds == Rectangle.Empty)
				textBounds = new Rectangle (bounds.Left + 3, bounds.Top, bounds.Width - 4, bounds.Height);

			if (comboBoxText != String.Empty)
				if (state == ComboBoxState.Disabled)
					TextRenderer.DrawText (g, comboBoxText, font, textBounds, SystemColors.GrayText, flags);
				else
					TextRenderer.DrawText (g, comboBoxText, font, textBounds, SystemColors.ControlText, flags);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, ComboBoxState state)
		{
			DrawTextBox (g, bounds, String.Empty, null, Rectangle.Empty, TextFormatFlags.VerticalCenter, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string comboBoxText, Font font, ComboBoxState state)
		{
			DrawTextBox (g, bounds, comboBoxText, font, Rectangle.Empty, TextFormatFlags.VerticalCenter, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string comboBoxText, Font font, Rectangle textBounds, ComboBoxState state)
		{
			DrawTextBox (g, bounds, comboBoxText, font, textBounds, TextFormatFlags.Default, state);
		}

		public static void DrawTextBox (Graphics g, Rectangle bounds, string comboBoxText, Font font, TextFormatFlags flags, ComboBoxState state)
		{
			DrawTextBox (g, bounds, comboBoxText, font, Rectangle.Empty, flags |= TextFormatFlags.VerticalCenter, state);
		}
		#endregion

		#region Private Static Methods
		private static VisualStyleRenderer GetComboRenderer (ComboBoxState state)
		{
			switch (state) {
				case ComboBoxState.Disabled:
					return new VisualStyleRenderer (VisualStyleElement.ComboBox.DropDownButton.Disabled);
				case ComboBoxState.Hot:
					return new VisualStyleRenderer (VisualStyleElement.ComboBox.DropDownButton.Hot);
				case ComboBoxState.Normal:
				default:
					return new VisualStyleRenderer (VisualStyleElement.ComboBox.DropDownButton.Normal);
				case ComboBoxState.Pressed:
					return new VisualStyleRenderer (VisualStyleElement.ComboBox.DropDownButton.Pressed);
			}
		}

		private static VisualStyleRenderer GetTextBoxRenderer (ComboBoxState state)
		{
			switch (state) {
				case ComboBoxState.Disabled:
					return new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Disabled);
				case ComboBoxState.Hot:
					return new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Hot);
				case ComboBoxState.Normal:
				case ComboBoxState.Pressed:
				default:
					return new VisualStyleRenderer (VisualStyleElement.TextBox.TextEdit.Normal);
			}
		}
		#endregion

		#region Public Static Properties
		public static bool IsSupported {
			get { return VisualStyleInformation.IsEnabledByUser && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled); }
		}
		#endregion
	}
}
