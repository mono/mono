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
using System.Drawing.Drawing2D;
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
		static bool render_client_areas;
		static bool render_non_client_areas;

		public ThemeVisualStyles ()
		{
			Update ();
		}

		public override void ResetDefaults ()
		{
			base.ResetDefaults ();
			Update ();
		}

		static void Update ()
		{
			bool visual_styles_is_enabled_by_user = VisualStyleInformation.IsEnabledByUser;
			render_client_areas =
				visual_styles_is_enabled_by_user &&
				(Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled ||
				Application.VisualStyleState == VisualStyleState.ClientAreaEnabled);
			render_non_client_areas =
				visual_styles_is_enabled_by_user &&
				(Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled/* ||
				Application.VisualStyleState == VisualStyleState.NonClientAreaEnabled*/);
		}

		public static bool RenderClientAreas {
			get { return render_client_areas; }
		}

		#region Controls
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
			if (!RenderClientAreas ||
				!button.UseVisualStyleBackColor) {
				base.DrawButtonBackground (g, button, clipArea);
				return;
			}
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
		#region ComboBox
		static VisualStyleElement ComboBoxGetVisualStyleElement (ComboBox comboBox, ButtonState state)
		{
			if (state == ButtonState.Inactive)
				return VisualStyleElement.ComboBox.DropDownButton.Disabled;
			if (state == ButtonState.Pushed)
				return VisualStyleElement.ComboBox.DropDownButton.Pressed;
			if (comboBox.DropDownButtonEntered)
				return VisualStyleElement.ComboBox.DropDownButton.Hot;
			return VisualStyleElement.ComboBox.DropDownButton.Normal;
		}
		public override void ComboBoxDrawNormalDropDownButton (ComboBox comboBox, Graphics g, Rectangle clippingArea, Rectangle area, ButtonState state)
		{
			if (!RenderClientAreas) {
				base.ComboBoxDrawNormalDropDownButton (comboBox, g, clippingArea, area, state);
				return;
			}
			VisualStyleElement element = ComboBoxGetVisualStyleElement (comboBox, state);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ComboBoxDrawNormalDropDownButton (comboBox, g, clippingArea, area, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, area, clippingArea);
		}
		public override bool ComboBoxNormalDropDownButtonHasTransparentBackground (ComboBox comboBox, ButtonState state)
		{
			if (!RenderClientAreas)
				return base.ComboBoxNormalDropDownButtonHasTransparentBackground (comboBox, state);
			VisualStyleElement element = ComboBoxGetVisualStyleElement (comboBox, state);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.ComboBoxNormalDropDownButtonHasTransparentBackground (comboBox, state);
			return new VisualStyleRenderer (element).IsBackgroundPartiallyTransparent ();
		}
		public override bool ComboBoxDropDownButtonHasHotElementStyle (ComboBox comboBox)
		{
			if (!RenderClientAreas)
				return base.ComboBoxDropDownButtonHasHotElementStyle (comboBox);
#if NET_2_0
			switch (comboBox.FlatStyle) {
			case FlatStyle.Flat:
			case FlatStyle.Popup:
				return base.ComboBoxDropDownButtonHasHotElementStyle (comboBox);
			}
#endif
			return true;
		}
		static bool ComboBoxShouldPaintBackground (ComboBox comboBox)
		{
			if (comboBox.DropDownStyle == ComboBoxStyle.Simple)
				return false;
#if NET_2_0
			switch (comboBox.FlatStyle) {
			case FlatStyle.Flat:
			case FlatStyle.Popup:
				return false;
			}
#endif 
			return true;
		}
		public override void ComboBoxDrawBackground (ComboBox comboBox, Graphics g, Rectangle clippingArea, FlatStyle style)
		{
			if (!RenderClientAreas || !ComboBoxShouldPaintBackground (comboBox)) {
				base.ComboBoxDrawBackground (comboBox, g, clippingArea, style);
				return;
			}
			VisualStyleElement element;
			if (!comboBox.Enabled)
				element = VisualStyleElement.ComboBox.Border.Disabled;
			else if (comboBox.Entered)
				element = VisualStyleElement.ComboBox.Border.Hot;
			else if (comboBox.Focused)
				element = VisualStyleElement.ComboBox.Border.Focused;
			else
				element = VisualStyleElement.ComboBox.Border.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ComboBoxDrawBackground (comboBox, g, clippingArea, style);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, new Rectangle (Point.Empty, comboBox.Size), clippingArea);
		}
		public override bool CombBoxBackgroundHasHotElementStyle (ComboBox comboBox)
		{
			if (RenderClientAreas &&
				ComboBoxShouldPaintBackground (comboBox) &&
				comboBox.Enabled &&
				VisualStyleRenderer.IsElementDefined (VisualStyleElement.ComboBox.Border.Hot))
				return true;
			return base.CombBoxBackgroundHasHotElementStyle (comboBox);
		}
		#endregion
		#region ControlPaint
		#region DrawButton
		public override void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat ||
				(state & ButtonState.Checked) == ButtonState.Checked) {
				base.CPDrawButton (dc, rectangle, state);
				return;
			}
			VisualStyleElement element;
			if ((state & ButtonState.Inactive) == ButtonState.Inactive)
				element = VisualStyleElement.Button.PushButton.Disabled;
			else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
				element = VisualStyleElement.Button.PushButton.Pressed;
			else
				element = VisualStyleElement.Button.PushButton.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawButton (dc, rectangle, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, rectangle);
		}
		#endregion
		#region DrawCaptionButton
		public override void CPDrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat ||
				(state & ButtonState.Checked) == ButtonState.Checked) {
				base.CPDrawCaptionButton (graphics, rectangle, button, state);
				return;
			}
			VisualStyleElement element = GetCaptionButtonVisualStyleElement (button, state);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawCaptionButton (graphics, rectangle, button, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (graphics, rectangle);
		}
		static VisualStyleElement GetCaptionButtonVisualStyleElement (CaptionButton button, ButtonState state)
		{
			switch (button) {
			case CaptionButton.Minimize:
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					return VisualStyleElement.Window.MinButton.Disabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					return VisualStyleElement.Window.MinButton.Pressed;
				else
					return VisualStyleElement.Window.MinButton.Normal;
			case CaptionButton.Maximize:
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					return VisualStyleElement.Window.MaxButton.Disabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					return VisualStyleElement.Window.MaxButton.Pressed;
				else
					return VisualStyleElement.Window.MaxButton.Normal;
			case CaptionButton.Close:
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					return VisualStyleElement.Window.CloseButton.Disabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					return VisualStyleElement.Window.CloseButton.Pressed;
				else
					return VisualStyleElement.Window.CloseButton.Normal;
			case CaptionButton.Restore:
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					return VisualStyleElement.Window.RestoreButton.Disabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					return VisualStyleElement.Window.RestoreButton.Pressed;
				else
					return VisualStyleElement.Window.RestoreButton.Normal;
			default:
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					return VisualStyleElement.Window.HelpButton.Disabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					return VisualStyleElement.Window.HelpButton.Pressed;
				else
					return VisualStyleElement.Window.HelpButton.Normal;
			}
		}
		#endregion
		#region DrawCheckBox
		public override void CPDrawCheckBox (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat) {
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
		#region DrawComboButton
		public override void CPDrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat ||
				(state & ButtonState.Checked) == ButtonState.Checked) {
				base.CPDrawComboButton (graphics, rectangle, state);
				return;
			}
			VisualStyleElement element;
			if ((state & ButtonState.Inactive) == ButtonState.Inactive)
				element = VisualStyleElement.ComboBox.DropDownButton.Disabled;
			else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
				element = VisualStyleElement.ComboBox.DropDownButton.Pressed;
			else
				element = VisualStyleElement.ComboBox.DropDownButton.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawComboButton (graphics, rectangle, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (graphics, rectangle);
		}
		#endregion
		#region DrawMixedCheckBox
		public override void CPDrawMixedCheckBox (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat) {
				base.CPDrawMixedCheckBox (dc, rectangle, state);
				return;
			}
			VisualStyleElement element;
			if ((state & ButtonState.Checked) == ButtonState.Checked)
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					element = VisualStyleElement.Button.CheckBox.MixedDisabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					element = VisualStyleElement.Button.CheckBox.MixedPressed;
				else
					element = VisualStyleElement.Button.CheckBox.MixedNormal;
			else
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					element = VisualStyleElement.Button.CheckBox.UncheckedDisabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					element = VisualStyleElement.Button.CheckBox.UncheckedPressed;
				else
					element = VisualStyleElement.Button.CheckBox.UncheckedNormal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawMixedCheckBox (dc, rectangle, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, rectangle);
		}
		#endregion
		#region DrawRadioButton
		public override void CPDrawRadioButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat) {
				base.CPDrawRadioButton (dc, rectangle, state);
				return;
			}
			VisualStyleElement element;
			if ((state & ButtonState.Checked) == ButtonState.Checked)
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					element = VisualStyleElement.Button.RadioButton.CheckedDisabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					element = VisualStyleElement.Button.RadioButton.CheckedPressed;
				else
					element = VisualStyleElement.Button.RadioButton.CheckedNormal;
			else
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					element = VisualStyleElement.Button.RadioButton.UncheckedDisabled;
				else if ((state & ButtonState.Pushed) == ButtonState.Pushed)
					element = VisualStyleElement.Button.RadioButton.UncheckedPressed;
				else
					element = VisualStyleElement.Button.RadioButton.UncheckedNormal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawRadioButton (dc, rectangle, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, rectangle);
		}
		#endregion
		#region DrawScrollButton
		public override void CPDrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state)
		{
			if (!RenderClientAreas ||
				(state & ButtonState.Flat) == ButtonState.Flat ||
				(state & ButtonState.Checked) == ButtonState.Checked) {
				base.CPDrawScrollButton (dc, area, type, state);
				return;
			}
			VisualStyleElement element = GetScrollButtonVisualStyleElement (type, state);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.CPDrawScrollButton (dc, area, type, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, area);
		}
		static VisualStyleElement GetScrollButtonVisualStyleElement (ScrollButton type, ButtonState state)
		{
			switch (type) {
			case ScrollButton.Left:
				if (IsDisabled (state))
					return VisualStyleElement.ScrollBar.ArrowButton.LeftDisabled;
				else if (IsPressed (state))
					return VisualStyleElement.ScrollBar.ArrowButton.LeftPressed;
				else
					return VisualStyleElement.ScrollBar.ArrowButton.LeftNormal;
			case ScrollButton.Right:
				if (IsDisabled (state))
					return VisualStyleElement.ScrollBar.ArrowButton.RightDisabled;
				else if (IsPressed (state))
					return VisualStyleElement.ScrollBar.ArrowButton.RightPressed;
				else
					return VisualStyleElement.ScrollBar.ArrowButton.RightNormal;
			case ScrollButton.Up:
				if (IsDisabled (state))
					return VisualStyleElement.ScrollBar.ArrowButton.UpDisabled;
				else if (IsPressed (state))
					return VisualStyleElement.ScrollBar.ArrowButton.UpPressed;
				else
					return VisualStyleElement.ScrollBar.ArrowButton.UpNormal;
			default:
				if (IsDisabled (state))
					return VisualStyleElement.ScrollBar.ArrowButton.DownDisabled;
				else if (IsPressed (state))
					return VisualStyleElement.ScrollBar.ArrowButton.DownPressed;
				else
					return VisualStyleElement.ScrollBar.ArrowButton.DownNormal;
			}
		}
		static bool IsDisabled (ButtonState state)
		{
			return (state & ButtonState.Inactive) == ButtonState.Inactive;
		}
		static bool IsPressed (ButtonState state)
		{
			return (state & ButtonState.Pushed) == ButtonState.Pushed;
		}
		#endregion
		#endregion
#if NET_2_0
		#region DataGridView
		#region DataGridViewHeaderCell
		#region DataGridViewRowHeaderCell
		public override bool DataGridViewRowHeaderCellDrawBackground (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds)
		{
			if (!RenderClientAreas ||
				!cell.DataGridView.EnableHeadersVisualStyles)
				return base.DataGridViewRowHeaderCellDrawBackground (cell, g, bounds);
			VisualStyleElement element = DataGridViewRowHeaderCellGetVisualStyleElement (cell);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.DataGridViewRowHeaderCellDrawBackground (cell, g, bounds);
			bounds.Width--;
			Bitmap bitmap = new Bitmap (bounds.Height, bounds.Width);
			Graphics bitmap_g = Graphics.FromImage (bitmap);
			Rectangle bitmap_rectangle = new Rectangle (Point.Empty, bitmap.Size);
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			if (!AreEqual (element, VisualStyleElement.Header.Item.Normal) && renderer.IsBackgroundPartiallyTransparent ())
				new VisualStyleRenderer (VisualStyleElement.Header.Item.Normal).DrawBackground (bitmap_g, bitmap_rectangle);
			renderer.DrawBackground (bitmap_g, bitmap_rectangle);
			bitmap_g.Dispose ();
			g.Transform = new Matrix(0, 1, 1, 0, 0, 0);
			g.DrawImage (bitmap, bounds.Y, bounds.X);
			bitmap.Dispose ();
			g.ResetTransform ();
			return true;
		}
		public override bool DataGridViewRowHeaderCellDrawSelectionBackground (DataGridViewRowHeaderCell cell)
		{
			if (!RenderClientAreas ||
				!cell.DataGridView.EnableHeadersVisualStyles || !VisualStyleRenderer.IsElementDefined (DataGridViewRowHeaderCellGetVisualStyleElement (cell)))
				return base.DataGridViewRowHeaderCellDrawSelectionBackground (cell);
			return true;
		}
		public override bool DataGridViewRowHeaderCellDrawBorder (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds)
		{
			if (!RenderClientAreas ||
				!cell.DataGridView.EnableHeadersVisualStyles || !VisualStyleRenderer.IsElementDefined (DataGridViewRowHeaderCellGetVisualStyleElement (cell)))
				return base.DataGridViewRowHeaderCellDrawBorder (cell, g, bounds);
			g.DrawLine (cell.GetBorderPen (), bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			return true;
		}
		static VisualStyleElement DataGridViewRowHeaderCellGetVisualStyleElement (DataGridViewRowHeaderCell cell)
		{
			if (cell.DataGridView.PressedHeaderCell == cell)
				return VisualStyleElement.Header.Item.Pressed;
			if (cell.DataGridView.EnteredHeaderCell == cell)
				return VisualStyleElement.Header.Item.Hot;
			if (cell.OwningRow.SelectedInternal)
				return VisualStyleElement.Header.Item.Pressed;
			return VisualStyleElement.Header.Item.Normal;
		}
		#endregion
		#region DataGridViewColumnHeaderCell
		public override bool DataGridViewColumnHeaderCellDrawBackground (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds)
		{
			if (!RenderClientAreas ||
				!cell.DataGridView.EnableHeadersVisualStyles || cell is DataGridViewTopLeftHeaderCell)
				return base.DataGridViewColumnHeaderCellDrawBackground (cell, g, bounds);
			VisualStyleElement element = DataGridViewColumnHeaderCellGetVisualStyleElement (cell);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.DataGridViewColumnHeaderCellDrawBackground (cell, g, bounds);
			bounds.Height--;
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			if (!AreEqual (element, VisualStyleElement.Header.Item.Normal) && renderer.IsBackgroundPartiallyTransparent ())
			    new VisualStyleRenderer (VisualStyleElement.Header.Item.Normal).DrawBackground (g, bounds);
			renderer.DrawBackground (g, bounds);
			return true;
		}
		public override bool DataGridViewColumnHeaderCellDrawBorder (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds)
		{
			if (!RenderClientAreas ||
				!cell.DataGridView.EnableHeadersVisualStyles ||
				cell is DataGridViewTopLeftHeaderCell ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.Header.Item.Normal))
				return base.DataGridViewColumnHeaderCellDrawBorder (cell, g, bounds);
			g.DrawLine (cell.GetBorderPen (), bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			return true;
		}
		static VisualStyleElement DataGridViewColumnHeaderCellGetVisualStyleElement (DataGridViewColumnHeaderCell cell)
		{
			if (cell.DataGridView.PressedHeaderCell == cell)
				return VisualStyleElement.Header.Item.Pressed;
			if (cell.DataGridView.EnteredHeaderCell == cell)
				return VisualStyleElement.Header.Item.Hot;
			return VisualStyleElement.Header.Item.Normal;
		}
		#endregion
		public override bool DataGridViewHeaderCellHasPressedStyle (DataGridView dataGridView)
		{
			if (!RenderClientAreas ||
				!dataGridView.EnableHeadersVisualStyles ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.Header.Item.Pressed))
				return base.DataGridViewHeaderCellHasPressedStyle (dataGridView);
			return true;
		}
		public override bool DataGridViewHeaderCellHasHotStyle (DataGridView dataGridView)
		{
			if (!RenderClientAreas ||
				!dataGridView.EnableHeadersVisualStyles ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.Header.Item.Hot))
				return base.DataGridViewHeaderCellHasHotStyle (dataGridView);
			return true;
		}
		#endregion
		#endregion
#endif
		#region DateTimePicker
		#region Border
		protected override void DateTimePickerDrawBorder (DateTimePicker dateTimePicker, Graphics g, Rectangle clippingArea)
		{
			if (!RenderClientAreas) {
				base.DateTimePickerDrawBorder (dateTimePicker, g, clippingArea);
				return;
			}
			VisualStyleElement element;
			if (!dateTimePicker.Enabled)
				element = VisualStyleElement.DatePicker.DateBorder.Disabled;
			else if (dateTimePicker.Entered)
				element = VisualStyleElement.DatePicker.DateBorder.Hot;
			else if (dateTimePicker.Focused)
				element = VisualStyleElement.DatePicker.DateBorder.Focused;
			else
				element = VisualStyleElement.DatePicker.DateBorder.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DateTimePickerDrawBorder (dateTimePicker, g, clippingArea);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, new Rectangle (Point.Empty, dateTimePicker.Size), clippingArea);
		}
		public override bool DateTimePickerBorderHasHotElementStyle {
			get {
				if (RenderClientAreas &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.DatePicker.DateBorder.Hot))
					return true;
				return base.DateTimePickerBorderHasHotElementStyle;
			}
		}
		#endregion
		#region Drop down button
		protected override void DateTimePickerDrawDropDownButton (DateTimePicker dateTimePicker, Graphics g, Rectangle clippingArea)
		{
			if (!RenderClientAreas) {
				base.DateTimePickerDrawDropDownButton (dateTimePicker, g, clippingArea);
				return;
			}
			VisualStyleElement element;
			if (!dateTimePicker.Enabled)
				element = VisualStyleElement.DatePicker.ShowCalendarButtonRight.Disabled;
			else if (dateTimePicker.is_drop_down_visible)
				element = VisualStyleElement.DatePicker.ShowCalendarButtonRight.Pressed;
			else if (dateTimePicker.DropDownButtonEntered)
				element = VisualStyleElement.DatePicker.ShowCalendarButtonRight.Hot;
			else
				element = VisualStyleElement.DatePicker.ShowCalendarButtonRight.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DateTimePickerDrawDropDownButton (dateTimePicker, g, clippingArea);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, dateTimePicker.drop_down_arrow_rect, clippingArea);
		}
		//TODO: Until somebody figures out how to obtain the proper width this will need to be updated when new Windows versions/themes are released.
		const int DateTimePickerDropDownWidthOnWindowsVista = 34;
		const int DateTimePickerDropDownHeightOnWindowsVista = 20;
		public override Rectangle DateTimePickerGetDropDownButtonArea (DateTimePicker dateTimePicker)
		{
			if (!RenderClientAreas)
				return base.DateTimePickerGetDropDownButtonArea (dateTimePicker);
			VisualStyleElement element = VisualStyleElement.DatePicker.ShowCalendarButtonRight.Pressed;
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.DateTimePickerGetDropDownButtonArea (dateTimePicker);
			Size size = new Size (DateTimePickerDropDownWidthOnWindowsVista, DateTimePickerDropDownHeightOnWindowsVista);
			return new Rectangle (dateTimePicker.Width - size.Width, 0, size.Width, size.Height);
		}
		public override Rectangle DateTimePickerGetDateArea (DateTimePicker dateTimePicker)
		{
			if (!RenderClientAreas ||
				dateTimePicker.ShowUpDown)
				return base.DateTimePickerGetDateArea (dateTimePicker);
			VisualStyleElement element = VisualStyleElement.DatePicker.DateBorder.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.DateTimePickerGetDateArea (dateTimePicker);
			Graphics g = dateTimePicker.CreateGraphics ();
			Rectangle result = new VisualStyleRenderer (element).GetBackgroundContentRectangle (g, dateTimePicker.ClientRectangle);
			g.Dispose ();
			result.Width -= DateTimePickerDropDownWidthOnWindowsVista;
			return result;
		}
		public override bool DateTimePickerDropDownButtonHasHotElementStyle {
			get {
				if (RenderClientAreas &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.DatePicker.ShowCalendarButtonRight.Hot))
					return true;
				return base.DateTimePickerDropDownButtonHasHotElementStyle;
			}
		}
		#endregion
		#endregion
		#region ListView
		protected override void ListViewDrawColumnHeaderBackground (ListView listView, ColumnHeader columnHeader, Graphics g, Rectangle area, Rectangle clippingArea)
		{
			if (!RenderClientAreas) {
				base.ListViewDrawColumnHeaderBackground (listView, columnHeader, g, area, clippingArea);
				return;
			}
			VisualStyleElement element;
			if (listView.HeaderStyle == ColumnHeaderStyle.Clickable)
				if (columnHeader.Pressed)
					element = VisualStyleElement.Header.Item.Pressed;
				else if (columnHeader == listView.EnteredColumnHeader)
					element = VisualStyleElement.Header.Item.Hot;
				else
					element = VisualStyleElement.Header.Item.Normal;
			else
				element = VisualStyleElement.Header.Item.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ListViewDrawColumnHeaderBackground (listView, columnHeader, g, area, clippingArea);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, area, clippingArea);
		}
		protected override void ListViewDrawUnusedHeaderBackground (ListView listView, Graphics g, Rectangle area, Rectangle clippingArea)
		{
			if (!RenderClientAreas) {
				base.ListViewDrawUnusedHeaderBackground (listView, g, area, clippingArea);
				return;
			}
			VisualStyleElement element = VisualStyleElement.Header.Item.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ListViewDrawUnusedHeaderBackground (listView, g, area, clippingArea);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, area, clippingArea);
		}
		public override bool ListViewHasHotHeaderStyle {
			get {
				if (!RenderClientAreas ||
					!VisualStyleRenderer.IsElementDefined (VisualStyleElement.Header.Item.Hot))
					return base.ListViewHasHotHeaderStyle;
				return true;
			}
		}
		public override int ListViewGetHeaderHeight (ListView listView, Font font)
		{
			if (!RenderClientAreas)
				return base.ListViewGetHeaderHeight (listView, font);
			VisualStyleElement element = VisualStyleElement.Header.Item.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.ListViewGetHeaderHeight (listView, font);
			Control control = null;
			Graphics g;
			if (listView == null) {
				control = new Control ();
				g = control.CreateGraphics ();
			} else
				g = listView.CreateGraphics ();
			int result = new VisualStyleRenderer (element).GetPartSize (g, ThemeSizeType.True).Height;
			g.Dispose ();
			if (listView == null)
				control.Dispose ();
			return result;
		}
		#endregion
		#region GroupBox
		public override void DrawGroupBox (Graphics dc, Rectangle area, GroupBox box)
		{
			GroupBoxRenderer.DrawGroupBox (
				dc,
				new Rectangle (Point.Empty, box.Size),
				box.Text,
				box.Font,
				box.ForeColor == GroupBox.DefaultForeColor ? Color.Empty : box.ForeColor,
				box.Enabled ? GroupBoxState.Normal : GroupBoxState.Disabled);
		}
		#endregion
		#region Managed window
		Rectangle ManagedWindowGetTitleBarRectangle (InternalWindowManager wm)
		{
			return new Rectangle (0, 0, wm.Form.Width, ManagedWindowTitleBarHeight (wm) + ManagedWindowBorderWidth (wm) * (wm.IsMinimized ? 2 : 1));
		}
		Region ManagedWindowGetWindowRegion (Form form)
		{
			if (form.WindowManager is MdiWindowManager && form.WindowManager.IsMaximized)
				return null;
			VisualStyleElement title_bar_element = ManagedWindowGetTitleBarVisualStyleElement (form.WindowManager);
			if (!VisualStyleRenderer.IsElementDefined (title_bar_element))
				return null;
			VisualStyleRenderer renderer = new VisualStyleRenderer (title_bar_element);
			if (!renderer.IsBackgroundPartiallyTransparent ())
				return null;
			IDeviceContext dc = GetMeasurementDeviceContext ();
			Rectangle title_bar_rectangle = ManagedWindowGetTitleBarRectangle (form.WindowManager);
			Region region = renderer.GetBackgroundRegion (dc, title_bar_rectangle);
			ReleaseMeasurementDeviceContext (dc);
			region.Union (new Rectangle (0, title_bar_rectangle.Bottom, form.Width, form.Height));
			return region;
		}
		public override void ManagedWindowOnSizeInitializedOrChanged (Form form)
		{
			base.ManagedWindowOnSizeInitializedOrChanged (form);
			if (!render_non_client_areas)
				return;
			form.Region = ManagedWindowGetWindowRegion (form);
		}
		protected override Rectangle ManagedWindowDrawTitleBarAndBorders (Graphics dc, Rectangle clip, InternalWindowManager wm)
		{
			if (!render_non_client_areas)
				return base.ManagedWindowDrawTitleBarAndBorders (dc, clip, wm);
			VisualStyleElement title_bar_element = ManagedWindowGetTitleBarVisualStyleElement (wm);
			VisualStyleElement left_border_element;
			VisualStyleElement right_border_element;
			VisualStyleElement bottom_border_element;
			ManagedWindowGetBorderVisualStyleElements (wm, out left_border_element, out right_border_element, out bottom_border_element);
			if (!VisualStyleRenderer.IsElementDefined (title_bar_element) ||
				(!wm.IsMinimized && (
				!VisualStyleRenderer.IsElementDefined (left_border_element) ||
				!VisualStyleRenderer.IsElementDefined (right_border_element) ||
				!VisualStyleRenderer.IsElementDefined (bottom_border_element))))
				return base.ManagedWindowDrawTitleBarAndBorders (dc, clip, wm);
			VisualStyleRenderer renderer = new VisualStyleRenderer (title_bar_element);
			Rectangle title_bar_rectangle = ManagedWindowGetTitleBarRectangle (wm);
			renderer.DrawBackground (dc, title_bar_rectangle, clip);
			if (!wm.IsMinimized) {
				int border_width = ManagedWindowBorderWidth (wm);
				renderer.SetParameters (left_border_element);
				renderer.DrawBackground (dc, new Rectangle (
					0,
					title_bar_rectangle.Bottom,
					border_width,
					wm.Form.Height - title_bar_rectangle.Bottom
					), clip);
				renderer.SetParameters (right_border_element);
				renderer.DrawBackground (dc, new Rectangle (
					wm.Form.Width - border_width,
					title_bar_rectangle.Bottom,
					border_width,
					wm.Form.Height - title_bar_rectangle.Bottom
					), clip);
				renderer.SetParameters (bottom_border_element);
				renderer.DrawBackground (dc, new Rectangle (
					0,
					wm.Form.Height - border_width,
					wm.Form.Width,
					border_width
					), clip);
			}
			return title_bar_rectangle;
		}
		static FormWindowState ManagedWindowGetWindowState (InternalWindowManager wm)
		{
			return wm.GetWindowState ();
		}
		static bool ManagedWindowIsDisabled (InternalWindowManager wm)
		{
			return !wm.Form.Enabled;
		}
		static bool ManagedWindowIsActive (InternalWindowManager wm)
		{
			return wm.IsActive;
		}
		static VisualStyleElement ManagedWindowGetTitleBarVisualStyleElement (InternalWindowManager wm)
		{
			if (wm.IsToolWindow)
				#region Small window
				switch (ManagedWindowGetWindowState (wm)) {
				case FormWindowState.Minimized:
					if (ManagedWindowIsDisabled (wm))
						return VisualStyleElement.Window.SmallMinCaption.Disabled;
					else if (ManagedWindowIsActive (wm))
						return VisualStyleElement.Window.SmallMinCaption.Active;
					return VisualStyleElement.Window.SmallMinCaption.Inactive;
				case FormWindowState.Maximized:
					if (ManagedWindowIsDisabled (wm))
						return VisualStyleElement.Window.SmallMaxCaption.Disabled;
					else if (ManagedWindowIsActive (wm))
						return VisualStyleElement.Window.SmallMaxCaption.Active;
					return VisualStyleElement.Window.SmallMaxCaption.Inactive;
				default:
					if (ManagedWindowIsDisabled (wm))
						return VisualStyleElement.Window.SmallCaption.Disabled;
					else if (ManagedWindowIsActive (wm))
						return VisualStyleElement.Window.SmallCaption.Active;
					return VisualStyleElement.Window.SmallCaption.Inactive;
				}
				#endregion
			else
				#region Normal window
				switch (ManagedWindowGetWindowState (wm)) {
				case FormWindowState.Minimized:
					if (ManagedWindowIsDisabled (wm))
						return VisualStyleElement.Window.MinCaption.Disabled;
					else if (ManagedWindowIsActive (wm))
						return VisualStyleElement.Window.MinCaption.Active;
					return VisualStyleElement.Window.MinCaption.Inactive;
				case FormWindowState.Maximized:
					if (ManagedWindowIsDisabled (wm))
						return VisualStyleElement.Window.MaxCaption.Disabled;
					else if (ManagedWindowIsActive (wm))
						return VisualStyleElement.Window.MaxCaption.Active;
					return VisualStyleElement.Window.MaxCaption.Inactive;
				default:
					if (ManagedWindowIsDisabled (wm))
						return VisualStyleElement.Window.Caption.Disabled;
					else if (ManagedWindowIsActive (wm))
						return VisualStyleElement.Window.Caption.Active;
					return VisualStyleElement.Window.Caption.Inactive;
				}
				#endregion
		}
		static void ManagedWindowGetBorderVisualStyleElements (InternalWindowManager wm, out VisualStyleElement left, out VisualStyleElement right, out VisualStyleElement bottom)
		{
			bool active = !ManagedWindowIsDisabled (wm) && ManagedWindowIsActive (wm);
			if (wm.IsToolWindow) {
				if (active) {
					left = VisualStyleElement.Window.SmallFrameLeft.Active;
					right = VisualStyleElement.Window.SmallFrameRight.Active;
					bottom = VisualStyleElement.Window.SmallFrameBottom.Active;
				} else {
					left = VisualStyleElement.Window.SmallFrameLeft.Inactive;
					right = VisualStyleElement.Window.SmallFrameRight.Inactive;
					bottom = VisualStyleElement.Window.SmallFrameBottom.Inactive;
				}
			} else {
				if (active) {
					left = VisualStyleElement.Window.FrameLeft.Active;
					right = VisualStyleElement.Window.FrameRight.Active;
					bottom = VisualStyleElement.Window.FrameBottom.Active;
				} else {
					left = VisualStyleElement.Window.FrameLeft.Inactive;
					right = VisualStyleElement.Window.FrameRight.Inactive;
					bottom = VisualStyleElement.Window.FrameBottom.Inactive;
				}
			}
		}
		public override bool ManagedWindowTitleButtonHasHotElementStyle (TitleButton button, Form form)
		{
			if (render_non_client_areas && (button.State & ButtonState.Inactive) != ButtonState.Inactive) {
				VisualStyleElement element;
				if (ManagedWindowIsMaximizedMdiChild (form))
					switch (button.Caption) {
					case CaptionButton.Close:
						element = VisualStyleElement.Window.MdiCloseButton.Hot;
						break;
					case CaptionButton.Help:
						element = VisualStyleElement.Window.MdiHelpButton.Hot;
						break;
					case CaptionButton.Minimize:
						element = VisualStyleElement.Window.MdiMinButton.Hot;
						break;
					default:
						element = VisualStyleElement.Window.MdiRestoreButton.Hot;
						break;
					}
				else if (form.WindowManager.IsToolWindow)
					element = VisualStyleElement.Window.SmallCloseButton.Hot;
				else
					switch (button.Caption) {
					case CaptionButton.Close:
						element = VisualStyleElement.Window.CloseButton.Hot;
						break;
					case CaptionButton.Help:
						element = VisualStyleElement.Window.HelpButton.Hot;
						break;
					case CaptionButton.Maximize:
						element = VisualStyleElement.Window.MaxButton.Hot;
						break;
					case CaptionButton.Minimize:
						element = VisualStyleElement.Window.MinButton.Hot;
						break;
					default:
						element = VisualStyleElement.Window.RestoreButton.Hot;
						break;
					}
				if (VisualStyleRenderer.IsElementDefined (element))
					return true;
			}
			return base.ManagedWindowTitleButtonHasHotElementStyle (button, form);
		}
		static bool ManagedWindowIsMaximizedMdiChild (Form form)
		{
			return form.WindowManager is MdiWindowManager &&
				ManagedWindowGetWindowState (form.WindowManager) == FormWindowState.Maximized;
		}
		static bool ManagedWindowTitleButtonIsDisabled (TitleButton button, InternalWindowManager wm)
		{
			return (button.State & ButtonState.Inactive) == ButtonState.Inactive;
		}
		static bool ManagedWindowTitleButtonIsPressed (TitleButton button)
		{
			return (button.State & ButtonState.Pushed) == ButtonState.Pushed;
		}
		static VisualStyleElement ManagedWindowGetTitleButtonVisualStyleElement (TitleButton button, Form form)
		{
			if (form.WindowManager.IsToolWindow) {
				if (ManagedWindowTitleButtonIsDisabled (button, form.WindowManager))
					return VisualStyleElement.Window.SmallCloseButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.SmallCloseButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.SmallCloseButton.Hot;
				return VisualStyleElement.Window.SmallCloseButton.Normal;
			}
			switch (button.Caption) {
			case CaptionButton.Close:
				if (ManagedWindowTitleButtonIsDisabled (button, form.WindowManager))
					return VisualStyleElement.Window.CloseButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.CloseButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.CloseButton.Hot;
				return VisualStyleElement.Window.CloseButton.Normal;
			case CaptionButton.Help:
				if (ManagedWindowTitleButtonIsDisabled (button, form.WindowManager))
					return VisualStyleElement.Window.HelpButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.HelpButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.HelpButton.Hot;
				return VisualStyleElement.Window.HelpButton.Normal;
			case CaptionButton.Maximize:
				if (ManagedWindowTitleButtonIsDisabled (button, form.WindowManager))
					return VisualStyleElement.Window.MaxButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.MaxButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.MaxButton.Hot;
				return VisualStyleElement.Window.MaxButton.Normal;
			case CaptionButton.Minimize:
				if (ManagedWindowTitleButtonIsDisabled (button, form.WindowManager))
					return VisualStyleElement.Window.MinButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.MinButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.MinButton.Hot;
				return VisualStyleElement.Window.MinButton.Normal;
			default:
				if (ManagedWindowTitleButtonIsDisabled (button, form.WindowManager))
					return VisualStyleElement.Window.RestoreButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.RestoreButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.RestoreButton.Hot;
				return VisualStyleElement.Window.RestoreButton.Normal;
			}
		}
		protected override void ManagedWindowDrawTitleButton (Graphics dc, TitleButton button, Rectangle clip, Form form)
		{
			if (!render_non_client_areas) {
				base.ManagedWindowDrawTitleButton (dc, button, clip, form);
				return;
			}
			VisualStyleElement element = ManagedWindowGetTitleButtonVisualStyleElement (button, form);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ManagedWindowDrawTitleButton (dc, button, clip, form);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, button.Rectangle, clip);
		}
		public override Size ManagedWindowButtonSize (InternalWindowManager wm)
		{
			if (!render_non_client_areas)
				return base.ManagedWindowButtonSize (wm);
			VisualStyleElement element = wm.IsToolWindow && !wm.IsMinimized ?
				VisualStyleElement.Window.SmallCloseButton.Normal :
				VisualStyleElement.Window.CloseButton.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.ManagedWindowButtonSize (wm);
			IDeviceContext dc = GetMeasurementDeviceContext ();
			Size result = new VisualStyleRenderer (element).GetPartSize (dc, ThemeSizeType.True);
			ReleaseMeasurementDeviceContext (dc);
			return result;
		}
		public override void ManagedWindowDrawMenuButton (Graphics dc, TitleButton button, Rectangle clip, InternalWindowManager wm)
		{
			if (!render_non_client_areas) {
				base.ManagedWindowDrawMenuButton (dc, button, clip, wm);
				return;
			}
			VisualStyleElement element = ManagedWindowGetMenuButtonVisualStyleElement (button, wm);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ManagedWindowDrawMenuButton (dc, button, clip, wm);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, button.Rectangle, clip);
		}
		static VisualStyleElement ManagedWindowGetMenuButtonVisualStyleElement (TitleButton button, InternalWindowManager wm)
		{
			switch (button.Caption) {
			case CaptionButton.Close:
				if (ManagedWindowTitleButtonIsDisabled (button, wm))
					return VisualStyleElement.Window.MdiCloseButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.MdiCloseButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.MdiCloseButton.Hot;
				return VisualStyleElement.Window.MdiCloseButton.Normal;
			case CaptionButton.Help:
				if (ManagedWindowTitleButtonIsDisabled (button, wm))
					return VisualStyleElement.Window.MdiHelpButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.MdiHelpButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.MdiHelpButton.Hot;
				return VisualStyleElement.Window.MdiHelpButton.Normal;
			case CaptionButton.Minimize:
				if (ManagedWindowTitleButtonIsDisabled (button, wm))
					return VisualStyleElement.Window.MdiMinButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.MdiMinButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.MdiMinButton.Hot;
				return VisualStyleElement.Window.MdiMinButton.Normal;
			default:
				if (ManagedWindowTitleButtonIsDisabled (button, wm))
					return VisualStyleElement.Window.MdiRestoreButton.Disabled;
				if (ManagedWindowTitleButtonIsPressed (button))
					return VisualStyleElement.Window.MdiRestoreButton.Pressed;
				if (button.Entered)
					return VisualStyleElement.Window.MdiRestoreButton.Hot;
				return VisualStyleElement.Window.MdiRestoreButton.Normal;
			}
		}
		#endregion
		#region ProgressBar
		public override void DrawProgressBar (Graphics dc, Rectangle clip_rect, ProgressBar ctrl)
		{
			if (!RenderClientAreas ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.ProgressBar.Bar.Normal) ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.ProgressBar.Chunk.Normal)) {
				base.DrawProgressBar (dc, clip_rect, ctrl);
				return;
			}
			VisualStyleRenderer renderer = new VisualStyleRenderer (VisualStyleElement.ProgressBar.Bar.Normal);
			renderer.DrawBackground (dc, ctrl.ClientRectangle, clip_rect);
			Rectangle client_area = renderer.GetBackgroundContentRectangle (dc, new Rectangle (Point.Empty, ctrl.Size));
			renderer = new VisualStyleRenderer (VisualStyleElement.ProgressBar.Chunk.Normal);
			/* Draw Blocks */
			int draw_mode = 0;
			int max_blocks = int.MaxValue;
			int start_pixel = client_area.X;
#if NET_2_0
			draw_mode = (int)ctrl.Style;
#endif
			switch (draw_mode) {
#if NET_2_0
			case 1: // Continuous
				client_area.Width = (int)(client_area.Width * ((double)(ctrl.Value - ctrl.Minimum) / (double)(Math.Max (ctrl.Maximum - ctrl.Minimum, 1))));
				renderer.DrawBackground (dc, client_area, clip_rect);
				break;
			case 2: // Marquee
				int ms_diff = (int)(DateTime.Now - ctrl.start).TotalMilliseconds;
				double percent_done = (double) ms_diff / ProgressBarMarqueeSpeedScaling 
					% (double)ctrl.MarqueeAnimationSpeed / (double)ctrl.MarqueeAnimationSpeed;
				max_blocks = 5;
				start_pixel = client_area.X + (int)(client_area.Width * percent_done);
				goto default;
#endif
			default: // Blocks
				int block_width = renderer.GetInteger (IntegerProperty.ProgressChunkSize);
				block_width = Math.Max (block_width, 0); // block_width is used to break out the loop below, it must be >= 0!
				int first_pixel_outside_filled_area = (int)(((double)(ctrl.Value - ctrl.Minimum) * client_area.Width) / (Math.Max (ctrl.Maximum - ctrl.Minimum, 1))) + client_area.X;
				int block_count = 0;
				int increment = block_width + renderer.GetInteger (IntegerProperty.ProgressSpaceSize);
				Rectangle block_rect = new Rectangle (start_pixel, client_area.Y, block_width, client_area.Height);
				while (true) {
					if (max_blocks != int.MaxValue) {
						if (block_count == max_blocks)
							break;
						if (block_rect.Right >= client_area.Width)
							block_rect.X -= client_area.Width;
					} else {
						if (block_rect.X >= first_pixel_outside_filled_area)
							break;
						if (block_rect.Right >= first_pixel_outside_filled_area)
							if (first_pixel_outside_filled_area == client_area.Right)
								block_rect.Width = first_pixel_outside_filled_area - block_rect.X;
							else
								break;
					}
					if (clip_rect.IntersectsWith (block_rect))
						renderer.DrawBackground (dc, block_rect, clip_rect);
					block_rect.X += increment;
					block_count++;
				}
				break;
			}
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
		#region ScrollBar
		public override void DrawScrollBar (Graphics dc, Rectangle clip, ScrollBar bar)
		{
			if (!RenderClientAreas ||
				!ScrollBarAreElementsDefined) {
				base.DrawScrollBar (dc, clip, bar);
				return;
			}
			VisualStyleElement element;
			VisualStyleRenderer renderer;
			int scroll_button_width = bar.scrollbutton_width;
			int scroll_button_height = bar.scrollbutton_height;
			if (bar.vert) {
				bar.FirstArrowArea = new Rectangle (0, 0, bar.Width, scroll_button_height);
				bar.SecondArrowArea = new Rectangle (
					0,
					bar.ClientRectangle.Height - scroll_button_height,
					bar.Width,
					scroll_button_height);
				Rectangle thumb_pos = bar.ThumbPos;
				thumb_pos.Width = bar.Width;
				bar.ThumbPos = thumb_pos;
				#region Background, upper track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Backwards)
					element = VisualStyleElement.ScrollBar.LowerTrackVertical.Pressed;
				else
					element = bar.Enabled ?
						VisualStyleElement.ScrollBar.LowerTrackVertical.Normal :
						VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled;
				renderer = new VisualStyleRenderer (element);
				Rectangle upper_track_rect = new Rectangle (
					0,
					0,
					bar.ClientRectangle.Width,
					bar.ThumbPos.Top);
				if (clip.IntersectsWith (upper_track_rect))
					renderer.DrawBackground (dc, upper_track_rect, clip);
				#endregion
				#region Background, lower track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Forward)
					element = VisualStyles.VisualStyleElement.ScrollBar.LowerTrackVertical.Pressed;
				else
					element = bar.Enabled ?
						VisualStyleElement.ScrollBar.LowerTrackVertical.Normal :
						VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled;
				renderer = new VisualStyleRenderer (element);
				Rectangle lower_track_rect = new Rectangle (
					0,
					bar.ThumbPos.Bottom,
					bar.ClientRectangle.Width,
					bar.ClientRectangle.Height - bar.ThumbPos.Bottom);
				if (clip.IntersectsWith (lower_track_rect))
					renderer.DrawBackground (dc, lower_track_rect, clip);
				#endregion
				#region Buttons
				if (clip.IntersectsWith (bar.FirstArrowArea)) {
					if (!bar.Enabled)
						element = VisualStyleElement.ScrollBar.ArrowButton.UpDisabled;
					else if (bar.firstbutton_state == ButtonState.Pushed)
						element = VisualStyleElement.ScrollBar.ArrowButton.UpPressed;
					else if (bar.FirstButtonEntered)
						element = VisualStyleElement.ScrollBar.ArrowButton.UpHot;
					else if (ScrollBarHasHoverArrowButtonStyleVisualStyles && bar.Entered)
						element = VisualStyleElement.ScrollBar.ArrowButton.UpHover;
					else
						element = VisualStyleElement.ScrollBar.ArrowButton.UpNormal;
					renderer = new VisualStyleRenderer (element);
					renderer.DrawBackground (dc, bar.FirstArrowArea);
				}
				if (clip.IntersectsWith (bar.SecondArrowArea)) {
					if (!bar.Enabled)
						element = VisualStyleElement.ScrollBar.ArrowButton.DownDisabled;
					else if (bar.secondbutton_state == ButtonState.Pushed)
						element = VisualStyleElement.ScrollBar.ArrowButton.DownPressed;
					else if (bar.SecondButtonEntered)
						element = VisualStyleElement.ScrollBar.ArrowButton.DownHot;
					else if (ScrollBarHasHoverArrowButtonStyleVisualStyles && bar.Entered)
						element = VisualStyleElement.ScrollBar.ArrowButton.DownHover;
					else
						element = VisualStyleElement.ScrollBar.ArrowButton.DownNormal;
					renderer = new VisualStyleRenderer (element);
					renderer.DrawBackground (dc, bar.SecondArrowArea);
				}
				#endregion
				#region Thumb and grip
				if (!bar.Enabled)
					element = VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled;
				else if (bar.ThumbPressed)
					element = VisualStyleElement.ScrollBar.ThumbButtonVertical.Pressed;
				else if (bar.ThumbEntered)
					element = VisualStyleElement.ScrollBar.ThumbButtonVertical.Hot;
				else
					element = VisualStyleElement.ScrollBar.ThumbButtonVertical.Normal;
				renderer = new VisualStyleRenderer (element);
				renderer.DrawBackground (dc, bar.ThumbPos, clip);

				if (bar.Enabled && bar.ThumbPos.Height >= 20) {
					element = VisualStyleElement.ScrollBar.GripperVertical.Normal;
					if (VisualStyleRenderer.IsElementDefined (element)) {
						renderer = new VisualStyleRenderer (element);
						renderer.DrawBackground (dc, bar.ThumbPos, clip);
					}
				}
				#endregion
			} else {
				bar.FirstArrowArea = new Rectangle (0, 0, scroll_button_width, bar.Height);
				bar.SecondArrowArea = new Rectangle (
					bar.ClientRectangle.Width - scroll_button_width,
					0,
					scroll_button_width,
					bar.Height);
				Rectangle thumb_pos = bar.ThumbPos;
				thumb_pos.Height = bar.Height;
				bar.ThumbPos = thumb_pos;
				#region Background, left track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Backwards)
					element = VisualStyleElement.ScrollBar.LeftTrackHorizontal.Pressed;
				else
					element = bar.Enabled ?
						VisualStyleElement.ScrollBar.LeftTrackHorizontal.Normal :
						VisualStyleElement.ScrollBar.LeftTrackHorizontal.Disabled;
				renderer = new VisualStyleRenderer (element);
				Rectangle left_track_rect = new Rectangle (
					0,
					0,
					bar.ThumbPos.Left,
					bar.ClientRectangle.Height);
				if (clip.IntersectsWith (left_track_rect))
					renderer.DrawBackground (dc, left_track_rect, clip);
				#endregion
				#region Background, right track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Forward)
					element = VisualStyleElement.ScrollBar.RightTrackHorizontal.Pressed;
				else
					element = bar.Enabled ?
						VisualStyleElement.ScrollBar.RightTrackHorizontal.Normal :
						VisualStyleElement.ScrollBar.RightTrackHorizontal.Disabled;
				renderer = new VisualStyleRenderer (element);
				Rectangle right_track_rect = new Rectangle (
					bar.ThumbPos.Right,
					0,
					bar.ClientRectangle.Width - bar.ThumbPos.Right,
					bar.ClientRectangle.Height);
				if (clip.IntersectsWith (right_track_rect))
					renderer.DrawBackground (dc, right_track_rect, clip);
				#endregion
				#region Buttons
				if (clip.IntersectsWith (bar.FirstArrowArea)) {
					if (!bar.Enabled)
						element = VisualStyleElement.ScrollBar.ArrowButton.LeftDisabled;
					else if (bar.firstbutton_state == ButtonState.Pushed)
						element = VisualStyleElement.ScrollBar.ArrowButton.LeftPressed;
					else if (bar.FirstButtonEntered)
						element = VisualStyleElement.ScrollBar.ArrowButton.LeftHot;
					else if (ScrollBarHasHoverArrowButtonStyleVisualStyles && bar.Entered)
						element = VisualStyleElement.ScrollBar.ArrowButton.LeftHover;
					else
						element = VisualStyleElement.ScrollBar.ArrowButton.LeftNormal;
					renderer = new VisualStyleRenderer (element);
					renderer.DrawBackground (dc, bar.FirstArrowArea);
				}
				if (clip.IntersectsWith (bar.SecondArrowArea)) {
					if (!bar.Enabled)
						element = VisualStyleElement.ScrollBar.ArrowButton.RightDisabled;
					else if (bar.secondbutton_state == ButtonState.Pushed)
						element = VisualStyleElement.ScrollBar.ArrowButton.RightPressed;
					else if (bar.SecondButtonEntered)
						element = VisualStyleElement.ScrollBar.ArrowButton.RightHot;
					else if (ScrollBarHasHoverArrowButtonStyleVisualStyles && bar.Entered)
						element = VisualStyleElement.ScrollBar.ArrowButton.RightHover;
					else
						element = VisualStyleElement.ScrollBar.ArrowButton.RightNormal;
					renderer = new VisualStyleRenderer (element);
					renderer.DrawBackground (dc, bar.SecondArrowArea);
				}
				#endregion
				#region Thumb and grip
				if (!bar.Enabled)
					element = VisualStyleElement.ScrollBar.RightTrackHorizontal.Disabled;
				else if (bar.ThumbPressed)
					element = VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Pressed;
				else if (bar.ThumbEntered)
					element = VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Hot;
				else
					element = VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Normal;
				renderer = new VisualStyleRenderer (element);
				renderer.DrawBackground (dc, bar.ThumbPos, clip);

				if (bar.Enabled && bar.ThumbPos.Height >= 20) {
					element = VisualStyleElement.ScrollBar.GripperHorizontal.Normal;
					if (VisualStyleRenderer.IsElementDefined (element)) {
						renderer = new VisualStyleRenderer (element);
						renderer.DrawBackground (dc, bar.ThumbPos, clip);
					}
				}
				#endregion
			}
		}
		public override bool ScrollBarHasHotElementStyles {
			get {
				if (!RenderClientAreas)
					return base.ScrollBarHasHotElementStyles;
				return ScrollBarAreElementsDefined;
			}
		}
		public override bool ScrollBarHasPressedThumbStyle {
			get {
				if (!RenderClientAreas)
					return base.ScrollBarHasPressedThumbStyle;
				return ScrollBarAreElementsDefined;
			}
		}
		const int WindowsVistaMajorVersion = 6;
		static bool ScrollBarHasHoverArrowButtonStyleVisualStyles =
			Environment.OSVersion.Version.Major >= WindowsVistaMajorVersion;
		public override bool ScrollBarHasHoverArrowButtonStyle {
			get {
				if (RenderClientAreas &&
					ScrollBarHasHoverArrowButtonStyleVisualStyles)
					return ScrollBarAreElementsDefined;
				return base.ScrollBarHasHoverArrowButtonStyle;
			}
		}
		static bool ScrollBarAreElementsDefined {
			get {
				return
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.ArrowButton.DownDisabled) &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.LeftTrackHorizontal.Disabled) &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled) &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.RightTrackHorizontal.Disabled) &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Disabled) &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.ThumbButtonVertical.Disabled) &&
					VisualStyleRenderer.IsElementDefined (VisualStyleElement.ScrollBar.UpperTrackVertical.Disabled);
			}
		}
		#endregion
		#region StatusBar
		protected override void DrawStatusBarBackground(Graphics dc, Rectangle clip, StatusBar sb) {
			if (!RenderClientAreas) {
				base.DrawStatusBarBackground (dc, clip, sb);
				return;
			}
			VisualStyleElement element = VisualStyleElement.Status.Bar.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DrawStatusBarBackground (dc, clip, sb);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, sb.ClientRectangle, clip);
		}
		protected override void DrawStatusBarSizingGrip (Graphics dc, Rectangle clip, StatusBar sb, Rectangle area)
		{
			if (!RenderClientAreas) {
				base.DrawStatusBarSizingGrip (dc, clip, sb, area);
				return;
			}
			VisualStyleElement element = VisualStyleElement.Status.Gripper.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DrawStatusBarSizingGrip (dc, clip, sb, area);
				return;
			}
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			Rectangle sizing_grip_rectangle = new Rectangle (Point.Empty, renderer.GetPartSize (dc, ThemeSizeType.True));
			sizing_grip_rectangle.X = sb.Width - sizing_grip_rectangle.Width;
			sizing_grip_rectangle.Y = sb.Height - sizing_grip_rectangle.Height;
			renderer.DrawBackground (dc, sizing_grip_rectangle, clip);
		}
		protected override void DrawStatusBarPanelBackground (Graphics dc, Rectangle area, StatusBarPanel panel)
		{
			if (!RenderClientAreas) {
				base.DrawStatusBarPanelBackground (dc, area, panel);
				return;
			}
			VisualStyleElement element = VisualStyleElement.Status.Pane.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DrawStatusBarPanelBackground (dc, area, panel);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, area);
		}
		#endregion
		#region TextBoxBase
		static bool TextBoxBaseShouldPaint (TextBoxBase textBoxBase)
		{
			return textBoxBase.BorderStyle == BorderStyle.Fixed3D;
		}
		static VisualStyleElement TextBoxBaseGetVisualStyleElement (TextBoxBase textBoxBase)
		{
			if (!textBoxBase.Enabled)
				return VisualStyleElement.TextBox.TextEdit.Disabled;
			if (textBoxBase.ReadOnly)
				return VisualStyleElement.TextBox.TextEdit.ReadOnly;
			if (textBoxBase.Entered)
				return VisualStyleElement.TextBox.TextEdit.Hot;
			if (textBoxBase.Focused)
				return VisualStyleElement.TextBox.TextEdit.Focused;
			return VisualStyleElement.TextBox.TextEdit.Normal;
		}
		public override void TextBoxBaseFillBackground (TextBoxBase textBoxBase, Graphics g, Rectangle clippingArea)
		{
			if (!RenderClientAreas ||
				!TextBoxBaseShouldPaint (textBoxBase)) {
				base.TextBoxBaseFillBackground (textBoxBase, g, clippingArea);
				return;
			}
			VisualStyleElement element = TextBoxBaseGetVisualStyleElement (textBoxBase);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TextBoxBaseFillBackground (textBoxBase, g, clippingArea);
				return;
			}
			Rectangle bounds = new Rectangle(Point.Empty, textBoxBase.Size);
			bounds.X -= (bounds.Width - textBoxBase.ClientSize.Width) / 2;
			bounds.Y -= (bounds.Height - textBoxBase.ClientSize.Height) / 2;
			new VisualStyleRenderer (element).DrawBackground (g, bounds, clippingArea);
		}
		public override bool TextBoxBaseHandleWmNcPaint (TextBoxBase textBoxBase, ref Message m)
		{
			if (!RenderClientAreas ||
				!TextBoxBaseShouldPaint (textBoxBase))
				return base.TextBoxBaseHandleWmNcPaint (textBoxBase, ref m);
			VisualStyleElement element = TextBoxBaseGetVisualStyleElement (textBoxBase);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.TextBoxBaseHandleWmNcPaint (textBoxBase, ref m);
			PaintEventArgs e = XplatUI.PaintEventStart (ref m, textBoxBase.Handle, false);
			new VisualStyleRenderer (element).DrawBackgroundExcludingArea (
				e.Graphics,
				new Rectangle (Point.Empty, textBoxBase.Size),
				new Rectangle (new Point ((textBoxBase.Width - textBoxBase.ClientSize.Width) / 2,
					(textBoxBase.Height - textBoxBase.ClientSize.Height) / 2),
					textBoxBase.ClientSize));
			XplatUI.PaintEventEnd (ref m, textBoxBase.Handle, false);
			return true;
		}
		public override bool TextBoxBaseShouldPaintBackground (TextBoxBase textBoxBase)
		{
			if (!RenderClientAreas ||
				!TextBoxBaseShouldPaint (textBoxBase))
				return base.TextBoxBaseShouldPaintBackground (textBoxBase);
			VisualStyleElement element = TextBoxBaseGetVisualStyleElement (textBoxBase);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.TextBoxBaseShouldPaintBackground (textBoxBase);
			return new VisualStyleRenderer (element).IsBackgroundPartiallyTransparent ();
		}
		#endregion
		#region ToolBar
		static bool ToolBarIsDisabled (ToolBarItem item)
		{
			return !item.Button.Enabled;
		}
		static bool ToolBarIsPressed (ToolBarItem item)
		{
			return item.Pressed;
		}
		static bool ToolBarIsChecked (ToolBarItem item)
		{
			return item.Button.Pushed;
		}
		static bool ToolBarIsHot (ToolBarItem item)
		{
			return item.Hilight;
		}
		#region Border
		protected override void DrawToolBarButtonBorder (Graphics dc, ToolBarItem item, bool is_flat)
		{
			if (!RenderClientAreas) {
				base.DrawToolBarButtonBorder (dc, item, is_flat);
				return;
			}
			if (item.Button.Style == ToolBarButtonStyle.Separator)
				return;
			VisualStyleElement element;
			if (item.Button.Style == ToolBarButtonStyle.DropDownButton)
				element = ToolBarGetDropDownButtonVisualStyleElement (item);
			else
				element = ToolBarGetButtonVisualStyleElement (item);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DrawToolBarButtonBorder (dc, item, is_flat);
				return;
			}
			Rectangle rectangle = item.Rectangle;
			if (item.Button.Style == ToolBarButtonStyle.DropDownButton && item.Button.Parent.DropDownArrows)
				rectangle.Width -= ToolBarDropDownWidth;
			new VisualStyleRenderer (element).DrawBackground (dc, rectangle);
		}
		private static VisualStyleElement ToolBarGetDropDownButtonVisualStyleElement (ToolBarItem item)
		{
			if (item.Button.Parent.DropDownArrows) {
				if (ToolBarIsDisabled (item))
					return VisualStyleElement.ToolBar.SplitButton.Disabled;
				if (ToolBarIsPressed (item))
					return VisualStyleElement.ToolBar.SplitButton.Pressed;
				if (ToolBarIsChecked (item))
					if (ToolBarIsHot (item))
						return VisualStyleElement.ToolBar.SplitButton.HotChecked;
					else
						return VisualStyleElement.ToolBar.SplitButton.Checked;
				if (ToolBarIsHot (item))
					return VisualStyleElement.ToolBar.SplitButton.Hot;
				return VisualStyleElement.ToolBar.SplitButton.Normal;
			} else {
				if (ToolBarIsDisabled (item))
					return VisualStyleElement.ToolBar.DropDownButton.Disabled;
				if (ToolBarIsPressed (item))
					return VisualStyleElement.ToolBar.DropDownButton.Pressed;
				if (ToolBarIsChecked (item))
					if (ToolBarIsHot (item))
						return VisualStyleElement.ToolBar.DropDownButton.HotChecked;
					else
						return VisualStyleElement.ToolBar.DropDownButton.Checked;
				if (ToolBarIsHot (item))
					return VisualStyleElement.ToolBar.DropDownButton.Hot;
				return VisualStyleElement.ToolBar.DropDownButton.Normal;
			}
		}
		private static VisualStyleElement ToolBarGetButtonVisualStyleElement (ToolBarItem item)
		{
			if (ToolBarIsDisabled (item))
				return VisualStyleElement.ToolBar.Button.Disabled;
			if (ToolBarIsPressed (item))
				return VisualStyleElement.ToolBar.Button.Pressed;
			if (ToolBarIsChecked (item))
				if (ToolBarIsHot (item))
					return VisualStyleElement.ToolBar.Button.HotChecked;
				else
					return VisualStyleElement.ToolBar.Button.Checked;
			if (ToolBarIsHot (item))
				return VisualStyleElement.ToolBar.Button.Hot;
			return VisualStyleElement.ToolBar.Button.Normal;
		}
		#endregion
		#region Separator
		protected override void DrawToolBarSeparator (Graphics dc, ToolBarItem item)
		{
			if (!RenderClientAreas) {
				base.DrawToolBarSeparator (dc, item);
				return;
			}
			VisualStyleElement element = ToolBarGetSeparatorVisualStyleElement (item);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DrawToolBarSeparator (dc, item);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, item.Rectangle);
		}
		static VisualStyleElement ToolBarGetSeparatorVisualStyleElement (ToolBarItem toolBarItem)
		{
			return toolBarItem.Button.Parent.Vertical ?
				VisualStyleElement.ToolBar.SeparatorVertical.Normal :
				VisualStyleElement.ToolBar.SeparatorHorizontal.Normal;
		}
		#endregion
		#region Toggle button background
		protected override void DrawToolBarToggleButtonBackground (Graphics dc, ToolBarItem item)
		{
			if (!RenderClientAreas ||
				!VisualStyleRenderer.IsElementDefined (ToolBarGetButtonVisualStyleElement (item)))
				base.DrawToolBarToggleButtonBackground (dc, item);
		}
		#endregion
		#region Drop down arrow
		protected override void DrawToolBarDropDownArrow (Graphics dc, ToolBarItem item, bool is_flat)
		{
			if (!RenderClientAreas) {
				base.DrawToolBarDropDownArrow (dc, item, is_flat);
				return;
			}
			VisualStyleElement element = ToolBarGetDropDownArrowVisualStyleElement (item);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.DrawToolBarDropDownArrow (dc, item, is_flat);
				return;
			}
			Rectangle rect = item.Rectangle;
			rect.X = item.Rectangle.Right - ToolBarDropDownWidth;
			rect.Width = ToolBarDropDownWidth;
			new VisualStyleRenderer (element).DrawBackground (dc, rect);
		}
		private static VisualStyleElement ToolBarGetDropDownArrowVisualStyleElement (ToolBarItem item)
		{
			if (ToolBarIsDisabled (item))
				return VisualStyleElement.ToolBar.SplitButtonDropDown.Disabled;
			if (ToolBarIsPressed (item))
				return VisualStyleElement.ToolBar.SplitButtonDropDown.Pressed;
			if (ToolBarIsChecked (item))
				if (ToolBarIsHot (item))
					return VisualStyleElement.ToolBar.SplitButtonDropDown.HotChecked;
				else
					return VisualStyleElement.ToolBar.SplitButtonDropDown.Checked;
			if (ToolBarIsHot (item))
				return VisualStyleElement.ToolBar.SplitButtonDropDown.Hot;
			return VisualStyleElement.ToolBar.SplitButtonDropDown.Normal;
		}
		#endregion
		public override bool ToolBarHasHotElementStyles (ToolBar toolBar)
		{
			if (!RenderClientAreas)
				return base.ToolBarHasHotElementStyles (toolBar);
			return true;
		}
		public override bool ToolBarHasHotCheckedElementStyles {
			get {
				if (!RenderClientAreas)
					return base.ToolBarHasHotCheckedElementStyles;
				return true;
			}
		}
		#endregion
		#region ToolTip
		protected override void ToolTipDrawBackground (Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control)
		{
			if (!RenderClientAreas) {
				base.ToolTipDrawBackground (dc, clip_rectangle, control);
				return;
			}
			VisualStyleElement element = VisualStyleElement.ToolTip.Standard.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.ToolTipDrawBackground (dc, clip_rectangle, control);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, control.ClientRectangle);
		}
		public override bool ToolTipTransparentBackground {
			get {
				if (!RenderClientAreas)
					return base.ToolTipTransparentBackground;
				VisualStyleElement element = VisualStyleElement.ToolTip.Standard.Normal;
				if (!VisualStyleRenderer.IsElementDefined (element))
					return base.ToolTipTransparentBackground;
				return new VisualStyleRenderer (element).IsBackgroundPartiallyTransparent ();
			}
		}
		#endregion
		#region TrackBar
		protected override Size TrackBarGetThumbSize (TrackBar trackBar)
		{
			if (!RenderClientAreas)
				return base.TrackBarGetThumbSize (trackBar);
			VisualStyleElement element = TrackBarGetThumbVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.TrackBarGetThumbSize (trackBar);
			Graphics g = trackBar.CreateGraphics ();
			Size result = new VisualStyleRenderer (element).GetPartSize (g, ThemeSizeType.True);
			g.Dispose ();
			return trackBar.Orientation == Orientation.Horizontal ? result : TrackBarRotateVerticalThumbSize (result);
		}
		static VisualStyleElement TrackBarGetThumbVisualStyleElement (TrackBar trackBar)
		{
			if (trackBar.Orientation == Orientation.Horizontal)
				switch (trackBar.TickStyle) {
				case TickStyle.BottomRight:
				case TickStyle.None:
					return TrackBarGetHorizontalThumbBottomVisualStyleElement (trackBar);
				case TickStyle.TopLeft:
					return TrackBarGetHorizontalThumbTopVisualStyleElement (trackBar);
				default:
					return TrackBarGetHorizontalThumbVisualStyleElement (trackBar);
				}
			else
				switch (trackBar.TickStyle) {
				case TickStyle.BottomRight:
				case TickStyle.None:
					return TrackBarGetVerticalThumbRightVisualStyleElement (trackBar);
				case TickStyle.TopLeft:
					return TrackBarGetVerticalThumbLeftVisualStyleElement (trackBar);
				default:
					return TrackBarGetVerticalThumbVisualStyleElement (trackBar);
				}
		}
		static Size TrackBarRotateVerticalThumbSize (Size value)
		{
			int temporary = value.Width;
			value.Width = value.Height;
			value.Height = temporary;
			return value;
		}
		#region Track
		protected override void TrackBarDrawHorizontalTrack (Graphics dc, Rectangle thumb_area, Point channel_startpoint, Rectangle clippingArea)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawHorizontalTrack (dc, thumb_area, channel_startpoint, clippingArea);
				return;
			}
			VisualStyleElement element = VisualStyleElement.TrackBar.Track.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawHorizontalTrack (dc, thumb_area, channel_startpoint, clippingArea);
				return;
			}
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			renderer.DrawBackground (dc, new Rectangle (channel_startpoint, new Size (thumb_area.Width, renderer.GetPartSize (dc, ThemeSizeType.True).Height)), clippingArea);
		}
		protected override void TrackBarDrawVerticalTrack (Graphics dc, Rectangle thumb_area, Point channel_startpoint, Rectangle clippingArea)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawVerticalTrack (dc, thumb_area, channel_startpoint, clippingArea);
				return;
			}
			VisualStyleElement element = VisualStyleElement.TrackBar.TrackVertical.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawVerticalTrack (dc, thumb_area, channel_startpoint, clippingArea);
				return;
			}
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			renderer.DrawBackground (dc, new Rectangle (channel_startpoint, new Size (renderer.GetPartSize (dc, ThemeSizeType.True).Width, thumb_area.Height)), clippingArea);
		}
		#endregion
		#region Thumb
		static bool TrackBarIsDisabled (TrackBar trackBar)
		{
			return !trackBar.Enabled;
		}
		static bool TrackBarIsHot (TrackBar trackBar)
		{
			return trackBar.ThumbEntered;
		}
		static bool TrackBarIsPressed (TrackBar trackBar)
		{
			return trackBar.thumb_pressed;
		}
		static bool TrackBarIsFocused (TrackBar trackBar)
		{
			return trackBar.Focused;
		}
		#region Horizontal
		protected override void TrackBarDrawHorizontalThumbBottom (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawHorizontalThumbBottom (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			VisualStyleElement element = TrackBarGetHorizontalThumbBottomVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawHorizontalThumbBottom (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, thumb_pos, clippingArea);
		}
		static VisualStyleElement TrackBarGetHorizontalThumbBottomVisualStyleElement (TrackBar trackBar)
		{
			if (TrackBarIsDisabled (trackBar))
				return VisualStyleElement.TrackBar.ThumbBottom.Disabled;
			else if (TrackBarIsPressed (trackBar))
				return VisualStyleElement.TrackBar.ThumbBottom.Pressed;
			else if (TrackBarIsHot (trackBar))
				return VisualStyleElement.TrackBar.ThumbBottom.Hot;
			else if (TrackBarIsFocused (trackBar))
				return VisualStyleElement.TrackBar.ThumbBottom.Focused;
			return VisualStyleElement.TrackBar.ThumbBottom.Normal;
		}
		protected override void TrackBarDrawHorizontalThumbTop (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawHorizontalThumbTop (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			VisualStyleElement element = TrackBarGetHorizontalThumbTopVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawHorizontalThumbTop (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, thumb_pos, clippingArea);
		}
		static VisualStyleElement TrackBarGetHorizontalThumbTopVisualStyleElement (TrackBar trackBar)
		{
			if (TrackBarIsDisabled (trackBar))
				return VisualStyleElement.TrackBar.ThumbTop.Disabled;
			else if (TrackBarIsPressed (trackBar))
				return VisualStyleElement.TrackBar.ThumbTop.Pressed;
			else if (TrackBarIsHot (trackBar))
				return VisualStyleElement.TrackBar.ThumbTop.Hot;
			else if (TrackBarIsFocused (trackBar))
				return VisualStyleElement.TrackBar.ThumbTop.Focused;
			return VisualStyleElement.TrackBar.ThumbTop.Normal;
		}
		protected override void TrackBarDrawHorizontalThumb (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawHorizontalThumb (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			VisualStyleElement element = TrackBarGetHorizontalThumbVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawHorizontalThumb (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, thumb_pos, clippingArea);
		}
		static VisualStyleElement TrackBarGetHorizontalThumbVisualStyleElement (TrackBar trackBar)
		{
			if (TrackBarIsDisabled (trackBar))
				return VisualStyleElement.TrackBar.Thumb.Disabled;
			else if (TrackBarIsPressed (trackBar))
				return VisualStyleElement.TrackBar.Thumb.Pressed;
			else if (TrackBarIsHot (trackBar))
				return VisualStyleElement.TrackBar.Thumb.Hot;
			else if (TrackBarIsFocused (trackBar))
				return VisualStyleElement.TrackBar.Thumb.Focused;
			return VisualStyleElement.TrackBar.Thumb.Normal;
		}
		#endregion
		#region Vertical
		static Rectangle TrackBarRotateVerticalThumbSize (Rectangle value)
		{
			int temporary = value.Width;
			value.Width = value.Height;
			value.Height = temporary;
			return value;
		}
		protected override void TrackBarDrawVerticalThumbRight (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawVerticalThumbRight (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			VisualStyleElement element = TrackBarGetVerticalThumbRightVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawVerticalThumbRight (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, TrackBarRotateVerticalThumbSize (thumb_pos), clippingArea);
		}
		static VisualStyleElement TrackBarGetVerticalThumbRightVisualStyleElement (TrackBar trackBar)
		{
			if (TrackBarIsDisabled (trackBar))
				return VisualStyleElement.TrackBar.ThumbRight.Disabled;
			else if (TrackBarIsPressed (trackBar))
				return VisualStyleElement.TrackBar.ThumbRight.Pressed;
			else if (TrackBarIsHot (trackBar))
				return VisualStyleElement.TrackBar.ThumbRight.Hot;
			else if (TrackBarIsFocused (trackBar))
				return VisualStyleElement.TrackBar.ThumbRight.Focused;
			return VisualStyleElement.TrackBar.ThumbRight.Normal;
		}
		protected override void TrackBarDrawVerticalThumbLeft (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawVerticalThumbLeft (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			VisualStyleElement element = TrackBarGetVerticalThumbLeftVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawVerticalThumbLeft (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, TrackBarRotateVerticalThumbSize (thumb_pos), clippingArea);
		}
		static VisualStyleElement TrackBarGetVerticalThumbLeftVisualStyleElement (TrackBar trackBar)
		{
			if (TrackBarIsDisabled (trackBar))
				return VisualStyleElement.TrackBar.ThumbLeft.Disabled;
			else if (TrackBarIsPressed (trackBar))
				return VisualStyleElement.TrackBar.ThumbLeft.Pressed;
			else if (TrackBarIsHot (trackBar))
				return VisualStyleElement.TrackBar.ThumbLeft.Hot;
			else if (TrackBarIsFocused (trackBar))
				return VisualStyleElement.TrackBar.ThumbLeft.Focused;
			return VisualStyleElement.TrackBar.ThumbLeft.Normal;
		}
		protected override void TrackBarDrawVerticalThumb (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			if (!RenderClientAreas) {
				base.TrackBarDrawVerticalThumb (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			VisualStyleElement element = TrackBarGetVerticalThumbVisualStyleElement (trackBar);
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TrackBarDrawVerticalThumb (dc, thumb_pos, br_thumb, clippingArea, trackBar);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, TrackBarRotateVerticalThumbSize (thumb_pos), clippingArea);
		}
		static VisualStyleElement TrackBarGetVerticalThumbVisualStyleElement (TrackBar trackBar)
		{
			if (TrackBarIsDisabled (trackBar))
				return VisualStyleElement.TrackBar.ThumbVertical.Disabled;
			else if (TrackBarIsPressed (trackBar))
				return VisualStyleElement.TrackBar.ThumbVertical.Pressed;
			else if (TrackBarIsHot (trackBar))
				return VisualStyleElement.TrackBar.ThumbVertical.Hot;
			else if (TrackBarIsFocused (trackBar))
				return VisualStyleElement.TrackBar.ThumbVertical.Focused;
			return VisualStyleElement.TrackBar.ThumbVertical.Normal;
		}
		#endregion
		#endregion
		#region Ticks
		const EdgeStyle TrackBarTickEdgeStyle = EdgeStyle.Bump;
		const EdgeEffects TrackBarTickEdgeEffects = EdgeEffects.None;
		#region Horizontal
		protected override ITrackBarTickPainter TrackBarGetHorizontalTickPainter (Graphics g)
		{
			if (!RenderClientAreas ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.TrackBar.Ticks.Normal))
				return base.TrackBarGetHorizontalTickPainter (g);
			return new TrackBarHorizontalTickPainter (g);
		}
		class TrackBarHorizontalTickPainter : ITrackBarTickPainter
		{
			readonly Graphics g;
			readonly VisualStyleRenderer renderer;
			public TrackBarHorizontalTickPainter (Graphics g)
			{
				this.g = g;
				renderer = new VisualStyleRenderer (VisualStyleElement.TrackBar.Ticks.Normal);
			}
			public void Paint (float x1, float y1, float x2, float y2)
			{
				renderer.DrawEdge (g, new Rectangle (
					(int)Math.Round (x1),
					(int)Math.Round (y1),
					1,
					(int)Math.Round (y2 - y1) + 1), Edges.Left, TrackBarTickEdgeStyle, TrackBarTickEdgeEffects);
			}
		}
		#endregion
		#region Vertical
		protected override ITrackBarTickPainter TrackBarGetVerticalTickPainter (Graphics g)
		{
			if (!RenderClientAreas ||
				!VisualStyleRenderer.IsElementDefined (VisualStyleElement.TrackBar.TicksVertical.Normal))
				return base.TrackBarGetVerticalTickPainter (g);
			return new TrackBarVerticalTickPainter (g);
		}
		class TrackBarVerticalTickPainter : ITrackBarTickPainter
		{
			readonly Graphics g;
			readonly VisualStyleRenderer renderer;
			public TrackBarVerticalTickPainter (Graphics g)
			{
				this.g = g;
				renderer = new VisualStyleRenderer (VisualStyleElement.TrackBar.TicksVertical.Normal);
			}
			public void Paint (float x1, float y1, float x2, float y2)
			{
				renderer.DrawEdge (g, new Rectangle (
					(int)Math.Round (x1),
					(int)Math.Round (y1),
					(int)Math.Round (x2 - x1) + 1,
					1), Edges.Top, TrackBarTickEdgeStyle, TrackBarTickEdgeEffects);
			}
		}
		#endregion
		#endregion
		public override bool TrackBarHasHotThumbStyle {
			get {
				if (!RenderClientAreas)
					return base.TrackBarHasHotThumbStyle;
				return true;
			}
		}
		#endregion
		#region TreeView
		[MonoInternalNote ("Use the sizing information provided by the VisualStyles API.")]
		public override void TreeViewDrawNodePlusMinus (TreeView treeView, TreeNode node, Graphics dc, int x, int middle)
		{
			if (!RenderClientAreas) {
				base.TreeViewDrawNodePlusMinus (treeView, node, dc, x, middle);
				return;
			}
			VisualStyleElement element = node.IsExpanded ?
				VisualStyleElement.TreeView.Glyph.Opened : 
				VisualStyleElement.TreeView.Glyph.Closed;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.TreeViewDrawNodePlusMinus (treeView, node, dc, x, middle);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (dc, new Rectangle (x, middle - 4, 9, 9));
		}
		#endregion
		#region UpDownBase
		public override void UpDownBaseDrawButton (Graphics g, Rectangle bounds, bool top, PushButtonState state)
		{
			if (!RenderClientAreas) {
				base.UpDownBaseDrawButton (g, bounds, top, state);
				return;
			}
			VisualStyleElement element;
			if (top)
				switch (state) {
				case PushButtonState.Disabled:
					element = VisualStyleElement.Spin.Up.Disabled;
					break;
				case PushButtonState.Pressed:
					element = VisualStyleElement.Spin.Up.Pressed;
					break;
				case PushButtonState.Hot:
					element = VisualStyleElement.Spin.Up.Hot;
					break;
				default:
					element = VisualStyleElement.Spin.Up.Normal;
					break;
				}
			else
				switch (state) {
				case PushButtonState.Disabled:
					element = VisualStyleElement.Spin.Down.Disabled;
					break;
				case PushButtonState.Pressed:
					element = VisualStyleElement.Spin.Down.Pressed;
					break;
				case PushButtonState.Hot:
					element = VisualStyleElement.Spin.Down.Hot;
					break;
				default:
					element = VisualStyleElement.Spin.Down.Normal;
					break;
				}
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.UpDownBaseDrawButton (g, bounds, top, state);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (g, bounds);
		}
		public override bool UpDownBaseHasHotButtonStyle {
			get {
				if (!RenderClientAreas)
					return base.UpDownBaseHasHotButtonStyle;
				return true;
			}
		}
		#endregion
		#endregion

#if NET_2_0
		static bool AreEqual (VisualStyleElement value1, VisualStyleElement value2)
		{
			return
				value1.ClassName == value1.ClassName &&
				value1.Part == value2.Part &&
				value1.State == value2.State;
		}
#endif
		#region Measurement device context
		static Control control;
		static IDeviceContext GetMeasurementDeviceContext ()
		{
			if (control == null)
				control = new Control ();
			return control.CreateGraphics ();
		}
		static void ReleaseMeasurementDeviceContext (IDeviceContext dc)
		{
			dc.Dispose ();
		}
		#endregion
	}
}
