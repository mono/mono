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
// Copyright (c) 2008 George Giolfan
//
// Authors:
//	George Giolfan (georgegiolfan@yahoo.com)

using System.Drawing;
using System.Windows.Forms.VisualStyles;
namespace System.Windows.Forms.Theming.VisualStyles
{
	class CheckBoxPainter : Default.CheckBoxPainter
	{
		public override void DrawNormalCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			CheckBoxState check_box_state;
			switch (state) {
			case CheckState.Checked:
				check_box_state = CheckBoxState.CheckedNormal;
				break;
			case CheckState.Indeterminate:
				check_box_state = CheckBoxState.MixedNormal;
				break;
			default:
				check_box_state = CheckBoxState.UncheckedNormal;
				break;
			}
			DrawCheckBox (g, bounds, check_box_state);
		}
		public override void DrawHotCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			CheckBoxState check_box_state;
			switch (state) {
			case CheckState.Checked:
				check_box_state = CheckBoxState.CheckedHot;
				break;
			case CheckState.Indeterminate:
				check_box_state = CheckBoxState.MixedHot;
				break;
			default:
				check_box_state = CheckBoxState.UncheckedHot;
				break;
			}
			DrawCheckBox (g, bounds, check_box_state);
		}
		public override void DrawPressedCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			CheckBoxState check_box_state;
			switch (state) {
			case CheckState.Checked:
				check_box_state = CheckBoxState.CheckedPressed;
				break;
			case CheckState.Indeterminate:
				check_box_state = CheckBoxState.MixedPressed;
				break;
			default:
				check_box_state = CheckBoxState.UncheckedPressed;
				break;
			}
			DrawCheckBox (g, bounds, check_box_state);
		}
		public override void DrawDisabledCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			CheckBoxState check_box_state;
			switch (state) {
			case CheckState.Checked:
				check_box_state = CheckBoxState.CheckedDisabled;
				break;
			case CheckState.Indeterminate:
				check_box_state = CheckBoxState.MixedDisabled;
				break;
			default:
				check_box_state = CheckBoxState.UncheckedDisabled;
				break;
			}
			DrawCheckBox (g, bounds, check_box_state);
		}
		static void DrawCheckBox (Graphics g, Rectangle bounds, CheckBoxState state)
		{
			CheckBoxRenderer.DrawCheckBox (
				g,
				bounds.Location,
				state
			);
		}
	}
}
