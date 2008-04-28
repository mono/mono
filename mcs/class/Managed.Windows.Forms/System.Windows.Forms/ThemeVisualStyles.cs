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
			if (!button.UseVisualStyleBackColor) {
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
		#region ControlPaint
		#region DrawButton
		public override void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if ((state & ButtonState.Flat) == ButtonState.Flat ||
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
			if ((state & ButtonState.Flat) == ButtonState.Flat ||
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
		#region DrawRadioButton
		public override void CPDrawRadioButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
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
			if ((state & ButtonState.Flat) == ButtonState.Flat ||
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
		#region Managed windows
		[MonoTODO("When other VisualStyles implementations are supported, check if the visual style elements are defined.")]
		[MonoTODO("When the rest of the MDI code is fixed, make sure minimized windows are painted correctly,restrict the caption text drawing area to exclude the title buttons and handle the title bar height correctly.")]
		public override void DrawManagedWindowDecorations (Graphics dc, Rectangle clip, InternalWindowManager wm)
		{
			if (!wm.HasBorders || (wm.Form.IsMdiChild && wm.IsMaximized))
				return;
			VisualStyleElement element;
			#region Title bar background
			#region Select caption visual style element
			if (wm.IsToolWindow)
				#region Small window
				switch (wm.form.window_state) {
				case FormWindowState.Minimized:
					if (!wm.Form.Enabled)
						element = VisualStyleElement.Window.SmallMinCaption.Disabled;
					else if (wm.IsActive)
						element = VisualStyleElement.Window.SmallMinCaption.Active;
					else
						element = VisualStyleElement.Window.SmallMinCaption.Inactive;
					break;
				case FormWindowState.Maximized:
					if (!wm.Form.Enabled)
						element = VisualStyleElement.Window.SmallMaxCaption.Disabled;
					else if (wm.IsActive)
						element = VisualStyleElement.Window.SmallMaxCaption.Active;
					else
						element = VisualStyleElement.Window.SmallMaxCaption.Inactive;
					break;
				default:
					if (!wm.Form.Enabled)
						element = VisualStyleElement.Window.SmallCaption.Disabled;
					else if (wm.IsActive)
						element = VisualStyleElement.Window.SmallCaption.Active;
					else
						element = VisualStyleElement.Window.SmallCaption.Inactive;
					break;
				}
				#endregion
			else
				#region Normal window
				switch (wm.form.window_state) {
				case FormWindowState.Minimized:
					if (!wm.Form.Enabled)
						element = VisualStyleElement.Window.MinCaption.Disabled;
					else if (wm.IsActive)
						element = VisualStyleElement.Window.MinCaption.Active;
					else
						element = VisualStyleElement.Window.MinCaption.Inactive;
					break;
				case FormWindowState.Maximized:
					if (!wm.Form.Enabled)
						element = VisualStyleElement.Window.MaxCaption.Disabled;
					else if (wm.IsActive)
						element = VisualStyleElement.Window.MaxCaption.Active;
					else
						element = VisualStyleElement.Window.MaxCaption.Inactive;
					break;
				default:
					if (!wm.Form.Enabled)
						element = VisualStyleElement.Window.Caption.Disabled;
					else if (wm.IsActive)
						element = VisualStyleElement.Window.Caption.Active;
					else
						element = VisualStyleElement.Window.Caption.Inactive;
					break;
				}
				#endregion
			#endregion
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			Rectangle title_bar_rectangle = new Rectangle (
				0,
				0,
				wm.Form.Width,
				renderer.GetPartSize (dc, ThemeSizeType.True).Height
			);
			Rectangle caption_text_area = title_bar_rectangle;
			renderer.DrawBackground (dc, title_bar_rectangle, clip);
			#endregion
			int border_width = ManagedWindowBorderWidth (wm);
			#region Icon
			if (!wm.IsToolWindow && wm.Form.FormBorderStyle != FormBorderStyle.FixedDialog
#if NET_2_0
			&& wm.Form.ShowIcon
#endif
			) {
				Rectangle icon_rectangle = new Rectangle (
					border_width + 3,
					border_width + 2,
					wm.IconWidth,
					wm.IconWidth);
				caption_text_area.X += icon_rectangle.Width;
				if (icon_rectangle.IntersectsWith (clip))
					dc.DrawIcon (wm.Form.Icon, icon_rectangle);
			}
			#endregion
			#region Title bar buttons
			foreach (TitleButton button in wm.TitleButtons.AllButtons) {
				if (!button.Visible || !button.Rectangle.IntersectsWith (clip))
					continue;
				new VisualStyleRenderer (GetCaptionButtonVisualStyleElement (button.Caption, button.State)).DrawBackground (dc, button.Rectangle, clip);
			}
			#endregion
			#region Borders
			if (wm.GetWindowState () == FormWindowState.Normal) {
				#region Left
				if (wm.IsToolWindow)
					if (wm.IsActive)
						element = VisualStyleElement.Window.SmallFrameLeft.Active;
					else
						element = VisualStyleElement.Window.SmallFrameLeft.Inactive;
				else
					if (wm.IsActive)
						element = VisualStyleElement.Window.FrameLeft.Active;
					else
						element = VisualStyleElement.Window.FrameLeft.Inactive;
				renderer = new VisualStyleRenderer (element);
				renderer.DrawBackground (dc, new Rectangle (
					0,
					title_bar_rectangle.Bottom,
					border_width,
					wm.form.Height - title_bar_rectangle.Bottom
					), clip);
				#endregion
				#region Right
				if (wm.IsToolWindow)
					if (wm.IsActive)
						element = VisualStyleElement.Window.SmallFrameRight.Active;
					else
						element = VisualStyleElement.Window.SmallFrameRight.Inactive;
				else
					if (wm.IsActive)
						element = VisualStyleElement.Window.FrameRight.Active;
					else
						element = VisualStyleElement.Window.FrameRight.Inactive;

				renderer = new VisualStyleRenderer (element);
				renderer.DrawBackground (dc, new Rectangle (
					wm.form.Width - border_width,
					title_bar_rectangle.Bottom,
					border_width,
					wm.form.Height - title_bar_rectangle.Bottom
					), clip);
				#endregion
				#region Bottom
				if (wm.IsToolWindow)
					if (wm.IsActive)
						element = VisualStyleElement.Window.SmallFrameBottom.Active;
					else
						element = VisualStyleElement.Window.SmallFrameBottom.Inactive;
				else
					if (wm.IsActive)
						element = VisualStyleElement.Window.FrameBottom.Active;
					else
						element = VisualStyleElement.Window.FrameBottom.Inactive;

				renderer = new VisualStyleRenderer (element);
				renderer.DrawBackground (dc, new Rectangle (
					0,
					wm.form.Height - border_width,
					wm.form.Width,
					border_width
					), clip);
				#endregion
				caption_text_area.X += border_width;
			}
			#endregion
			#region Caption text
			string window_caption = wm.Form.Text;
			if (window_caption != null && window_caption.Length != 0 && caption_text_area.IntersectsWith (clip)) {
				StringFormat format = new StringFormat ();
				format.FormatFlags = StringFormatFlags.NoWrap;
				format.Trimming = StringTrimming.EllipsisCharacter;
				format.LineAlignment = StringAlignment.Center;
				dc.DrawString (
					window_caption,
					WindowBorderFont,
					ThemeEngine.Current.ResPool.GetSolidBrush (Color.White),
					caption_text_area,
					format
				);
			}
			#endregion
		}
		#endregion
		#region ProgressBar
		public override void DrawProgressBar (Graphics dc, Rectangle clip_rect, ProgressBar ctrl)
		{
			if (!VisualStyleRenderer.IsElementDefined (VisualStyleElement.ProgressBar.Bar.Normal) ||
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
				double percent_done = (double)ms_diff % (double)ctrl.MarqueeAnimationSpeed / (double)ctrl.MarqueeAnimationSpeed;
				max_blocks = 5;
				start_pixel = client_area.X + (int)(client_area.Width * percent_done);
				goto default;
#endif
			default: // Blocks
				int block_width = renderer.GetInteger (IntegerProperty.ProgressChunkSize);
				int first_pixel_outside_filled_area = ((ctrl.Value - ctrl.Minimum) * client_area.Width) / (Math.Max (ctrl.Maximum - ctrl.Minimum, 1)) + client_area.X;
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
	}
}