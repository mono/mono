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
	class RadioButtonPainter : Default.RadioButtonPainter
	{
		public override void DrawNormalRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			DrawRadioButton (g, bounds, isChecked ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal);
		}
		public override void DrawHotRadioButton(Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			DrawRadioButton (g, bounds, isChecked ? RadioButtonState.CheckedHot : RadioButtonState.UncheckedHot);
		}
		public override void DrawPressedRadioButton(Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			DrawRadioButton (g, bounds, isChecked ? RadioButtonState.CheckedPressed : RadioButtonState.UncheckedPressed);
		}
		public override void DrawDisabledRadioButton(Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			DrawRadioButton (g, bounds, isChecked ? RadioButtonState.CheckedDisabled : RadioButtonState.UncheckedDisabled);
		}
		static void DrawRadioButton (Graphics g, Rectangle bounds, RadioButtonState state)
		{
			RadioButtonRenderer.DrawRadioButton (
				g,
				bounds.Location,
				state
			);
		}
	}
}
