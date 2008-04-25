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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	George Giolfan, georgegiolfan@yahoo.com
//	Ernesto Carrea, equistango@gmail.com

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
	/// <summary>
	/// VisualStyles theme.
	/// </summary>
	/// <remarks>
	/// This theme uses only the managed VisualStyles API.
	/// To select it, set MONO_THEME to VisualStyles and call <see cref="Application.EnableVisualStyles"/>.
	/// </remarks>
	class ThemeVisualStyles : ThemeWin32Classic
	{
		#region ButtonBase
		public override void DrawButtonBase (Graphics dc, Rectangle clip_area, ButtonBase button)
		{
			if (button.FlatStyle == FlatStyle.System) {
				ButtonRenderer.DrawButton (
					dc,
					new Rectangle (Point.Empty, button.Size),
					button.Text,
					button.Font,
					button.TextFormatFlags,
					null,
					Rectangle.Empty,
					ShouldPaintFocusRectagle (button),
					GetPushButtonState (button)
				);
				return;
			}
			base.DrawButtonBase (dc, clip_area, button);
		}
		static PushButtonState GetPushButtonState (ButtonBase button)
		{
			if (!button.Enabled)
				return PushButtonState.Disabled;
			if (button.Pressed)
				return PushButtonState.Pressed;
			if (button.Entered)
				return PushButtonState.Hot;
			if (button.IsDefault || button.Focused || button.paint_as_acceptbutton)
				return PushButtonState.Default;
			return PushButtonState.Normal;
		}
		#endregion
#if NET_2_0
		#region Button 2.0
		public override void DrawButtonBackground (Graphics g, Button button, Rectangle clipArea)
		{
			ButtonRenderer.GetPushButtonRenderer (GetPushButtonState (button)).DrawBackground (g, new Rectangle (Point.Empty, button.Size));
		}
		#endregion
#endif
		#region CheckBox
		protected override void CheckBox_DrawCheckBox (Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle)
		{
			if (checkbox.Appearance == Appearance.Normal && checkbox.FlatStyle == FlatStyle.System) {
				CheckBoxRenderer.DrawCheckBox (
					dc,
					new Point (checkbox_rectangle.Left, checkbox_rectangle.Top),
					GetCheckBoxState (checkbox)
				);
				return;
			}
			base.CheckBox_DrawCheckBox(dc, checkbox, state, checkbox_rectangle);
		}
		static CheckBoxState GetCheckBoxState (CheckBox checkBox)
		{
			switch (checkBox.CheckState) {
			case CheckState.Checked:
				if (!checkBox.Enabled)
					return CheckBoxState.CheckedDisabled;
				else if (checkBox.Pressed)
					return CheckBoxState.CheckedPressed;
				else if (checkBox.Entered)
					return CheckBoxState.CheckedHot;
				return CheckBoxState.CheckedNormal;
			case CheckState.Indeterminate:
				if (!checkBox.Enabled)
					return CheckBoxState.MixedDisabled;
				else if (checkBox.Pressed)
					return CheckBoxState.MixedPressed;
				else if (checkBox.Entered)
					return CheckBoxState.MixedHot;
				return CheckBoxState.MixedNormal;
			default:
				if (!checkBox.Enabled)
					return CheckBoxState.UncheckedDisabled;
				else if (checkBox.Pressed)
					return CheckBoxState.UncheckedPressed;
				else if (checkBox.Entered)
					return CheckBoxState.UncheckedHot;
				return CheckBoxState.UncheckedNormal;
			}
		}
		#endregion
		#region ControlPaint
		public override void CPDrawCheckBox (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				base.CPDrawCheckBox (dc, rectangle, state);
				return;
			}
			VisualStyleElement element;
			if ((state & ButtonState.Checked) == ButtonState.Checked)
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					element = VisualStyleElement.Button.CheckBox.CheckedDisabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					element = VisualStyleElement.Button.CheckBox.CheckedPressed;
				else
					element = VisualStyleElement.Button.CheckBox.CheckedNormal;
			else
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					element = VisualStyleElement.Button.CheckBox.UncheckedDisabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					element = VisualStyleElement.Button.CheckBox.UncheckedPressed;
				else
					element = VisualStyleElement.Button.CheckBox.UncheckedNormal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawCheckBox (dc, rectangle, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, rectangle);
		}
		#endregion
		#region RadioButton
		protected override void RadioButton_DrawButton (RadioButton radio_button, Graphics dc, ButtonState state, Rectangle radiobutton_rectangle) {
			if (radio_button.Appearance == Appearance.Normal && radio_button.FlatStyle == FlatStyle.System) {
				RadioButtonRenderer.DrawRadioButton (
					dc,
					new Point (radiobutton_rectangle.Left, radiobutton_rectangle.Top),
					GetRadioButtonState (radio_button)
				);
				return;
			}
			base.RadioButton_DrawButton(radio_button, dc, state, radiobutton_rectangle);
		}
		static RadioButtonState GetRadioButtonState (RadioButton checkBox)
		{
			if (checkBox.Checked) {
				if (!checkBox.Enabled)
					return RadioButtonState.CheckedDisabled;
				else if (checkBox.Pressed)
					return RadioButtonState.CheckedPressed;
				else if (checkBox.Entered)
					return RadioButtonState.CheckedHot;
				return RadioButtonState.CheckedNormal;
			} else {
				if (!checkBox.Enabled)
					return RadioButtonState.UncheckedDisabled;
				else if (checkBox.Pressed)
					return RadioButtonState.UncheckedPressed;
				else if (checkBox.Entered)
					return RadioButtonState.UncheckedHot;
				return RadioButtonState.UncheckedNormal;
			}
		}
		#endregion
	}
}