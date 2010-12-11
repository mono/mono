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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Peter Bartok, pbartok@novell.com
//	John BouAntoun, jba-mono@optusnet.com.au
//	Marek Safar, marek.safar@seznam.cz
//	Alexander Olk, alex.olk@googlemail.com
//

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms.Theming;

namespace System.Windows.Forms
{

	internal class ThemeWin32Classic : Theme
	{		
		public override Version Version {
			get {
				return new Version(0, 1, 0, 0);
			}
		}

		/* Hardcoded colour values not exposed in the API constants in all configurations */
		protected static readonly Color arrow_color = Color.Black;
		protected static readonly Color pen_ticks_color = Color.Black;
		protected static StringFormat string_format_menu_text;
		protected static StringFormat string_format_menu_shortcut;
		protected static StringFormat string_format_menu_menubar_text;
		static ImageAttributes imagedisabled_attributes = null;
		const int SEPARATOR_HEIGHT = 6;
		const int SEPARATOR_MIN_WIDTH = 20;
		const int SM_CXBORDER = 1;
		const int SM_CYBORDER = 1;		
		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tabd
		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items

		#region	Principal Theme Methods
		public ThemeWin32Classic ()
		{			
			ResetDefaults ();
		}

		public override void ResetDefaults() {
			defaultWindowBackColor = this.ColorWindow;
			defaultWindowForeColor = this.ColorControlText;
			window_border_font = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Bold);
			
			/* Menu string formats */
			string_format_menu_text = new StringFormat ();
			string_format_menu_text.LineAlignment = StringAlignment.Center;
			string_format_menu_text.Alignment = StringAlignment.Near;
			string_format_menu_text.HotkeyPrefix = HotkeyPrefix.Show;
			string_format_menu_text.SetTabStops (0f, new float [] { 50f });
			string_format_menu_text.FormatFlags |= StringFormatFlags.NoWrap;

			string_format_menu_shortcut = new StringFormat ();	
			string_format_menu_shortcut.LineAlignment = StringAlignment.Center;
			string_format_menu_shortcut.Alignment = StringAlignment.Far;

			string_format_menu_menubar_text = new StringFormat ();
			string_format_menu_menubar_text.LineAlignment = StringAlignment.Center;
			string_format_menu_menubar_text.Alignment = StringAlignment.Center;
			string_format_menu_menubar_text.HotkeyPrefix = HotkeyPrefix.Show;
		}

		public override bool DoubleBufferingSupported {
			get {return true; }
		}

		public override int HorizontalScrollBarHeight {
			get {
				return XplatUI.HorizontalScrollBarHeight;
			}
		}

		public override int VerticalScrollBarWidth {
			get {
				return XplatUI.VerticalScrollBarWidth;
			}
		}

		#endregion	// Principal Theme Methods

		#region	Internal Methods
		protected Brush GetControlBackBrush (Color c) {
			if (c.ToArgb () == DefaultControlBackColor.ToArgb ())
				return SystemBrushes.Control;
			return ResPool.GetSolidBrush (c);
		}

		protected Brush GetControlForeBrush (Color c) {
			if (c.ToArgb () == DefaultControlForeColor.ToArgb ())
				return SystemBrushes.ControlText;
			return ResPool.GetSolidBrush (c);
		}
		#endregion	// Internal Methods

		#region Control
		public override Font GetLinkFont (Control control) 
		{
			return new Font (control.Font.FontFamily, control.Font.Size, control.Font.Style | FontStyle.Underline, control.Font.Unit); 
		}
		#endregion	// Control

		#region OwnerDraw Support
		public  override void DrawOwnerDrawBackground (DrawItemEventArgs e)
		{
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				e.Graphics.FillRectangle (SystemBrushes.Highlight, e.Bounds);
				return;
			}

			e.Graphics.FillRectangle (ResPool.GetSolidBrush(e.BackColor), e.Bounds);
		}

		public  override void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Focus)
				CPDrawFocusRectangle (e.Graphics, e.Bounds, e.ForeColor, e.BackColor);
		}
		#endregion	// OwnerDraw Support

		#region Button
		#region Standard Button Style
		public override void DrawButton (Graphics g, Button b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			DrawButtonBackground (g, b, clipRectangle);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawButtonImage (g, b, imageBounds);

			// If we're focused, draw a focus rectangle
			if (b.Focused && b.Enabled && b.ShowFocusCues)
				DrawButtonFocus (g, b);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawButtonText (g, b, textBounds);
		}

		public virtual void DrawButtonBackground (Graphics g, Button button, Rectangle clipArea) 
		{
			if (button.Pressed)
				ThemeElements.DrawButton (g, button.ClientRectangle, ButtonThemeState.Pressed, button.BackColor, button.ForeColor);
			else if (button.InternalSelected)
				ThemeElements.DrawButton (g, button.ClientRectangle, ButtonThemeState.Default, button.BackColor, button.ForeColor);
			else if (button.Entered)
				ThemeElements.DrawButton (g, button.ClientRectangle, ButtonThemeState.Entered, button.BackColor, button.ForeColor);
			else if (!button.Enabled)
				ThemeElements.DrawButton (g, button.ClientRectangle, ButtonThemeState.Disabled, button.BackColor, button.ForeColor);
			else
				ThemeElements.DrawButton (g, button.ClientRectangle, ButtonThemeState.Normal, button.BackColor, button.ForeColor);
		}

		public virtual void DrawButtonFocus (Graphics g, Button button)
		{
			ControlPaint.DrawFocusRectangle (g, Rectangle.Inflate (button.ClientRectangle, -4, -4));
		}

		public virtual void DrawButtonImage (Graphics g, ButtonBase button, Rectangle imageBounds)
		{
			if (button.Enabled)
				g.DrawImage (button.Image, imageBounds);
			else
				CPDrawImageDisabled (g, button.Image, imageBounds.Left, imageBounds.Top, ColorControl);
		}

		public virtual void DrawButtonText (Graphics g, ButtonBase button, Rectangle textBounds)
		{
			// Ensure that at least one line is going to get displayed.
			// Line limit does not ensure that despite its description.
			textBounds.Height = Math.Max (textBounds.Height, button.Font.Height);
			
			if (button.Enabled)
				TextRenderer.DrawTextInternal (g, button.Text, button.Font, textBounds, button.ForeColor, button.TextFormatFlags, button.UseCompatibleTextRendering);
			else
				DrawStringDisabled20 (g, button.Text, button.Font, textBounds, button.BackColor, button.TextFormatFlags, button.UseCompatibleTextRendering);
		}
		#endregion

		#region FlatStyle Button Style
		public override void DrawFlatButton (Graphics g, ButtonBase b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			if (b.BackgroundImage == null)
				DrawFlatButtonBackground (g, b, clipRectangle);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawFlatButtonImage (g, b, imageBounds);

			// If we're focused, draw a focus rectangle
			if (b.Focused && b.Enabled && b.ShowFocusCues)
				DrawFlatButtonFocus (g, b);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawFlatButtonText (g, b, textBounds);
		}

		public virtual void DrawFlatButtonBackground (Graphics g, ButtonBase button, Rectangle clipArea)
		{
			if (button.Pressed)
				ThemeElements.DrawFlatButton (g, button.ClientRectangle, ButtonThemeState.Pressed, button.BackColor, button.ForeColor, button.FlatAppearance);
			else if (button.InternalSelected) {
				if (button.Entered) 
					ThemeElements.DrawFlatButton (g, button.ClientRectangle, ButtonThemeState.Default | ButtonThemeState.Entered, button.BackColor, button.ForeColor, button.FlatAppearance);
				else
					ThemeElements.DrawFlatButton (g, button.ClientRectangle, ButtonThemeState.Default, button.BackColor, button.ForeColor, button.FlatAppearance);
			}
			else if (button.Entered)
				ThemeElements.DrawFlatButton (g, button.ClientRectangle, ButtonThemeState.Entered, button.BackColor, button.ForeColor, button.FlatAppearance);
			else if (!button.Enabled)
				ThemeElements.DrawFlatButton (g, button.ClientRectangle, ButtonThemeState.Disabled, button.BackColor, button.ForeColor, button.FlatAppearance);
			else
				ThemeElements.DrawFlatButton (g, button.ClientRectangle, ButtonThemeState.Normal, button.BackColor, button.ForeColor, button.FlatAppearance);
		}

		public virtual void DrawFlatButtonFocus (Graphics g, ButtonBase button)
		{
			if (!button.Pressed) {
				Color focus_color = ControlPaint.Dark (button.BackColor);
				g.DrawRectangle (ResPool.GetPen (focus_color), new Rectangle (button.ClientRectangle.Left + 4, button.ClientRectangle.Top + 4, button.ClientRectangle.Width - 9, button.ClientRectangle.Height - 9));
			}
		}

		public virtual void DrawFlatButtonImage (Graphics g, ButtonBase button, Rectangle imageBounds)
		{
			// No changes from Standard for image for this theme
			DrawButtonImage (g, button, imageBounds);
		}

		public virtual void DrawFlatButtonText (Graphics g, ButtonBase button, Rectangle textBounds)
		{
			// No changes from Standard for text for this theme
			DrawButtonText (g, button, textBounds);
		}
		#endregion

		#region Popup Button Style
		public override void DrawPopupButton (Graphics g, Button b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			DrawPopupButtonBackground (g, b, clipRectangle);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawPopupButtonImage (g, b, imageBounds);

			// If we're focused, draw a focus rectangle
			if (b.Focused && b.Enabled && b.ShowFocusCues)
				DrawPopupButtonFocus (g, b);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawPopupButtonText (g, b, textBounds);
		}

		public virtual void DrawPopupButtonBackground (Graphics g, Button button, Rectangle clipArea)
		{
			if (button.Pressed)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Pressed, button.BackColor, button.ForeColor);
			else if (button.Entered)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Entered, button.BackColor, button.ForeColor);
			else if (button.InternalSelected)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Default, button.BackColor, button.ForeColor);
			else if (!button.Enabled)
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Disabled, button.BackColor, button.ForeColor);
			else
				ThemeElements.DrawPopupButton (g, button.ClientRectangle, ButtonThemeState.Normal, button.BackColor, button.ForeColor);
		}

		public virtual void DrawPopupButtonFocus (Graphics g, Button button)
		{
			// No changes from Standard for image for this theme
			DrawButtonFocus (g, button);
		}

		public virtual void DrawPopupButtonImage (Graphics g, Button button, Rectangle imageBounds)
		{
			// No changes from Standard for image for this theme
			DrawButtonImage (g, button, imageBounds);
		}

		public virtual void DrawPopupButtonText (Graphics g, Button button, Rectangle textBounds)
		{
			// No changes from Standard for image for this theme
			DrawButtonText (g, button, textBounds);
		}
		#endregion

		#region Button Layout Calculations
#if NET_2_0
		public override Size CalculateButtonAutoSize (Button button)
		{
			Size ret_size = Size.Empty;
			Size text_size = TextRenderer.MeasureTextInternal (button.Text, button.Font, button.UseCompatibleTextRendering);
			Size image_size = button.Image == null ? Size.Empty : button.Image.Size;
			
			// Pad the text size
			if (button.Text.Length != 0) {
				text_size.Height += 4;
				text_size.Width += 4;
			}
			
			switch (button.TextImageRelation) {
				case TextImageRelation.Overlay:
					ret_size.Height = Math.Max (button.Text.Length == 0 ? 0 : text_size.Height, image_size.Height);
					ret_size.Width = Math.Max (text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageAboveText:
				case TextImageRelation.TextAboveImage:
					ret_size.Height = text_size.Height + image_size.Height;
					ret_size.Width = Math.Max (text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageBeforeText:
				case TextImageRelation.TextBeforeImage:
					ret_size.Height = Math.Max (text_size.Height, image_size.Height);
					ret_size.Width = text_size.Width + image_size.Width;
					break;
			}

			// Pad the result
			ret_size.Height += (button.Padding.Vertical + 6);
			ret_size.Width += (button.Padding.Horizontal + 6);
			
			return ret_size;
		}
#endif

		public override void CalculateButtonTextAndImageLayout (ButtonBase button, out Rectangle textRectangle, out Rectangle imageRectangle)
		{
			Image image = button.Image;
			string text = button.Text;
			Rectangle content_rect = button.ClientRectangle;
			Size text_size = TextRenderer.MeasureTextInternal (text, button.Font, content_rect.Size, button.TextFormatFlags, button.UseCompatibleTextRendering);
			Size image_size = image == null ? Size.Empty : image.Size;

			textRectangle = Rectangle.Empty;
			imageRectangle = Rectangle.Empty;
			
			switch (button.TextImageRelation) {
				case TextImageRelation.Overlay:
					// Overlay is easy, text always goes here
					textRectangle = Rectangle.Inflate (content_rect, -4, -4);

					if (button.Pressed)
						textRectangle.Offset (1, 1);
						
					// Image is dependent on ImageAlign
					if (image == null)
						return;
						
					int image_x = 0;
					int image_y = 0;
					int image_height = image.Height;
					int image_width = image.Width;
					
					switch (button.ImageAlign) {
						case System.Drawing.ContentAlignment.TopLeft:
							image_x = 5;
							image_y = 5;
							break;
						case System.Drawing.ContentAlignment.TopCenter:
							image_x = (content_rect.Width - image_width) / 2;
							image_y = 5;
							break;
						case System.Drawing.ContentAlignment.TopRight:
							image_x = content_rect.Width - image_width - 5;
							image_y = 5;
							break;
						case System.Drawing.ContentAlignment.MiddleLeft:
							image_x = 5;
							image_y = (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.MiddleCenter:
							image_x = (content_rect.Width - image_width) / 2;
							image_y = (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.MiddleRight:
							image_x = content_rect.Width - image_width - 4;
							image_y = (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.BottomLeft:
							image_x = 5;
							image_y = content_rect.Height - image_height - 4;
							break;
						case System.Drawing.ContentAlignment.BottomCenter:
							image_x = (content_rect.Width - image_width) / 2;
							image_y = content_rect.Height - image_height - 4;
							break;
						case System.Drawing.ContentAlignment.BottomRight:
							image_x = content_rect.Width - image_width - 4;
							image_y = content_rect.Height - image_height - 4;
							break;
						default:
							image_x = 5;
							image_y = 5;
							break;
					}
					
					imageRectangle = new Rectangle (image_x, image_y, image_width, image_height);
					break;
				case TextImageRelation.ImageAboveText:
					content_rect.Inflate (-4, -4);
					LayoutTextAboveOrBelowImage (content_rect, false, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.TextAboveImage:
					content_rect.Inflate (-4, -4);
					LayoutTextAboveOrBelowImage (content_rect, true, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.ImageBeforeText:
					content_rect.Inflate (-4, -4);
					LayoutTextBeforeOrAfterImage (content_rect, false, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.TextBeforeImage:
					content_rect.Inflate (-4, -4);
					LayoutTextBeforeOrAfterImage (content_rect, true, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
			}
		}

		private void LayoutTextBeforeOrAfterImage (Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, System.Drawing.ContentAlignment textAlign, System.Drawing.ContentAlignment imageAlign, out Rectangle textRect, out Rectangle imageRect)
		{
			int element_spacing = 0;	// Spacing between the Text and the Image
			int total_width = textSize.Width + element_spacing + imageSize.Width;
			
			if (!textFirst)
				element_spacing += 2;
				
			// If the text is too big, chop it down to the size we have available to it
			if (total_width > totalArea.Width) {
				textSize.Width = totalArea.Width - element_spacing - imageSize.Width;
				total_width = totalArea.Width;
			}
			
			int excess_width = totalArea.Width - total_width;
			int offset = 0;

			Rectangle final_text_rect;
			Rectangle final_image_rect;

			HorizontalAlignment h_text = GetHorizontalAlignment (textAlign);
			HorizontalAlignment h_image = GetHorizontalAlignment (imageAlign);

			if (h_image == HorizontalAlignment.Left)
				offset = 0;
			else if (h_image == HorizontalAlignment.Right && h_text == HorizontalAlignment.Right)
				offset = excess_width;
			else if (h_image == HorizontalAlignment.Center && (h_text == HorizontalAlignment.Left || h_text == HorizontalAlignment.Center))
				offset += (int)(excess_width / 3);
			else
				offset += (int)(2 * (excess_width / 3));

			if (textFirst) {
				final_text_rect = new Rectangle (totalArea.Left + offset, AlignInRectangle (totalArea, textSize, textAlign).Top, textSize.Width, textSize.Height);
				final_image_rect = new Rectangle (final_text_rect.Right + element_spacing, AlignInRectangle (totalArea, imageSize, imageAlign).Top, imageSize.Width, imageSize.Height);
			}
			else {
				final_image_rect = new Rectangle (totalArea.Left + offset, AlignInRectangle (totalArea, imageSize, imageAlign).Top, imageSize.Width, imageSize.Height);
				final_text_rect = new Rectangle (final_image_rect.Right + element_spacing, AlignInRectangle (totalArea, textSize, textAlign).Top, textSize.Width, textSize.Height);
			}

			textRect = final_text_rect;
			imageRect = final_image_rect;
		}

		private void LayoutTextAboveOrBelowImage (Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, System.Drawing.ContentAlignment textAlign, System.Drawing.ContentAlignment imageAlign, out Rectangle textRect, out Rectangle imageRect)
		{
			int element_spacing = 0;	// Spacing between the Text and the Image
			int total_height = textSize.Height + element_spacing + imageSize.Height;

			if (textFirst)
				element_spacing += 2;

			if (textSize.Width > totalArea.Width)
				textSize.Width = totalArea.Width;
				
			// If the there isn't enough room and we're text first, cut out the image
			if (total_height > totalArea.Height && textFirst) {
				imageSize = Size.Empty;
				total_height = totalArea.Height;
			}

			int excess_height = totalArea.Height - total_height;
			int offset = 0;

			Rectangle final_text_rect;
			Rectangle final_image_rect;

			VerticalAlignment v_text = GetVerticalAlignment (textAlign);
			VerticalAlignment v_image = GetVerticalAlignment (imageAlign);

			if (v_image == VerticalAlignment.Top)
				offset = 0;
			else if (v_image == VerticalAlignment.Bottom && v_text == VerticalAlignment.Bottom)
				offset = excess_height;
			else if (v_image == VerticalAlignment.Center && (v_text == VerticalAlignment.Top || v_text == VerticalAlignment.Center))
				offset += (int)(excess_height / 3);
			else
				offset += (int)(2 * (excess_height / 3));

			if (textFirst) {
				final_text_rect = new Rectangle (AlignInRectangle (totalArea, textSize, textAlign).Left, totalArea.Top + offset, textSize.Width, textSize.Height);
				final_image_rect = new Rectangle (AlignInRectangle (totalArea, imageSize, imageAlign).Left, final_text_rect.Bottom + element_spacing, imageSize.Width, imageSize.Height);
			}
			else {
				final_image_rect = new Rectangle (AlignInRectangle (totalArea, imageSize, imageAlign).Left, totalArea.Top + offset, imageSize.Width, imageSize.Height);
				final_text_rect = new Rectangle (AlignInRectangle (totalArea, textSize, textAlign).Left, final_image_rect.Bottom + element_spacing, textSize.Width, textSize.Height);
				
				if (final_text_rect.Bottom > totalArea.Bottom)
					final_text_rect.Y = totalArea.Top;
			}

			textRect = final_text_rect;
			imageRect = final_image_rect;
		}
		
		private HorizontalAlignment GetHorizontalAlignment (System.Drawing.ContentAlignment align)
		{
			switch (align) {
				case System.Drawing.ContentAlignment.BottomLeft:
				case System.Drawing.ContentAlignment.MiddleLeft:
				case System.Drawing.ContentAlignment.TopLeft:
					return HorizontalAlignment.Left;
				case System.Drawing.ContentAlignment.BottomCenter:
				case System.Drawing.ContentAlignment.MiddleCenter:
				case System.Drawing.ContentAlignment.TopCenter:
					return HorizontalAlignment.Center;
				case System.Drawing.ContentAlignment.BottomRight:
				case System.Drawing.ContentAlignment.MiddleRight:
				case System.Drawing.ContentAlignment.TopRight:
					return HorizontalAlignment.Right;
			}

			return HorizontalAlignment.Left;
		}

		private enum VerticalAlignment
		{
			Top = 0,
			Center = 1,
			Bottom = 2
		}
		
		private VerticalAlignment GetVerticalAlignment (System.Drawing.ContentAlignment align)
		{
			switch (align) {
				case System.Drawing.ContentAlignment.TopLeft:
				case System.Drawing.ContentAlignment.TopCenter:
				case System.Drawing.ContentAlignment.TopRight:
					return VerticalAlignment.Top;
				case System.Drawing.ContentAlignment.MiddleLeft:
				case System.Drawing.ContentAlignment.MiddleCenter:
				case System.Drawing.ContentAlignment.MiddleRight:
					return VerticalAlignment.Center;
				case System.Drawing.ContentAlignment.BottomLeft:
				case System.Drawing.ContentAlignment.BottomCenter:
				case System.Drawing.ContentAlignment.BottomRight:
					return VerticalAlignment.Bottom;
			}

			return VerticalAlignment.Top;
		}

		internal Rectangle AlignInRectangle (Rectangle outer, Size inner, System.Drawing.ContentAlignment align)
		{
			int x = 0;
			int y = 0;

			if (align == System.Drawing.ContentAlignment.BottomLeft || align == System.Drawing.ContentAlignment.MiddleLeft || align == System.Drawing.ContentAlignment.TopLeft)
				x = outer.X;
			else if (align == System.Drawing.ContentAlignment.BottomCenter || align == System.Drawing.ContentAlignment.MiddleCenter || align == System.Drawing.ContentAlignment.TopCenter)
				x = Math.Max (outer.X + ((outer.Width - inner.Width) / 2), outer.Left);
			else if (align == System.Drawing.ContentAlignment.BottomRight || align == System.Drawing.ContentAlignment.MiddleRight || align == System.Drawing.ContentAlignment.TopRight)
				x = outer.Right - inner.Width;
			if (align == System.Drawing.ContentAlignment.TopCenter || align == System.Drawing.ContentAlignment.TopLeft || align == System.Drawing.ContentAlignment.TopRight)
				y = outer.Y;
			else if (align == System.Drawing.ContentAlignment.MiddleCenter || align == System.Drawing.ContentAlignment.MiddleLeft || align == System.Drawing.ContentAlignment.MiddleRight)
				y = outer.Y + (outer.Height - inner.Height) / 2;
			else if (align == System.Drawing.ContentAlignment.BottomCenter || align == System.Drawing.ContentAlignment.BottomRight || align == System.Drawing.ContentAlignment.BottomLeft)
				y = outer.Bottom - inner.Height;

			return new Rectangle (x, y, Math.Min (inner.Width, outer.Width), Math.Min (inner.Height, outer.Height));
		}
		#endregion
		#endregion

		#region ButtonBase
		public override void DrawButtonBase(Graphics dc, Rectangle clip_area, ButtonBase button)
		{
			// Draw the button: Draw border, etc.
			ButtonBase_DrawButton(button, dc);

			// Draw the image
			if (button.FlatStyle != FlatStyle.System && ((button.image != null) || (button.image_list != null)))
				ButtonBase_DrawImage(button, dc);
			
			// Draw the focus rectangle
			if (ShouldPaintFocusRectagle (button))
				ButtonBase_DrawFocus(button, dc);
			
			// Now the text
			if (button.Text != null && button.Text != String.Empty)
				ButtonBase_DrawText(button, dc);
		}

		protected static bool ShouldPaintFocusRectagle (ButtonBase button)
		{
			return (button.Focused || button.paint_as_acceptbutton) && button.Enabled && button.ShowFocusCues;
		}

		protected virtual void ButtonBase_DrawButton (ButtonBase button, Graphics dc)
		{
			Rectangle borderRectangle;
			bool check_or_radio = false;
			bool check_or_radio_checked = false;
			
			bool is_ColorControl = button.BackColor.ToArgb () == ColorControl.ToArgb () ? true : false;
			
			CPColor cpcolor = is_ColorControl ? CPColor.Empty : ResPool.GetCPColor (button.BackColor);
			
			if (button is CheckBox) {
				check_or_radio = true;
				check_or_radio_checked = ((CheckBox)button).Checked;
			} else if (button is RadioButton) {
				check_or_radio = true;
				check_or_radio_checked = ((RadioButton)button).Checked;
			}
			
			if (button.Focused && button.Enabled && !check_or_radio) {
				// shrink the rectangle for the normal button drawing inside the focus rectangle
				borderRectangle = Rectangle.Inflate (button.ClientRectangle, -1, -1);
			} else {
				borderRectangle = button.ClientRectangle;
			}
			
			if (button.FlatStyle == FlatStyle.Popup) {
				if (!button.is_pressed && !button.is_entered && !check_or_radio_checked)
					Internal_DrawButton (dc, borderRectangle, 1, cpcolor, is_ColorControl, button.BackColor);
				else if (!button.is_pressed && button.is_entered &&!check_or_radio_checked)
					Internal_DrawButton (dc, borderRectangle, 2, cpcolor, is_ColorControl, button.BackColor);
				else if (button.is_pressed || check_or_radio_checked)
					Internal_DrawButton (dc, borderRectangle, 1, cpcolor, is_ColorControl, button.BackColor);
			} else if (button.FlatStyle == FlatStyle.Flat) {
				if (button.is_entered && !button.is_pressed && !check_or_radio_checked) {
					if ((button.image == null) && (button.image_list == null)) {
						Brush brush = is_ColorControl ? SystemBrushes.ControlDark : ResPool.GetSolidBrush (cpcolor.Dark);
						dc.FillRectangle (brush, borderRectangle);
					}
				} else if (button.is_pressed || check_or_radio_checked) {
					if ((button.image == null) && (button.image_list == null)) {
						Brush brush = is_ColorControl ? SystemBrushes.ControlLightLight : ResPool.GetSolidBrush (cpcolor.LightLight);
						dc.FillRectangle (brush, borderRectangle);
					}
					
					Pen pen = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
					dc.DrawRectangle (pen, borderRectangle.X + 4, borderRectangle.Y + 4,
							  borderRectangle.Width - 9, borderRectangle.Height - 9);
				}
				
				Internal_DrawButton (dc, borderRectangle, 3, cpcolor, is_ColorControl, button.BackColor);
			} else {
				if ((!button.is_pressed || !button.Enabled) && !check_or_radio_checked)
					Internal_DrawButton (dc, borderRectangle, 0, cpcolor, is_ColorControl, button.BackColor);
				else
					Internal_DrawButton (dc, borderRectangle, 1, cpcolor, is_ColorControl, button.BackColor);
			}
		}
		
		private void Internal_DrawButton (Graphics dc, Rectangle rect, int state, CPColor cpcolor, bool is_ColorControl, Color backcolor)
		{
			switch (state) {
			case 0: // normal or normal disabled button
				Pen pen = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				dc.DrawLine (pen, rect.X, rect.Y, rect.X, rect.Bottom - 2);
				dc.DrawLine (pen, rect.X + 1, rect.Y, rect.Right - 2, rect.Y);
				
				pen = is_ColorControl ? SystemPens.Control : ResPool.GetPen (backcolor);
				dc.DrawLine (pen, rect.X + 1, rect.Y + 1, rect.X + 1, rect.Bottom - 3);
				dc.DrawLine (pen, rect.X + 2, rect.Y + 1, rect.Right - 3, rect.Y + 1);
				
				pen = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				dc.DrawLine (pen, rect.X + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
				dc.DrawLine (pen, rect.Right - 2, rect.Y + 1, rect.Right - 2, rect.Bottom - 3);
				
				pen = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				dc.DrawLine (pen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				dc.DrawLine (pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 2);
				break;
			case 1: // popup button normal (or pressed normal or popup button)
				pen = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				dc.DrawRectangle (pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
				break;
			case 2: // popup button poped up
				pen = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				dc.DrawLine (pen, rect.X, rect.Y, rect.X, rect.Bottom - 2);
				dc.DrawLine (pen, rect.X + 1, rect.Y, rect.Right - 2, rect.Y);
				
				pen = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				dc.DrawLine (pen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				dc.DrawLine (pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 2);
				break;
			case 3: // flat button not entered
				pen = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				dc.DrawRectangle (pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
				break;
			default:
				break;
			}
		}
		
		protected virtual void ButtonBase_DrawImage(ButtonBase button, Graphics dc)
		{
			// Need to draw a picture
			Image	i;
			int	image_x;
			int	image_y;
			int	image_width;
			int	image_height;
			
			int width = button.ClientSize.Width;
			int height = button.ClientSize.Height;

			if (button.ImageIndex != -1) {	 // We use ImageIndex instead of image_index since it will return -1 if image_list is null
				i = button.image_list.Images[button.ImageIndex];
			} else {
				i = button.image;
			}

			image_width = i.Width;
			image_height = i.Height;
			
			switch (button.ImageAlign) {
				case ContentAlignment.TopLeft: {
					image_x = 5;
					image_y = 5;
					break;
				}
					
				case ContentAlignment.TopCenter: {
					image_x = (width - image_width) / 2;
					image_y = 5;
					break;
				}
					
				case ContentAlignment.TopRight: {
					image_x = width - image_width - 5;
					image_y = 5;
					break;
				}
					
				case ContentAlignment.MiddleLeft: {
					image_x = 5;
					image_y = (height - image_height) / 2;
					break;
				}
					
				case ContentAlignment.MiddleCenter: {
					image_x = (width - image_width) / 2;
					image_y = (height - image_height) / 2;
					break;
				}
					
				case ContentAlignment.MiddleRight: {
					image_x = width - image_width - 4;
					image_y = (height - image_height) / 2;
					break;
				}
					
				case ContentAlignment.BottomLeft: {
					image_x = 5;
					image_y = height - image_height - 4;
					break;
				}
					
				case ContentAlignment.BottomCenter: {
					image_x = (width - image_width) / 2;
					image_y = height - image_height - 4;
					break;
				}
					
				case ContentAlignment.BottomRight: {
					image_x = width - image_width - 4;
					image_y = height - image_height - 4;
					break;
				}
					
				default: {
					image_x = 5;
					image_y = 5;
					break;
				}
			}
			
			dc.SetClip (new Rectangle(3, 3, width - 5, height - 5));

			if (button.Enabled)
				dc.DrawImage (i, image_x, image_y, image_width, image_height);
			else
				CPDrawImageDisabled (dc, i, image_x, image_y, ColorControl);

			dc.ResetClip ();
		}
		
		protected virtual void ButtonBase_DrawFocus(ButtonBase button, Graphics dc)
		{
			Color focus_color = button.ForeColor;
			
			int inflate_value = -3;
			
			if (!(button is CheckBox) && !(button is RadioButton)) {
				inflate_value = -4;
				
				if (button.FlatStyle == FlatStyle.Popup && !button.is_pressed)
					focus_color = ControlPaint.Dark(button.BackColor);
				
				dc.DrawRectangle (ResPool.GetPen (focus_color), button.ClientRectangle.X, button.ClientRectangle.Y, 
						  button.ClientRectangle.Width - 1, button.ClientRectangle.Height - 1);
			}
			
			if (button.Focused) {
				Rectangle rect = Rectangle.Inflate (button.ClientRectangle, inflate_value, inflate_value);
				ControlPaint.DrawFocusRectangle (dc, rect);
			}
		}
		
		protected virtual void ButtonBase_DrawText(ButtonBase button, Graphics dc)
		{
			Rectangle buttonRectangle = button.ClientRectangle;
			Rectangle text_rect = Rectangle.Inflate(buttonRectangle, -4, -4);
			
			if (button.is_pressed) {
				text_rect.X++;
				text_rect.Y++;
			}
			
			// Ensure that at least one line is going to get displayed.
			// Line limit does not ensure that despite its description.
			text_rect.Height = Math.Max (button.Font.Height, text_rect.Height);
			
			if (button.Enabled) {					
				dc.DrawString(button.Text, button.Font, ResPool.GetSolidBrush (button.ForeColor), text_rect, button.text_format);
			} else {
				if (button.FlatStyle == FlatStyle.Flat || button.FlatStyle == FlatStyle.Popup) {
					dc.DrawString(button.Text, button.Font, ResPool.GetSolidBrush (ColorGrayText), text_rect, button.text_format);
				} else {
					CPDrawStringDisabled (dc, button.Text, button.Font, button.BackColor, text_rect, button.text_format);
				}
			}
		}
		
		public override Size ButtonBaseDefaultSize {
			get {
				return new Size (75, 23);
			}
		}
		#endregion	// ButtonBase

		#region CheckBox
#if NET_2_0
		public override void DrawCheckBox (Graphics g, CheckBox cb, Rectangle glyphArea, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			if (cb.Appearance == Appearance.Button && cb.FlatStyle != FlatStyle.Flat)
				ButtonBase_DrawButton (cb, g);
			else if (cb.Appearance != Appearance.Button)
				DrawCheckBoxGlyph (g, cb, glyphArea);

			// Draw the borders and such for a Flat CheckBox Button
			if (cb.Appearance == Appearance.Button && cb.FlatStyle == FlatStyle.Flat)
			DrawFlatButton (g, cb, textBounds, imageBounds, clipRectangle);
			
			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawCheckBoxImage (g, cb, imageBounds);

			if (cb.Focused && cb.Enabled && cb.ShowFocusCues && textBounds != Rectangle.Empty)
				DrawCheckBoxFocus (g, cb, textBounds);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawCheckBoxText (g, cb, textBounds);
		}

		public virtual void DrawCheckBoxGlyph (Graphics g, CheckBox cb, Rectangle glyphArea)
		{
			if (cb.Pressed)
				ThemeElements.CurrentTheme.CheckBoxPainter.PaintCheckBox (g, glyphArea, cb.BackColor, cb.ForeColor, ElementState.Pressed, cb.FlatStyle, cb.CheckState);
			else if (cb.InternalSelected)
				ThemeElements.CurrentTheme.CheckBoxPainter.PaintCheckBox (g, glyphArea, cb.BackColor, cb.ForeColor, ElementState.Normal, cb.FlatStyle, cb.CheckState);
			else if (cb.Entered)
				ThemeElements.CurrentTheme.CheckBoxPainter.PaintCheckBox (g, glyphArea, cb.BackColor, cb.ForeColor, ElementState.Hot, cb.FlatStyle, cb.CheckState);
			else if (!cb.Enabled)
				ThemeElements.CurrentTheme.CheckBoxPainter.PaintCheckBox (g, glyphArea, cb.BackColor, cb.ForeColor, ElementState.Disabled, cb.FlatStyle, cb.CheckState);
			else
				ThemeElements.CurrentTheme.CheckBoxPainter.PaintCheckBox (g, glyphArea, cb.BackColor, cb.ForeColor, ElementState.Normal, cb.FlatStyle, cb.CheckState);
		}

		public virtual void DrawCheckBoxFocus (Graphics g, CheckBox cb, Rectangle focusArea)
		{
			ControlPaint.DrawFocusRectangle (g, focusArea);
		}

		public virtual void DrawCheckBoxImage (Graphics g, CheckBox cb, Rectangle imageBounds)
		{
			if (cb.Enabled)
				g.DrawImage (cb.Image, imageBounds);
			else
				CPDrawImageDisabled (g, cb.Image, imageBounds.Left, imageBounds.Top, ColorControl);
		}

		public virtual void DrawCheckBoxText (Graphics g, CheckBox cb, Rectangle textBounds)
		{
			if (cb.Enabled)
				TextRenderer.DrawTextInternal (g, cb.Text, cb.Font, textBounds, cb.ForeColor, cb.TextFormatFlags, cb.UseCompatibleTextRendering);
			else
				DrawStringDisabled20 (g, cb.Text, cb.Font, textBounds, cb.BackColor, cb.TextFormatFlags, cb.UseCompatibleTextRendering);
		}

		public override void CalculateCheckBoxTextAndImageLayout (ButtonBase button, Point p, out Rectangle glyphArea, out Rectangle textRectangle, out Rectangle imageRectangle)
		{
			int check_size = 13;
			
			if (button is CheckBox)
				check_size = (button as CheckBox).Appearance == Appearance.Normal ? 13 : 0;
				
			glyphArea = new Rectangle (0, 2, check_size, check_size);
			
			Rectangle content_rect = button.ClientRectangle;
			ContentAlignment align = ContentAlignment.TopLeft;;
			
			if (button is CheckBox)
				align = (button as CheckBox).CheckAlign;
			else if (button is RadioButton)
				align = (button as RadioButton).CheckAlign;

			switch (align) {
				case ContentAlignment.BottomCenter:
					glyphArea.Y = button.Height - check_size;
					glyphArea.X = (button.Width - check_size) / 2 - 2;
					break;
				case ContentAlignment.BottomLeft:
					glyphArea.Y = button.Height - check_size - 2;
					content_rect.Width -= check_size;
					content_rect.Offset (check_size, 0);
					break;
				case ContentAlignment.BottomRight:
					glyphArea.Y = button.Height - check_size - 2;
					glyphArea.X = button.Width - check_size;
					content_rect.Width -= check_size;
					break;
				case ContentAlignment.MiddleCenter:
					glyphArea.Y = (button.Height - check_size) / 2;
					glyphArea.X = (button.Width - check_size) / 2;
					break;
				case ContentAlignment.MiddleLeft:
					glyphArea.Y = (button.Height - check_size) / 2;
					content_rect.Width -= check_size;
					content_rect.Offset (check_size, 0);
					break;
				case ContentAlignment.MiddleRight:
					glyphArea.Y = (button.Height - check_size) / 2;
					glyphArea.X = button.Width - check_size;
					content_rect.Width -= check_size;
					break;
				case ContentAlignment.TopCenter:
					glyphArea.X = (button.Width - check_size) / 2;
					break;
				case ContentAlignment.TopLeft:
					content_rect.Width -= check_size;
					content_rect.Offset (check_size, 0);
					break;
				case ContentAlignment.TopRight:
					glyphArea.X = button.Width - check_size;
					content_rect.Width -= check_size;
					break;
			}
			
			Image image = button.Image;
			string text = button.Text;
			
			Size proposed = Size.Empty;
			
			// Force wrapping if we aren't AutoSize and our text is too long
			if (!button.AutoSize)
				proposed.Width = button.Width - glyphArea.Width - 2;

			Size text_size = TextRenderer.MeasureTextInternal (text, button.Font, proposed, button.TextFormatFlags, button.UseCompatibleTextRendering);
			
			// Text can't be bigger than the content rectangle
			text_size.Height = Math.Min (text_size.Height, content_rect.Height);
			text_size.Width = Math.Min (text_size.Width, content_rect.Width);
			
			Size image_size = image == null ? Size.Empty : image.Size;

			textRectangle = Rectangle.Empty;
			imageRectangle = Rectangle.Empty;

			switch (button.TextImageRelation) {
				case TextImageRelation.Overlay:
					// Text is centered vertically, and 2 pixels to the right
					textRectangle.X = content_rect.Left + 2;
					textRectangle.Y = ((content_rect.Height - text_size.Height) / 2) - 1;
					textRectangle.Size = text_size;

					// Image is dependent on ImageAlign
					if (image == null)
						return;

					int image_x = 0;
					int image_y = 0;
					int image_height = image.Height;
					int image_width = image.Width;

					switch (button.ImageAlign) {
						case System.Drawing.ContentAlignment.TopLeft:
							image_x = 5;
							image_y = 5;
							break;
						case System.Drawing.ContentAlignment.TopCenter:
							image_x = (content_rect.Width - image_width) / 2;
							image_y = 5;
							break;
						case System.Drawing.ContentAlignment.TopRight:
							image_x = content_rect.Width - image_width - 5;
							image_y = 5;
							break;
						case System.Drawing.ContentAlignment.MiddleLeft:
							image_x = 5;
							image_y = (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.MiddleCenter:
							image_x = (content_rect.Width - image_width) / 2;
							image_y = (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.MiddleRight:
							image_x = content_rect.Width - image_width - 4;
							image_y = (content_rect.Height - image_height) / 2;
							break;
						case System.Drawing.ContentAlignment.BottomLeft:
							image_x = 5;
							image_y = content_rect.Height - image_height - 4;
							break;
						case System.Drawing.ContentAlignment.BottomCenter:
							image_x = (content_rect.Width - image_width) / 2;
							image_y = content_rect.Height - image_height - 4;
							break;
						case System.Drawing.ContentAlignment.BottomRight:
							image_x = content_rect.Width - image_width - 4;
							image_y = content_rect.Height - image_height - 4;
							break;
						default:
							image_x = 5;
							image_y = 5;
							break;
					}

					imageRectangle = new Rectangle (image_x + check_size, image_y, image_width, image_height);
					break;
				case TextImageRelation.ImageAboveText:
					content_rect.Inflate (-4, -4);
					LayoutTextAboveOrBelowImage (content_rect, false, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.TextAboveImage:
					content_rect.Inflate (-4, -4);
					LayoutTextAboveOrBelowImage (content_rect, true, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.ImageBeforeText:
					content_rect.Inflate (-4, -4);
					LayoutTextBeforeOrAfterImage (content_rect, false, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
				case TextImageRelation.TextBeforeImage:
					content_rect.Inflate (-4, -4);
					LayoutTextBeforeOrAfterImage (content_rect, true, text_size, image_size, button.TextAlign, button.ImageAlign, out textRectangle, out imageRectangle);
					break;
			}
		}

		public override Size CalculateCheckBoxAutoSize (CheckBox checkBox)
		{
			Size ret_size = Size.Empty;
			Size text_size = TextRenderer.MeasureTextInternal (checkBox.Text, checkBox.Font, checkBox.UseCompatibleTextRendering);
			Size image_size = checkBox.Image == null ? Size.Empty : checkBox.Image.Size;

			// Pad the text size
			if (checkBox.Text.Length != 0) {
				text_size.Height += 4;
				text_size.Width += 4;
			}

			switch (checkBox.TextImageRelation) {
				case TextImageRelation.Overlay:
					ret_size.Height = Math.Max (checkBox.Text.Length == 0 ? 0 : text_size.Height, image_size.Height);
					ret_size.Width = Math.Max (text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageAboveText:
				case TextImageRelation.TextAboveImage:
					ret_size.Height = text_size.Height + image_size.Height;
					ret_size.Width = Math.Max (text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageBeforeText:
				case TextImageRelation.TextBeforeImage:
					ret_size.Height = Math.Max (text_size.Height, image_size.Height);
					ret_size.Width = text_size.Width + image_size.Width;
					break;
			}

			// Pad the result
			ret_size.Height += (checkBox.Padding.Vertical);
			ret_size.Width += (checkBox.Padding.Horizontal) + 15;

			// There seems to be a minimum height
			if (ret_size.Height == checkBox.Padding.Vertical)
				ret_size.Height += 14;
				
			return ret_size;
		}
#endif

		public override void DrawCheckBox(Graphics dc, Rectangle clip_area, CheckBox checkbox) {
			StringFormat		text_format;
			Rectangle		client_rectangle;
			Rectangle		text_rectangle;
			Rectangle		checkbox_rectangle;
			int			checkmark_size=13;
			int			checkmark_space = 4;

			client_rectangle = checkbox.ClientRectangle;
			text_rectangle = client_rectangle;
			checkbox_rectangle = new Rectangle(text_rectangle.X, text_rectangle.Y, checkmark_size, checkmark_size);

			text_format = new StringFormat();
			text_format.Alignment = StringAlignment.Near;
			text_format.LineAlignment = StringAlignment.Center;
			if (checkbox.ShowKeyboardCuesInternal)
				text_format.HotkeyPrefix = HotkeyPrefix.Show;
			else
				text_format.HotkeyPrefix = HotkeyPrefix.Hide;

			/* Calculate the position of text and checkbox rectangle */
			if (checkbox.appearance!=Appearance.Button) {
				switch(checkbox.check_alignment) {
					case ContentAlignment.BottomCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						text_rectangle.Height=client_rectangle.Height-checkbox_rectangle.Y-checkmark_space;
						break;
					}

					case ContentAlignment.BottomLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X+checkmark_size+checkmark_space;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;						
						break;
					}

					case ContentAlignment.BottomRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;						
						break;
					}

					case ContentAlignment.MiddleCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						break;
					}

					default:
					case ContentAlignment.MiddleLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X+checkmark_size+checkmark_space;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;												
						break;
					}

					case ContentAlignment.MiddleRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;
						break;
					}

					case ContentAlignment.TopCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=client_rectangle.Top;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						text_rectangle.Y=checkmark_size+checkmark_space;
						text_rectangle.Height=client_rectangle.Height-checkmark_size-checkmark_space;
						break;
					}

					case ContentAlignment.TopLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						text_rectangle.X=client_rectangle.X+checkmark_size+checkmark_space;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;
						break;
					}

					case ContentAlignment.TopRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;
						break;
					}
				}
			} else {
				text_rectangle.X=client_rectangle.X;
				text_rectangle.Width=client_rectangle.Width;
			}
			
			/* Set the horizontal alignment of our text */
			switch(checkbox.text_alignment) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft: {
					text_format.Alignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter: {
					text_format.Alignment=StringAlignment.Center;
					break;
				}

				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight: {
					text_format.Alignment=StringAlignment.Far;
					break;
				}
			}

			/* Set the vertical alignment of our text */
			switch(checkbox.text_alignment) {
				case ContentAlignment.TopLeft: 
				case ContentAlignment.TopCenter: 
				case ContentAlignment.TopRight: {
					text_format.LineAlignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight: {
					text_format.LineAlignment=StringAlignment.Far;
					break;
				}

				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight: {
					text_format.LineAlignment=StringAlignment.Center;
					break;
				}
			}

			ButtonState state = ButtonState.Normal;
			if (checkbox.FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (checkbox.Checked) {
				state |= ButtonState.Checked;
			}
			
			if (checkbox.ThreeState && (checkbox.CheckState == CheckState.Indeterminate)) {
				state |= ButtonState.Checked;
				state |= ButtonState.Pushed;				
			}
			
			// finally make sure the pushed and inavtive states are rendered
			if (!checkbox.Enabled) {
				state |= ButtonState.Inactive;
			}
			else if (checkbox.is_pressed) {
				state |= ButtonState.Pushed;
			}
			
			// Start drawing
			
			CheckBox_DrawCheckBox(dc, checkbox, state, checkbox_rectangle);
			
			if ((checkbox.image != null) || (checkbox.image_list != null))
				ButtonBase_DrawImage(checkbox, dc);
			
			CheckBox_DrawText(checkbox, text_rectangle, dc, text_format);

			if (checkbox.Focused && checkbox.Enabled && checkbox.appearance != Appearance.Button && checkbox.Text != String.Empty && checkbox.ShowFocusCues) {
				SizeF text_size = dc.MeasureString (checkbox.Text, checkbox.Font);
				
				Rectangle focus_rect = Rectangle.Empty;
				focus_rect.X = text_rectangle.X;
				focus_rect.Y = (int)((text_rectangle.Height - text_size.Height) / 2);
				focus_rect.Size = text_size.ToSize ();
				CheckBox_DrawFocus (checkbox, dc, focus_rect);
			}

			text_format.Dispose ();
		}

		protected virtual void CheckBox_DrawCheckBox( Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle )
		{
			Brush brush = checkbox.BackColor.ToArgb () == ColorControl.ToArgb () ? SystemBrushes.Control : ResPool.GetSolidBrush (checkbox.BackColor);
			dc.FillRectangle (brush, checkbox.ClientRectangle);			
			// render as per normal button
			if (checkbox.appearance==Appearance.Button) {
				ButtonBase_DrawButton (checkbox, dc);
				
				if ((checkbox.Focused) && checkbox.Enabled)
					ButtonBase_DrawFocus(checkbox, dc);
			} else {
				// establish if we are rendering a flat style of some sort
				if (checkbox.FlatStyle == FlatStyle.Flat || checkbox.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleCheckBox (dc, checkbox_rectangle, checkbox);
				} else {
					CPDrawCheckBox (dc, checkbox_rectangle, state);
				}
			}
		}
		
		protected virtual void CheckBox_DrawText( CheckBox checkbox, Rectangle text_rectangle, Graphics dc, StringFormat text_format )
		{
			DrawCheckBox_and_RadioButtonText (checkbox, text_rectangle, dc, 
							  text_format, checkbox.Appearance, checkbox.Checked);
		}
		
		protected virtual void CheckBox_DrawFocus( CheckBox checkbox, Graphics dc, Rectangle text_rectangle )
		{
			DrawInnerFocusRectangle (dc, text_rectangle, checkbox.BackColor);
		}

		// renders a checkBox with the Flat and Popup FlatStyle
		protected virtual void DrawFlatStyleCheckBox (Graphics graphics, Rectangle rectangle, CheckBox checkbox)
		{
			Pen			pen;			
			Rectangle	rect;
			Rectangle	checkbox_rectangle;
			Rectangle	fill_rectangle;
			int			lineWidth;
			int			Scale;
			
			// set up our rectangles first
			if (checkbox.FlatStyle == FlatStyle.Popup && checkbox.is_entered) {
				// clip one pixel from bottom right for non popup rendered checkboxes
				checkbox_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max(rectangle.Width-1, 0), Math.Max(rectangle.Height-1,0));
				fill_rectangle = new Rectangle(checkbox_rectangle.X+1, checkbox_rectangle.Y+1, Math.Max(checkbox_rectangle.Width-3, 0), Math.Max(checkbox_rectangle.Height-3,0));
			} else {
				// clip two pixels from bottom right for non popup rendered checkboxes
				checkbox_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max(rectangle.Width-2, 0), Math.Max(rectangle.Height-2,0));
				fill_rectangle = new Rectangle(checkbox_rectangle.X+1, checkbox_rectangle.Y+1, Math.Max(checkbox_rectangle.Width-2, 0), Math.Max(checkbox_rectangle.Height-2,0));
			}	
			
			
			// if disabled render in disabled state
			if (checkbox.Enabled) {
				// process the state of the checkbox
				if (checkbox.is_entered || checkbox.Capture) {
					// decide on which background color to use
					if (checkbox.FlatStyle == FlatStyle.Popup && checkbox.is_entered && checkbox.Capture) {
						graphics.FillRectangle(ResPool.GetSolidBrush (checkbox.BackColor), fill_rectangle);
					} else if (checkbox.FlatStyle == FlatStyle.Flat) { 
						if (!checkbox.is_pressed) {
							graphics.FillRectangle(ResPool.GetSolidBrush (checkbox.BackColor), fill_rectangle);
						} else
							graphics.FillRectangle(ResPool.GetSolidBrush (ControlPaint.LightLight (checkbox.BackColor)), fill_rectangle);
					} else {
						// use regular window background color
						graphics.FillRectangle(ResPool.GetSolidBrush (ControlPaint.LightLight (checkbox.BackColor)), fill_rectangle);
					}
					
					// render the outer border
					if (checkbox.FlatStyle == FlatStyle.Flat) {
						ControlPaint.DrawBorder(graphics, checkbox_rectangle, checkbox.ForeColor, ButtonBorderStyle.Solid);
					} else {
						// draw sunken effect
						CPDrawBorder3D (graphics, checkbox_rectangle, Border3DStyle.SunkenInner, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, checkbox.BackColor);
					}
				} else {
					graphics.FillRectangle(ResPool.GetSolidBrush (ControlPaint.LightLight (checkbox.BackColor)), fill_rectangle);				
					
					if (checkbox.FlatStyle == FlatStyle.Flat) {
						ControlPaint.DrawBorder(graphics, checkbox_rectangle, checkbox.ForeColor, ButtonBorderStyle.Solid);
					} else {
						// draw the outer border
						ControlPaint.DrawBorder(graphics, checkbox_rectangle, ControlPaint.DarkDark (checkbox.BackColor), ButtonBorderStyle.Solid);
					}			
				}
			} else {
				if (checkbox.FlatStyle == FlatStyle.Popup) {
					graphics.FillRectangle(SystemBrushes.Control, fill_rectangle);
				}	
			
				// draw disabled state,
				ControlPaint.DrawBorder(graphics, checkbox_rectangle, ColorControlDark, ButtonBorderStyle.Solid);
			}		
			
			if (checkbox.Checked) {
				/* Need to draw a check-mark */
				
				/* Make sure we've got at least a line width of 1 */
				lineWidth = Math.Max(3, fill_rectangle.Width/3);
				Scale=Math.Max(1, fill_rectangle.Width/9);
				
				// flat style check box is rendered inside a rectangle shifted down by one
				rect=new Rectangle(fill_rectangle.X, fill_rectangle.Y+1, fill_rectangle.Width, fill_rectangle.Height);
				if (checkbox.Enabled) {
					pen=ResPool.GetPen(checkbox.ForeColor);
				} else {
					pen=SystemPens.ControlDark;
				}
				
				for (int i=0; i<lineWidth; i++) {
					graphics.DrawLine(pen, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
					graphics.DrawLine(pen, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
				}
			}					
		}

		private void DrawCheckBox_and_RadioButtonText (ButtonBase button_base, Rectangle text_rectangle, Graphics dc, 
							       StringFormat text_format, Appearance appearance, bool ischecked)
		{
			// offset the text if it's pressed and a button
			if (appearance == Appearance.Button) {
				if (ischecked || (button_base.Capture && button_base.FlatStyle != FlatStyle.Flat)) {
					text_rectangle.X ++;
					text_rectangle.Y ++;
				}
				
				text_rectangle.Inflate (-4, -4);
			}
			
			/* Place the text; to be compatible with Windows place it after the checkbox has been drawn */
			
			// Windows seems to not wrap text in certain situations, this matches as close as I could get it
			if ((float)(button_base.Font.Height * 1.5f) > text_rectangle.Height) {
				text_format.FormatFlags |= StringFormatFlags.NoWrap;
			}
			if (button_base.Enabled) {
				dc.DrawString (button_base.Text, button_base.Font, ResPool.GetSolidBrush (button_base.ForeColor), text_rectangle, text_format);			
			} else if (button_base.FlatStyle == FlatStyle.Flat || button_base.FlatStyle == FlatStyle.Popup) {
				dc.DrawString (button_base.Text, button_base.Font, SystemBrushes.ControlDarkDark, text_rectangle, text_format);
			} else {
				CPDrawStringDisabled (dc, button_base.Text, button_base.Font, button_base.BackColor, text_rectangle, text_format);
			}
		}
		#endregion	// CheckBox
		
		#region CheckedListBox
		
		public override void DrawCheckedListBoxItem (CheckedListBox ctrl, DrawItemEventArgs e)
		{			
			Color back_color, fore_color;
			Rectangle item_rect = e.Bounds;
			ButtonState state;

			/* Draw checkbox */		

			if ((e.State & DrawItemState.Checked) == DrawItemState.Checked) {
				state = ButtonState.Checked;
				if ((e.State & DrawItemState.Inactive) == DrawItemState.Inactive)
					state |= ButtonState.Inactive;
			} else
				state = ButtonState.Normal;

			if (ctrl.ThreeDCheckBoxes == false)
				state |= ButtonState.Flat;

			Rectangle checkbox_rect = new Rectangle (2, (item_rect.Height - 11) / 2, 13, 13);
			ControlPaint.DrawCheckBox (e.Graphics,
				item_rect.X + checkbox_rect.X, item_rect.Y + checkbox_rect.Y,
				checkbox_rect.Width, checkbox_rect.Height,
				state);

			item_rect.X += checkbox_rect.Right;
			item_rect.Width -= checkbox_rect.Right;
			
			/* Draw text*/
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}
			
			e.Graphics.FillRectangle (ResPool.GetSolidBrush
				(back_color), item_rect);

			e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
				ResPool.GetSolidBrush (fore_color),
				item_rect, ctrl.StringFormat);
					
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				CPDrawFocusRectangle (e.Graphics, item_rect,
					fore_color, back_color);
			}
		}
		
		#endregion // CheckedListBox
		
		#region ComboBox		
		public override void DrawComboBoxItem (ComboBox ctrl, DrawItemEventArgs e)
		{
			Color back_color, fore_color;
			Rectangle text_draw = e.Bounds;
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.LineLimit;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}
			
			if (!ctrl.Enabled)
				fore_color = ColorInactiveCaptionText;
							
			e.Graphics.FillRectangle (ResPool.GetSolidBrush (back_color), e.Bounds);

			if (e.Index != -1) {
				e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
					ResPool.GetSolidBrush (fore_color),
					text_draw, string_format);
			}
			
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				CPDrawFocusRectangle (e.Graphics, e.Bounds, fore_color, back_color);
			}

			string_format.Dispose ();
		}
		
		public override void DrawFlatStyleComboButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left + 1, centerY);
			P2=new Point(rect.Right - 1, centerY);
			P3=new Point(centerX, rect.Bottom - 1);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;
			
			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				/* Move away from the shadow */
				arrow[0].X += 1;	arrow[0].Y += 1;
				arrow[1].X += 1;	arrow[1].Y += 1;
				arrow[2].X += 1;	arrow[2].Y += 1;
				
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;

				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}		
		}
		public override void ComboBoxDrawNormalDropDownButton (ComboBox comboBox, Graphics g, Rectangle clippingArea, Rectangle area, ButtonState state)
		{
			CPDrawComboButton (g, area, state);
		}
		public override bool ComboBoxNormalDropDownButtonHasTransparentBackground (ComboBox comboBox, ButtonState state)
		{
			return true;
		}
		public override bool ComboBoxDropDownButtonHasHotElementStyle (ComboBox comboBox)
		{
			return false;
		}
		public override void ComboBoxDrawBackground (ComboBox comboBox, Graphics g, Rectangle clippingArea, FlatStyle style)
		{
			if (!comboBox.Enabled)
				g.FillRectangle (ResPool.GetSolidBrush (ColorControl), comboBox.ClientRectangle);

			if (comboBox.DropDownStyle == ComboBoxStyle.Simple)
				g.FillRectangle (ResPool.GetSolidBrush (comboBox.Parent.BackColor), comboBox.ClientRectangle);

			if (style == FlatStyle.Popup && (comboBox.Entered || comboBox.Focused)) {
				Rectangle area = comboBox.TextArea;
				area.Height -= 1;
				area.Width -= 1;
				g.DrawRectangle (ResPool.GetPen (SystemColors.ControlDark), area);
				g.DrawLine (ResPool.GetPen (SystemColors.ControlDark), comboBox.ButtonArea.X - 1, comboBox.ButtonArea.Top, comboBox.ButtonArea.X - 1, comboBox.ButtonArea.Bottom);
			}
			bool is_flat = style == FlatStyle.Flat || style == FlatStyle.Popup;
			if (!is_flat && clippingArea.IntersectsWith (comboBox.TextArea))
				ControlPaint.DrawBorder3D (g, comboBox.TextArea, Border3DStyle.Sunken);
		}
		public override bool CombBoxBackgroundHasHotElementStyle (ComboBox comboBox)
		{
			return false;
		}
		#endregion ComboBox
		
		#region Datagrid
		public override int DataGridPreferredColumnWidth { get { return 75;} }
		public override int DataGridMinimumColumnCheckBoxHeight { get { return 16;} }
		public override int DataGridMinimumColumnCheckBoxWidth { get { return 16;} }
		public override Color DataGridAlternatingBackColor { get { return ColorWindow;} }
		public override Color DataGridBackColor { get  { return  ColorWindow;} }		
		public override Color DataGridBackgroundColor { get  { return  ColorAppWorkspace;} }
		public override Color DataGridCaptionBackColor { get  { return ColorActiveCaption;} }
		public override Color DataGridCaptionForeColor { get  { return ColorActiveCaptionText;} }
		public override Color DataGridGridLineColor { get { return ColorControl;} }
		public override Color DataGridHeaderBackColor { get  { return ColorControl;} }
		public override Color DataGridHeaderForeColor { get  { return ColorControlText;} }
		public override Color DataGridLinkColor { get  { return ColorHotTrack;} }
		public override Color DataGridLinkHoverColor { get  { return ColorHotTrack;} }
		public override Color DataGridParentRowsBackColor { get  { return ColorControl;} }
		public override Color DataGridParentRowsForeColor { get  { return ColorWindowText;} }
		public override Color DataGridSelectionBackColor { get  { return ColorActiveCaption;} }
		public override Color DataGridSelectionForeColor { get  { return ColorActiveCaptionText;} }
		
		public override void DataGridPaint (PaintEventArgs pe, DataGrid grid)
		{
			DataGridPaintCaption (pe.Graphics, pe.ClipRectangle, grid);
			DataGridPaintParentRows (pe.Graphics, pe.ClipRectangle, grid);
			DataGridPaintColumnHeaders (pe.Graphics, pe.ClipRectangle, grid);
			DataGridPaintRows (pe.Graphics, grid.cells_area, pe.ClipRectangle, grid);

			// Paint scrollBar corner
			if (grid.VScrollBar.Visible && grid.HScrollBar.Visible) {

				Rectangle corner = new Rectangle (grid.ClientRectangle.X + grid.ClientRectangle.Width - grid.VScrollBar.Width,
								  grid.ClientRectangle.Y + grid.ClientRectangle.Height - grid.HScrollBar.Height,
								  grid.VScrollBar.Width, grid.HScrollBar.Height);

				if (pe.ClipRectangle.IntersectsWith (corner)) {
					pe.Graphics.FillRectangle (ResPool.GetSolidBrush (grid.ParentRowsBackColor),
								   corner);
				}
			}
		}

		public override void DataGridPaintCaption (Graphics g, Rectangle clip, DataGrid grid)
		{
			Rectangle bounds = clip;
			bounds.Intersect (grid.caption_area);

			// Background
			g.FillRectangle (ResPool.GetSolidBrush (grid.CaptionBackColor), bounds);

			// Bottom line
    		g.DrawLine (ResPool.GetPen (grid.CurrentTableStyle.CurrentHeaderForeColor),
					bounds.X, bounds.Y + bounds.Height -1, 
                    bounds.X + bounds.Width, bounds.Y + bounds.Height -1);

			// Caption text
			if (grid.CaptionText != String.Empty) {
				Rectangle text_rect = grid.caption_area;
				text_rect.Y += text_rect.Height / 2 - grid.CaptionFont.Height / 2;
				text_rect.Height = grid.CaptionFont.Height;

				g.DrawString (grid.CaptionText, grid.CaptionFont,
					      ResPool.GetSolidBrush (grid.CaptionForeColor),
					      text_rect);
			}

			// Back button
			if (bounds.IntersectsWith (grid.back_button_rect)) {
				g.DrawImage (grid.back_button_image, grid.back_button_rect);
				if (grid.back_button_mouseover) {
					CPDrawBorder3D (g, grid.back_button_rect, grid.back_button_active ? Border3DStyle.Sunken : Border3DStyle.Raised, all_sides);
				}
			}

			// Rows button
			if (bounds.IntersectsWith (grid.parent_rows_button_rect)) {
				g.DrawImage (grid.parent_rows_button_image, grid.parent_rows_button_rect);
				if (grid.parent_rows_button_mouseover) {
					CPDrawBorder3D (g, grid.parent_rows_button_rect, grid.parent_rows_button_active ? Border3DStyle.Sunken : Border3DStyle.Raised, all_sides);
				}
			}
		}

		public override void DataGridPaintColumnHeaders (Graphics g, Rectangle clip, DataGrid grid)
		{
			if (!grid.CurrentTableStyle.ColumnHeadersVisible)
				return;

			Rectangle columns_area = grid.column_headers_area;

			// Paint corner shared between row and column header
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				Rectangle rect_bloc = grid.column_headers_area;
				rect_bloc.Width = grid.RowHeaderWidth;
				if (clip.IntersectsWith (rect_bloc)) {
					if (grid.FlatMode)
						g.FillRectangle (ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderBackColor), rect_bloc);
					else
						CPDrawBorder3D (g, rect_bloc, Border3DStyle.RaisedInner, 
							Border3DSide.Left | Border3DSide.Right | 
							Border3DSide.Top | Border3DSide.Bottom | Border3DSide.Middle, 
							grid.CurrentTableStyle.CurrentHeaderBackColor);
				}

				columns_area.X += grid.RowHeaderWidth;
				columns_area.Width -= grid.RowHeaderWidth;
			}

			// Set column painting
			Rectangle rect_columnhdr = new Rectangle ();
			int col_pixel;
			Region current_clip;
			Region prev_clip = g.Clip;
			rect_columnhdr.Y = columns_area.Y;
			rect_columnhdr.Height = columns_area.Height;

			int column_cnt = grid.FirstVisibleColumn + grid.VisibleColumnCount;
			for (int column = grid.FirstVisibleColumn; column < column_cnt; column++) {
				if (grid.CurrentTableStyle.GridColumnStyles[column].bound == false)
					continue;
				
				col_pixel = grid.GetColumnStartingPixel (column);
				rect_columnhdr.X = columns_area.X + col_pixel - grid.HorizPixelOffset;
				rect_columnhdr.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

				if (clip.IntersectsWith (rect_columnhdr) == false)
					continue;

				current_clip = new Region (rect_columnhdr);
				current_clip.Intersect (columns_area);
				current_clip.Intersect (prev_clip);
				g.Clip = current_clip;

				DataGridPaintColumnHeader (g, rect_columnhdr, grid, column);

				current_clip.Dispose ();
			}

			g.Clip = prev_clip;
				
			Rectangle not_usedarea = grid.column_headers_area;
			not_usedarea.X = (column_cnt == 0) ? grid.RowHeaderWidth : rect_columnhdr.X + rect_columnhdr.Width;
			not_usedarea.Width = grid.ClientRectangle.X + grid.ClientRectangle.Width - not_usedarea.X;
			g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor), not_usedarea);
		}

		public override void DataGridPaintColumnHeader (Graphics g, Rectangle bounds, DataGrid grid, int col)
		{
			// Background
			g.FillRectangle (ResPool.GetSolidBrush (grid.CurrentTableStyle.HeaderBackColor), bounds);

			// Paint Borders
			if (!grid.FlatMode) {
				g.DrawLine (ResPool.GetPen (ColorControlLightLight),
					bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);
				
				if (col == 0) {
					g.DrawLine (ResPool.GetPen (ColorControlLightLight),
						bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height);
				} else {
					g.DrawLine (ResPool.GetPen (ColorControlLightLight),
						bounds.X, bounds.Y + 2, bounds.X, bounds.Y + bounds.Height - 3);
				}
				
				if (col == (grid.VisibleColumnCount -1)) {
					g.DrawLine (ResPool.GetPen (ColorControlDark),
						bounds.X + bounds.Width - 1, bounds.Y, 
						bounds.X + bounds.Width - 1, bounds.Y + bounds.Height);
				} else {
					g.DrawLine (ResPool.GetPen (ColorControlDark),
						bounds.X + bounds.Width - 1, bounds.Y + 2, 
						bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 3);
				}

				g.DrawLine (ResPool.GetPen (ColorControlDark),
					bounds.X, bounds.Y + bounds.Height - 1, 
					bounds.X + bounds.Width, bounds.Y + bounds.Height - 1);
			}

			bounds.X += 2;
			bounds.Width -= 2;

			DataGridColumnStyle style = grid.CurrentTableStyle.GridColumnStyles[col];

			if (style.ArrowDrawingMode != DataGridColumnStyle.ArrowDrawing.No)
				bounds.Width -= 16;

			// Caption
			StringFormat format = new StringFormat ();
			format.FormatFlags |= StringFormatFlags.NoWrap;
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.Character;

			g.DrawString (style.HeaderText, grid.CurrentTableStyle.HeaderFont, 
				ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderForeColor), 
				bounds, format);

			// Arrow (6 x 6)
			if (style.ArrowDrawingMode != DataGridColumnStyle.ArrowDrawing.No) {
				Point pnt = new Point (bounds.X + bounds.Width + 4, bounds.Y + ((bounds.Height - 6)/2));
				
				if (style.ArrowDrawingMode == DataGridColumnStyle.ArrowDrawing.Ascending) {
					g.DrawLine (SystemPens.ControlLightLight, pnt.X + 6, pnt.Y + 6, pnt.X + 3, pnt.Y);
					g.DrawLine (SystemPens.ControlDark, pnt.X, pnt.Y + 6, pnt.X + 6, pnt.Y + 6);
					g.DrawLine (SystemPens.ControlDark, pnt.X, pnt.Y + 6, pnt.X + 3, pnt.Y);
				} else {
					g.DrawLine (SystemPens.ControlLightLight, pnt.X + 6, pnt.Y, pnt.X + 3, pnt.Y + 6);
					g.DrawLine (SystemPens.ControlDark, pnt.X, pnt.Y, pnt.X + 6, pnt.Y);
					g.DrawLine (SystemPens.ControlDark, pnt.X, pnt.Y, pnt.X + 3, pnt.Y + 6);
				}
			}
		}

		public override void DataGridPaintParentRows (Graphics g, Rectangle clip, DataGrid grid)
		{
			Rectangle rect_row = new Rectangle ();

			rect_row.X = grid.ParentRowsArea.X;
			rect_row.Width = grid.ParentRowsArea.Width;
			rect_row.Height = (grid.CaptionFont.Height + 3);

			object[] parentRows = grid.data_source_stack.ToArray();
			
			Region current_clip;
			Region prev_clip = g.Clip;
			for (int row = 0; row < parentRows.Length; row++) {
				rect_row.Y = grid.ParentRowsArea.Y + row * rect_row.Height;

				if (clip.IntersectsWith (rect_row) == false)
					continue;

				current_clip = new Region (rect_row);
				current_clip.Intersect (prev_clip);
				g.Clip = current_clip;

				DataGridPaintParentRow (g, rect_row, (DataGridDataSource)parentRows[parentRows.Length - row - 1], grid);

				current_clip.Dispose ();
			}
			
			g.Clip = prev_clip;
		}

		public override void DataGridPaintParentRow (Graphics g, Rectangle bounds, DataGridDataSource row, DataGrid grid)
		{
			// Background
			g.FillRectangle (ResPool.GetSolidBrush (grid.ParentRowsBackColor),
					 bounds);

			Font bold_font = new Font (grid.Font.FontFamily, grid.Font.Size, grid.Font.Style | FontStyle.Bold);
			// set up some standard string formating variables
			StringFormat text_format = new StringFormat();
			text_format.LineAlignment = StringAlignment.Center;
			text_format.Alignment = StringAlignment.Near;

			string table_name = "";
			if (row.view is DataRowView)
				table_name = ((ITypedList)((DataRowView)row.view).DataView).GetListName (null) + ": ";
			// XXX else?

			Rectangle	text_rect;
			Size		text_size;

			text_size = g.MeasureString (table_name, bold_font).ToSize();
			text_rect = new Rectangle(new Point(bounds.X + 3, bounds.Y + bounds.Height - text_size.Height), text_size);

			g.DrawString (table_name,
				      bold_font, ResPool.GetSolidBrush (grid.ParentRowsForeColor), text_rect, text_format);

			foreach (PropertyDescriptor pd in ((ICustomTypeDescriptor)row.view).GetProperties()) {
				if (typeof(IBindingList).IsAssignableFrom (pd.PropertyType))
					continue;

				text_rect.X += text_rect.Size.Width + 5;

				string text = String.Format ("{0}: {1}",
							     pd.Name,
							     pd.GetValue (row.view));

				text_rect.Size = g.MeasureString (text, grid.Font).ToSize();
				text_rect.Y = bounds.Y + bounds.Height - text_rect.Height; // XXX

				g.DrawString (text,
					      grid.Font, ResPool.GetSolidBrush (grid.ParentRowsForeColor), text_rect, text_format);
			}

            // Paint Borders
			if (!grid.FlatMode) {
                CPDrawBorder3D (g, bounds, Border3DStyle.RaisedInner, 
                    Border3DSide.Left | Border3DSide.Right | 
                    Border3DSide.Top | Border3DSide.Bottom);
			}
		}

		public override void DataGridPaintRowHeaderArrow (Graphics g, Rectangle bounds, DataGrid grid) 
		{		
			Point[] arrow = new Point[3];
			Point P1, P2, P3;
			int centerX, centerY, shiftX;			
			Rectangle rect;
			
			rect = new Rectangle (bounds.X + bounds.Width /4, 
				bounds.Y + bounds.Height/4, bounds.Width / 2, bounds.Height / 2);
			
			centerX = rect.Left + rect.Width / 2;
			centerY = rect.Top + rect.Height / 2;
			shiftX = Math.Max (1, rect.Width / 8);			
			rect.X -= shiftX;
			centerX -= shiftX;			
			P1 = new Point (centerX, rect.Top - 1);
			P2 = new Point (centerX, rect.Bottom);
			P3 = new Point (rect.Right, centerY);			
			arrow[0] = P1;
			arrow[1] = P2;
			arrow[2] = P3;
			
			g.FillPolygon (ResPool.GetSolidBrush 
				(grid.CurrentTableStyle.CurrentHeaderForeColor), arrow, FillMode.Winding);
		}

		public override void DataGridPaintRowHeaderStar (Graphics g, Rectangle bounds, DataGrid grid) 
		{
			int x = bounds.X + 4;
			int y = bounds.Y + 3;
			Pen pen = ResPool.GetPen (grid.CurrentTableStyle.CurrentHeaderForeColor);

			g.DrawLine (pen, x + 4, y, x + 4, y + 8);
			g.DrawLine (pen, x, y + 4, x + 8, y + 4);
			g.DrawLine (pen, x + 1, y + 1, x + 7, y + 7);
			g.DrawLine (pen, x + 7, y + 1, x + 1, y + 7);
		}		

		public override void DataGridPaintRowHeader (Graphics g, Rectangle bounds, int row, DataGrid grid)
		{
			bool is_add_row = grid.ShowEditRow && row == grid.DataGridRows.Length - 1;
			bool is_current_row = row == grid.CurrentCell.RowNumber;

			// Background
			g.FillRectangle (ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderBackColor), bounds);

			// Draw arrow
			if (is_current_row) {
				if (grid.IsChanging) {
					g.DrawString ("...", grid.Font,
						      ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderForeColor),
						      bounds);
				} else {
					Rectangle rect = new Rectangle (bounds.X - 2, bounds.Y, 18, 18);
					DataGridPaintRowHeaderArrow (g, rect, grid);
				}
			}
			else if (is_add_row) {
				DataGridPaintRowHeaderStar (g, bounds, grid);
			}

			if (!grid.FlatMode && !is_add_row) {
				CPDrawBorder3D (g, bounds, Border3DStyle.RaisedInner, 
					Border3DSide.Left | Border3DSide.Right | 
					Border3DSide.Top | Border3DSide.Bottom);
			}
		}
		
		public override void DataGridPaintRows (Graphics g, Rectangle cells, Rectangle clip, DataGrid grid)
		{
			Rectangle rect_row = new Rectangle ();
			Rectangle not_usedarea = new Rectangle ();

			int rowcnt = grid.VisibleRowCount;
			
			bool showing_add_row = false;

			if (grid.RowsCount < grid.DataGridRows.Length) {
				/* the table has an add row */

				if (grid.FirstVisibleRow + grid.VisibleRowCount >= grid.DataGridRows.Length) {
					showing_add_row = true;
				}
			}

			rect_row.Width = cells.Width + grid.RowHeadersArea.Width;
			for (int r = 0; r < rowcnt; r++) {
				int row = grid.FirstVisibleRow + r;
				if (row == grid.DataGridRows.Length - 1)
					rect_row.Height = grid.DataGridRows[row].Height;
				else
					rect_row.Height = grid.DataGridRows[row + 1].VerticalOffset - grid.DataGridRows[row].VerticalOffset;
				rect_row.Y = cells.Y + grid.DataGridRows[row].VerticalOffset - grid.DataGridRows[grid.FirstVisibleRow].VerticalOffset;
				if (clip.IntersectsWith (rect_row)) {
					if (grid.CurrentTableStyle.HasRelations
					    && !(showing_add_row && row == grid.DataGridRows.Length - 1))
						DataGridPaintRelationRow (g, row, rect_row, false, clip, grid);
					else
						DataGridPaintRow (g, row, rect_row, showing_add_row && row == grid.DataGridRows.Length - 1, clip, grid);
				}
			}

			not_usedarea.X = 0;
			// the rowcnt == 0 check is needed because
			// otherwise we'd draw over the caption on
			// empty datasources (since rect_row would be
			// Empty)
			if (rowcnt == 0)
				not_usedarea.Y = cells.Y;
			else
				not_usedarea.Y = rect_row.Y + rect_row.Height;
			not_usedarea.Height = cells.Y + cells.Height - rect_row.Y - rect_row.Height;
			not_usedarea.Width = cells.Width + grid.RowHeadersArea.Width;

			g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor), not_usedarea);
		}

		public override void DataGridPaintRelationRow (Graphics g, int row, Rectangle row_rect, bool is_newrow,
							       Rectangle clip, DataGrid grid)
		{
			Rectangle rect_header;
			Rectangle icon_bounds = new Rectangle ();
			Pen pen = ThemeEngine.Current.ResPool.GetPen (grid.CurrentTableStyle.ForeColor);

			/* paint the header if it's visible and intersects the clip */
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				rect_header = row_rect;
				rect_header.Width = grid.RowHeaderWidth;
				row_rect.X += grid.RowHeaderWidth;
				if (clip.IntersectsWith (rect_header)) {
					DataGridPaintRowHeader (g, rect_header, row, grid);
				}

				icon_bounds = rect_header;
				icon_bounds.X += icon_bounds.Width / 2;
				icon_bounds.Y += 3;
				icon_bounds.Width = 8;
				icon_bounds.Height = 8;

				g.DrawRectangle (pen, icon_bounds);

				/* the - part of the icon */
				g.DrawLine (pen,
					    icon_bounds.X + 2, icon_bounds.Y + icon_bounds.Height / 2,
					    icon_bounds.X + icon_bounds.Width - 2, icon_bounds.Y + icon_bounds.Height / 2);
					    
				if (!grid.IsExpanded (row)) {
					/* the | part of the icon */
					g.DrawLine (pen,
						    icon_bounds.X + icon_bounds.Width / 2, icon_bounds.Y + 2,
						    icon_bounds.X + icon_bounds.Width / 2, icon_bounds.Y + icon_bounds.Height - 2);
				}
			}

			Rectangle nested_rect = row_rect;

			if (grid.DataGridRows[row].IsExpanded)
				nested_rect.Height -= grid.DataGridRows[row].RelationHeight;

			DataGridPaintRowContents (g, row, nested_rect, is_newrow, clip, grid);

			if (grid.DataGridRows[row].IsExpanded) {
				// XXX we should create this in the
				// datagrid and cache it for use by
				// the theme instead of doing it each
				// time through here
				string[] relations = grid.CurrentTableStyle.Relations;
				StringBuilder relation_builder = new StringBuilder ("");

				for (int i = 0; i < relations.Length; i ++) {
					if (i > 0)
						relation_builder.Append ("\n");

					relation_builder.Append (relations[i]);
				}
				string relation_text = relation_builder.ToString ();

				StringFormat string_format = new StringFormat ();
				string_format.FormatFlags |= StringFormatFlags.NoWrap;


				//Region prev_clip = g.Clip;
				//Region current_clip;
				Rectangle rect_cell = row_rect;

				rect_cell.X = nested_rect.X + grid.GetColumnStartingPixel (grid.FirstVisibleColumn) - grid.HorizPixelOffset;
				rect_cell.Y += nested_rect.Height;
				rect_cell.Height = grid.DataGridRows[row].RelationHeight;

				rect_cell.Width = 0;
				int column_cnt = grid.FirstVisibleColumn + grid.VisibleColumnCount;
				for (int column = grid.FirstVisibleColumn; column < column_cnt; column++) {
					if (grid.CurrentTableStyle.GridColumnStyles[column].bound == false)
						continue;
					rect_cell.Width += grid.CurrentTableStyle.GridColumnStyles[column].Width;
				}
				rect_cell.Width = Math.Max (rect_cell.Width, grid.DataGridRows[row].relation_area.Width);

				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.CurrentTableStyle.BackColor),
						 rect_cell);


				/* draw the line leading from the +/- to the relation area */
				Rectangle outline = grid.DataGridRows[row].relation_area;
				outline.Y = rect_cell.Y;
				outline.Height --;

				g.DrawLine (pen,
					    icon_bounds.X + icon_bounds.Width / 2, icon_bounds.Y + icon_bounds.Height,
					    icon_bounds.X + icon_bounds.Width / 2, outline.Y + outline.Height / 2);

				g.DrawLine (pen,
					    icon_bounds.X + icon_bounds.Width / 2, outline.Y + outline.Height / 2,
					    outline.X, outline.Y + outline.Height / 2);

				g.DrawRectangle (pen, outline);

				g.DrawString (relation_text, grid.LinkFont, ResPool.GetSolidBrush (grid.LinkColor),
					      outline, string_format);

				if (row_rect.X + row_rect.Width > rect_cell.X + rect_cell.Width) {
					Rectangle not_usedarea = new Rectangle ();
					not_usedarea.X = rect_cell.X + rect_cell.Width;
					not_usedarea.Width = row_rect.X + row_rect.Width - rect_cell.X - rect_cell.Width;
					not_usedarea.Y = row_rect.Y;
					not_usedarea.Height = row_rect.Height;
					if (clip.IntersectsWith (not_usedarea))
						g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor),
								 not_usedarea);
				}
			}
		}

		public override void DataGridPaintRowContents (Graphics g, int row, Rectangle row_rect, bool is_newrow,
							       Rectangle clip, DataGrid grid)
		{
			Rectangle rect_cell = new Rectangle ();
			int col_pixel;
			Color backcolor, forecolor;
			Brush backBrush, foreBrush;
			Rectangle not_usedarea = Rectangle.Empty;

			rect_cell.Y = row_rect.Y;
			rect_cell.Height = row_rect.Height;

			if (grid.IsSelected (row)) {
				backcolor =  grid.SelectionBackColor;
				forecolor =  grid.SelectionForeColor;
			} else {
				if (row % 2 == 0) {
					backcolor =  grid.BackColor;
				} else {
					backcolor =  grid.AlternatingBackColor;
				}

				forecolor =  grid.ForeColor;
			}			


			backBrush = ResPool.GetSolidBrush (backcolor);
			foreBrush = ResPool.GetSolidBrush (forecolor);

			// PaintCells at row, column
			int column_cnt = grid.FirstVisibleColumn + grid.VisibleColumnCount;
			DataGridCell current_cell = grid.CurrentCell;

			if (column_cnt > 0) {
				Region prev_clip = g.Clip;
				Region current_clip;

				for (int column = grid.FirstVisibleColumn; column < column_cnt; column++) {
					if (grid.CurrentTableStyle.GridColumnStyles[column].bound == false)
						continue;

					col_pixel = grid.GetColumnStartingPixel (column);

					rect_cell.X = row_rect.X + col_pixel - grid.HorizPixelOffset;
					rect_cell.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

					if (clip.IntersectsWith (rect_cell)) {
						current_clip = new Region (rect_cell);
						current_clip.Intersect (row_rect);
						current_clip.Intersect (prev_clip);
						g.Clip = current_clip;

						Brush colBackBrush = backBrush;
						Brush colForeBrush = foreBrush;

						// If we are in the precise cell we are editing, then use the normal colors
						// even if we are selected.
						if (grid.is_editing && column == current_cell.ColumnNumber && row == current_cell.RowNumber) {
							colBackBrush = ResPool.GetSolidBrush (grid.BackColor);
							colForeBrush = ResPool.GetSolidBrush (grid.ForeColor);
						}

						if (is_newrow) {
							grid.CurrentTableStyle.GridColumnStyles[column].PaintNewRow (g, rect_cell, 
														     colBackBrush,
														     colForeBrush);
						} else {
							grid.CurrentTableStyle.GridColumnStyles[column].Paint (g, rect_cell, grid.ListManager, row,
													       colBackBrush,
													       colForeBrush,
													       grid.RightToLeft == RightToLeft.Yes);
						}

						current_clip.Dispose ();
					}
				}

				g.Clip = prev_clip;
			
				if (row_rect.X + row_rect.Width > rect_cell.X + rect_cell.Width) {
					not_usedarea.X = rect_cell.X + rect_cell.Width;
					not_usedarea.Width = row_rect.X + row_rect.Width - rect_cell.X - rect_cell.Width;
					not_usedarea.Y = row_rect.Y;
					not_usedarea.Height = row_rect.Height;
				}
			}
			else {
				not_usedarea = row_rect;
			}

			if (!not_usedarea.IsEmpty && clip.IntersectsWith (not_usedarea))
				g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor),
						 not_usedarea);
		}

		public override void DataGridPaintRow (Graphics g, int row, Rectangle row_rect, bool is_newrow,
						       Rectangle clip, DataGrid grid)
		{			
			/* paint the header if it's visible and intersects the clip */
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				Rectangle rect_header = row_rect;
				rect_header.Width = grid.RowHeaderWidth;
				row_rect.X += grid.RowHeaderWidth;
				if (clip.IntersectsWith (rect_header)) {
					DataGridPaintRowHeader (g, rect_header, row, grid);
				}
			}

			DataGridPaintRowContents (g, row, row_rect, is_newrow, clip, grid);
		}
		
		#endregion // Datagrid

#if NET_2_0
		#region DataGridView
		#region DataGridViewHeaderCell
		#region DataGridViewRowHeaderCell
		public override bool DataGridViewRowHeaderCellDrawBackground (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}

		public override bool DataGridViewRowHeaderCellDrawSelectionBackground (DataGridViewRowHeaderCell cell)
		{
			return false;
		}

		public override bool DataGridViewRowHeaderCellDrawBorder (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}
		#endregion

		#region DataGridViewColumnHeaderCell
		public override bool DataGridViewColumnHeaderCellDrawBackground (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}

		public override bool DataGridViewColumnHeaderCellDrawBorder (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds)
		{
			return false;
		}
		#endregion

		public override bool DataGridViewHeaderCellHasPressedStyle  (DataGridView dataGridView)
		{
			return false;
		}

		public override bool DataGridViewHeaderCellHasHotStyle (DataGridView dataGridView)
		{
			return false;
		}
		#endregion
		#endregion
#endif

		#region DateTimePicker
		protected virtual void DateTimePickerDrawBorder (DateTimePicker dateTimePicker, Graphics g, Rectangle clippingArea)
		{
			this.CPDrawBorder3D (g, dateTimePicker.ClientRectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, dateTimePicker.BackColor);
		}

		protected virtual void DateTimePickerDrawDropDownButton (DateTimePicker dateTimePicker, Graphics g, Rectangle clippingArea)
		{
			ButtonState state = dateTimePicker.is_drop_down_visible ? ButtonState.Pushed : ButtonState.Normal;
			g.FillRectangle (ResPool.GetSolidBrush (ColorControl), dateTimePicker.drop_down_arrow_rect);
			this.CPDrawComboButton ( 
			  g, 
			  dateTimePicker.drop_down_arrow_rect, 
			  state);
		}

		public override void DrawDateTimePicker(Graphics dc, Rectangle clip_rectangle, DateTimePicker dtp)
		{

			if (!clip_rectangle.IntersectsWith (dtp.ClientRectangle))
				return;

			// draw the outer border
			Rectangle button_bounds = dtp.ClientRectangle;
			DateTimePickerDrawBorder (dtp, dc, clip_rectangle);

			// deflate by the border width
			if (clip_rectangle.IntersectsWith (dtp.drop_down_arrow_rect)) {
				button_bounds.Inflate (-2,-2);
				if (!dtp.ShowUpDown) {
					DateTimePickerDrawDropDownButton (dtp, dc, clip_rectangle);
				} else {
					ButtonState up_state = dtp.is_up_pressed ? ButtonState.Pushed : ButtonState.Normal;
					ButtonState down_state = dtp.is_down_pressed ? ButtonState.Pushed : ButtonState.Normal;
					Rectangle up_bounds = dtp.drop_down_arrow_rect;
					Rectangle down_bounds = dtp.drop_down_arrow_rect;

					up_bounds.Height = up_bounds.Height / 2;
					down_bounds.Y = up_bounds.Height;
					down_bounds.Height = dtp.Height - up_bounds.Height;
					if (down_bounds.Height > up_bounds.Height)
					{
						down_bounds.Y += 1;
						down_bounds.Height -= 1;
					}

					up_bounds.Inflate (-1, -1);
					down_bounds.Inflate (-1, -1);

					ControlPaint.DrawScrollButton (dc, up_bounds, ScrollButton.Up, up_state);
					ControlPaint.DrawScrollButton (dc, down_bounds, ScrollButton.Down, down_state);
				}
			}

			// render the date part
			if (!clip_rectangle.IntersectsWith (dtp.date_area_rect))
				return;

			// fill the background
			dc.FillRectangle (SystemBrushes.Window, dtp.date_area_rect);

			// Update date_area_rect if we are drawing the checkbox
			Rectangle date_area_rect = dtp.date_area_rect;
			if (dtp.ShowCheckBox) {
				Rectangle check_box_rect = dtp.CheckBoxRect;
				date_area_rect.X = date_area_rect.X + check_box_rect.Width + DateTimePicker.check_box_space * 2;
				date_area_rect.Width = date_area_rect.Width - check_box_rect.Width - DateTimePicker.check_box_space * 2;

				ButtonState bs = dtp.Checked ? ButtonState.Checked : ButtonState.Normal;
				CPDrawCheckBox(dc, check_box_rect, bs);

				if (dtp.is_checkbox_selected)
					CPDrawFocusRectangle (dc, check_box_rect, dtp.foreground_color, dtp.background_color);
			}

			// render each text part
			using (StringFormat text_format = StringFormat.GenericTypographic)
			{
				text_format.LineAlignment = StringAlignment.Near;
				text_format.Alignment = StringAlignment.Near;
				text_format.FormatFlags = text_format.FormatFlags | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
				text_format.FormatFlags &= ~StringFormatFlags.NoClip;

				// Calculate the rectangles for each part 
				if (dtp.part_data.Length > 0 && dtp.part_data[0].drawing_rectangle.IsEmpty)
				{
					Graphics gr = dc;
					for (int i = 0; i < dtp.part_data.Length; i++)
					{
						DateTimePicker.PartData fd = dtp.part_data[i];
						RectangleF text_rect = new RectangleF();
						string text = fd.GetText(dtp.Value);
						text_rect.Size = gr.MeasureString (text, dtp.Font, 250, text_format);
						if (!fd.is_literal)
							text_rect.Width = Math.Max (dtp.CalculateMaxWidth(fd.value, gr, text_format), text_rect.Width);

						if (i > 0) {
							text_rect.X = dtp.part_data[i - 1].drawing_rectangle.Right;
						} else {
							text_rect.X = date_area_rect.X;
						}
						text_rect.Y = 2;
						text_rect.Inflate (1, 0);
						fd.drawing_rectangle = text_rect;
					}
				}
				
				// draw the text part
				Brush text_brush = ResPool.GetSolidBrush (dtp.ShowCheckBox && dtp.Checked == false ?
						SystemColors.GrayText : dtp.ForeColor); // Use GrayText if Checked is false
				RectangleF clip_rectangleF = clip_rectangle;

				for (int i = 0; i < dtp.part_data.Length; i++)
				{
					DateTimePicker.PartData fd = dtp.part_data [i];
					string text;

					if (!clip_rectangleF.IntersectsWith (fd.drawing_rectangle))
						continue;

					text = dtp.editing_part_index == i ? dtp.editing_text : fd.GetText (dtp.Value);

					PointF text_position = new PointF ();
					SizeF text_size;
					RectangleF text_rect;

					text_size = dc.MeasureString (text, dtp.Font, 250, text_format);
					text_position.X = (fd.drawing_rectangle.Left + fd.drawing_rectangle.Width / 2) - text_size.Width / 2;
					text_position.Y = (fd.drawing_rectangle.Top + fd.drawing_rectangle.Height / 2) - text_size.Height / 2;
					text_rect = new RectangleF (text_position, text_size);
					text_rect = RectangleF.Intersect (text_rect, date_area_rect);
					
					if (text_rect.IsEmpty)
						break;

					if (text_rect.Right >= date_area_rect.Right)
						text_format.FormatFlags &= ~StringFormatFlags.NoClip;
					else
						text_format.FormatFlags |= StringFormatFlags.NoClip;
					
					if (fd.Selected) {
						dc.FillRectangle (SystemBrushes.Highlight, text_rect);
						dc.DrawString (text, dtp.Font, SystemBrushes.HighlightText, text_rect, text_format);
					
					} else {
						dc.DrawString (text, dtp.Font, text_brush, text_rect, text_format);
					}

					if (fd.drawing_rectangle.Right > date_area_rect.Right)
						break; // the next part would be not be visible, so don't draw anything more.
				}
			}
		}

		public override bool DateTimePickerBorderHasHotElementStyle {
			get {
				return false;
			}
		}

		public override Rectangle DateTimePickerGetDropDownButtonArea (DateTimePicker dateTimePicker)
		{
			Rectangle rect = dateTimePicker.ClientRectangle;
			rect.X = rect.Right - SystemInformation.VerticalScrollBarWidth - 2;
			if (rect.Width > (SystemInformation.VerticalScrollBarWidth + 2)) {
				rect.Width = SystemInformation.VerticalScrollBarWidth;
			} else {
				rect.Width = Math.Max (rect.Width - 2, 0);
			}
			
			rect.Inflate (0, -2);
			return rect;
		}

		public override Rectangle DateTimePickerGetDateArea (DateTimePicker dateTimePicker)
		{
			Rectangle rect = dateTimePicker.ClientRectangle;
			if (dateTimePicker.ShowUpDown) {
				// set the space to the left of the up/down button
				if (rect.Width > (DateTimePicker.up_down_width + 4)) {
					rect.Width -= (DateTimePicker.up_down_width + 4);
				} else {
					rect.Width = 0;
				}
			} else {
				// set the space to the left of the up/down button
				// TODO make this use up down button
				if (rect.Width > (SystemInformation.VerticalScrollBarWidth + 4)) {
					rect.Width -= SystemInformation.VerticalScrollBarWidth;
				} else {
					rect.Width = 0;
				}
			}
			
			rect.Inflate (-2, -2);
			return rect;
		}
		public override bool DateTimePickerDropDownButtonHasHotElementStyle {
			get {
				return false;
			}
		}
		#endregion // DateTimePicker

		#region GroupBox
		public override void DrawGroupBox (Graphics dc,  Rectangle area, GroupBox box) {
			StringFormat	text_format;
			SizeF		size;
			int		width;
			int		y;

			dc.FillRectangle (GetControlBackBrush (box.BackColor), box.ClientRectangle);
			
			text_format = new StringFormat();
			text_format.HotkeyPrefix = HotkeyPrefix.Show;

			size = dc.MeasureString (box.Text, box.Font);
			width = 0;

			if (size.Width > 0) {
				width = ((int) size.Width) + 7;
			
				if (width > box.Width - 16)
					width = box.Width - 16;
			}
			
			y = box.Font.Height / 2;

			// Clip the are that the text will be in
			Region prev_clip = dc.Clip;
			dc.SetClip (new Rectangle (10, 0, width, box.Font.Height), CombineMode.Exclude);
			/* Draw group box*/
			CPDrawBorder3D (dc, new Rectangle (0, y, box.Width, box.Height - y), Border3DStyle.Etched, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, box.BackColor);
			dc.Clip = prev_clip;

			/* Text */
			if (box.Text.Length != 0) {
				if (box.Enabled) {
					dc.DrawString (box.Text, box.Font, ResPool.GetSolidBrush (box.ForeColor), 10, 0, text_format);
				} else {
					CPDrawStringDisabled (dc, box.Text, box.Font, box.BackColor, 
							      new RectangleF (10, 0, width,  box.Font.Height), text_format);
				}
			}
			
			text_format.Dispose ();	
		}

		public override Size GroupBoxDefaultSize {
			get {
				return new Size (200,100);
			}
		}
		#endregion

		#region HScrollBar
		public override Size HScrollBarDefaultSize {
			get {
				return new Size (80, this.ScrollBarButtonSize);
			}
		}

		#endregion	// HScrollBar

		#region ListBox

		public override void DrawListBoxItem (ListBox ctrl, DrawItemEventArgs e)
		{
			Color back_color, fore_color;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			} else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}

			e.Graphics.FillRectangle (ResPool.GetSolidBrush (back_color), e.Bounds);

			e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
					       ResPool.GetSolidBrush (fore_color),
					       e.Bounds, ctrl.StringFormat);
					
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
				CPDrawFocusRectangle (e.Graphics, e.Bounds, fore_color, back_color);
		}
		
		#endregion ListBox

		#region ListView
		// Drawing
		public override void DrawListViewItems (Graphics dc, Rectangle clip, ListView control)
		{
			bool details = control.View == View.Details;
			int first = control.FirstVisibleIndex;	
			int lastvisibleindex = control.LastVisibleIndex;

#if NET_2_0
			if (control.VirtualMode)
				control.OnCacheVirtualItems (new CacheVirtualItemsEventArgs (first, lastvisibleindex));
#endif

			for (int i = first; i <= lastvisibleindex; i++) {					
				ListViewItem item = control.GetItemAtDisplayIndex (i);
				if (clip.IntersectsWith (item.Bounds)) {
#if NET_2_0
					bool owner_draw = false;
					if (control.OwnerDraw)
						owner_draw = DrawListViewItemOwnerDraw (dc, item, i);
					if (!owner_draw)
#endif
					{
						DrawListViewItem (dc, control, item);
						if (control.View == View.Details)
							DrawListViewSubItems (dc, control, item);
					}
				}
			}	

#if NET_2_0
			if (control.UsingGroups) {
				// Use InternalCount instead of Count to take into account Default Group as needed
				for (int i = 0; i < control.Groups.InternalCount; i++) {
					ListViewGroup group = control.Groups.GetInternalGroup (i);
					if (group.ItemCount > 0 && clip.IntersectsWith (group.HeaderBounds))
						DrawListViewGroupHeader (dc, control, group);
				}
			}

			ListViewInsertionMark insertion_mark = control.InsertionMark;
			int insertion_mark_index = insertion_mark.Index;
			if (Application.VisualStylesEnabled && insertion_mark.Bounds != Rectangle.Empty &&
					(control.View != View.Details && control.View != View.List) &&
					insertion_mark_index > -1 && insertion_mark_index < control.Items.Count) {

				Brush brush = ResPool.GetSolidBrush (insertion_mark.Color);
				dc.FillRectangle (brush, insertion_mark.Line);
				dc.FillPolygon (brush, insertion_mark.TopTriangle);
				dc.FillPolygon (brush, insertion_mark.BottomTriangle);
			}
#endif
			
			// draw the gridlines
#if NET_2_0
			if (details && control.GridLines && !control.UsingGroups) {
#else
			if (details && control.GridLines) {
#endif
				Size control_size = control.ClientSize;
				int top = (control.HeaderStyle == ColumnHeaderStyle.None) ?
					0 : control.header_control.Height;

				// draw vertical gridlines
				foreach (ColumnHeader col in control.Columns) {
					int column_right = col.Rect.Right - control.h_marker;
					dc.DrawLine (SystemPens.Control,
						     column_right, top,
						     column_right, control_size.Height);
				}

				// draw horizontal gridlines
				int item_height = control.ItemSize.Height;
				if (item_height == 0)
					item_height =  control.Font.Height + 2;

				int y = top + item_height - (control.v_marker % item_height); // scroll bar offset
				while (y < control_size.Height) {
					dc.DrawLine (SystemPens.Control, 0, y, control_size.Width, y);
					y += item_height;
				}
			}			
			
			// Draw corner between the two scrollbars
			if (control.h_scroll.Visible == true && control.v_scroll.Visible == true) {
				Rectangle rect = new Rectangle ();
				rect.X = control.h_scroll.Location.X + control.h_scroll.Width;
				rect.Width = control.v_scroll.Width;
				rect.Y = control.v_scroll.Location.Y + control.v_scroll.Height;
				rect.Height = control.h_scroll.Height;
				dc.FillRectangle (SystemBrushes.Control, rect);
			}

			Rectangle box_select_rect = control.item_control.BoxSelectRectangle;
			if (!box_select_rect.Size.IsEmpty)
				dc.DrawRectangle (ResPool.GetDashPen (ColorControlText, DashStyle.Dot), box_select_rect);

		}

		public override void DrawListViewHeader (Graphics dc, Rectangle clip, ListView control)
		{	
			bool details = (control.View == View.Details);
				
			// border is drawn directly in the Paint method
			if (details && control.HeaderStyle != ColumnHeaderStyle.None) {				
				dc.FillRectangle (SystemBrushes.Control,
						  0, 0, control.TotalWidth, control.Font.Height + 5);
				if (control.Columns.Count > 0) {
					foreach (ColumnHeader col in control.Columns) {
						Rectangle rect = col.Rect;
						rect.X -= control.h_marker;

#if NET_2_0
						bool owner_draw = false;
						if (control.OwnerDraw)
							owner_draw = DrawListViewColumnHeaderOwnerDraw (dc, control, col, rect);
						if (owner_draw)
							continue;
#endif

						ListViewDrawColumnHeaderBackground (control, col, dc, rect, clip);
						rect.X += 5;
						rect.Width -= 10;
						if (rect.Width <= 0)
							continue;

#if NET_2_0
						int image_index;
						if (control.SmallImageList == null)
							image_index = -1;
						else 
							image_index = col.ImageKey == String.Empty ? col.ImageIndex : control.SmallImageList.Images.IndexOfKey (col.ImageKey);

						if (image_index > -1 && image_index < control.SmallImageList.Images.Count) {
							int image_width = control.SmallImageList.ImageSize.Width + 5;
							int text_width = (int)dc.MeasureString (col.Text, control.Font).Width;
							int x_origin = rect.X;
							int y_origin = rect.Y + ((rect.Height - control.SmallImageList.ImageSize.Height) / 2);

							switch (col.TextAlign) {
								case HorizontalAlignment.Left:
									break;
								case HorizontalAlignment.Right:
									x_origin = rect.Right - (text_width + image_width);
									break;
								case HorizontalAlignment.Center:
									x_origin = (rect.Width - (text_width + image_width)) / 2 + rect.X;
									break;
							}

							if (x_origin < rect.X)
								x_origin = rect.X;

							control.SmallImageList.Draw (dc, new Point (x_origin, y_origin), image_index);
							rect.X += image_width;
							rect.Width -= image_width;
						}
#endif

						dc.DrawString (col.Text, control.Font, SystemBrushes.ControlText, rect, col.Format);
					}
					int right = control.GetReorderedColumn (control.Columns.Count - 1).Rect.Right - control.h_marker;
					if (right < control.Right) {
						Rectangle rect = control.Columns [0].Rect;
						rect.X = right;
						rect.Width = control.Right - right;
						ListViewDrawUnusedHeaderBackground (control, dc, rect, clip);
					}
				}
			}
		}

		protected virtual void ListViewDrawColumnHeaderBackground (ListView listView, ColumnHeader columnHeader, Graphics g, Rectangle area, Rectangle clippingArea)
		{
			ButtonState state;
			if (listView.HeaderStyle == ColumnHeaderStyle.Clickable)
				state = columnHeader.Pressed ? ButtonState.Pushed : ButtonState.Normal;
			else
				state = ButtonState.Flat;
			CPDrawButton (g, area, state);
		}
		
		protected virtual void ListViewDrawUnusedHeaderBackground (ListView listView, Graphics g, Rectangle area, Rectangle clippingArea)
		{
			ButtonState state;
			if (listView.HeaderStyle == ColumnHeaderStyle.Clickable)
				state = ButtonState.Normal;
			else
				state = ButtonState.Flat;
			CPDrawButton (g, area, state);
		}

		public override void DrawListViewHeaderDragDetails (Graphics dc, ListView view, ColumnHeader col, int target_x)
		{
			Rectangle rect = col.Rect;
			rect.X -= view.h_marker;
			Color color = Color.FromArgb (0x7f, ColorControlDark.R, ColorControlDark.G, ColorControlDark.B);
			dc.FillRectangle (ResPool.GetSolidBrush (color), rect);
			rect.X += 3;
			rect.Width -= 8;
			if (rect.Width <= 0)
				return;
			color = Color.FromArgb (0x7f, ColorControlText.R, ColorControlText.G, ColorControlText.B);
			dc.DrawString (col.Text, view.Font, ResPool.GetSolidBrush (color), rect, col.Format);
			dc.DrawLine (ResPool.GetSizedPen (ColorHighlight, 2), target_x, 0, target_x, col.Rect.Height);
		}

#if NET_2_0
		protected virtual bool DrawListViewColumnHeaderOwnerDraw (Graphics dc, ListView control, ColumnHeader column, Rectangle bounds)
		{
			ListViewItemStates state = ListViewItemStates.ShowKeyboardCues;
			if (column.Pressed)
				state |= ListViewItemStates.Selected;

			DrawListViewColumnHeaderEventArgs args = new DrawListViewColumnHeaderEventArgs (dc,
					bounds, column.Index, column, state, SystemColors.ControlText, ThemeEngine.Current.ColorControl, DefaultFont);
			control.OnDrawColumnHeader (args);

			return !args.DrawDefault;
		}

		protected virtual bool DrawListViewItemOwnerDraw (Graphics dc, ListViewItem item, int index)
		{
			ListViewItemStates item_state = ListViewItemStates.ShowKeyboardCues;
			if (item.Selected)
				item_state |= ListViewItemStates.Selected;
			if (item.Focused)
				item_state |= ListViewItemStates.Focused;
						
			DrawListViewItemEventArgs args = new DrawListViewItemEventArgs (dc,
					item, item.Bounds, index, item_state);
			item.ListView.OnDrawItem (args);

			if (args.DrawDefault)
				return false;

			if (item.ListView.View == View.Details) {
				int count = Math.Min (item.ListView.Columns.Count, item.SubItems.Count);
				
				// Do system drawing for subitems if no owner draw is done
				for (int j = 0; j < count; j++) {
					if (!DrawListViewSubItemOwnerDraw (dc, item, item_state, j)) {
						if (j == 0) // The first sub item contains the main item semantics
							DrawListViewItem (dc, item.ListView, item);
						else
							DrawListViewSubItem (dc, item.ListView, item, j);
					}
				}
			}
			
			return true;
		}
#endif

		protected virtual void DrawListViewItem (Graphics dc, ListView control, ListViewItem item)
		{				
			Rectangle rect_checkrect = item.CheckRectReal;
			Rectangle icon_rect = item.GetBounds (ItemBoundsPortion.Icon);
			Rectangle full_rect = item.GetBounds (ItemBoundsPortion.Entire);
			Rectangle text_rect = item.GetBounds (ItemBoundsPortion.Label);			

#if NET_2_0
			// Tile view doesn't support CheckBoxes
			if (control.CheckBoxes && control.View != View.Tile) {
#else
			if (control.CheckBoxes) {
#endif
				if (control.StateImageList == null) {
					// Make sure we've got at least a line width of 1
					int check_wd = Math.Max (3, rect_checkrect.Width / 6);
					int scale = Math.Max (1, rect_checkrect.Width / 12);

					// set the checkbox background
					dc.FillRectangle (SystemBrushes.Window,
							  rect_checkrect);
					// define a rectangle inside the border area
					Rectangle rect = new Rectangle (rect_checkrect.X + 2,
									rect_checkrect.Y + 2,
									rect_checkrect.Width - 4,
									rect_checkrect.Height - 4);
					Pen pen = ResPool.GetSizedPen (this.ColorWindowText, 2);
					dc.DrawRectangle (pen, rect);

					// Need to draw a check-mark
					if (item.Checked) {
						Pen check_pen = ResPool.GetSizedPen (this.ColorWindowText, 1);
						// adjustments to get the check-mark at the right place
						rect.X ++; rect.Y ++;
						// following logic is taken from DrawFrameControl method
						int x_offset = rect.Width / 5;
						int y_offset = rect.Height / 3;
						for (int i = 0; i < check_wd; i++) {
							dc.DrawLine (check_pen, rect.Left + x_offset,
								     rect.Top + y_offset + i,
								     rect.Left + x_offset + 2 * scale,
								     rect.Top + y_offset + 2 * scale + i);
							dc.DrawLine (check_pen,
								     rect.Left + x_offset + 2 * scale,
								     rect.Top + y_offset + 2 * scale + i,
								     rect.Left + x_offset + 6 * scale,
								     rect.Top + y_offset - 2 * scale + i);
						}
					}
				}
				else {
					int simage_idx;
					if (item.Checked)
#if NET_2_0
						simage_idx = control.StateImageList.Images.Count > 1 ? 1 : -1;
#else
						simage_idx = control.StateImageList.Images.Count > 1 ? 1 : 0;
#endif
					else
						simage_idx = control.StateImageList.Images.Count > 0 ? 0 : -1;

					if (simage_idx > -1)
						control.StateImageList.Draw (dc, rect_checkrect.Location, simage_idx);
				}
			}

			ImageList image_list = control.View == View.LargeIcon 
#if NET_2_0
				|| control.View == View.Tile
#endif
				? control.LargeImageList : control.SmallImageList;
			if (image_list != null) {
				int idx;

#if NET_2_0
				if (item.ImageKey != String.Empty)
					idx = image_list.Images.IndexOfKey (item.ImageKey);
				else
#endif
					idx = item.ImageIndex;

				if (idx > -1 && idx < image_list.Images.Count)
					image_list.Draw (dc, icon_rect.Location, idx);
			}

			// draw the item text			
			// format for the item text
			StringFormat format = new StringFormat ();
			if (control.View == View.SmallIcon || control.View == View.LargeIcon)
				format.LineAlignment = StringAlignment.Near;
			else
				format.LineAlignment = StringAlignment.Center;
			if (control.View == View.LargeIcon)
				format.Alignment = StringAlignment.Center;
			else
				format.Alignment = StringAlignment.Near;
			
#if NET_2_0
			if (control.LabelWrap && control.View != View.Details && control.View != View.Tile)
#else
			if (control.LabelWrap && control.View != View.Details)
#endif
				format.FormatFlags = StringFormatFlags.LineLimit;
			else
				format.FormatFlags = StringFormatFlags.NoWrap;

			if ((control.View == View.LargeIcon && !item.Focused)
					|| control.View == View.Details 
#if NET_2_0
					|| control.View == View.Tile
#endif
			   )
				format.Trimming = StringTrimming.EllipsisCharacter;

			Rectangle highlight_rect = text_rect;
			if (control.View == View.Details) { // Adjustments for Details view
				Size text_size = Size.Ceiling (dc.MeasureString (item.Text, item.Font));

				if (!control.FullRowSelect) // Selection shouldn't be outside the item bounds
					highlight_rect.Width = Math.Min (text_size.Width + 4, text_rect.Width);
			}

			if (item.Selected && control.Focused)
				dc.FillRectangle (SystemBrushes.Highlight, highlight_rect);
			else if (item.Selected && !control.HideSelection)
				dc.FillRectangle (SystemBrushes.Control, highlight_rect);
			else
				dc.FillRectangle (ResPool.GetSolidBrush (item.BackColor), text_rect);
			
			Brush textBrush =
				!control.Enabled ? SystemBrushes.ControlLight :
				(item.Selected && control.Focused) ? SystemBrushes.HighlightText :
				this.ResPool.GetSolidBrush (item.ForeColor);

#if NET_2_0
			// Tile view renders its Text in a different fashion
			if (control.View == View.Tile && Application.VisualStylesEnabled) {
				// Item.Text is drawn using its first subitem's bounds
				dc.DrawString (item.Text, item.Font, textBrush, item.SubItems [0].Bounds, format);

				int count = Math.Min (control.Columns.Count, item.SubItems.Count);
				for (int i = 1; i < count; i++) {
					ListViewItem.ListViewSubItem sub_item = item.SubItems [i];
					if (sub_item.Text == null || sub_item.Text.Length == 0)
						continue;

					Brush itemBrush = item.Selected && control.Focused ? 
						SystemBrushes.HighlightText : GetControlForeBrush (sub_item.ForeColor);
					dc.DrawString (sub_item.Text, sub_item.Font, itemBrush, sub_item.Bounds, format);
				}
			} else
#endif
			
			if (item.Text != null && item.Text.Length > 0) {
				Font font = item.Font;
#if NET_2_0
				if (control.HotTracking && item.Hot)
					font = item.HotFont;
#endif

				if (item.Selected && control.Focused)
					dc.DrawString (item.Text, font, textBrush, highlight_rect, format);
				else
					dc.DrawString (item.Text, font, textBrush, text_rect, format);
			}

			if (item.Focused && control.Focused) {				
				Rectangle focus_rect = highlight_rect;
				if (control.FullRowSelect && control.View == View.Details) {
					int width = 0;
					foreach (ColumnHeader col in control.Columns)
						width += col.Width;
					focus_rect = new Rectangle (0, full_rect.Y, width, full_rect.Height);
				}
				if (control.ShowFocusCues) {
					if (item.Selected)
						CPDrawFocusRectangle (dc, focus_rect, ColorHighlightText, ColorHighlight);
					else
						CPDrawFocusRectangle (dc, focus_rect, control.ForeColor, control.BackColor);
				}
			}

			format.Dispose ();
		}

		protected virtual void DrawListViewSubItems (Graphics dc, ListView control, ListViewItem item)
		{
			int columns_count = control.Columns.Count;
			int count = Math.Min (item.SubItems.Count, columns_count);
			// 0th item already done (in this case)
			for (int i = 1; i < count; i++)
				DrawListViewSubItem (dc, control, item, i);

			// Fill in selection for remaining columns if Column.Count > SubItems.Count
			Rectangle sub_item_rect = item.GetBounds (ItemBoundsPortion.Label);
			if (item.Selected && (control.Focused || !control.HideSelection) && control.FullRowSelect) {
				for (int index = count; index < columns_count; index++) {
					ColumnHeader col = control.Columns [index];
					sub_item_rect.X = col.Rect.X - control.h_marker;
					sub_item_rect.Width = col.Wd;
					dc.FillRectangle (control.Focused ? SystemBrushes.Highlight : SystemBrushes.Control, 
							sub_item_rect);
				}
			}
		}

		protected virtual void DrawListViewSubItem (Graphics dc, ListView control, ListViewItem item, int index)
		{
			ListViewItem.ListViewSubItem subItem = item.SubItems [index];
			ColumnHeader col = control.Columns [index];
			StringFormat format = new StringFormat ();
			format.Alignment = col.Format.Alignment;
			format.LineAlignment = StringAlignment.Center;
			format.FormatFlags = StringFormatFlags.NoWrap;
			format.Trimming = StringTrimming.EllipsisCharacter;

			Rectangle sub_item_rect = subItem.Bounds;
			Rectangle sub_item_text_rect = sub_item_rect;
			sub_item_text_rect.X += 3;
			sub_item_text_rect.Width -= ListViewItemPaddingWidth;
						
			SolidBrush sub_item_back_br = null;
			SolidBrush sub_item_fore_br = null;
			Font sub_item_font = null;
						
			if (item.UseItemStyleForSubItems) {
				sub_item_back_br = ResPool.GetSolidBrush (item.BackColor);
				sub_item_fore_br = ResPool.GetSolidBrush (item.ForeColor);
#if NET_2_0
				// Hot tracking for subitems only applies when UseStyle is true
				if (control.HotTracking && item.Hot)
					sub_item_font = item.HotFont;
				else
#endif
					sub_item_font = item.Font;
			} else {
				sub_item_back_br = ResPool.GetSolidBrush (subItem.BackColor);
				sub_item_fore_br = ResPool.GetSolidBrush (subItem.ForeColor);
				sub_item_font = subItem.Font;
			}
						
			if (item.Selected && (control.Focused || !control.HideSelection) && control.FullRowSelect) {
				Brush bg, text;
				if (control.Focused) {
					bg = SystemBrushes.Highlight;
					text = SystemBrushes.HighlightText;
				} else {
					bg = SystemBrushes.Control;
					text = sub_item_fore_br;
							
				}
							
				dc.FillRectangle (bg, sub_item_rect);
				if (subItem.Text != null && subItem.Text.Length > 0)
					dc.DrawString (subItem.Text, sub_item_font,
							text, sub_item_text_rect, format);
			} else {
				dc.FillRectangle (sub_item_back_br, sub_item_rect);
				if (subItem.Text != null && subItem.Text.Length > 0)
					dc.DrawString (subItem.Text, sub_item_font,
							sub_item_fore_br,
							sub_item_text_rect, format);
			}

			format.Dispose ();
		}

#if NET_2_0
		protected virtual bool DrawListViewSubItemOwnerDraw (Graphics dc, ListViewItem item, ListViewItemStates state, int index)
		{
			ListView control = item.ListView;
			ListViewItem.ListViewSubItem subitem = item.SubItems [index];

			DrawListViewSubItemEventArgs args = new DrawListViewSubItemEventArgs (dc, subitem.Bounds, item, 
					subitem, item.Index, index, control.Columns [index], state);
			control.OnDrawSubItem (args);
			
			return !args.DrawDefault;
		}

		protected virtual void DrawListViewGroupHeader (Graphics dc, ListView control, ListViewGroup group)
		{
			Rectangle text_bounds = group.HeaderBounds;
			Rectangle header_bounds = group.HeaderBounds;
			text_bounds.Offset (8, 0);
			text_bounds.Inflate (-8, 0);
			int text_height = control.Font.Height + 2; // add a tiny padding between the text and the group line

			Font font = new Font (control.Font, control.Font.Style | FontStyle.Bold);
			Brush brush = new LinearGradientBrush (new Point (header_bounds.Left, 0), new Point (header_bounds.Left + ListViewGroupLineWidth, 0), 
					SystemColors.Desktop, Color.White);
			Pen pen = new Pen (brush);

			StringFormat sformat = new StringFormat ();
			switch (group.HeaderAlignment) {
				case HorizontalAlignment.Left:
					sformat.Alignment = StringAlignment.Near;
					break;
				case HorizontalAlignment.Center:
					sformat.Alignment = StringAlignment.Center;
					break;
				case HorizontalAlignment.Right:
					sformat.Alignment = StringAlignment.Far;
					break;
			}

			sformat.LineAlignment = StringAlignment.Near;
			dc.DrawString (group.Header, font, SystemBrushes.ControlText, text_bounds, sformat);
			dc.DrawLine (pen, header_bounds.Left, header_bounds.Top + text_height, header_bounds.Left + ListViewGroupLineWidth, 
					header_bounds.Top + text_height);

			sformat.Dispose ();
			font.Dispose ();
			pen.Dispose ();
			brush.Dispose ();
		}
#endif

		public override bool ListViewHasHotHeaderStyle {
			get {
				return false;
			}
		}

		// Sizing
		public override int ListViewGetHeaderHeight (ListView listView, Font font)
		{
			return ListViewGetHeaderHeight (font);
		}

		static int ListViewGetHeaderHeight (Font font)
		{
			return font.Height + 5;
		}

		public static int ListViewGetHeaderHeight ()
		{
			return ListViewGetHeaderHeight (ThemeEngine.Current.DefaultFont);
		}

		public override Size ListViewCheckBoxSize {
			get { return new Size (16, 16); }
		}

		public override int ListViewColumnHeaderHeight {
			get { return 16; }
		}

		public override int ListViewDefaultColumnWidth {
			get { return 60; }
		}

		public override int ListViewVerticalSpacing {
			get { return 22; }
		}

		public override int ListViewEmptyColumnWidth {
			get { return 10; }
		}

		public override int ListViewHorizontalSpacing {
			get { return 4; }
		}

		public override int ListViewItemPaddingWidth {
			get { return 6; }
		}

		public override Size ListViewDefaultSize {
			get { return new Size (121, 97); }
		}

		public override int ListViewGroupHeight { 
			get { return 20; }
		}

		public int ListViewGroupLineWidth {
			get { return 200; }
		}

		public override int ListViewTileWidthFactor {
			get { return 22; }
		}

		public override int ListViewTileHeightFactor {
			get { return 3; }
		}
		#endregion	// ListView
		
		#region Menus
		
		public override void CalcItemSize (Graphics dc, MenuItem item, int y, int x, bool menuBar)
		{
			item.X = x;
			item.Y = y;

			if (item.Visible == false) {
				item.Width = 0;
				item.Height = 0;
				return;
			}

			if (item.Separator == true) {
				item.Height = SEPARATOR_HEIGHT;
				item.Width = SEPARATOR_MIN_WIDTH;
				return;
			}
			
			if (item.MeasureEventDefined) {
				MeasureItemEventArgs mi = new MeasureItemEventArgs (dc, item.Index);
				item.PerformMeasureItem (mi);
				item.Height = mi.ItemHeight;
				item.Width = mi.ItemWidth;
				return;
			} else {		
				SizeF size;
				size =  dc.MeasureString (item.Text, MenuFont, int.MaxValue, string_format_menu_text);
				item.Width = (int) size.Width;
				item.Height = (int) size.Height;
	
				if (!menuBar) {
					if (item.Shortcut != Shortcut.None && item.ShowShortcut) {
						item.XTab = MenuCheckSize.Width + MENU_TAB_SPACE + (int) size.Width;
						size =  dc.MeasureString (" " + item.GetShortCutText (), MenuFont);
						item.Width += MENU_TAB_SPACE + (int) size.Width;
					}
	
					item.Width += 4 + (MenuCheckSize.Width * 2);
				} else {
					item.Width += MENU_BAR_ITEMS_SPACE;
					x += item.Width;
				}
	
				if (item.Height < MenuHeight)
					item.Height = MenuHeight;
			}
		}
		
		// Updates the menu rect and returns the height
		public override int CalcMenuBarSize (Graphics dc, Menu menu, int width)
		{
			int x = 0;
			int y = 0;
			menu.Height = 0;

			foreach (MenuItem item in menu.MenuItems) {

				CalcItemSize (dc, item, y, x, true);

				if (x + item.Width > width) {
					item.X = 0;
					y += item.Height;
					item.Y = y;
					x = 0;
				}

				x += item.Width;
				item.MenuBar = true;				

				if (y + item.Height > menu.Height)
					menu.Height = item.Height + y;
			}

			menu.Width = width;						
			return menu.Height;
		}

		public override void CalcPopupMenuSize (Graphics dc, Menu menu)
		{
			int x = 3;
			int start = 0;
			int i, n, y, max;

			menu.Height = 0;

			while (start < menu.MenuItems.Count) {
				y = 3;
				max = 0;
				for (i = start; i < menu.MenuItems.Count; i++) {
					MenuItem item = menu.MenuItems [i];

					if ((i != start) && (item.Break || item.BarBreak))
						break;

					CalcItemSize (dc, item, y, x, false);
					y += item.Height;

					if (item.Width > max)
						max = item.Width;
				}

				// Replace the -1 by the menu width (separators)
				for (n = start; n < i; n++, start++)
					menu.MenuItems [n].Width = max;

				if (y > menu.Height)
					menu.Height = y;

				x+= max;
			}

			menu.Width = x;
			
			//space for border
			menu.Width += 2;
			menu.Height += 2;

			menu.Width += SM_CXBORDER;
    		menu.Height += SM_CYBORDER;
		}
		
		// Draws a menu bar in a window
		public override void DrawMenuBar (Graphics dc, Menu menu, Rectangle rect)
		{
			if (menu.Height == 0)
				CalcMenuBarSize (dc, menu, rect.Width);

			bool keynav = (menu as MainMenu).tracker.hotkey_active;
			HotkeyPrefix hp = MenuAccessKeysUnderlined || keynav ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
			string_format_menu_menubar_text.HotkeyPrefix = hp;
			string_format_menu_text.HotkeyPrefix = hp;

			rect.Height = menu.Height;
			dc.FillRectangle (SystemBrushes.Menu, rect);
			
			for (int i = 0; i < menu.MenuItems.Count; i++) {
				MenuItem item = menu.MenuItems [i];
				Rectangle item_rect = item.bounds;
				item_rect.X += rect.X;
				item_rect.Y += rect.Y;
				item.MenuHeight = menu.Height;
				item.PerformDrawItem (new DrawItemEventArgs (dc, MenuFont, item_rect, i, item.Status));	
			}	
		}		
		
		protected Bitmap CreateGlyphBitmap (Size size, MenuGlyph glyph, Color color)
		{
			Color bg_color;
			if (color.R == 0 && color.G == 0 && color.B == 0)
				bg_color = Color.White;
			else
				bg_color = Color.Black;
			
			Bitmap	bmp = new Bitmap (size.Width, size.Height);
			Graphics gr = Graphics.FromImage (bmp);
			Rectangle rect = new Rectangle (Point.Empty, size);
			gr.FillRectangle (ResPool.GetSolidBrush (bg_color), rect);
			CPDrawMenuGlyph (gr, rect, glyph, color, Color.Empty);
			bmp.MakeTransparent (bg_color);
			gr.Dispose ();
			
			return bmp;
		}

		public override void DrawMenuItem (MenuItem item, DrawItemEventArgs e)
		{
			StringFormat string_format;
			Rectangle rect_text = e.Bounds;
			
			if (item.Visible == false)
				return;

			if (item.MenuBar)
				string_format = string_format_menu_menubar_text;
			else
				string_format = string_format_menu_text;

			if (item.Separator == true) {
				int liney = e.Bounds.Y + (e.Bounds.Height / 2);
				
				e.Graphics.DrawLine (SystemPens.ControlDark,
					e.Bounds.X, liney, e.Bounds.X + e.Bounds.Width, liney);

				e.Graphics.DrawLine (SystemPens.ControlLight,
					e.Bounds.X, liney + 1, e.Bounds.X + e.Bounds.Width, liney + 1);

				return;
			}

			if (!item.MenuBar)
				rect_text.X += MenuCheckSize.Width;

			if (item.BarBreak) { /* Draw vertical break bar*/
				Rectangle rect = e.Bounds;
				rect.Y++;
				rect.Width = 3;
				rect.Height = item.MenuHeight - 6;

				e.Graphics.DrawLine (SystemPens.ControlDark,
					rect.X, rect.Y , rect.X, rect.Y + rect.Height);

				e.Graphics.DrawLine (SystemPens.ControlLight,
					rect.X + 1, rect.Y , rect.X +1, rect.Y + rect.Height);
			}			
			
			Color color_text;
			Color color_back;
			Brush brush_text = null;
			Brush brush_back = null;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && !item.MenuBar) {
				color_text = ColorHighlightText;
				color_back = ColorHighlight;
				brush_text = SystemBrushes.HighlightText;
				brush_back = SystemBrushes.Highlight;
			} else {
				color_text = ColorMenuText;
				color_back = ColorMenu;
				brush_text = ResPool.GetSolidBrush (ColorMenuText);
				brush_back = SystemBrushes.Menu;
			}

			/* Draw background */
			if (!item.MenuBar)
				e.Graphics.FillRectangle (brush_back, e.Bounds);
			
			if (item.Enabled) {
				e.Graphics.DrawString (item.Text, e.Font,
					brush_text,
					rect_text, string_format);
				
				if (item.MenuBar) {
					Border3DStyle border_style = Border3DStyle.Adjust;
					if ((item.Status & DrawItemState.HotLight) != 0)
						border_style = Border3DStyle.RaisedInner;
					else if ((item.Status & DrawItemState.Selected) != 0)
						border_style = Border3DStyle.SunkenOuter;
					
					if (border_style != Border3DStyle.Adjust)
						CPDrawBorder3D(e.Graphics, e.Bounds, border_style,  Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, ColorMenu);
				}
			} else {
				if ((item.Status & DrawItemState.Selected) != DrawItemState.Selected) {
					e.Graphics.DrawString (item.Text, e.Font, Brushes.White, 
							       new RectangleF(rect_text.X + 1, rect_text.Y + 1, rect_text.Width, rect_text.Height),
							       string_format);

				}
				
				e.Graphics.DrawString (item.Text, e.Font, ResPool.GetSolidBrush(ColorGrayText), rect_text, string_format);
			}

			if (!item.MenuBar && item.Shortcut != Shortcut.None && item.ShowShortcut) {
				string str = item.GetShortCutText ();
				Rectangle rect = rect_text;
				rect.X = item.XTab;
				rect.Width -= item.XTab;

				if (item.Enabled) {
					e.Graphics.DrawString (str, e.Font, brush_text, rect, string_format_menu_shortcut);
				} else {
					if ((item.Status & DrawItemState.Selected) != DrawItemState.Selected) {
						e.Graphics.DrawString (str, e.Font, Brushes.White, 
								       new RectangleF(rect.X + 1, rect.Y + 1, rect.Width, rect_text.Height),
								       string_format_menu_shortcut);

					}
					e.Graphics.DrawString (str, e.Font, ResPool.GetSolidBrush(ColorGrayText), rect, string_format_menu_shortcut);
				}
			}

			/* Draw arrow */
			if (item.MenuBar == false && (item.IsPopup || item.MdiList)) {

				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				Bitmap	bmp = CreateGlyphBitmap (new Size (cx, cy), MenuGlyph.Arrow, color_text);
				
				if (item.Enabled) {
					e.Graphics.DrawImage (bmp, e.Bounds.X + e.Bounds.Width - cx,
						e.Bounds.Y + ((e.Bounds.Height - cy) /2));
				} else {
					ControlPaint.DrawImageDisabled (e.Graphics, bmp, e.Bounds.X + e.Bounds.Width - cx,
						e.Bounds.Y + ((e.Bounds.Height - cy) /2),  color_back);
				}
 
				bmp.Dispose ();
			}

			/* Draw checked or radio */
			if (item.MenuBar == false && item.Checked) {

				Rectangle area = e.Bounds;
				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				Bitmap	bmp = CreateGlyphBitmap (new Size (cx, cy), item.RadioCheck ? MenuGlyph.Bullet : MenuGlyph.Checkmark, color_text);

				e.Graphics.DrawImage (bmp, area.X, e.Bounds.Y + ((e.Bounds.Height - cy) / 2));

				bmp.Dispose ();
			}			
		}		
			
		public override void DrawPopupMenu (Graphics dc, Menu menu, Rectangle cliparea, Rectangle rect)
		{
			// Fill rectangle area
			dc.FillRectangle (SystemBrushes.Menu, cliparea);
			
			// Draw menu borders
			CPDrawBorder3D (dc, rect, Border3DStyle.Raised, all_sides);
			
			// Draw menu items
			for (int i = 0; i < menu.MenuItems.Count; i++) {
				if (cliparea.IntersectsWith (menu.MenuItems [i].bounds)) {
					MenuItem item = menu.MenuItems [i];
					item.MenuHeight = menu.Height;
					item.PerformDrawItem (new DrawItemEventArgs (dc, MenuFont, item.bounds, i, item.Status));
				}
			}
		}
		
		#endregion // Menus

		#region MonthCalendar

		// draw the month calendar
		public override void DrawMonthCalendar(Graphics dc, Rectangle clip_rectangle, MonthCalendar mc) 
		{
			Rectangle client_rectangle = mc.ClientRectangle;
			Size month_size = mc.SingleMonthSize;
			// cache local copies of Marshal-by-ref internal members (gets around error CS0197)
			Size calendar_spacing = (Size)((object)mc.calendar_spacing);
			Size date_cell_size = (Size)((object)mc.date_cell_size);
			
			// draw the singlecalendars
			int x_offset = 1;
			int y_offset = 1;
			// adjust for the position of the specific month
			for (int i=0; i < mc.CalendarDimensions.Height; i++) 
			{
				if (i > 0) 
				{
					y_offset += month_size.Height + calendar_spacing.Height;
				}
				// now adjust for x position	
				for (int j=0; j < mc.CalendarDimensions.Width; j++) 
				{
					if (j > 0) 
					{
						x_offset += month_size.Width + calendar_spacing.Width;
					} 
					else 
					{
						x_offset = 1;
					}

					Rectangle month_rect = new Rectangle (x_offset, y_offset, month_size.Width, month_size.Height);
					if (month_rect.IntersectsWith (clip_rectangle)) {
						DrawSingleMonth (
							dc,
							clip_rectangle,
							month_rect,
							mc,
							i,
							j);
					}
				}
			}
			
			Rectangle bottom_rect = new Rectangle (
						client_rectangle.X,
						Math.Max(client_rectangle.Bottom - date_cell_size.Height - 3, 0),
						client_rectangle.Width,
						date_cell_size.Height + 2);
			// draw the today date if it's set
			if (mc.ShowToday && bottom_rect.IntersectsWith (clip_rectangle)) 
			{
				dc.FillRectangle (GetControlBackBrush (mc.BackColor), bottom_rect);
				if (mc.ShowToday) {
					int today_offset = 5;
					if (mc.ShowTodayCircle) 
					{
						Rectangle today_circle_rect = new Rectangle (
							client_rectangle.X + 5,
							Math.Max(client_rectangle.Bottom - date_cell_size.Height - 2, 0),
							date_cell_size.Width,
							date_cell_size.Height);
							DrawTodayCircle (dc, today_circle_rect);
						today_offset += date_cell_size.Width + 5;
					}
					// draw today's date
					StringFormat text_format = new StringFormat();
					text_format.LineAlignment = StringAlignment.Center;
					text_format.Alignment = StringAlignment.Near;
					Rectangle today_rect = new Rectangle (
							today_offset + client_rectangle.X,
							Math.Max(client_rectangle.Bottom - date_cell_size.Height, 0),
							Math.Max(client_rectangle.Width - today_offset, 0),
							date_cell_size.Height);
					dc.DrawString ("Today: " + DateTime.Now.ToShortDateString(), mc.bold_font, GetControlForeBrush (mc.ForeColor), today_rect, text_format);
					text_format.Dispose ();
				}				
			}
			
			Brush border_brush;
			
			if (mc.owner == null)
				border_brush = GetControlBackBrush (mc.BackColor);
			else
				border_brush = SystemBrushes.ControlDarkDark;
				
			// finally paint the borders of the calendars as required
			for (int i = 0; i <= mc.CalendarDimensions.Width; i++) {
				if (i == 0 && clip_rectangle.X == client_rectangle.X) {
					dc.FillRectangle (border_brush, client_rectangle.X, client_rectangle.Y, 1, client_rectangle.Height);
				} else if (i == mc.CalendarDimensions.Width && clip_rectangle.Right == client_rectangle.Right) {
					dc.FillRectangle (border_brush, client_rectangle.Right - 1, client_rectangle.Y, 1, client_rectangle.Height);
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X + (month_size.Width*i) + (calendar_spacing.Width * (i-1)) + 1,
						client_rectangle.Y,
						calendar_spacing.Width,
						client_rectangle.Height);
					if (i < mc.CalendarDimensions.Width && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (border_brush, rect);
					}
				}
			}
			for (int i = 0; i <= mc.CalendarDimensions.Height; i++) {
				if (i == 0 && clip_rectangle.Y == client_rectangle.Y) {
					dc.FillRectangle (border_brush, client_rectangle.X, client_rectangle.Y, client_rectangle.Width, 1);
				} else if (i == mc.CalendarDimensions.Height && clip_rectangle.Bottom == client_rectangle.Bottom) {
					dc.FillRectangle (border_brush, client_rectangle.X, client_rectangle.Bottom - 1, client_rectangle.Width, 1);
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X,
						client_rectangle.Y + (month_size.Height*i) + (calendar_spacing.Height*(i-1)) + 1,
						client_rectangle.Width,
						calendar_spacing.Height);
					if (i < mc.CalendarDimensions.Height && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (border_brush, rect);
					}
				}
			}
			
			// draw the drop down border if need
			if (mc.owner != null) {
				Rectangle bounds = mc.ClientRectangle;
				if (clip_rectangle.Contains (mc.Location)) {
					// find out if top or left line to draw
					if(clip_rectangle.Contains (new Point (bounds.Left, bounds.Bottom))) {
					
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Y, bounds.X, bounds.Bottom-1);
					}
					if(clip_rectangle.Contains (new Point (bounds.Right, bounds.Y))) {
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Y, bounds.Right-1, bounds.Y);
					}
				}
				if (clip_rectangle.Contains (new Point(bounds.Right, bounds.Bottom))) {
					// find out if bottom or right line to draw
					if(clip_rectangle.Contains (new Point (bounds.Left, bounds.Bottom))) {
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1);
					}
					if(clip_rectangle.Contains (new Point (bounds.Right, bounds.Y))) {
						dc.DrawLine (SystemPens.ControlText, bounds.Right-1, bounds.Y, bounds.Right-1, bounds.Bottom-1);
					}
				}
			}
		}

		// darws a single part of the month calendar (with one month)
		private void DrawSingleMonth(Graphics dc, Rectangle clip_rectangle, Rectangle rectangle, MonthCalendar mc, int row, int col) 
		{
			// cache local copies of Marshal-by-ref internal members (gets around error CS0197)
			Size title_size = (Size)((object)mc.title_size);
			Size date_cell_size = (Size)((object)mc.date_cell_size);
			DateTime current_month = (DateTime)((object)mc.current_month);
			DateTime sunday = new DateTime(2006, 10, 1);
			
			// draw the title back ground
			DateTime this_month = current_month.AddMonths (row*mc.CalendarDimensions.Width+col);
			Rectangle title_rect = new Rectangle(rectangle.X, rectangle.Y, title_size.Width, title_size.Height);
			if (title_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), title_rect);
				// draw the title				
				string title_text = this_month.ToString ("MMMM yyyy");
				dc.DrawString (title_text, mc.bold_font, ResPool.GetSolidBrush (mc.TitleForeColor), title_rect, mc.centered_format);

				if (mc.ShowYearUpDown) {
					Rectangle year_rect;
					Rectangle upRect, downRect;
					ButtonState upState, downState;
					
					mc.GetYearNameRectangles (title_rect, row * mc.CalendarDimensions.Width + col, out year_rect, out upRect, out downRect);
					dc.FillRectangle (ResPool.GetSolidBrush (SystemColors.Control), year_rect);
					dc.DrawString (this_month.ToString ("yyyy"), mc.bold_font, ResPool.GetSolidBrush (Color.Black), year_rect, mc.centered_format);
					
					upState = mc.IsYearGoingUp ? ButtonState.Pushed : ButtonState.Normal;
					downState = mc.IsYearGoingDown ? ButtonState.Pushed : ButtonState.Normal;

					ControlPaint.DrawScrollButton (dc, upRect, ScrollButton.Up, upState);
					ControlPaint.DrawScrollButton (dc, downRect, ScrollButton.Down, downState);
				}

				// draw previous and next buttons if it's time
				if (row == 0 && col == 0) 
				{
					// draw previous button
					DrawMonthCalendarButton (
						dc,
						rectangle,
						mc,
						title_size,
						mc.button_x_offset,
						(System.Drawing.Size)((object)mc.button_size),
						true);
				}
				if (row == 0 && col == mc.CalendarDimensions.Width-1) 
				{
					// draw next button
					DrawMonthCalendarButton (
						dc,
						rectangle,
						mc,
						title_size,
						mc.button_x_offset,
						(System.Drawing.Size)((object)mc.button_size),
						false);
				}
			}
			
			// set the week offset and draw week nums if needed
			int col_offset = (mc.ShowWeekNumbers) ? 1 : 0;
			Rectangle day_name_rect = new Rectangle(
				rectangle.X,
				rectangle.Y + title_size.Height,
				(7 + col_offset) * date_cell_size.Width,
				date_cell_size.Height);
			if (day_name_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (GetControlBackBrush (mc.BackColor), day_name_rect);
				// draw the day names 
				DayOfWeek first_day_of_week = mc.GetDayOfWeek(mc.FirstDayOfWeek);
				for (int i=0; i < 7; i++) 
				{
					int position = i - (int) first_day_of_week;
					if (position < 0) 
					{
						position = 7 + position;
					}
					// draw it
					Rectangle day_rect = new Rectangle(
						day_name_rect.X + ((i + col_offset)* date_cell_size.Width),
						day_name_rect.Y,
						date_cell_size.Width,
						date_cell_size.Height);
					dc.DrawString (sunday.AddDays (i + (int) first_day_of_week).ToString ("ddd"), mc.Font, ResPool.GetSolidBrush (mc.TitleBackColor), day_rect, mc.centered_format);
				}
				
				// draw the vertical divider
				int vert_divider_y = Math.Max(title_size.Height+ date_cell_size.Height-1, 0);
				dc.DrawLine (
					ResPool.GetPen (mc.ForeColor),
					rectangle.X + (col_offset * date_cell_size.Width) + mc.divider_line_offset,
					rectangle.Y + vert_divider_y,
					rectangle.Right - mc.divider_line_offset,
					rectangle.Y + vert_divider_y);
			}


			// draw the actual date items in the grid (including the week numbers)
			Rectangle date_rect = new Rectangle (
				rectangle.X,
				rectangle.Y + title_size.Height + date_cell_size.Height,
				date_cell_size.Width,
				date_cell_size.Height);
			int month_row_count = 0;
			bool draw_week_num_divider = false;
			DateTime current_date = mc.GetFirstDateInMonthGrid ( new DateTime (this_month.Year, this_month.Month, 1));
			for (int i=0; i < 6; i++) 
			{
				// establish if this row is in our clip_area
				Rectangle row_rect = new Rectangle (
					rectangle.X,
					rectangle.Y + title_size.Height + (date_cell_size.Height * (i+1)),
					date_cell_size.Width * 7,
					date_cell_size.Height);
				if (mc.ShowWeekNumbers) {
					row_rect.Width += date_cell_size.Width;
				}
		
				bool draw_row = row_rect.IntersectsWith (clip_rectangle);
				if (draw_row) {
					dc.FillRectangle (GetControlBackBrush (mc.BackColor), row_rect);
				}
				// establish if this is a valid week to draw
				if (mc.IsValidWeekToDraw (this_month, current_date, row, col)) {
					month_row_count = i;
				}
				
				// draw the week number if required
				if (mc.ShowWeekNumbers && month_row_count == i) {
					if (!draw_week_num_divider) {
						draw_week_num_divider = draw_row;
					}
					// get the week for this row
					int week = mc.GetWeekOfYear (current_date);	

					if (draw_row) {
						dc.DrawString (
							week.ToString(),
							mc.Font,
							ResPool.GetSolidBrush (mc.TitleBackColor),
							date_rect,
							mc.centered_format);
					}
					date_rect.Offset(date_cell_size.Width, 0);
				}
								
				// only draw the days if we have to
				if(month_row_count == i) {
					for (int j=0; j < 7; j++) 
					{
						if (draw_row) {
							DrawMonthCalendarDate (
								dc,
								date_rect,
								mc,
								current_date,
								this_month,
								row,
								col);
						}

						// move the day on
						current_date = current_date.AddDays(1);
						date_rect.Offset(date_cell_size.Width, 0);
					}

					// shift the rectangle down one row
					int offset = (mc.ShowWeekNumbers) ? -8 : -7;
					date_rect.Offset(offset*date_cell_size.Width, date_cell_size.Height);
				}
			}

			// month_row_count is zero based, so add one
			month_row_count++;

			// draw week numbers if required
			if (draw_week_num_divider) {
				col_offset = 1;
				dc.DrawLine (
					ResPool.GetPen (mc.ForeColor),
					rectangle.X + date_cell_size.Width - 1,
					rectangle.Y + title_size.Height + date_cell_size.Height + mc.divider_line_offset,
					rectangle.X + date_cell_size.Width - 1,
					rectangle.Y + title_size.Height + date_cell_size.Height + (month_row_count * date_cell_size.Height) - mc.divider_line_offset);
			}
		}

		// draws the pervious or next button
		private void DrawMonthCalendarButton (Graphics dc, Rectangle rectangle, MonthCalendar mc, Size title_size, int x_offset, Size button_size, bool is_previous) 
		{
			const int arrow_width = 4;
			const int arrow_height = 7;

			bool is_clicked = false;
			Rectangle button_rect;
			PointF arrow_center;
			PointF [] arrow_path = new PointF [3];
			
			// prepare the button
			if (is_previous) 
			{
				is_clicked = mc.is_previous_clicked;

				button_rect = new Rectangle (
					rectangle.X + 1 + x_offset,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));

				arrow_center = new PointF (button_rect.X + ((button_rect.Width + arrow_width) / 2.0f), 
											rectangle.Y + ((button_rect.Height + arrow_height) / 2) + 1);
				if (is_clicked) {
					arrow_center.X += 1;
					arrow_center.Y += 1;
				}

				arrow_path [0].X = arrow_center.X;
				arrow_path [0].Y = arrow_center.Y - arrow_height / 2.0f + 0.5f;
				arrow_path [1].X = arrow_center.X;
				arrow_path [1].Y = arrow_center.Y + arrow_height / 2.0f + 0.5f;
				arrow_path [2].X = arrow_center.X - arrow_width;
				arrow_path [2].Y = arrow_center.Y + 0.5f;
			}
			else
			{
				is_clicked = mc.is_next_clicked;

				button_rect = new Rectangle (
					rectangle.Right - 1 - x_offset - button_size.Width,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));

				arrow_center = new PointF (button_rect.X + ((button_rect.Width + arrow_width) / 2.0f), 
											rectangle.Y + ((button_rect.Height + arrow_height) / 2) + 1);
				if (is_clicked) {
					arrow_center.X += 1;
					arrow_center.Y += 1;
				}

				arrow_path [0].X = arrow_center.X - arrow_width;
				arrow_path [0].Y = arrow_center.Y - arrow_height / 2.0f + 0.5f;
				arrow_path [1].X = arrow_center.X - arrow_width;
				arrow_path [1].Y = arrow_center.Y + arrow_height / 2.0f + 0.5f;
				arrow_path [2].X = arrow_center.X;
				arrow_path [2].Y = arrow_center.Y + 0.5f;
			}

			// fill the background
			dc.FillRectangle (SystemBrushes.Control, button_rect);
			// draw the border
			if (is_clicked) {
				dc.DrawRectangle (SystemPens.ControlDark, button_rect);
			}
			else {
				CPDrawBorder3D (dc, button_rect, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
			}
			// draw the arrow
			dc.FillPolygon (SystemBrushes.ControlText, arrow_path);			
			//dc.FillPolygon (SystemBrushes.ControlText, arrow_path, FillMode.Winding);
		}
		

		// draws one day in the calendar grid
		private void DrawMonthCalendarDate (Graphics dc, Rectangle rectangle, MonthCalendar mc,	DateTime date, DateTime month, int row, int col) {
			Color date_color = mc.ForeColor;
			Rectangle interior = new Rectangle (rectangle.X, rectangle.Y, Math.Max(rectangle.Width - 1, 0), Math.Max(rectangle.Height - 1, 0));

			// find out if we are the lead of the first calendar or the trail of the last calendar						
			if (date.Year != month.Year || date.Month != month.Month) {
				DateTime check_date = month.AddMonths (-1);
				// check if it's the month before 
				if (check_date.Year == date.Year && check_date.Month == date.Month && row == 0 && col == 0) {
					date_color = mc.TrailingForeColor;
				} else {
					// check if it's the month after
					check_date = month.AddMonths (1);
					if (check_date.Year == date.Year && check_date.Month == date.Month && row == mc.CalendarDimensions.Height-1 && col == mc.CalendarDimensions.Width-1) {
						date_color = mc.TrailingForeColor;
					} else {
						return;
					}
				}
			} else {
				date_color = mc.ForeColor;
			}

			const int inflate = -1;

			if (date == mc.SelectionStart.Date && date == mc.SelectionEnd.Date) {
				// see if the date is in the start of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate (rectangle, inflate, inflate);				
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 0, 360);
			} else if (date == mc.SelectionStart.Date) {
				// see if the date is in the start of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate (rectangle, inflate, inflate);				
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 90, 180);
				// fill the other side as a straight rect
				if (date < mc.SelectionEnd.Date) 
				{
					// use rectangle instead of rectangle to go all the way to edge of rect
					selection_rect.X = (int) Math.Floor((double)(rectangle.X + rectangle.Width / 2));
					selection_rect.Width = Math.Max(rectangle.Right - selection_rect.X, 0);
					dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
				}
			} else if (date == mc.SelectionEnd.Date) {
				// see if it is the end of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate (rectangle, inflate, inflate);
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 270, 180);
				// fill the other side as a straight rect
				if (date > mc.SelectionStart.Date) {
					selection_rect.X = rectangle.X;
					selection_rect.Width = rectangle.Width - (rectangle.Width / 2);
					dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
				}
			} else if (date > mc.SelectionStart.Date && date < mc.SelectionEnd.Date) {
				// now see if it's in the middle
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate (rectangle, 0, inflate);
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
			}

			// establish if it's a bolded font
			Font font = mc.IsBoldedDate (date) ? mc.bold_font : mc.Font;

			// just draw the date now
			dc.DrawString (date.Day.ToString(), font, ResPool.GetSolidBrush (date_color), rectangle, mc.centered_format);

			// today circle if needed
			if (mc.ShowTodayCircle && date == DateTime.Now.Date) {
				DrawTodayCircle (dc, interior);
			}

			// draw the selection grid
			if (mc.is_date_clicked && mc.clicked_date == date) {
				Pen pen = ResPool.GetDashPen (Color.Black, DashStyle.Dot);
				dc.DrawRectangle (pen, interior);
			}
		}

		private void DrawTodayCircle (Graphics dc, Rectangle rectangle) {
			Color circle_color = Color.FromArgb (248, 0, 0);
			// draw the left hand of the circle 
			Rectangle lhs_circle_rect = new Rectangle (rectangle.X + 1, rectangle.Y + 4, Math.Max(rectangle.Width - 2, 0), Math.Max(rectangle.Height - 5, 0));
			Rectangle rhs_circle_rect = new Rectangle (rectangle.X + 1, rectangle.Y + 1, Math.Max(rectangle.Width - 2, 0), Math.Max(rectangle.Height - 2, 0));
			Point [] curve_points = new Point [3];
			curve_points [0] = new Point (lhs_circle_rect.X, rhs_circle_rect.Y + rhs_circle_rect.Height/12);
			curve_points [1] = new Point (lhs_circle_rect.X + lhs_circle_rect.Width/9, rhs_circle_rect.Y);
			curve_points [2] = new Point (lhs_circle_rect.X + lhs_circle_rect.Width/2 + 1, rhs_circle_rect.Y);

			Pen pen = ResPool.GetSizedPen(circle_color, 2);
			dc.DrawArc (pen, lhs_circle_rect, 90, 180);
			dc.DrawArc (pen, rhs_circle_rect, 270, 180);					
			dc.DrawCurve (pen, curve_points);
			dc.DrawLine (ResPool.GetPen (circle_color), curve_points [2], new Point (curve_points [2].X, lhs_circle_rect.Y));
		}

		#endregion 	// MonthCalendar

		#region Panel
		public override Size PanelDefaultSize {
			get {
				return new Size (200, 100);
			}
		}
		#endregion	// Panel

		#region PictureBox
		public override void DrawPictureBox (Graphics dc, Rectangle clip, PictureBox pb) {
			Rectangle client = pb.ClientRectangle;

#if NET_2_0
			client = new Rectangle (client.Left + pb.Padding.Left, client.Top + pb.Padding.Top, client.Width - pb.Padding.Horizontal, client.Height - pb.Padding.Vertical);
#endif

			// FIXME - instead of drawing the whole picturebox every time
			// intersect the clip rectangle with the drawn picture and only draw what's needed,
			// Also, we only need a background fill where no image goes
			if (pb.Image != null) {
				switch (pb.SizeMode) {
				case PictureBoxSizeMode.StretchImage:
					dc.DrawImage (pb.Image, client.Left, client.Top, client.Width, client.Height);
					break;

				case PictureBoxSizeMode.CenterImage:
					dc.DrawImage (pb.Image, (client.Width / 2) - (pb.Image.Width / 2), (client.Height / 2) - (pb.Image.Height / 2));
					break;
#if NET_2_0
				case PictureBoxSizeMode.Zoom:
					Size image_size;
					
					if (((float)pb.Image.Width / (float)pb.Image.Height) >= ((float)client.Width / (float)client.Height))
						image_size = new Size (client.Width, (pb.Image.Height * client.Width) / pb.Image.Width);
					else
						image_size = new Size ((pb.Image.Width * client.Height) / pb.Image.Height, client.Height);

					dc.DrawImage (pb.Image, (client.Width / 2) - (image_size.Width / 2), (client.Height / 2) - (image_size.Height / 2), image_size.Width, image_size.Height);
					break;
#endif
				default:
					// Normal, AutoSize
					dc.DrawImage (pb.Image, client.Left, client.Top, pb.Image.Width, pb.Image.Height);
					break;
				}

				return;
			}
		}

		public override Size PictureBoxDefaultSize {
			get {
				return new Size (100, 50);
			}
		}
		#endregion	// PictureBox

		#region PrintPreviewControl
		public override int PrintPreviewControlPadding {
			get { return 8; }
		}

		public override Size PrintPreviewControlGetPageSize (PrintPreviewControl preview)
		{
			int page_width, page_height;
			int padding = PrintPreviewControlPadding;
			PreviewPageInfo[] pis = preview.page_infos;

			if (preview.AutoZoom) {
				int height_available = preview.ClientRectangle.Height - (preview.Rows) * padding - 2 * padding;
				int width_available = preview.ClientRectangle.Width - (preview.Columns - 1) * padding - 2 * padding;

				float image_ratio = (float)pis[0].Image.Width / pis[0].Image.Height;

				/* try to lay things out using the width to determine the size */
				page_width = width_available / preview.Columns;
				page_height = (int)(page_width / image_ratio);

				/* does the height fit? */
				if (page_height * (preview.Rows + 1) > height_available) {
					/* no, lay things out via the height */
					page_height = height_available / (preview.Rows + 1);
					page_width = (int)(page_height * image_ratio);
				}
			}
			else {
				page_width = (int)(pis[0].Image.Width * preview.Zoom);
				page_height = (int)(pis[0].Image.Height * preview.Zoom);
			}

			return new Size (page_width, page_height);
		}

		public override void PrintPreviewControlPaint (PaintEventArgs pe, PrintPreviewControl preview, Size page_size)
		{
			int padding = 8;
			PreviewPageInfo[] pis = preview.page_infos;
			if (pis == null)
				return;

			int page_x, page_y;

			int width = page_size.Width * preview.Columns + padding * (preview.Columns - 1) + 2 * padding;
			int height = page_size.Height * (preview.Rows + 1) + padding * preview.Rows + 2 * padding;

			Rectangle viewport = preview.ViewPort;

			pe.Graphics.Clip = new Region (viewport);

			/* center things if we can */
			int off_x = viewport.Width / 2 - width / 2;
			if (off_x < 0) off_x = 0;
			int off_y = viewport.Height / 2 - height / 2;
			if (off_y < 0) off_y = 0;

			page_y = off_y + padding - preview.vbar_value;

			if (preview.StartPage > 0) {
				int p = preview.StartPage - 1;
				for (int py = 0; py < preview.Rows + 1; py ++) {
					page_x = off_x + padding - preview.hbar_value;
					for (int px = 0; px < preview.Columns; px ++) {
						if (p >= pis.Length)
							continue;
						Image image = preview.image_cache[p];
						if (image == null)
							image = pis[p].Image;
						Rectangle dest = new Rectangle (new Point (page_x, page_y), page_size);

						pe.Graphics.DrawImage (image, dest, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

						page_x += padding + page_size.Width;
						p++;
					}
					page_y += padding + page_size.Height;
				}
			}
		}
		#endregion      // PrintPreviewControl

		#region ProgressBar
		public override void DrawProgressBar (Graphics dc, Rectangle clip_rect, ProgressBar ctrl) 
		{
			Rectangle client_area = ctrl.client_area;
			
			/* Draw border */
			CPDrawBorder3D (dc, ctrl.ClientRectangle, Border3DStyle.SunkenOuter, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom & ~Border3DSide.Middle, ColorControl);
			
			/* Draw Blocks */
			int draw_mode = 0;
			int max_blocks = int.MaxValue;
			int start_pixel = client_area.X;
#if NET_2_0
			draw_mode = (int) ctrl.Style;
#endif
			switch (draw_mode) {
#if NET_2_0
			case 1: { // Continuous
				int pixels_to_draw;
				pixels_to_draw = (int)(client_area.Width * ((double)(ctrl.Value - ctrl.Minimum) / (double)(Math.Max(ctrl.Maximum - ctrl.Minimum, 1))));
				dc.FillRectangle (ResPool.GetSolidBrush (ctrl.ForeColor), new Rectangle (client_area.X, client_area.Y, pixels_to_draw, client_area.Height));
				break;
			}
			case 2: // Marquee
				if (XplatUI.ThemesEnabled) {
					int ms_diff = (int) (DateTime.Now - ctrl.start).TotalMilliseconds;
					double percent_done = (double) ms_diff / ProgressBarMarqueeSpeedScaling 
						% (double)ctrl.MarqueeAnimationSpeed / (double)ctrl.MarqueeAnimationSpeed;
					max_blocks = 5;
					start_pixel = client_area.X + (int) (client_area.Width * percent_done);
				}
				
				goto case 0;
#endif
			case 0:
			default:  // Blocks
				Rectangle block_rect;
				int space_betweenblocks = ProgressBarChunkSpacing;
				int block_width;
				int increment;
				int barpos_pixels;
				int block_count = 0;
				
				block_width = ProgressBarGetChunkSize (client_area.Height);
				block_width = Math.Max (block_width, 0); // block_width is used to break out the loop below, it must be >= 0!
				barpos_pixels = (int)(((double)(ctrl.Value - ctrl.Minimum) * client_area.Width) / (Math.Max (ctrl.Maximum - ctrl.Minimum, 1)));
				increment = block_width + space_betweenblocks;
				
				block_rect = new Rectangle (start_pixel, client_area.Y, block_width, client_area.Height);
				while (true) {
					if (max_blocks != int.MaxValue) {
						if (block_count >= max_blocks)
							break;
						if (block_rect.X > client_area.Width)
							block_rect.X -= client_area.Width;
					} else {
						if ((block_rect.X - client_area.X) >= barpos_pixels)
							break;
					}
					
					if (clip_rect.IntersectsWith (block_rect) == true) {				
						dc.FillRectangle (ResPool.GetSolidBrush (ctrl.ForeColor), block_rect);
					}				
					
					block_rect.X  += increment;
					block_count++;
				}
				break;
			
			}
		}
		
		public const int ProgressBarChunkSpacing = 2;

		public static int ProgressBarGetChunkSize ()
		{
			return ProgressBarGetChunkSize (ProgressBarDefaultHeight);
		}
		
		static int ProgressBarGetChunkSize (int progressBarClientAreaHeight)
		{
			int size = (progressBarClientAreaHeight * 2) / 3;
			return size;
		}

		const int ProgressBarDefaultHeight = 23;

		public override Size ProgressBarDefaultSize {
			get {
				return new Size (100, ProgressBarDefaultHeight);
			}
		}

		public const double ProgressBarMarqueeSpeedScaling = 15;

		#endregion	// ProgressBar

		#region RadioButton
		public override void DrawRadioButton (Graphics dc, Rectangle clip_rectangle, RadioButton radio_button) {
			StringFormat	text_format;
			Rectangle 	client_rectangle;
			Rectangle	text_rectangle;
			Rectangle 	radiobutton_rectangle;
			int		radiobutton_size = 13;
			int 	radiobutton_space = 4;

			client_rectangle = radio_button.ClientRectangle;
			text_rectangle = client_rectangle;
			radiobutton_rectangle = new Rectangle(text_rectangle.X, text_rectangle.Y, radiobutton_size, radiobutton_size);

			text_format = new StringFormat();
			text_format.Alignment = StringAlignment.Near;
			text_format.LineAlignment = StringAlignment.Center;
			text_format.HotkeyPrefix = HotkeyPrefix.Show;

			/* Calculate the position of text and checkbox rectangle */
			if (radio_button.appearance!=Appearance.Button) {
				switch(radio_button.radiobutton_alignment) {
				case ContentAlignment.BottomCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width;
					text_rectangle.Height=client_rectangle.Height-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.BottomLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X+radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;					
					break;
				}

				case ContentAlignment.BottomRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.MiddleCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width;
					break;
				}

				default:
				case ContentAlignment.MiddleLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X+radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.MiddleRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.TopCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Y=radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width;
					text_rectangle.Height=client_rectangle.Height-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.TopLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X+radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.TopRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}
				}
			} else {
				text_rectangle.X=client_rectangle.X;
				text_rectangle.Width=client_rectangle.Width;
			}
			
			/* Set the horizontal alignment of our text */
			switch(radio_button.text_alignment) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft: {
					text_format.Alignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter: {
					text_format.Alignment=StringAlignment.Center;
					break;
				}

				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight: {
					text_format.Alignment=StringAlignment.Far;
					break;
				}
			}

			/* Set the vertical alignment of our text */
			switch(radio_button.text_alignment) {
				case ContentAlignment.TopLeft: 
				case ContentAlignment.TopCenter: 
				case ContentAlignment.TopRight: {
					text_format.LineAlignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight: {
					text_format.LineAlignment=StringAlignment.Far;
					break;
				}

				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight: {
					text_format.LineAlignment=StringAlignment.Center;
					break;
				}
			}

			ButtonState state = ButtonState.Normal;
			if (radio_button.FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (radio_button.Checked) {
				state |= ButtonState.Checked;
			}
			
			if (!radio_button.Enabled) {
				state |= ButtonState.Inactive;
			}

			// Start drawing
			RadioButton_DrawButton(radio_button, dc, state, radiobutton_rectangle);
			
			if ((radio_button.image != null) || (radio_button.image_list != null))
				ButtonBase_DrawImage(radio_button, dc);
			
			RadioButton_DrawText(radio_button, text_rectangle, dc, text_format);

			if (radio_button.Focused && radio_button.Enabled && radio_button.appearance != Appearance.Button && radio_button.Text != String.Empty && radio_button.ShowFocusCues) {
				SizeF text_size = dc.MeasureString (radio_button.Text, radio_button.Font);
				
				Rectangle focus_rect = Rectangle.Empty;
				focus_rect.X = text_rectangle.X;
				focus_rect.Y = (int)((text_rectangle.Height - text_size.Height) / 2);
				focus_rect.Size = text_size.ToSize ();

				RadioButton_DrawFocus (radio_button, dc, focus_rect);
			}
			
			text_format.Dispose ();
		}

		protected virtual void RadioButton_DrawButton(RadioButton radio_button, Graphics dc, ButtonState state, Rectangle radiobutton_rectangle)
		{
			dc.FillRectangle(GetControlBackBrush (radio_button.BackColor), radio_button.ClientRectangle);
			
			if (radio_button.appearance==Appearance.Button) {
				ButtonBase_DrawButton (radio_button, dc);
				
				if ((radio_button.Focused) && radio_button.Enabled)
					ButtonBase_DrawFocus(radio_button, dc);
			} else {
				// establish if we are rendering a flat style of some sort
				if (radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleRadioButton (dc, radiobutton_rectangle, radio_button);
				} else {
					CPDrawRadioButton(dc, radiobutton_rectangle, state);
				}
			}
		}
		
		protected virtual void RadioButton_DrawText(RadioButton radio_button, Rectangle text_rectangle, Graphics dc, StringFormat text_format)
		{
			DrawCheckBox_and_RadioButtonText (radio_button, text_rectangle, dc, 
							  text_format, radio_button.Appearance, radio_button.Checked);
		}
		
		protected virtual void RadioButton_DrawFocus(RadioButton radio_button, Graphics dc, Rectangle text_rectangle)
		{
			DrawInnerFocusRectangle (dc, text_rectangle, radio_button.BackColor);
		}
		
		
		// renders a radio button with the Flat and Popup FlatStyle
		protected virtual void DrawFlatStyleRadioButton (Graphics graphics, Rectangle rectangle, RadioButton radio_button)
		{
			int	lineWidth;
			
			if (radio_button.Enabled) {
				
				// draw the outer flatstyle arcs
				if (radio_button.FlatStyle == FlatStyle.Flat) {
					graphics.DrawArc (SystemPens.ControlDarkDark, rectangle, 0, 359);
					
					// fill in the area depending on whether or not the mouse is hovering
					if ((radio_button.is_entered || radio_button.Capture) && !radio_button.is_pressed) {
						graphics.FillPie (SystemBrushes.ControlLight, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
					} else {
						graphics.FillPie (SystemBrushes.ControlLightLight, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
					}
				} else {
					// must be a popup radio button
					// fill the control
					graphics.FillPie (SystemBrushes.ControlLightLight, rectangle, 0, 359);

					if (radio_button.is_entered || radio_button.Capture) {
						// draw the popup 3d button knob
						graphics.DrawArc (SystemPens.ControlLight, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 0, 359);

						graphics.DrawArc (SystemPens.ControlDark, rectangle, 135, 180);
						graphics.DrawArc (SystemPens.ControlLightLight, rectangle, 315, 180);
						
					} else {
						// just draw lighter flatstyle outer circle
						graphics.DrawArc (SystemPens.ControlDark, rectangle, 0, 359);						
					}										
				}
			} else {
				// disabled
				// fill control background color regardless of actual backcolor
				graphics.FillPie (SystemBrushes.Control, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
				// draw the ark as control dark
				graphics.DrawArc (SystemPens.ControlDark, rectangle, 0, 359);
			}

			// draw the check
			if (radio_button.Checked) {
				lineWidth = Math.Max (1, Math.Min(rectangle.Width, rectangle.Height)/3);
				
				Pen dot_pen = SystemPens.ControlDarkDark;
				Brush dot_brush = SystemBrushes.ControlDarkDark;

				if (!radio_button.Enabled || ((radio_button.FlatStyle == FlatStyle.Popup) && radio_button.is_pressed)) {
					dot_pen = SystemPens.ControlDark;
					dot_brush = SystemBrushes.ControlDark;
				} 
				
				if (rectangle.Height >  13) {
					graphics.FillPie (dot_brush, rectangle.X + lineWidth, rectangle.Y + lineWidth, rectangle.Width - lineWidth * 2, rectangle.Height - lineWidth * 2, 0, 359);
				} else {
					int x_half_pos = (rectangle.Width / 2) + rectangle.X;
					int y_half_pos = (rectangle.Height / 2) + rectangle.Y;
					
					graphics.DrawLine (dot_pen, x_half_pos - 1, y_half_pos, x_half_pos + 2, y_half_pos);
					graphics.DrawLine (dot_pen, x_half_pos - 1, y_half_pos + 1, x_half_pos + 2, y_half_pos + 1);
					
					graphics.DrawLine (dot_pen, x_half_pos, y_half_pos - 1, x_half_pos, y_half_pos + 2);
					graphics.DrawLine (dot_pen, x_half_pos + 1, y_half_pos - 1, x_half_pos + 1, y_half_pos + 2);
				}
			}
		}

		public override Size RadioButtonDefaultSize {
			get {
				return new Size (104,24);
			}
		}

#if NET_2_0
		public override void DrawRadioButton (Graphics g, RadioButton rb, Rectangle glyphArea, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle)
		{
			// Draw Button Background
			if (rb.FlatStyle == FlatStyle.Flat || rb.FlatStyle == FlatStyle.Popup) {
				glyphArea.Height -= 2;
				glyphArea.Width -= 2;
			}
			
			DrawRadioButtonGlyph (g, rb, glyphArea);

			// If we have an image, draw it
			if (imageBounds.Size != Size.Empty)
				DrawRadioButtonImage (g, rb, imageBounds);

			if (rb.Focused && rb.Enabled && rb.ShowFocusCues && textBounds.Size != Size.Empty)
				DrawRadioButtonFocus (g, rb, textBounds);

			// If we have text, draw it
			if (textBounds != Rectangle.Empty)
				DrawRadioButtonText (g, rb, textBounds);
		}

		public virtual void DrawRadioButtonGlyph (Graphics g, RadioButton rb, Rectangle glyphArea)
		{
			if (rb.Pressed)
				ThemeElements.CurrentTheme.RadioButtonPainter.PaintRadioButton (g, glyphArea, rb.BackColor, rb.ForeColor, ElementState.Pressed, rb.FlatStyle, rb.Checked);
			else if (rb.InternalSelected)
				ThemeElements.CurrentTheme.RadioButtonPainter.PaintRadioButton (g, glyphArea, rb.BackColor, rb.ForeColor, ElementState.Normal, rb.FlatStyle, rb.Checked);
			else if (rb.Entered)
				ThemeElements.CurrentTheme.RadioButtonPainter.PaintRadioButton (g, glyphArea, rb.BackColor, rb.ForeColor, ElementState.Hot, rb.FlatStyle, rb.Checked);
			else if (!rb.Enabled)
				ThemeElements.CurrentTheme.RadioButtonPainter.PaintRadioButton (g, glyphArea, rb.BackColor, rb.ForeColor, ElementState.Disabled, rb.FlatStyle, rb.Checked);
			else
				ThemeElements.CurrentTheme.RadioButtonPainter.PaintRadioButton (g, glyphArea, rb.BackColor, rb.ForeColor, ElementState.Normal, rb.FlatStyle, rb.Checked);
		}

		public virtual void DrawRadioButtonFocus (Graphics g, RadioButton rb, Rectangle focusArea)
		{
			ControlPaint.DrawFocusRectangle (g, focusArea);
		}

		public virtual void DrawRadioButtonImage (Graphics g, RadioButton rb, Rectangle imageBounds)
		{
			if (rb.Enabled)
				g.DrawImage (rb.Image, imageBounds);
			else
				CPDrawImageDisabled (g, rb.Image, imageBounds.Left, imageBounds.Top, ColorControl);
		}

		public virtual void DrawRadioButtonText (Graphics g, RadioButton rb, Rectangle textBounds)
		{
			if (rb.Enabled)
				TextRenderer.DrawTextInternal (g, rb.Text, rb.Font, textBounds, rb.ForeColor, rb.TextFormatFlags, rb.UseCompatibleTextRendering);
			else
				DrawStringDisabled20 (g, rb.Text, rb.Font, textBounds, rb.BackColor, rb.TextFormatFlags, rb.UseCompatibleTextRendering);
		}

		public override Size CalculateRadioButtonAutoSize (RadioButton rb)
		{
			Size ret_size = Size.Empty;
			Size text_size = TextRenderer.MeasureTextInternal (rb.Text, rb.Font, rb.UseCompatibleTextRendering);
			Size image_size = rb.Image == null ? Size.Empty : rb.Image.Size;

			// Pad the text size
			if (rb.Text.Length != 0) {
				text_size.Height += 4;
				text_size.Width += 4;
			}

			switch (rb.TextImageRelation) {
				case TextImageRelation.Overlay:
					ret_size.Height = Math.Max (rb.Text.Length == 0 ? 0 : text_size.Height, image_size.Height);
					ret_size.Width = Math.Max (text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageAboveText:
				case TextImageRelation.TextAboveImage:
					ret_size.Height = text_size.Height + image_size.Height;
					ret_size.Width = Math.Max (text_size.Width, image_size.Width);
					break;
				case TextImageRelation.ImageBeforeText:
				case TextImageRelation.TextBeforeImage:
					ret_size.Height = Math.Max (text_size.Height, image_size.Height);
					ret_size.Width = text_size.Width + image_size.Width;
					break;
			}

			// Pad the result
			ret_size.Height += (rb.Padding.Vertical);
			ret_size.Width += (rb.Padding.Horizontal) + 15;

			// There seems to be a minimum height
			if (ret_size.Height == rb.Padding.Vertical)
				ret_size.Height += 14;

			return ret_size;
		}

		public override void CalculateRadioButtonTextAndImageLayout (ButtonBase b, Point offset, out Rectangle glyphArea, out Rectangle textRectangle, out Rectangle imageRectangle)
		{
			CalculateCheckBoxTextAndImageLayout (b, offset, out glyphArea, out textRectangle, out imageRectangle);
		}
#endif
		#endregion	// RadioButton

		#region ScrollBar
		public override void DrawScrollBar (Graphics dc, Rectangle clip, ScrollBar bar)
		{
			int		scrollbutton_width = bar.scrollbutton_width;
			int		scrollbutton_height = bar.scrollbutton_height;
			Rectangle	first_arrow_area;
			Rectangle	second_arrow_area;			
			Rectangle	thumb_pos;
			
			thumb_pos = bar.ThumbPos;

			if (bar.vert) {
				first_arrow_area = new Rectangle(0, 0, bar.Width, scrollbutton_height);
				bar.FirstArrowArea = first_arrow_area;

				second_arrow_area = new Rectangle(0, bar.ClientRectangle.Height - scrollbutton_height, bar.Width, scrollbutton_height);
				bar.SecondArrowArea = second_arrow_area;

				thumb_pos.Width = bar.Width;
				bar.ThumbPos = thumb_pos;

				Brush VerticalBrush;
				/* Background, upper track */
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Backwards)
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle UpperTrack = new Rectangle (0, 0, bar.ClientRectangle.Width, bar.ThumbPos.Bottom);
				if (clip.IntersectsWith (UpperTrack))
					dc.FillRectangle (VerticalBrush, UpperTrack);

				/* Background, lower track */
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Forward)
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					VerticalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle LowerTrack = new Rectangle (0, bar.ThumbPos.Bottom, bar.ClientRectangle.Width, bar.ClientRectangle.Height - bar.ThumbPos.Bottom);
				if (clip.IntersectsWith (LowerTrack))
					dc.FillRectangle (VerticalBrush, LowerTrack);

				/* Buttons */
				if (clip.IntersectsWith (first_arrow_area))
					CPDrawScrollButton (dc, first_arrow_area, ScrollButton.Up, bar.firstbutton_state);
				if (clip.IntersectsWith (second_arrow_area))
					CPDrawScrollButton (dc, second_arrow_area, ScrollButton.Down, bar.secondbutton_state);
			} else {
				first_arrow_area = new Rectangle(0, 0, scrollbutton_width, bar.Height);
				bar.FirstArrowArea = first_arrow_area;

				second_arrow_area = new Rectangle (bar.ClientRectangle.Width - scrollbutton_width, 0, scrollbutton_width, bar.Height);
				bar.SecondArrowArea = second_arrow_area;

				thumb_pos.Height = bar.Height;
				bar.ThumbPos = thumb_pos;

				Brush HorizontalBrush;
				//Background, left track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Backwards)
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle LeftTrack = new Rectangle (0, 0, bar.ThumbPos.Right, bar.ClientRectangle.Height);
				if (clip.IntersectsWith (LeftTrack))
					dc.FillRectangle (HorizontalBrush, LeftTrack);

				//Background, right track
				if (bar.thumb_moving == ScrollBar.ThumbMoving.Forward)
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63, 63, 63), Color.Black);
				else
					HorizontalBrush = ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, Color.White);
				Rectangle RightTrack = new Rectangle (bar.ThumbPos.Right, 0, bar.ClientRectangle.Width - bar.ThumbPos.Right, bar.ClientRectangle.Height);
				if (clip.IntersectsWith (RightTrack))
					dc.FillRectangle (HorizontalBrush, RightTrack);

				/* Buttons */
				if (clip.IntersectsWith (first_arrow_area))
					CPDrawScrollButton (dc, first_arrow_area, ScrollButton.Left, bar.firstbutton_state);
				if (clip.IntersectsWith (second_arrow_area))
					CPDrawScrollButton (dc, second_arrow_area, ScrollButton.Right, bar.secondbutton_state);
			}

			/* Thumb */
			ScrollBar_DrawThumb(bar, thumb_pos, clip, dc);				
		}

		protected virtual void ScrollBar_DrawThumb(ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc)
		{
			if (bar.Enabled && thumb_pos.Width > 0 && thumb_pos.Height > 0 && clip.IntersectsWith(thumb_pos))
				DrawScrollButtonPrimitive(dc, thumb_pos, ButtonState.Normal);
		}

		public override int ScrollBarButtonSize {
			get { return 16; }
		}

		public override bool ScrollBarHasHotElementStyles {
			get {
				return false;
			}
		}

		public override bool ScrollBarHasPressedThumbStyle {
			get { 
				return false;
			}
		}

		public override bool ScrollBarHasHoverArrowButtonStyle {
			get {
				return false;
			}
		}
		#endregion	// ScrollBar

		#region StatusBar
		public	override void DrawStatusBar (Graphics real_dc, Rectangle clip, StatusBar sb) {
			Rectangle area = sb.ClientRectangle;
			int horz_border = 2;
			int vert_border = 2;

			Image backbuffer = new Bitmap (sb.ClientSize.Width, sb.ClientSize.Height, real_dc);
			Graphics dc = Graphics.FromImage (backbuffer);
			
			DrawStatusBarBackground (dc, clip, sb);
			
			if (!sb.ShowPanels && sb.Text != String.Empty) {
				string text = sb.Text;
				StringFormat string_format = new StringFormat ();
				string_format.Trimming = StringTrimming.Character;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
				
				if (text.Length > 127)
					text = text.Substring (0, 127);

				if (text [0] == '\t') {
					string_format.Alignment = StringAlignment.Center;
					text = text.Substring (1);
					if (text [0] == '\t') {
						string_format.Alignment = StringAlignment.Far;
						text = text.Substring (1);
					}
				}
		
				dc.DrawString (text, sb.Font, ResPool.GetSolidBrush (sb.ForeColor),
						new Rectangle(area.X + 2, area.Y + 2, area.Width - 4, area.Height - 4), string_format);
				string_format.Dispose ();
			} else if (sb.ShowPanels) {
				Brush br_forecolor = GetControlForeBrush (sb.ForeColor);
				int prev_x = area.X + horz_border;
				int y = area.Y + vert_border;
				for (int i = 0; i < sb.Panels.Count; i++) {
					Rectangle pr = new Rectangle (prev_x, y,
						sb.Panels [i].Width, area.Height);
					prev_x += pr.Width + StatusBarHorzGapWidth;
					if (pr.IntersectsWith (clip))
						DrawStatusBarPanel (dc, pr, i, br_forecolor, sb.Panels [i]);
				}
			}

			if (sb.SizingGrip)
				DrawStatusBarSizingGrip (dc, clip, sb, area);
			
			real_dc.DrawImage (backbuffer, 0, 0);
			dc.Dispose ();
			backbuffer.Dispose ();

		}

		protected virtual void DrawStatusBarBackground (Graphics dc, Rectangle clip, StatusBar sb)
		{
			bool is_color_control = sb.BackColor.ToArgb () == ColorControl.ToArgb ();

			Brush brush = is_color_control ? SystemBrushes.Control : ResPool.GetSolidBrush (sb.BackColor);
			dc.FillRectangle (brush, clip);
		}

		protected virtual void DrawStatusBarSizingGrip (Graphics dc, Rectangle clip, StatusBar sb, Rectangle area)
		{
			area = new Rectangle (area.Right - 16 - 2, area.Bottom - 12 - 1, 16, 16);
			CPDrawSizeGrip (dc, ColorControl, area);
		}

		protected virtual void DrawStatusBarPanel (Graphics dc, Rectangle area, int index,
			Brush br_forecolor, StatusBarPanel panel) {
			int border_size = 3; // this is actually const, even if the border style is none
			int icon_width = 16;
			
			area.Height -= border_size;

			DrawStatusBarPanelBackground (dc, area, panel);

			if (panel.Style == StatusBarPanelStyle.OwnerDraw) {
				StatusBarDrawItemEventArgs e = new StatusBarDrawItemEventArgs (
					dc, panel.Parent.Font, area, index, DrawItemState.Default,
					panel, panel.Parent.ForeColor, panel.Parent.BackColor);
				panel.Parent.OnDrawItemInternal (e);
				return;
			}

			string text = panel.Text;
			StringFormat string_format = new StringFormat ();
			string_format.Trimming = StringTrimming.Character;
			string_format.FormatFlags = StringFormatFlags.NoWrap;

			
			if (text != null && text.Length > 0 && text [0] == '\t') {
				string_format.Alignment = StringAlignment.Center;
				text = text.Substring (1);
				if (text [0] == '\t') {
					string_format.Alignment = StringAlignment.Far;
					text = text.Substring (1);
				}
			}

			Rectangle string_rect = Rectangle.Empty;
			int x;
			int len;
			int icon_x = 0;
			int y = (area.Height / 2 - (int) panel.Parent.Font.Size / 2) - 1;

			switch (panel.Alignment) {
			case HorizontalAlignment.Right:
				len = (int) dc.MeasureString (text, panel.Parent.Font).Width;
				x = area.Right - len - 4;
				string_rect = new Rectangle (x, y, 
						area.Right - x - border_size,
						area.Bottom - y - border_size);
				if (panel.Icon != null) {
					icon_x = x - icon_width - 2;
				}
				break;
			case HorizontalAlignment.Center:
				len = (int) dc.MeasureString (text, panel.Parent.Font).Width;
				x = area.Left + ((panel.Width - len) / 2);
				
				string_rect = new Rectangle (x, y, 
						area.Right - x - border_size,
						area.Bottom - y - border_size);

				if (panel.Icon != null) {
					icon_x = x - icon_width - 2;
				}
				break;

				
			default:
				int left = area.Left + border_size;;
				if (panel.Icon != null) {
					icon_x = area.Left + 2;
					left = icon_x + icon_width + 2;
				}

				x = left;
				string_rect = new Rectangle (x, y, 
						area.Right - x - border_size,
						area.Bottom - y - border_size);
				break;
			}

			RectangleF clip_bounds = dc.ClipBounds;
			dc.SetClip (area);
			dc.DrawString (text, panel.Parent.Font, br_forecolor, string_rect, string_format);			
			dc.SetClip (clip_bounds);

			if (panel.Icon != null) {
				dc.DrawIcon (panel.Icon, new Rectangle (icon_x, y, icon_width, icon_width));
			}
		}

		protected virtual void DrawStatusBarPanelBackground (Graphics dc, Rectangle area, StatusBarPanel panel)
		{
			if (panel.BorderStyle != StatusBarPanelBorderStyle.None) {
				Border3DStyle border_style = Border3DStyle.SunkenOuter;
				if (panel.BorderStyle == StatusBarPanelBorderStyle.Raised)
					border_style = Border3DStyle.RaisedInner;
					
				CPDrawBorder3D(dc, area, border_style, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, panel.Parent.BackColor);
			}
		}

		public override int StatusBarSizeGripWidth {
			get { return 15; }
		}

		public override int StatusBarHorzGapWidth {
			get { return 3; }
		}

		public override Size StatusBarDefaultSize {
			get {
				return new Size (100, 22);
			}
		}
		#endregion	// StatusBar

		#region TabControl

		#region TabControl settings

		public override Size TabControlDefaultItemSize {
			get { return ThemeElements.CurrentTheme.TabControlPainter.DefaultItemSize; }
		}

		public override Point TabControlDefaultPadding {
			get { return ThemeElements.CurrentTheme.TabControlPainter.DefaultPadding; }
		}

		public override int TabControlMinimumTabWidth {
			get { return ThemeElements.CurrentTheme.TabControlPainter.MinimumTabWidth; }
		}

		public override Rectangle TabControlSelectedDelta {
			get { return ThemeElements.CurrentTheme.TabControlPainter.SelectedTabDelta; }
		}

		public override int TabControlSelectedSpacing {
			get { return ThemeElements.CurrentTheme.TabControlPainter.SelectedSpacing; }
		}
		
		public override int TabPanelOffsetX {
			get { return ThemeElements.CurrentTheme.TabControlPainter.TabPanelOffset.X; }
		}
		
		public override int TabPanelOffsetY {
			get { return ThemeElements.CurrentTheme.TabControlPainter.TabPanelOffset.Y; }
		}

		public override int TabControlColSpacing {
			get { return ThemeElements.CurrentTheme.TabControlPainter.ColSpacing; }
		}

		public override Point TabControlImagePadding {
			get { return ThemeElements.CurrentTheme.TabControlPainter.ImagePadding; }
		}

		public override int TabControlScrollerWidth {
			get {return ThemeElements.CurrentTheme.TabControlPainter.ScrollerWidth; }
		}


		public override Size TabControlGetSpacing (TabControl tab) 
		{
			try {
				return ThemeElements.CurrentTheme.TabControlPainter.RowSpacing (tab);
			} catch {
				throw new Exception ("Invalid Appearance value: " + tab.Appearance);
			}
		}
		#endregion

		public override void DrawTabControl (Graphics dc, Rectangle area, TabControl tab)
		{
			ThemeElements.CurrentTheme.TabControlPainter.Draw (dc, area, tab);
		}

		public override Rectangle TabControlGetDisplayRectangle (TabControl tab)
		{
			return ThemeElements.CurrentTheme.TabControlPainter.GetDisplayRectangle (tab);
		}

		public override Rectangle TabControlGetPanelRect (TabControl tab)
		{
			return ThemeElements.CurrentTheme.TabControlPainter.GetTabPanelRect (tab);
		}

		#endregion

		#region TextBox
		public override void TextBoxBaseFillBackground (TextBoxBase textBoxBase, Graphics g, Rectangle clippingArea)
		{
			if (textBoxBase.backcolor_set || (textBoxBase.Enabled && !textBoxBase.read_only)) {
				g.FillRectangle(ResPool.GetSolidBrush(textBoxBase.BackColor), clippingArea);
			} else {
				g.FillRectangle(ResPool.GetSolidBrush(ColorControl), clippingArea);
			}
		}

		public override bool TextBoxBaseHandleWmNcPaint (TextBoxBase textBoxBase, ref Message m)
		{
			return false;
		}

		public override bool TextBoxBaseShouldPaintBackground (TextBoxBase textBoxBase)
		{
			return true;
		}
		#endregion

		#region ToolBar
		public  override void DrawToolBar (Graphics dc, Rectangle clip_rectangle, ToolBar control) 
		{
			StringFormat format = new StringFormat ();
			format.Trimming = StringTrimming.EllipsisCharacter;
			format.LineAlignment = StringAlignment.Center;
			if (control.ShowKeyboardCuesInternal)
				format.HotkeyPrefix = HotkeyPrefix.Show;
			else
				format.HotkeyPrefix = HotkeyPrefix.Hide;

			if (control.TextAlign == ToolBarTextAlign.Underneath)
				format.Alignment = StringAlignment.Center;
			else
				format.Alignment = StringAlignment.Near;
#if !NET_2_0
			if (control is PropertyGrid.PropertyToolBar) {
				dc.FillRectangle (ResPool.GetSolidBrush(control.BackColor), clip_rectangle);
				
				if (clip_rectangle.X == 0) {
					dc.DrawLine (SystemPens.ControlLightLight, clip_rectangle.X, 1, clip_rectangle.X, control.Bottom);
				}

				if (clip_rectangle.Y < 2) {
					dc.DrawLine (SystemPens.ControlLightLight, clip_rectangle.X, 1, clip_rectangle.Right, 1);
				}

				if (clip_rectangle.Bottom == control.Bottom) {
					dc.DrawLine (SystemPens.ControlDark, clip_rectangle.X, clip_rectangle.Bottom - 1, clip_rectangle.Right, clip_rectangle.Bottom - 1);
				}

				if (clip_rectangle.Right == control.Right) {
					dc.DrawLine (SystemPens.ControlDark, clip_rectangle.Right - 1, 1, clip_rectangle.Right - 1, control.Bottom - 1);
				}
			} else {
#endif
				if (control.Appearance != ToolBarAppearance.Flat || control.Parent == null) {
					dc.FillRectangle (SystemBrushes.Control, clip_rectangle);
				}

				if (control.Divider && clip_rectangle.Y < 2) {
					if (clip_rectangle.Y < 1) {
						dc.DrawLine (SystemPens.ControlDark, clip_rectangle.X, 0, clip_rectangle.Right, 0);
					}
					dc.DrawLine (SystemPens.ControlLightLight, clip_rectangle.X, 1, clip_rectangle.Right, 1);
				}
#if !NET_2_0
			}
#endif

			foreach (ToolBarItem item in control.items)
				if (item.Button.Visible && clip_rectangle.IntersectsWith (item.Rectangle))
					DrawToolBarButton (dc, control, item, format);

			format.Dispose ();
		}

		protected virtual void DrawToolBarButton (Graphics dc, ToolBar control, ToolBarItem item, StringFormat format)
		{
			bool is_flat = (control.Appearance == ToolBarAppearance.Flat);
			
			DrawToolBarButtonBorder (dc, item, is_flat);

			switch (item.Button.Style) {
			case ToolBarButtonStyle.DropDownButton:
				if (control.DropDownArrows)
					DrawToolBarDropDownArrow (dc, item, is_flat);
				DrawToolBarButtonContents (dc, control, item, format);
				break;

			case ToolBarButtonStyle.Separator:
				if (is_flat)
					DrawToolBarSeparator (dc, item);
				break;

			case ToolBarButtonStyle.ToggleButton:
				DrawToolBarToggleButtonBackground (dc, item);
				DrawToolBarButtonContents (dc, control, item, format);
				break;

			default:
				DrawToolBarButtonContents (dc, control, item, format);
				break;
			}
		}

		const Border3DSide all_sides = Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom;

		protected virtual void DrawToolBarButtonBorder (Graphics dc, ToolBarItem item, bool is_flat)
		{
			if (item.Button.Style == ToolBarButtonStyle.Separator)
				return;

			Border3DStyle style;

			if (is_flat) {
				if (item.Button.Pushed || item.Pressed)
					style = Border3DStyle.SunkenOuter;
				else if (item.Hilight)
					style = Border3DStyle.RaisedInner;
				else
					return;

			} else {
				if (item.Button.Pushed || item.Pressed)
					style = Border3DStyle.Sunken;
				else 
					style = Border3DStyle.Raised;
			}
			
			Rectangle rect = item.Rectangle;
			if ((item.Button.Style == ToolBarButtonStyle.DropDownButton) && (item.Button.Parent.DropDownArrows) && is_flat)
				rect.Width -= ToolBarDropDownWidth;

			CPDrawBorder3D (dc, rect, style, all_sides);
		}

		protected virtual void DrawToolBarSeparator (Graphics dc, ToolBarItem item)
		{
			Rectangle area = item.Rectangle;
			int offset = (int) SystemPens.Control.Width + 1;
			dc.DrawLine (SystemPens.ControlDark, area.X + 1, area.Y, area.X + 1, area.Bottom);
			dc.DrawLine (SystemPens.ControlLight, area.X + offset, area.Y, area.X + offset, area.Bottom);
		}

		protected virtual void DrawToolBarToggleButtonBackground (Graphics dc, ToolBarItem item)
		{
			Brush brush;
			Rectangle area = item.Rectangle;
			area.X += ToolBarImageGripWidth;
			area.Y += ToolBarImageGripWidth;
			area.Width -= 2 * ToolBarImageGripWidth;
			area.Height -= 2 * ToolBarImageGripWidth;
			
			if (item.Button.Pushed)
				brush = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorScrollBar, ColorControlLightLight);
			else if (item.Button.PartialPush)
				brush = SystemBrushes.ControlLight;
			else
				brush = SystemBrushes.Control;
			 
			dc.FillRectangle (brush, area);
		}

		protected virtual void DrawToolBarDropDownArrow (Graphics dc, ToolBarItem item, bool is_flat)
		{
			Rectangle rect = item.Rectangle;
			rect.X = item.Rectangle.Right - ToolBarDropDownWidth;
			rect.Width = ToolBarDropDownWidth;
			
			if (is_flat) {
				if (item.DDPressed)
					CPDrawBorder3D (dc, rect, Border3DStyle.SunkenOuter, all_sides);
				else if (item.Button.Pushed || item.Pressed)
					CPDrawBorder3D (dc, rect, Border3DStyle.SunkenOuter, all_sides);
				else if (item.Hilight)
					CPDrawBorder3D (dc, rect, Border3DStyle.RaisedInner, all_sides);
			} else {
				if (item.DDPressed)
					CPDrawBorder3D (dc, rect, Border3DStyle.Flat, all_sides);
				else if (item.Button.Pushed || item.Pressed)
					CPDrawBorder3D (dc, Rectangle.Inflate(rect, -1, -1), Border3DStyle.SunkenOuter, all_sides);
				else
					CPDrawBorder3D (dc, rect, Border3DStyle.Raised, all_sides);
			}
			
			PointF [] vertices = new PointF [3];
			PointF ddCenter = new PointF (rect.X + (rect.Width/2.0f), rect.Y + (rect.Height / 2));
			
			// Increase vertical and horizontal position by 1 when button is pressed
			if (item.Pressed || item.Button.Pushed || item.DDPressed) {
			    ddCenter.X += 1;
			    ddCenter.Y += 1;
			}
			
			vertices [0].X = ddCenter.X - ToolBarDropDownArrowWidth / 2.0f + 0.5f;
			vertices [0].Y = ddCenter.Y;
			vertices [1].X = ddCenter.X + ToolBarDropDownArrowWidth / 2.0f + 0.5f;
			vertices [1].Y = ddCenter.Y;
			vertices [2].X = ddCenter.X + 0.5f; // 0.5 is added for adjustment
			vertices [2].Y = ddCenter.Y + ToolBarDropDownArrowHeight;
			dc.FillPolygon (SystemBrushes.ControlText, vertices);
		}

		protected virtual void DrawToolBarButtonContents (Graphics dc, ToolBar control, ToolBarItem item, StringFormat format)
		{
			if (item.Button.Image != null) {
				int x = item.ImageRectangle.X + ToolBarImageGripWidth;
				int y = item.ImageRectangle.Y + ToolBarImageGripWidth;
				
				// Increase vertical and horizontal position by 1 when button is pressed
				if (item.Pressed || item.Button.Pushed) {
				    x += 1;
				    y += 1;
				}
				
				if (item.Button.Enabled)
					dc.DrawImage (item.Button.Image, x, y);
				else 
					CPDrawImageDisabled (dc, item.Button.Image, x, y, ColorControl);
			}

			Rectangle text_rect = item.TextRectangle;
			if (text_rect.Width <= 0 || text_rect.Height <= 0)
				return;

			if (item.Pressed || item.Button.Pushed) {
				text_rect.X += 1;
				text_rect.Y += 1;
			}
			
			if (item.Button.Enabled)
				dc.DrawString (item.Button.Text, control.Font, SystemBrushes.ControlText, text_rect, format);
			else
				CPDrawStringDisabled (dc, item.Button.Text, control.Font, control.BackColor, text_rect, format);
		}

		// Grip width for the ToolBar
		public override int ToolBarGripWidth {
			get { return 2;}
		}

		// Grip width for the Image on the ToolBarButton
		public override int ToolBarImageGripWidth {
			get { return 2;}
		}

		// width of the separator
		public override int ToolBarSeparatorWidth {
			get { return 4; }
		}

		// width of the dropdown arrow rect
		public override int ToolBarDropDownWidth {
			get { return 13; }
		}

		// width for the dropdown arrow on the ToolBarButton
		public override int ToolBarDropDownArrowWidth {
			get { return 5;}
		}

		// height for the dropdown arrow on the ToolBarButton
		public override int ToolBarDropDownArrowHeight {
			get { return 3;}
		}

		public override Size ToolBarDefaultSize {
			get {
				return new Size (100, 42);
			}
		}

		public override bool ToolBarHasHotElementStyles (ToolBar toolBar)
		{
			return toolBar.Appearance == ToolBarAppearance.Flat;
		}

		public override bool ToolBarHasHotCheckedElementStyles {
			get {
				return false;
			}
		}
		#endregion	// ToolBar

		#region ToolTip
		public override void DrawToolTip(Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control)
		{
			ToolTipDrawBackground (dc, clip_rectangle, control);

			TextFormatFlags flags = TextFormatFlags.HidePrefix;
#if NET_2_0
			Color foreground = control.ForeColor;
			if (control.title.Length > 0) {
				Font bold_font = new Font (control.Font, control.Font.Style | FontStyle.Bold);
				TextRenderer.DrawTextInternal (dc, control.title, bold_font, control.title_rect,
						foreground, flags, false);
				bold_font.Dispose ();
			}

			if (control.icon != null)
				dc.DrawIcon (control.icon, control.icon_rect);
#else
			Color foreground = this.ColorInfoText;
#endif

			TextRenderer.DrawTextInternal (dc, control.Text, control.Font, control.text_rect, foreground, flags, false);
		}

		protected virtual void ToolTipDrawBackground (Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control)
		{
#if NET_2_0
			Brush back_brush = ResPool.GetSolidBrush (control.BackColor);;
#else
			Brush back_brush = SystemBrushes.Info;
#endif
			dc.FillRectangle (back_brush, control.ClientRectangle);
			dc.DrawRectangle (SystemPens.WindowFrame, 0, 0, control.Width - 1, control.Height - 1);
		}

		public override Size ToolTipSize(ToolTip.ToolTipWindow tt, string text)
		{
			Size size = TextRenderer.MeasureTextInternal (text, tt.Font, false);
			size.Width += 4;
			size.Height += 3;
			Rectangle text_rect = new Rectangle (Point.Empty, size);
			text_rect.Inflate (-2, -1);
			tt.text_rect = text_rect;
#if NET_2_0
			tt.icon_rect = tt.title_rect = Rectangle.Empty;

			Size title_size = Size.Empty;
			if (tt.title.Length > 0) {
				Font bold_font = new Font (tt.Font, tt.Font.Style | FontStyle.Bold);
				title_size = TextRenderer.MeasureTextInternal (tt.title, bold_font, false);
				bold_font.Dispose ();
			}

			Size icon_size = Size.Empty;
			if (tt.icon != null)
				icon_size = new Size (size.Height, size.Height);

			if (icon_size != Size.Empty || title_size != Size.Empty) {
				int padding = 8;
				int top_area_width = 0;
				int top_area_height = icon_size.Height > title_size.Height ? icon_size.Height : title_size.Height;
				Size text_size = size;
				Point location = new Point (padding, padding);

				if (icon_size != Size.Empty) {
					tt.icon_rect = new Rectangle (location, icon_size);
					top_area_width = icon_size.Width + padding;
				}

				if (title_size != Size.Empty) {
					Rectangle title_rect = new Rectangle (location, new Size (title_size.Width, top_area_height));
					if (icon_size != Size.Empty)
						title_rect.X += icon_size.Width + padding;

					tt.title_rect = title_rect;
					top_area_width += title_size.Width;
				}

				tt.text_rect = new Rectangle (new Point (location.X, location.Y + top_area_height + padding),
						text_size);

				size.Height += padding + top_area_height;
				if (top_area_width > size.Width)
					size.Width = top_area_width;

				// margins
				size.Width += padding * 2;
				size.Height += padding * 2;
			}
#endif

			return size;
		}
		
		public override bool ToolTipTransparentBackground {
			get {
				return false;
			}
 		}
		#endregion	// ToolTip

		#region BalloonWindow
#if NET_2_0
		NotifyIcon.BalloonWindow balloon_window;
		
		public override void ShowBalloonWindow (IntPtr handle, int timeout, string title, string text, ToolTipIcon icon)
		{
			Control control = Control.FromHandle(handle);
			
			if (control == null)
				return;

			if (balloon_window != null) {
				balloon_window.Close ();
				balloon_window.Dispose ();
			}

			balloon_window = new NotifyIcon.BalloonWindow (handle);
			balloon_window.Title = title;
			balloon_window.Text = text;
			balloon_window.Icon = icon;
			balloon_window.Timeout = timeout;
			balloon_window.Show ();
		}

		public override void HideBalloonWindow (IntPtr handle)
		{
			if (balloon_window == null || balloon_window.OwnerHandle != handle)
				return;

			balloon_window.Close ();
			balloon_window.Dispose ();
			balloon_window = null;
		}

		private const int balloon_iconsize = 16;
		private const int balloon_bordersize = 8; 
		
		public override void DrawBalloonWindow (Graphics dc, Rectangle clip, NotifyIcon.BalloonWindow control) 
		{
			Brush solidbrush = ResPool.GetSolidBrush (this.ColorInfoText);
			Rectangle rect = control.ClientRectangle;
			int iconsize = (control.Icon == ToolTipIcon.None) ? 0 : balloon_iconsize;
			
			// Rectangle borders and background.
			dc.FillRectangle (ResPool.GetSolidBrush (ColorInfo), rect);
			dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame), 0, 0, rect.Width - 1, rect.Height - 1);

			// Icon
			Image image;
			switch (control.Icon) {
				case ToolTipIcon.Info: {
					image = ThemeEngine.Current.Images(UIIcon.MessageBoxInfo, balloon_iconsize);
					break;
				}

				case ToolTipIcon.Warning: {
					image = ThemeEngine.Current.Images(UIIcon.MessageBoxError, balloon_iconsize);
					break;
				}

				case ToolTipIcon.Error: {
					image = ThemeEngine.Current.Images(UIIcon.MessageBoxWarning, balloon_iconsize);
					break;
				}
				
				default: {
					image = null;
					break;
				}
			}

			if (control.Icon != ToolTipIcon.None)
				dc.DrawImage (image, new Rectangle (balloon_bordersize, balloon_bordersize, iconsize, iconsize));
			
			// Title
			Rectangle titlerect = new Rectangle (rect.X + balloon_bordersize + iconsize + (iconsize > 0 ? balloon_bordersize : 0), 
												rect.Y + balloon_bordersize, 
												rect.Width - ((3 * balloon_bordersize) + iconsize), 
												rect.Height - (2 * balloon_bordersize));
			
			Font titlefont = new Font (control.Font.FontFamily, control.Font.Size, control.Font.Style | FontStyle.Bold, control.Font.Unit);
			dc.DrawString (control.Title, titlefont, solidbrush, titlerect, control.Format);
			
			// Text
			Rectangle textrect = new Rectangle (rect.X + balloon_bordersize, 
												rect.Y + balloon_bordersize, 
												rect.Width - (2 * balloon_bordersize), 
												rect.Height - (2 * balloon_bordersize));

			StringFormat textformat = control.Format;
			textformat.LineAlignment = StringAlignment.Far;
			dc.DrawString (control.Text, control.Font, solidbrush, textrect, textformat);
		}

		public override Rectangle BalloonWindowRect (NotifyIcon.BalloonWindow control)
		{
			Rectangle deskrect = Screen.GetWorkingArea (control);
			SizeF maxsize = new SizeF (250, 200);

			SizeF titlesize = TextRenderer.MeasureString (control.Title, control.Font, maxsize, control.Format);
			SizeF textsize = TextRenderer.MeasureString (control.Text, control.Font, maxsize, control.Format);
			
			if (titlesize.Height < balloon_iconsize)
				titlesize.Height = balloon_iconsize;
			
			Rectangle rect = new Rectangle ();
			rect.Height = (int) (titlesize.Height + textsize.Height + (3 * balloon_bordersize));
			rect.Width = (int) ((titlesize.Width > textsize.Width) ? titlesize.Width : textsize.Width) + (2 * balloon_bordersize);
			rect.X = deskrect.Width - rect.Width - 2;
			rect.Y = deskrect.Height - rect.Height - 2;
			
			return rect;
		}
#endif
		#endregion	// BalloonWindow

		#region	TrackBar
		public override int TrackBarValueFromMousePosition (int x, int y, TrackBar tb)
		{
			int result = tb.Value;
			int value_pos = tb.Value;
			float pixels_betweenticks;
			Rectangle thumb_pos = Rectangle.Empty, thumb_area = Rectangle.Empty;
			Point channel_startpoint = Point.Empty, na_point = Point.Empty;
			
			GetTrackBarDrawingInfo (tb, out pixels_betweenticks, out thumb_area, out thumb_pos, out channel_startpoint, out na_point, out na_point);
			
			/* Convert thumb position from mouse position to value*/
			if (tb.Orientation == Orientation.Vertical) {
				value_pos = (int)Math.Round (((thumb_area.Bottom - y - (float)thumb_pos.Height / 2) / (float)pixels_betweenticks), 0);

				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
				else if (value_pos + tb.Minimum < tb.Minimum)
					value_pos = 0;

				result = value_pos + tb.Minimum;
			} else {
				value_pos = (int)Math.Round (((x - channel_startpoint.X - (float)thumb_pos.Width / 2) / (float) pixels_betweenticks), 0);

				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
				else if (value_pos + tb.Minimum < tb.Minimum)
					value_pos = 0;

				result = value_pos + tb.Minimum;
			}
			
			return result;
		}
		
		private void GetTrackBarDrawingInfo (TrackBar tb, out float pixels_betweenticks, out Rectangle thumb_area, out Rectangle thumb_pos, out Point channel_startpoint, out Point bottomtick_startpoint, out Point toptick_startpoint)
		{
			thumb_area = Rectangle.Empty;
			thumb_pos = Rectangle.Empty;
			
			if (tb.Orientation == Orientation.Vertical) {
				toptick_startpoint = new Point ();
				bottomtick_startpoint = new Point ();
				channel_startpoint = new Point ();
				float pixel_len;
				const int space_from_right = 8;
				const int space_from_left = 8;
				const int space_from_bottom = 11;
				Rectangle area = tb.ClientRectangle;

				switch (tb.TickStyle) {
				case TickStyle.BottomRight:
				case TickStyle.None:
					channel_startpoint.Y = 8;
					channel_startpoint.X = 9;
					bottomtick_startpoint.Y = 13;
					bottomtick_startpoint.X = 24;
					break;
				case TickStyle.TopLeft:
					channel_startpoint.Y = 8;
					channel_startpoint.X = 19;
					toptick_startpoint.Y = 13;
					toptick_startpoint.X = 8;
					break;
				case TickStyle.Both:
					channel_startpoint.Y = 8;
					channel_startpoint.X = 18;
					bottomtick_startpoint.Y = 13;
					bottomtick_startpoint.X = 32;
					toptick_startpoint.Y = 13;
					toptick_startpoint.X = 8;
					break;
				default:
					break;
				}

				thumb_area.X = area.X + channel_startpoint.X;
				thumb_area.Y = area.Y + channel_startpoint.Y;
				thumb_area.Height = area.Height - space_from_right - space_from_left;
				thumb_area.Width = TrackBarVerticalTrackWidth;

				pixel_len = thumb_area.Height - 11;
				if (tb.Maximum == tb.Minimum) {
					pixels_betweenticks = 0;
				} else {
					pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
				}

				thumb_pos.Y = thumb_area.Bottom - space_from_bottom - (int)(pixels_betweenticks * (float)(tb.Value - tb.Minimum));
			} else {	
				toptick_startpoint = new Point ();
				bottomtick_startpoint = new Point ();
				channel_startpoint = new Point ();
				float pixel_len;
				const int space_from_right = 8;
				const int space_from_left = 8;
				Rectangle area = tb.ClientRectangle;
							
				switch (tb.TickStyle) {
				case TickStyle.BottomRight:
				case TickStyle.None:
					channel_startpoint.X = 8;
					channel_startpoint.Y = 9;
					bottomtick_startpoint.X = 13;
					bottomtick_startpoint.Y = 24;				
					break;
				case TickStyle.TopLeft:
					channel_startpoint.X = 8;
					channel_startpoint.Y = 19;
					toptick_startpoint.X = 13;
					toptick_startpoint.Y = 8;
					break;
				case TickStyle.Both:
					channel_startpoint.X = 8;
					channel_startpoint.Y = 18;	
					bottomtick_startpoint.X = 13;
					bottomtick_startpoint.Y = 32;				
					toptick_startpoint.X = 13;
					toptick_startpoint.Y = 8;				
					break;
				default:
					break;
				}
							
				thumb_area.X = area.X + channel_startpoint.X;
				thumb_area.Y = area.Y + channel_startpoint.Y;
				thumb_area.Width = area.Width - space_from_right - space_from_left;
				thumb_area.Height = TrackBarHorizontalTrackHeight;

				pixel_len = thumb_area.Width - 11;
				if (tb.Maximum == tb.Minimum) {
					pixels_betweenticks = 0;
				} else {
					pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
				}

				thumb_pos.X = channel_startpoint.X + (int)(pixels_betweenticks * (float) (tb.Value - tb.Minimum));
			}

			thumb_pos.Size = TrackBarGetThumbSize (tb);
		}

		protected virtual Size TrackBarGetThumbSize (TrackBar trackBar)
		{
			return TrackBarGetThumbSize ();
		}

		public static Size TrackBarGetThumbSize ()
		{
			/* Draw thumb fixed 10x22 size */
			return new Size (10, 22);
		}

		public const int TrackBarVerticalTrackWidth = 4;

		public const int TrackBarHorizontalTrackHeight = 4;

		#region Ticks
		protected interface ITrackBarTickPainter
		{
			void Paint (float x1, float y1, float x2, float y2);
		}

		class TrackBarTickPainter : ITrackBarTickPainter
		{
			readonly Graphics g;
			readonly Pen pen;
			public TrackBarTickPainter (Graphics g, Pen pen)
			{
				this.g = g;
				this.pen = pen;
			}
			public void Paint (float x1, float y1, float x2, float y2)
			{
				g.DrawLine (pen, x1, y1, x2, y2);
			}
		}
		protected virtual ITrackBarTickPainter GetTrackBarTickPainter (Graphics g)
		{
			return new TrackBarTickPainter (g, ResPool.GetPen (pen_ticks_color));
		}
		#endregion

		#region DrawTrackBar_Vertical
		private void DrawTrackBar_Vertical (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area,  Brush br_thumb,
			float ticks, int value_pos, bool mouse_value) {			

			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			Rectangle area = tb.ClientRectangle;
			
			GetTrackBarDrawingInfo (tb, out pixels_betweenticks, out thumb_area, out thumb_pos, out channel_startpoint, out bottomtick_startpoint, out toptick_startpoint);

			#region Track
			TrackBarDrawVerticalTrack (dc, thumb_area, channel_startpoint, clip_rectangle);
			#endregion

			#region Thumb
			switch (tb.TickStyle) 	{
			case TickStyle.BottomRight:
			case TickStyle.None:
				thumb_pos.X = channel_startpoint.X - 8;
				TrackBarDrawVerticalThumbRight (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			case TickStyle.TopLeft:
				thumb_pos.X = channel_startpoint.X - 10;
				TrackBarDrawVerticalThumbLeft (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			default:
				thumb_pos.X = area.X + 10;
				TrackBarDrawVerticalThumb (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			}
			#endregion

			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			thumb_area.X = thumb_pos.X;
			thumb_area.Y = channel_startpoint.Y;
			thumb_area.Width = thumb_pos.Height;

			#region Ticks
			if (pixels_betweenticks <= 0)
				return;
			if (tb.TickStyle == TickStyle.None)
				return;
			Region outside = new Region (area);
			outside.Exclude (thumb_area);			
			
			if (outside.IsVisible (clip_rectangle)) {
				ITrackBarTickPainter tick_painter = TrackBarGetVerticalTickPainter (dc);

				if ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight) {
					float x = area.X + bottomtick_startpoint.X;
					for (float inc = 0; inc < pixel_len + 1; inc += pixels_betweenticks) 	{
						float y = area.Y + bottomtick_startpoint.Y + inc;
						tick_painter.Paint (
							x, y,
							x + (inc == 0 || inc + pixels_betweenticks >= pixel_len + 1 ? 3 : 2), y);
					}
				}

				if ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft) {
					float x = area.X + toptick_startpoint.X; 
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						float y = area.Y + toptick_startpoint.Y + inc;
						tick_painter.Paint (
							x - (inc == 0 || inc + pixels_betweenticks >= pixel_len + 1 ? 3 : 2), y,
							x, y);
					}			
				}
			}
			
			outside.Dispose ();
			#endregion
		}

		#region Track
		protected virtual void TrackBarDrawVerticalTrack (Graphics dc, Rectangle thumb_area, Point channel_startpoint, Rectangle clippingArea)
		{
			dc.FillRectangle (SystemBrushes.ControlDark, channel_startpoint.X, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (SystemBrushes.ControlDarkDark, channel_startpoint.X + 1, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (SystemBrushes.ControlLight, channel_startpoint.X + 3, channel_startpoint.Y,
				1, thumb_area.Height);
		}
		#endregion

		#region Thumb
		protected virtual void TrackBarDrawVerticalThumbRight (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 16, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X + 16, thumb_pos.Y, thumb_pos.X + 16 + 4, thumb_pos.Y + 4);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X + 15, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X + 16, thumb_pos.Y + 9, thumb_pos.X + 16 + 4, thumb_pos.Y + 9 - 4);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X + 16, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X + 16, thumb_pos.Y + 10, thumb_pos.X + 16 + 5, thumb_pos.Y + 10 - 5);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 16, 8);
			dc.FillRectangle (br_thumb, thumb_pos.X + 17, thumb_pos.Y + 2, 1, 6);
			dc.FillRectangle (br_thumb, thumb_pos.X + 18, thumb_pos.Y + 3, 1, 4);
			dc.FillRectangle (br_thumb, thumb_pos.X + 19, thumb_pos.Y + 4, 1, 2);
		}

		protected virtual void TrackBarDrawVerticalThumbLeft (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X + 4 + 16, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 4);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 9, thumb_pos.X + 4 + 16, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 9, thumb_pos.X, thumb_pos.Y + 5);
			dc.DrawLine (pen, thumb_pos.X + 19, thumb_pos.Y + 9, thumb_pos.X + 19, thumb_pos.Y + 1);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 10, thumb_pos.X + 4 + 16, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X + 4, thumb_pos.Y + 10, thumb_pos.X - 1, thumb_pos.Y + 5);
			dc.DrawLine (pen, thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X + 20, thumb_pos.Y + 10);

			dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 15, 8);
			dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 1, 6);
			dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 1, 4);
			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 1, 2);
		}

		protected virtual void TrackBarDrawVerticalThumb (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 19, thumb_pos.Y);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X + 19, thumb_pos.Y + 9);
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 1, thumb_pos.X + 19, thumb_pos.Y + 8);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X + 20, thumb_pos.Y + 10);
			dc.DrawLine (pen, thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X + 20, thumb_pos.Y + 9);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 18, 8);
		}
		#endregion

		#region Ticks
		protected virtual ITrackBarTickPainter TrackBarGetVerticalTickPainter (Graphics g)
		{
			return GetTrackBarTickPainter (g);
		}
		#endregion
		#endregion

		#region DrawTrackBar_Horizontal
		/* 
			Horizontal trackbar 
		  
			Does not matter the size of the control, Win32 always draws:
				- Ticks starting from pixel 13, 8
				- Channel starting at pos 8, 19 and ends at Width - 8
				- Autosize makes always the control 45 pixels high
				- Ticks are draw at (channel.Witdh - 10) / (Maximum - Minimum)
				
		*/
		private void DrawTrackBar_Horizontal (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area, Brush br_thumb,
			float ticks, int value_pos, bool mouse_value) {			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			Rectangle area = tb.ClientRectangle;
			
			GetTrackBarDrawingInfo (tb , out pixels_betweenticks, out thumb_area, out thumb_pos, out channel_startpoint, out bottomtick_startpoint, out toptick_startpoint);

			#region Track
			TrackBarDrawHorizontalTrack (dc, thumb_area, channel_startpoint, clip_rectangle);
			#endregion

			#region Thumb
			switch (tb.TickStyle) {
			case TickStyle.BottomRight:
			case TickStyle.None:
				thumb_pos.Y = channel_startpoint.Y - 8;
				TrackBarDrawHorizontalThumbBottom (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			case TickStyle.TopLeft:
				thumb_pos.Y = channel_startpoint.Y - 10;
				TrackBarDrawHorizontalThumbTop (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			default:
				thumb_pos.Y = area.Y + 10;
				TrackBarDrawHorizontalThumb (dc, thumb_pos, br_thumb, clip_rectangle, tb);
				break;
			}
			#endregion

			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ticks;

			thumb_area.Y = thumb_pos.Y;
			thumb_area.X = channel_startpoint.X;
			thumb_area.Height = thumb_pos.Height;
			#region Ticks
			if (pixels_betweenticks <= 0)
				return;
			if (tb.TickStyle == TickStyle.None)
				return;
			Region outside = new Region (area);
			outside.Exclude (thumb_area);

			if (outside.IsVisible (clip_rectangle)) {
				ITrackBarTickPainter tick_painter = TrackBarGetHorizontalTickPainter (dc);

				if ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight) {
					float y = area.Y + bottomtick_startpoint.Y;
					for (float inc = 0; inc < pixel_len + 1; inc += pixels_betweenticks) {					
						float x = area.X + bottomtick_startpoint.X + inc;
						tick_painter.Paint (
							x, y, 
							x, y + (inc == 0 || inc + pixels_betweenticks >= pixel_len + 1 ? 3 : 2));
					}
				}

				if ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft) {
					float y = area.Y + toptick_startpoint.Y;
					for (float inc = 0; inc < pixel_len + 1; inc += pixels_betweenticks) {					
						float x = area.X + toptick_startpoint.X + inc;
						tick_painter.Paint (
							x, y - (inc == 0 || (inc + pixels_betweenticks) >= pixel_len + 1 ? 3 : 2), 
							x, y);
					}			
				}
			}
			
			outside.Dispose ();
			#endregion
		}

		#region Track
		protected virtual void TrackBarDrawHorizontalTrack (Graphics dc, Rectangle thumb_area, Point channel_startpoint, Rectangle clippingArea)
		{
			dc.FillRectangle (SystemBrushes.ControlDark, channel_startpoint.X, channel_startpoint.Y,
				thumb_area.Width, 1);

			dc.FillRectangle (SystemBrushes.ControlDarkDark, channel_startpoint.X, channel_startpoint.Y + 1,
				thumb_area.Width, 1);

			dc.FillRectangle (SystemBrushes.ControlLight, channel_startpoint.X, channel_startpoint.Y + 3,
				thumb_area.Width, 1);
		}
		#endregion

		#region Thumb
		protected virtual void TrackBarDrawHorizontalThumbBottom (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 16);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 16, thumb_pos.X + 4, thumb_pos.Y + 16 + 4);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 15);
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 16, thumb_pos.X + 9 - 4, thumb_pos.Y + 16 + 4);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y + 16);
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 16, thumb_pos.X + 10 - 5, thumb_pos.Y + 16 + 5);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 16);
			dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 17, 6, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 18, 4, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 19, 2, 1);
		}

		protected virtual void TrackBarDrawHorizontalThumbTop (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X, thumb_pos.Y + 4 + 16);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X + 4, thumb_pos.Y);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 9, thumb_pos.Y + 4 + 16);
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 19, thumb_pos.X + 1, thumb_pos.Y + 19);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 10, thumb_pos.Y + 4 + 16);
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y - 1);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 10, thumb_pos.Y + 20);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 8, 15);
			dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 6, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 4, 1);
			dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 2, 1);
		}

		protected virtual void TrackBarDrawHorizontalThumb (Graphics dc, Rectangle thumb_pos, Brush br_thumb, Rectangle clippingArea, TrackBar trackBar)
		{
			Pen pen = SystemPens.ControlLightLight;
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 9, thumb_pos.Y);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 19);

			pen = SystemPens.ControlDark;
			dc.DrawLine (pen, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 19);
			dc.DrawLine (pen, thumb_pos.X + 1, thumb_pos.Y + 10, thumb_pos.X + 8, thumb_pos.Y + 19);

			pen = SystemPens.ControlDarkDark;
			dc.DrawLine (pen, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y + 20);
			dc.DrawLine (pen, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 9, thumb_pos.Y + 20);

			dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 18);
		}
		#endregion

		#region Ticks
		protected virtual ITrackBarTickPainter TrackBarGetHorizontalTickPainter (Graphics g)
		{
			return GetTrackBarTickPainter (g);
		}
		#endregion
		#endregion

		public override void DrawTrackBar (Graphics dc, Rectangle clip_rectangle, TrackBar tb) 
		{
			Brush		br_thumb;
			int		value_pos;
			bool		mouse_value;
			float		ticks = (tb.Maximum - tb.Minimum) / tb.tickFrequency; /* N of ticks draw*/
			Rectangle	area;
			Rectangle	thumb_pos = tb.ThumbPos;
			Rectangle	thumb_area = tb.ThumbArea;
			
			if (tb.thumb_pressed) {
				value_pos = tb.thumb_mouseclick;
				mouse_value = true;
			} else {
				value_pos = tb.Value - tb.Minimum;
				mouse_value = false;
			}

			area = tb.ClientRectangle;

			if (!tb.Enabled) {
				br_thumb = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLightLight, ColorControlLight);
			} else if (tb.thumb_pressed == true) {
				br_thumb = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl);
			} else {
				br_thumb = SystemBrushes.Control;
			}

			
			/* Control Background */
			if (tb.BackColor.ToArgb () == DefaultControlBackColor.ToArgb ()) {
				dc.FillRectangle (SystemBrushes.Control, clip_rectangle);
			} else {
				dc.FillRectangle (ResPool.GetSolidBrush (tb.BackColor), clip_rectangle);
			}
			
			if (tb.Focused) {
				CPDrawFocusRectangle(dc, area, tb.ForeColor, tb.BackColor);
			}

			if (tb.Orientation == Orientation.Vertical) {
				DrawTrackBar_Vertical (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			
			} else {
				DrawTrackBar_Horizontal (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			}

			tb.ThumbPos = thumb_pos;
			tb.ThumbArea = thumb_area;
		}

		public override Size TrackBarDefaultSize {
			get {
				return new Size (104, 42);
			}
		}

		public override bool TrackBarHasHotThumbStyle {
			get {
				return false;
			}
		}
		#endregion	// TrackBar

		#region UpDownBase
		public override void UpDownBaseDrawButton (Graphics g, Rectangle bounds, bool top, VisualStyles.PushButtonState state)
		{
			ControlPaint.DrawScrollButton (g, bounds, top ? ScrollButton.Up : ScrollButton.Down, state == VisualStyles.PushButtonState.Pressed ? ButtonState.Pushed : ButtonState.Normal);
		}

		public override bool UpDownBaseHasHotButtonStyle {
			get {
				return false;
			}
		}
		#endregion

		#region	VScrollBar
		public override Size VScrollBarDefaultSize {
			get {
				return new Size (this.ScrollBarButtonSize, 80);
			}
		}
		#endregion	// VScrollBar

		#region TreeView
		public override Size TreeViewDefaultSize {
			get {
				return new Size (121, 97);
			}
		}

		public override void TreeViewDrawNodePlusMinus (TreeView treeView, TreeNode node, Graphics dc, int x, int middle)
		{
			int height = treeView.ActualItemHeight - 2;
			dc.FillRectangle (ResPool.GetSolidBrush (treeView.BackColor), (x + 4) - (height / 2), node.GetY() + 1, height, height);
			
			dc.DrawRectangle (SystemPens.ControlDarkDark, x, middle - 4, 8, 8);

			if (node.IsExpanded) {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
			} else {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
				dc.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
			}
		}
		#endregion

		#region Managed window
		public override int ManagedWindowTitleBarHeight (InternalWindowManager wm)
		{
			if (wm.IsToolWindow && !wm.IsMinimized)
				return SystemInformation.ToolWindowCaptionHeight;
			if (wm.Form.FormBorderStyle == FormBorderStyle.None)
				return 0;
			return SystemInformation.CaptionHeight;
		}

		public override int ManagedWindowBorderWidth (InternalWindowManager wm)
		{
			if ((wm.IsToolWindow && wm.form.FormBorderStyle == FormBorderStyle.FixedToolWindow) ||
				wm.IsMinimized)
				return 3;
			else
				return 4;
		}

		public override int ManagedWindowIconWidth (InternalWindowManager wm)
		{
			return ManagedWindowTitleBarHeight (wm) - 5;
		}

		public override void ManagedWindowSetButtonLocations (InternalWindowManager wm)
		{
			TitleButtons buttons = wm.TitleButtons;
			Form form = wm.form;
			
			buttons.HelpButton.Visible = form.HelpButton;
			
			foreach (TitleButton button in buttons) {
				button.Visible = false;
			}
			
			switch (form.FormBorderStyle) {
			case FormBorderStyle.None:
				if (form.WindowState != FormWindowState.Normal)
					goto case FormBorderStyle.Sizable;
				break;
			case FormBorderStyle.FixedToolWindow:
			case FormBorderStyle.SizableToolWindow:
				buttons.CloseButton.Visible = true;
				if (form.WindowState != FormWindowState.Normal)
					goto case FormBorderStyle.Sizable;
				break;
			case FormBorderStyle.FixedSingle:
			case FormBorderStyle.Fixed3D:
			case FormBorderStyle.FixedDialog:
			case FormBorderStyle.Sizable:
				switch (form.WindowState) {
					case FormWindowState.Normal:
						buttons.MinimizeButton.Visible = true;
						buttons.MaximizeButton.Visible = true;
						buttons.RestoreButton.Visible = false;
						break;
					case FormWindowState.Maximized:
						buttons.MinimizeButton.Visible = true;
						buttons.MaximizeButton.Visible = false;
						buttons.RestoreButton.Visible = true;
						break;
					case FormWindowState.Minimized:
						buttons.MinimizeButton.Visible = false;
						buttons.MaximizeButton.Visible = true;
						buttons.RestoreButton.Visible = true;
						break;
				}
				buttons.CloseButton.Visible = true;
				break;
			}

			// Respect MinimizeBox/MaximizeBox
			if (form.MinimizeBox == false && form.MaximizeBox == false) {
				buttons.MinimizeButton.Visible = false;
				buttons.MaximizeButton.Visible = false;
			} else if (form.MinimizeBox == false)
				buttons.MinimizeButton.State = ButtonState.Inactive;
			else if (form.MaximizeBox == false)
				buttons.MaximizeButton.State = ButtonState.Inactive;

			int bw = ManagedWindowBorderWidth (wm);
			Size btsize = ManagedWindowButtonSize (wm);
			int btw = btsize.Width;
			int bth = btsize.Height;
			int top = bw + 2;
			int left = form.Width - bw - btw - ManagedWindowSpacingAfterLastTitleButton;
			
			if ((!wm.IsToolWindow || wm.IsMinimized) && wm.HasBorders) {
				buttons.CloseButton.Rectangle = new Rectangle (left, top, btw, bth);
				left -= 2 + btw;
				
				if (buttons.MaximizeButton.Visible) {
					buttons.MaximizeButton.Rectangle = new Rectangle (left, top, btw, bth);
					left -= 2 + btw;
				} 
				if (buttons.RestoreButton.Visible) {
					buttons.RestoreButton.Rectangle = new Rectangle (left, top, btw, bth);
					left -= 2 + btw;
				}

				buttons.MinimizeButton.Rectangle = new Rectangle (left, top, btw, bth);
				left -= 2 + btw;
			} else if (wm.IsToolWindow) {
				buttons.CloseButton.Rectangle = new Rectangle (left, top, btw, bth);
				left -= 2 + btw;
			}
		}

		protected virtual Rectangle ManagedWindowDrawTitleBarAndBorders (Graphics dc, Rectangle clip, InternalWindowManager wm)
		{
			Form form = wm.Form;
			int tbheight = ManagedWindowTitleBarHeight (wm);
			int bdwidth = ManagedWindowBorderWidth (wm);
			Color titlebar_color = Color.FromArgb (255, 10, 36, 106);
			Color titlebar_color2 = Color.FromArgb (255, 166, 202, 240);
			Color color = ThemeEngine.Current.ColorControlDark;
			Color color2 = Color.FromArgb (255, 192, 192, 192);

			Pen pen = ResPool.GetPen (ColorControl);
			Rectangle borders = new Rectangle (0, 0, form.Width, form.Height);
			ControlPaint.DrawBorder3D (dc, borders, Border3DStyle.Raised);
			// The 3d border is only 2 pixels wide, so we draw the innermost pixels ourselves
			borders = new Rectangle (2, 2, form.Width - 5, form.Height - 5);
			for (int i = 2; i < bdwidth; i++) {
				dc.DrawRectangle (pen, borders);
				borders.Inflate (-1, -1);
			}				


			bool draw_titlebar_enabled = false;
			if (wm.Form.Parent != null && wm.Form.Parent is Form) {
				draw_titlebar_enabled = false;
			} else if (wm.IsActive && !wm.IsMaximized) {
				draw_titlebar_enabled = true;
			}
			if (draw_titlebar_enabled) {
				color = titlebar_color;
				color2 = titlebar_color2;
			}

			Rectangle tb = new Rectangle (bdwidth, bdwidth, form.Width - (bdwidth * 2), tbheight - 1);

			// HACK: For now always draw the titlebar until we get updates better
			if (tb.Width > 0 && tb.Height > 0) {
				using (System.Drawing.Drawing2D.LinearGradientBrush gradient = new LinearGradientBrush (tb, color, color2, LinearGradientMode.Horizontal))
				{
					dc.FillRectangle (gradient, tb);
				}	
			}
			
			if (!wm.IsMinimized)
				// Draw the line just beneath the title bar
				dc.DrawLine (ResPool.GetPen (SystemColors.Control), bdwidth,
						tbheight + bdwidth - 1, form.Width - bdwidth - 1,
						tbheight + bdwidth - 1);
			return tb;
		}

		public override void DrawManagedWindowDecorations (Graphics dc, Rectangle clip, InternalWindowManager wm)
		{
#if debug
			Console.WriteLine (DateTime.Now.ToLongTimeString () + " DrawManagedWindowDecorations");
			dc.FillRectangle (Brushes.Black, clip);
#endif
			Rectangle tb = ManagedWindowDrawTitleBarAndBorders (dc, clip, wm);

			Form form = wm.Form;
			if (wm.ShowIcon) {
				Rectangle icon = ManagedWindowGetTitleBarIconArea (wm);
				if (icon.IntersectsWith (clip))
					dc.DrawIcon (form.Icon, icon);
				const int SpacingBetweenIconAndCaption = 2;
				tb.Width -= icon.Right + SpacingBetweenIconAndCaption - tb.X ;
				tb.X = icon.Right + SpacingBetweenIconAndCaption;
			}
			
			foreach (TitleButton button in wm.TitleButtons.AllButtons) {
				tb.Width -= Math.Max (0, tb.Right - DrawTitleButton (dc, button, clip, form));
			}
			const int SpacingBetweenCaptionAndLeftMostButton = 3;
			tb.Width -= SpacingBetweenCaptionAndLeftMostButton;

			string window_caption = form.Text;
			window_caption = window_caption.Replace (Environment.NewLine, string.Empty);

			if (window_caption != null && window_caption != string.Empty) {
				StringFormat format = new StringFormat ();
				format.FormatFlags = StringFormatFlags.NoWrap;
				format.Trimming = StringTrimming.EllipsisCharacter;
				format.LineAlignment = StringAlignment.Center;

				if (tb.IntersectsWith (clip))
					dc.DrawString (window_caption, WindowBorderFont,
						ThemeEngine.Current.ResPool.GetSolidBrush (Color.White),
						tb, format);
			}
		}

		public override Size ManagedWindowButtonSize (InternalWindowManager wm)
		{
			int height = ManagedWindowTitleBarHeight (wm);
			if (!wm.IsMaximized && !wm.IsMinimized) {
				if (wm.IsToolWindow)
					return new Size (SystemInformation.ToolWindowCaptionButtonSize.Width - 2,
							height - 5);
				if (wm.Form.FormBorderStyle == FormBorderStyle.None)
					return Size.Empty;
			} else
				height = SystemInformation.CaptionHeight;

			return new Size (SystemInformation.CaptionButtonSize.Width - 2,
					height - 5);
		}

		private int DrawTitleButton (Graphics dc, TitleButton button, Rectangle clip, Form form)
		{
			if (!button.Visible) {
				return int.MaxValue;
			}
			
			if (button.Rectangle.IntersectsWith (clip)) {
				ManagedWindowDrawTitleButton (dc, button, clip, form);
			}
			return button.Rectangle.Left;
		}

		protected virtual void ManagedWindowDrawTitleButton (Graphics dc, TitleButton button, Rectangle clip, Form form)
		{
			dc.FillRectangle (SystemBrushes.Control, button.Rectangle);

			ControlPaint.DrawCaptionButton (dc, button.Rectangle,
					button.Caption, button.State);
		}

		public override Rectangle ManagedWindowGetTitleBarIconArea (InternalWindowManager wm)
		{
			int bw = ManagedWindowBorderWidth (wm);
			return new Rectangle (bw + 3, bw + 2, wm.IconWidth, wm.IconWidth);
		}

		public override Size ManagedWindowGetMenuButtonSize (InternalWindowManager wm)
		{
			Size result = SystemInformation.MenuButtonSize;
			result.Width -= 2;
			result.Height -= 4;
			return result;
		}

		public override bool ManagedWindowTitleButtonHasHotElementStyle (TitleButton button, Form form)
		{
			return false;
		}

		public override void ManagedWindowDrawMenuButton (Graphics dc, TitleButton button, Rectangle clip, InternalWindowManager wm)
		{
			dc.FillRectangle (SystemBrushes.Control, button.Rectangle);
			ControlPaint.DrawCaptionButton (dc, button.Rectangle,
					button.Caption, button.State);
		}

		public override void ManagedWindowOnSizeInitializedOrChanged (Form form)
		{
		}
		#endregion

		#region ControlPaint
		public override void CPDrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle) {
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public override void CPDrawBorder (Graphics graphics, RectangleF bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle) {
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
			CPDrawBorder3D(graphics, rectangle, style, sides, ColorControl);
		}

		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color)
		{
			Pen		penTopLeft;
			Pen		penTopLeftInner;
			Pen		penBottomRight;
			Pen		penBottomRightInner;
			Rectangle	rect= new Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			bool is_ColorControl = control_color.ToArgb () == ColorControl.ToArgb () ? true : false;
			
			if ((style & Border3DStyle.Adjust) != 0) {
				rect.Y -= 2;
				rect.X -= 2;
				rect.Width += 4;
				rect.Height += 4;
			}
			
			penTopLeft = penTopLeftInner = penBottomRight = penBottomRightInner = is_ColorControl ? SystemPens.Control : ResPool.GetPen (control_color);
			
			CPColor cpcolor = CPColor.Empty;
			
			if (!is_ColorControl)
				cpcolor = ResPool.GetCPColor (control_color);
			
			switch (style) {
			case Border3DStyle.Raised:
				penTopLeftInner = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				penBottomRight = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				penBottomRightInner = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				break;
			case Border3DStyle.Sunken:
				penTopLeft = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				penTopLeftInner = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				penBottomRight = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				break;
			case Border3DStyle.Etched:
				penTopLeft = penBottomRightInner = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				penTopLeftInner = penBottomRight = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				break;
			case Border3DStyle.RaisedOuter:
				penBottomRight = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				break;
			case Border3DStyle.SunkenOuter:
				penTopLeft = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				penBottomRight = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				break;
			case Border3DStyle.RaisedInner:
				penTopLeft = is_ColorControl ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
				penBottomRight = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				break;
			case Border3DStyle.SunkenInner:
				penTopLeft = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				break;
			case Border3DStyle.Flat:
				penTopLeft = penBottomRight = is_ColorControl ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
				break;
			case Border3DStyle.Bump:
				penTopLeftInner = penBottomRight = is_ColorControl ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
				break;
			default:
				break;
			}
			
			bool inner = ((style != Border3DStyle.RaisedOuter) && (style != Border3DStyle.SunkenOuter));
			
			if ((sides & Border3DSide.Middle) != 0) {
				Brush brush = is_ColorControl ? SystemBrushes.Control : ResPool.GetSolidBrush (control_color);
				graphics.FillRectangle (brush, rect);
			}
			
			if ((sides & Border3DSide.Left) != 0) {
				graphics.DrawLine (penTopLeft, rect.Left, rect.Bottom - 2, rect.Left, rect.Top);
				if ((rect.Width > 2) && inner)
					graphics.DrawLine (penTopLeftInner, rect.Left + 1, rect.Bottom - 2, rect.Left + 1, rect.Top);
			}
			
			if ((sides & Border3DSide.Top) != 0) {
				graphics.DrawLine (penTopLeft, rect.Left, rect.Top, rect.Right - 2, rect.Top);
				if ((rect.Height > 2) && inner)
					graphics.DrawLine (penTopLeftInner, rect.Left + 1, rect.Top + 1, rect.Right - 3, rect.Top + 1);
			}
			
			if ((sides & Border3DSide.Right) != 0) {
				graphics.DrawLine (penBottomRight, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				if ((rect.Width > 3) && inner)
					graphics.DrawLine (penBottomRightInner, rect.Right - 2, rect.Top + 1, rect.Right - 2, rect.Bottom - 2);
			}
			
			if ((sides & Border3DSide.Bottom) != 0) {
				graphics.DrawLine (penBottomRight, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				if ((rect.Height > 3) && inner)
					graphics.DrawLine (penBottomRightInner, rect.Left + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
			}
		}

		public override void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			CPDrawButtonInternal (dc, rectangle, state, SystemPens.ControlDarkDark, SystemPens.ControlDark, SystemPens.ControlLight);
		}

		private void CPDrawButtonInternal (Graphics dc, Rectangle rectangle, ButtonState state, Pen DarkPen, Pen NormalPen, Pen LightPen)
		{
			// sadly enough, the rectangle gets always filled with a hatchbrush
			dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50,
								 Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
										 ColorControl.G, ColorControl.B),
								 ColorControl),
					  rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);
			
			if ((state & ButtonState.All) == ButtonState.All || ((state & ButtonState.Checked) == ButtonState.Checked && (state & ButtonState.Flat) == ButtonState.Flat)) {
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl), rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4);
				
				dc.DrawRectangle (SystemPens.ControlDark, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
			} else
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				dc.DrawRectangle (SystemPens.ControlDark, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
			} else
			if ((state & ButtonState.Checked) == ButtonState.Checked) {
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl), rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4);
				
				Pen pen = DarkPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				
				pen = NormalPen;
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 3);
				dc.DrawLine (pen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
				
				pen = LightPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
				dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1);
			} else
			if (((state & ButtonState.Pushed) == ButtonState.Pushed) && ((state & ButtonState.Normal) == ButtonState.Normal)) {
				Pen pen = DarkPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				
				pen = NormalPen;
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 3);
				dc.DrawLine (pen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
				
				pen = LightPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
				dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1);
			} else
			if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Normal) == ButtonState.Normal)) {
				Pen pen = LightPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
				
				pen = NormalPen;
				dc.DrawLine (pen, rectangle.X + 1, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 2);
				dc.DrawLine (pen, rectangle.Right - 2, rectangle.Y + 1, rectangle.Right - 2, rectangle.Bottom - 3);
				
				pen = DarkPen;
				dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 2);
			}
		}


		public override void CPDrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state) {
			Rectangle	captionRect;
			int			lineWidth;

			CPDrawButtonInternal (graphics, rectangle, state, SystemPens.ControlDarkDark, SystemPens.ControlDark, SystemPens.ControlLightLight);

			if (rectangle.Width<rectangle.Height) {
				captionRect=new Rectangle(rectangle.X+1, rectangle.Y+rectangle.Height/2-rectangle.Width/2+1, rectangle.Width-4, rectangle.Width-4);
			} else {
				captionRect=new Rectangle(rectangle.X+rectangle.Width/2-rectangle.Height/2+1, rectangle.Y+1, rectangle.Height-4, rectangle.Height-4);
			}

			if ((state & ButtonState.Pushed)!=0) {
				captionRect=new Rectangle(rectangle.X+2, rectangle.Y+2, rectangle.Width-3, rectangle.Height-3);
			}

			/* Make sure we've got at least a line width of 1 */
			lineWidth=Math.Max(1, captionRect.Width/7);

			switch(button) {
			case CaptionButton.Close: {
				Pen	pen;

				if ((state & ButtonState.Inactive)!=0) {
					pen = ResPool.GetSizedPen (ColorControlLight, lineWidth);
					DrawCaptionHelper(graphics, ColorControlLight, pen, lineWidth, 1, captionRect, button);

					pen = ResPool.GetSizedPen (ColorControlDark, lineWidth);
					DrawCaptionHelper(graphics, ColorControlDark, pen, lineWidth, 0, captionRect, button);
					return;
				} else {
					pen = ResPool.GetSizedPen (ColorControlText, lineWidth);
					DrawCaptionHelper(graphics, ColorControlText, pen, lineWidth, 0, captionRect, button);
					return;
				}
			}

			case CaptionButton.Help:
			case CaptionButton.Maximize:
			case CaptionButton.Minimize:
			case CaptionButton.Restore: {
				if ((state & ButtonState.Inactive)!=0) {
					DrawCaptionHelper(graphics, ColorControlLight, SystemPens.ControlLightLight, lineWidth, 1, captionRect, button);

					DrawCaptionHelper(graphics, ColorControlDark, SystemPens.ControlDark, lineWidth, 0, captionRect, button);
					return;
				} else {
					DrawCaptionHelper(graphics, ColorControlText, SystemPens.ControlText, lineWidth, 0, captionRect, button);
					return;
				}
			}
			}
		}

		public override void CPDrawCheckBox (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			Pen check_pen = Pens.Black;
			
			Rectangle cb_rect = new Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			
			if ((state & ButtonState.All) == ButtonState.All) {
				cb_rect.Width -= 2;
				cb_rect.Height -= 2;
				
				dc.FillRectangle (SystemBrushes.Control, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				dc.DrawRectangle (SystemPens.ControlDark, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				
				check_pen = SystemPens.ControlDark;
			} else
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				cb_rect.Width -= 2;
				cb_rect.Height -= 2;
				
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					dc.FillRectangle (SystemBrushes.ControlLight, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				else
					dc.FillRectangle (Brushes.White, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
				dc.DrawRectangle (SystemPens.ControlDark, cb_rect.X, cb_rect.Y, cb_rect.Width - 1, cb_rect.Height - 1);
			} else {
				cb_rect.Width -= 1;
				cb_rect.Height -= 1;
				
				int check_box_visible_size = (cb_rect.Height > cb_rect.Width) ? cb_rect.Width : cb_rect.Height;
				
				int x_pos = Math.Max (0, cb_rect.X + (cb_rect.Width / 2) - check_box_visible_size / 2);
				int y_pos = Math.Max (0, cb_rect.Y + (cb_rect.Height / 2) - check_box_visible_size / 2);
				
				Rectangle rect = new Rectangle (x_pos, y_pos, check_box_visible_size, check_box_visible_size);
				
				if (((state & ButtonState.Pushed) == ButtonState.Pushed) || ((state & ButtonState.Inactive) == ButtonState.Inactive)) {
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50,
										 Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
												 ColorControl.G, ColorControl.B),
										 ColorControl), rect.X + 2, rect.Y + 2, rect.Width - 3, rect.Height - 3);
				} else
					dc.FillRectangle (SystemBrushes.ControlLightLight, rect.X + 2, rect.Y + 2, rect.Width - 3, rect.Height - 3);
				
				Pen pen = SystemPens.ControlDark;
				dc.DrawLine (pen, rect.X, rect.Y, rect.X, rect.Bottom - 1);
				dc.DrawLine (pen, rect.X + 1, rect.Y, rect.Right - 1, rect.Y);
				
				pen = SystemPens.ControlDarkDark;
				dc.DrawLine (pen, rect.X + 1, rect.Y + 1, rect.X + 1, rect.Bottom - 2);
				dc.DrawLine (pen, rect.X + 2, rect.Y + 1, rect.Right - 2, rect.Y + 1);
				
				pen = SystemPens.ControlLightLight;
				dc.DrawLine (pen, rect.Right, rect.Y, rect.Right, rect.Bottom);
				dc.DrawLine (pen, rect.X, rect.Bottom, rect.Right, rect.Bottom);
				
				// oh boy, matching ms is like fighting against windmills
				using (Pen h_pen = new Pen (ResPool.GetHatchBrush (HatchStyle.Percent50,
										   Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
												   ColorControl.G, ColorControl.B), ColorControl))) {
					dc.DrawLine (h_pen, rect.X + 1, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
					dc.DrawLine (h_pen, rect.Right - 1, rect.Y + 1, rect.Right - 1, rect.Bottom - 1);
				}
				
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					check_pen = SystemPens.ControlDark;
			}
			
			if ((state & ButtonState.Checked) == ButtonState.Checked) {
				int check_size = (cb_rect.Height > cb_rect.Width) ? cb_rect.Width / 2: cb_rect.Height / 2;
				
				if (check_size < 7) {
					int lineWidth = Math.Max (3, check_size / 3);
					int Scale = Math.Max (1, check_size / 9);
					
					Rectangle rect = new Rectangle (cb_rect.X + (cb_rect.Width / 2) - (int)Math.Ceiling ((float)check_size / 2) - 1, cb_rect.Y + (cb_rect.Height / 2) - (check_size / 2) - 1, 
									check_size, check_size);
					
					for (int i = 0; i < lineWidth; i++) {
						dc.DrawLine (check_pen, rect.Left + lineWidth / 2, rect.Top + lineWidth + i, rect.Left + lineWidth / 2 + 2 * Scale, rect.Top + lineWidth + 2 * Scale + i);
						dc.DrawLine (check_pen, rect.Left + lineWidth / 2 + 2 * Scale, rect.Top + lineWidth + 2 * Scale + i, rect.Left + lineWidth / 2 + 6 * Scale, rect.Top + lineWidth - 2 * Scale + i);
					}
				} else {
					int lineWidth = Math.Max (3, check_size / 3) + 1;
					
					int x_half = cb_rect.Width / 2;
					int y_half = cb_rect.Height / 2;
					
					Rectangle rect = new Rectangle (cb_rect.X + x_half - (check_size / 2) - 1, cb_rect.Y + y_half - (check_size / 2), 
									check_size, check_size);
					
					int gradient_left = check_size / 3;
					int gradient_right = check_size - gradient_left - 1;
					
					
					for (int i = 0; i < lineWidth; i++) {
						dc.DrawLine (check_pen, rect.X, rect.Bottom - 1 - gradient_left - i, rect.X + gradient_left, rect.Bottom - 1 - i);
						dc.DrawLine (check_pen, rect.X + gradient_left, rect.Bottom - 1 - i, rect.Right - 1, rect.Bottom - i  - 1 - gradient_right);
					}
				}
			}
		}

		public override void CPDrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			if ((state & ButtonState.Checked)!=0) {
				graphics.FillRectangle(ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLightLight, ColorControlLight),rectangle);				
			}

			if ((state & ButtonState.Flat)!=0) {
				ControlPaint.DrawBorder(graphics, rectangle, ColorControlDark, ButtonBorderStyle.Solid);
			} else {
				if ((state & (ButtonState.Pushed | ButtonState.Checked))!=0) {
					// this needs to render like a pushed button - jba
					// CPDrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
					Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
					graphics.DrawRectangle (SystemPens.ControlDark, trace_rectangle);
				} else {
					CPDrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
				}
			}

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left, centerY);
			P2=new Point(rect.Right, centerY);
			P3=new Point(centerX, rect.Bottom);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;
			
			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				/* Move away from the shadow */
				arrow[0].X += 1;	arrow[0].Y += 1;
				arrow[1].X += 1;	arrow[1].Y += 1;
				arrow[2].X += 1;	arrow[2].Y += 1;
				
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;

				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}


		public override void CPDrawContainerGrabHandle (Graphics graphics, Rectangle bounds)
		{
			Pen			pen	= Pens.Black;
			Rectangle	rect	= new Rectangle (bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);	// Dunno why, but MS does it that way, too
			int			X;
			int			Y;
			
			graphics.FillRectangle (SystemBrushes.ControlLightLight, rect);
			graphics.DrawRectangle (pen, rect);
			
			X = rect.X + rect.Width / 2;
			Y = rect.Y + rect.Height / 2;
			
			/* Draw the cross */
			graphics.DrawLine (pen, X, rect.Y + 2, X, rect.Bottom - 2);
			graphics.DrawLine (pen, rect.X + 2, Y, rect.Right - 2, Y);
			
			/* Draw 'arrows' for vertical lines */
			graphics.DrawLine (pen, X - 1, rect.Y + 3, X + 1, rect.Y + 3);
			graphics.DrawLine (pen, X - 1, rect.Bottom - 3, X + 1, rect.Bottom - 3);
			
			/* Draw 'arrows' for horizontal lines */
			graphics.DrawLine (pen, rect.X + 3, Y - 1, rect.X + 3, Y + 1);
			graphics.DrawLine (pen, rect.Right - 3, Y - 1, rect.Right - 3, Y + 1);
		}

		public virtual void DrawFlatStyleFocusRectangle (Graphics graphics, Rectangle rectangle, ButtonBase button, Color foreColor, Color backColor) {
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			
			Color outerColor = foreColor;
			// adjust focus color according to the flatstyle
			if (button.FlatStyle == FlatStyle.Popup && !button.is_pressed) {
				outerColor = (backColor.ToArgb () == ColorControl.ToArgb ()) ? ControlPaint.Dark(ColorControl) : ColorControlText;				
			}
			
			// draw the outer rectangle
			graphics.DrawRectangle (ResPool.GetPen (outerColor), trace_rectangle);			
			
			// draw the inner rectangle						
			if (button.FlatStyle == FlatStyle.Popup) {
				DrawInnerFocusRectangle (graphics, Rectangle.Inflate (rectangle, -4, -4), backColor);
			} else {
				// draw a flat inner rectangle
				Pen pen = ResPool.GetPen (ControlPaint.LightLight (backColor));
				graphics.DrawRectangle(pen, Rectangle.Inflate (trace_rectangle, -4, -4));				
			}
		}
		
		public virtual void DrawInnerFocusRectangle(Graphics graphics, Rectangle rectangle, Color backColor)
		{	
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			
#if NotUntilCairoIsFixed
			Color colorBackInverted = Color.FromArgb (Math.Abs (backColor.R-255), Math.Abs (backColor.G-255), Math.Abs (backColor.B-255));
			DashStyle oldStyle; // used for caching old penstyle
			Pen pen = ResPool.GetPen (colorBackInverted);

			oldStyle = pen.DashStyle; 
			pen.DashStyle = DashStyle.Dot;

			graphics.DrawRectangle (pen, trace_rectangle);
			pen.DashStyle = oldStyle;
#else
			CPDrawFocusRectangle(graphics, trace_rectangle, Color.Wheat, backColor);
#endif
		}
				

		public override void CPDrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) 
		{			
			Rectangle rect = rectangle;
			Pen pen;
			HatchBrush brush;
				
			if (backColor.GetBrightness () >= 0.5) {
				foreColor = Color.Transparent;
				backColor = Color.Black;
				
			} else {
				backColor = Color.FromArgb (Math.Abs (backColor.R-255), Math.Abs (backColor.G-255), Math.Abs (backColor.B-255));
				foreColor = Color.Black;
			}
						
			brush = ResPool.GetHatchBrush (HatchStyle.Percent50, backColor, foreColor);
			pen = new Pen (brush, 1);
						
			rect.Width--;
			rect.Height--;			
			
			graphics.DrawRectangle (pen, rect);
			pen.Dispose ();
		}
		
		public override void CPDrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled)
		{
			Brush	sb;
			Pen pen;
			
			if (primary == true) {
				pen = Pens.Black;
				if (enabled == true) {
					sb = Brushes.White;
				} else {
					sb = SystemBrushes.Control;
				}
			} else {
				pen = Pens.White;
				if (enabled == true) {
					sb = Brushes.Black;
				} else {
					sb = SystemBrushes.Control;
				}
			}
			graphics.FillRectangle (sb, rectangle);
			graphics.DrawRectangle (pen, rectangle);			
		}


		public override void CPDrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor) {
			Color	foreColor;
			int	h;
			int	b;
			int	s;

			ControlPaint.Color2HBS(backColor, out h, out b, out s);
			
			if (b>127) {
				foreColor=Color.Black;
			} else {
				foreColor=Color.White;
			}

			// still not perfect. it seems that ms calculates the position of the first dot or line

			using (Pen pen = new Pen (foreColor)) {
				pen.DashPattern = new float [] {1.0f, pixelsBetweenDots.Width - 1};
				
				for (int y = area.Top; y < area.Bottom; y += pixelsBetweenDots.Height)
					graphics.DrawLine (pen, area.X, y, area.Right - 1, y);
			}
		}

		public override void CPDrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background) {
			/*
				Microsoft seems to ignore the background and simply make
				the image grayscale. At least when having > 256 colors on
				the display.
			*/
			
			if (imagedisabled_attributes == null) {				
				imagedisabled_attributes = new ImageAttributes ();
				ColorMatrix colorMatrix=new ColorMatrix(new float[][] {
					  // This table would create a perfect grayscale image, based on luminance
					  //				new float[]{0.3f,0.3f,0.3f,0,0},
					  //				new float[]{0.59f,0.59f,0.59f,0,0},
					  //				new float[]{0.11f,0.11f,0.11f,0,0},
					  //				new float[]{0,0,0,1,0,0},
					  //				new float[]{0,0,0,0,1,0},
					  //				new float[]{0,0,0,0,0,1}
		
					  // This table generates a image that is grayscaled and then
					  // brightened up. Seems to match MS close enough.
					  new float[]{0.2f,0.2f,0.2f,0,0},
					  new float[]{0.41f,0.41f,0.41f,0,0},
					  new float[]{0.11f,0.11f,0.11f,0,0},
					  new float[]{0.15f,0.15f,0.15f,1,0,0},
					  new float[]{0.15f,0.15f,0.15f,0,1,0},
					  new float[]{0.15f,0.15f,0.15f,0,0,1}
				  });
				  
				 imagedisabled_attributes.SetColorMatrix (colorMatrix);
			}
			
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imagedisabled_attributes);
			
		}


		public override void CPDrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary) {
			Pen	penBorder;
			Pen	penInside;

			if (primary) {
				penBorder = ResPool.GetSizedPen (Color.White, 2);
				penInside = ResPool.GetPen (Color.Black);
			} else {
				penBorder = ResPool.GetSizedPen (Color.Black, 2);
				penInside = ResPool.GetPen (Color.White);
			}
			penBorder.Alignment=PenAlignment.Inset;
			penInside.Alignment=PenAlignment.Inset;

			graphics.DrawRectangle(penBorder, rectangle);
			graphics.DrawRectangle(penInside, rectangle.X+2, rectangle.Y+2, rectangle.Width-5, rectangle.Height-5);
		}


		public override void CPDrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph, Color color, Color backColor) {
			Rectangle	rect;
			int			lineWidth;

			if (backColor != Color.Empty)
				graphics.FillRectangle (ResPool.GetSolidBrush (backColor), rectangle);
				
			Brush brush = ResPool.GetSolidBrush (color);

			switch(glyph) {
			case MenuGlyph.Arrow: {
				float height = rectangle.Height * 0.7f;
				float width  = height / 2.0f;
				
				PointF ddCenter = new PointF (rectangle.X + ((rectangle.Width-width) / 2.0f), rectangle.Y + (rectangle.Height / 2.0f));

				PointF [] vertices = new PointF [3];
				vertices [0].X = ddCenter.X;
				vertices [0].Y = ddCenter.Y - (height / 2.0f);
				vertices [1].X = ddCenter.X;
				vertices [1].Y = ddCenter.Y + (height / 2.0f);
				vertices [2].X = ddCenter.X + width + 0.1f;
				vertices [2].Y = ddCenter.Y;
				
				graphics.FillPolygon (brush, vertices);

				return;
			}

			case MenuGlyph.Bullet: {
				
				lineWidth=Math.Max(2, rectangle.Width/3);
				rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);
				
				graphics.FillEllipse(brush, rect);
				
				return;
			}

			case MenuGlyph.Checkmark: {
				
				Pen pen = ResPool.GetPen (color);
				lineWidth = Math.Max (2, rectangle.Width / 6);
				rect = new Rectangle(rectangle.X + lineWidth, rectangle.Y + lineWidth, rectangle.Width - lineWidth * 2, rectangle.Height- lineWidth * 2);

				int Scale = Math.Max (1, rectangle.Width / 12);
				int top = (rect.Y + lineWidth + ((rect.Height - ((2 * Scale) + lineWidth)) / 2));

				for (int i=0; i<lineWidth; i++) {
					graphics.DrawLine (pen, rect.Left+lineWidth/2, top+i, rect.Left+lineWidth/2+2*Scale, top+2*Scale+i);
					graphics.DrawLine (pen, rect.Left+lineWidth/2+2*Scale, top+2*Scale+i, rect.Left+lineWidth/2+6*Scale, top-2*Scale+i);
				}
				return;
			}
			}

		}

		[MonoInternalNote ("Does not respect Mixed")]
		public override void CPDrawMixedCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			CPDrawCheckBox (graphics, rectangle, state);
		}

		public override void CPDrawRadioButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			CPColor cpcolor = ResPool.GetCPColor (ColorControl);
			
			Color dot_color = Color.Black;
			
			Color top_left_outer = Color.Black;
			Color top_left_inner = Color.Black;
			Color bottom_right_outer = Color.Black;
			Color bottom_right_inner = Color.Black;
			
			int ellipse_diameter = (rectangle.Width > rectangle.Height) ? (int)(rectangle.Height  * 0.9f) : (int)(rectangle.Width * 0.9f);
			int radius = ellipse_diameter / 2;
			
			Rectangle rb_rect = new Rectangle (rectangle.X + (rectangle.Width / 2) - radius, rectangle.Y + (rectangle.Height / 2) - radius, ellipse_diameter, ellipse_diameter);
			
			Brush brush = null;
			
			if ((state & ButtonState.All) == ButtonState.All) {
				brush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
												     ColorControl.G, ColorControl.B), ColorControl);
				dot_color = cpcolor.Dark;
			} else
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Pushed) == ButtonState.Pushed))
					brush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255), ColorControl.G, ColorControl.B), ColorControl);
				else
					brush = SystemBrushes.ControlLightLight;
			} else {
				if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Pushed) == ButtonState.Pushed))
					brush = ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255), ColorControl.G, ColorControl.B), ColorControl);
				else
					brush = SystemBrushes.ControlLightLight;
				
				top_left_outer = cpcolor.Dark;
				top_left_inner = cpcolor.DarkDark;
				bottom_right_outer = cpcolor.Light;
				bottom_right_inner = Color.Transparent;
				
				if ((state & ButtonState.Inactive) == ButtonState.Inactive)
					dot_color = cpcolor.Dark;
			}
			
			dc.FillEllipse (brush, rb_rect.X + 1, rb_rect.Y + 1, ellipse_diameter - 1, ellipse_diameter - 1);
			
			int line_width = Math.Max (1, (int)(ellipse_diameter * 0.08f));
			
			dc.DrawArc (ResPool.GetSizedPen (top_left_outer, line_width), rb_rect, 135.0f, 180.0f);
			dc.DrawArc (ResPool.GetSizedPen (top_left_inner, line_width), Rectangle.Inflate (rb_rect, -line_width, -line_width), 135.0f, 180.0f);
			dc.DrawArc (ResPool.GetSizedPen (bottom_right_outer, line_width), rb_rect, 315.0f, 180.0f);
			
			if (bottom_right_inner != Color.Transparent)
				dc.DrawArc (ResPool.GetSizedPen (bottom_right_inner, line_width), Rectangle.Inflate (rb_rect, -line_width, -line_width), 315.0f, 180.0f);
			else
				using (Pen h_pen = new Pen (ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255), ColorControl.G, ColorControl.B), ColorControl), line_width)) {
					dc.DrawArc (h_pen, Rectangle.Inflate (rb_rect, -line_width, -line_width), 315.0f, 180.0f);
				}
			
			if ((state & ButtonState.Checked) == ButtonState.Checked) {
				int inflate = line_width * 4;
				Rectangle tmp = Rectangle.Inflate (rb_rect, -inflate, -inflate);
				if (rectangle.Height >  13) {
					tmp.X += 1;
					tmp.Y += 1;
					tmp.Height -= 1;
					dc.FillEllipse (ResPool.GetSolidBrush (dot_color), tmp);
				} else {
					Pen pen = ResPool.GetPen (dot_color);
					dc.DrawLine (pen, tmp.X, tmp.Y + (tmp.Height / 2), tmp.Right, tmp.Y + (tmp.Height / 2));
					dc.DrawLine (pen, tmp.X, tmp.Y + (tmp.Height / 2) + 1, tmp.Right, tmp.Y + (tmp.Height / 2) + 1);
					
					dc.DrawLine (pen, tmp.X + (tmp.Width / 2), tmp.Y, tmp.X + (tmp.Width / 2), tmp.Bottom);
					dc.DrawLine (pen, tmp.X + (tmp.Width / 2) + 1, tmp.Y, tmp.X + (tmp.Width / 2) + 1, tmp.Bottom);
				}
			}
		}

		public override void CPDrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {

		}


		public override void CPDrawReversibleLine (Point start, Point end, Color backColor) {

		}


		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state)
		{
			DrawScrollButtonPrimitive (dc, area, state);
			
			bool fill_rect = true;
			int offset = 0;
			
			if ((state & ButtonState.Pushed) != 0)
				offset = 1;
			
			// skip the border
			Rectangle rect = new Rectangle (area.X + 2 + offset, area.Y + 2 + offset, area.Width - 4, area.Height - 4);
			
			Point [] arrow = new Point [3];
			for (int i = 0; i < 3; i++)
				arrow [i] = new Point ();
			
			Pen pen = SystemPens.ControlText;
			
			if ((state & ButtonState.Inactive) != 0) {
				pen = SystemPens.ControlDark;
			}
			
			switch (type) {
				default:
				case ScrollButton.Down:
					int x_middle = (int)Math.Round (rect.Width / 2.0f) - 1;
					int y_middle = (int)Math.Round (rect.Height / 2.0f) - 1;
					if (x_middle == 1)
						x_middle = 2;
					
					int triangle_height;
					
					if (rect.Height < 8) {
						triangle_height = 2;
						fill_rect = false;
					} else if (rect.Height == 11) {
						triangle_height = 3;
					} else {
						triangle_height = (int)Math.Round (rect.Height / 3.0f);
					}
					
					arrow [0].X = rect.X + x_middle;
					arrow [0].Y = rect.Y + y_middle + triangle_height / 2;
					
					arrow [1].X = arrow [0].X + triangle_height - 1;
					arrow [1].Y = arrow [0].Y - triangle_height + 1;
					arrow [2].X = arrow [0].X - triangle_height + 1;
					arrow [2].Y = arrow [1].Y;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X + 1, arrow [1].Y + 1, arrow [0].X + 1, arrow [0].Y + 1);
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X, arrow [1].Y + 1, arrow [0].X + 1, arrow [0].Y);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [0].Y - arrow [1].Y; i++) {
							dc.DrawLine (pen, arrow [1].X, arrow [1].Y + i, arrow [2].X, arrow [1].Y + i);
							arrow [1].X -= 1;
							arrow [2].X += 1;
						}
					}
					break;
					
				case ScrollButton.Up:
					x_middle = (int)Math.Round (rect.Width / 2.0f) - 1;
					y_middle = (int)Math.Round (rect.Height / 2.0f);
					if (x_middle == 1)
						x_middle = 2;
					
					if (y_middle == 1)
						y_middle = 2;
					
					if (rect.Height < 8) {
						triangle_height = 2;
						fill_rect = false;
					} else if (rect.Height == 11) {
						triangle_height = 3;
					} else {
						triangle_height = (int)Math.Round (rect.Height / 3.0f);
					}
					
					arrow [0].X = rect.X + x_middle;
					arrow [0].Y = rect.Y + y_middle - triangle_height / 2;
					
					arrow [1].X = arrow [0].X + triangle_height - 1;
					arrow [1].Y = arrow [0].Y + triangle_height - 1;
					arrow [2].X = arrow [0].X - triangle_height + 1;
					arrow [2].Y = arrow [1].Y;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X + 1, arrow [1].Y + 1, arrow [2].X + 1, arrow [1].Y + 1);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [1].Y - arrow [0].Y; i++) {
							dc.DrawLine (pen, arrow [2].X, arrow [1].Y - i, arrow [1].X, arrow [1].Y - i);
							arrow [1].X -= 1;
							arrow [2].X += 1;
						}
					}
					break;
					
				case ScrollButton.Left:
					y_middle = (int)Math.Round (rect.Height / 2.0f) - 1;
					if (y_middle == 1)
						y_middle = 2;
					
					int triangle_width;
					
					if (rect.Width < 8) {
						triangle_width = 2;
						fill_rect = false;
					} else if (rect.Width == 11) {
						triangle_width = 3;
					} else {
						triangle_width = (int)Math.Round (rect.Width / 3.0f);
					}
					
					arrow [0].X = rect.Left + triangle_width - 1;
					arrow [0].Y = rect.Y + y_middle;
					
					if (arrow [0].X - 1 == rect.X)
						arrow [0].X += 1;
					
					arrow [1].X = arrow [0].X + triangle_width - 1;
					arrow [1].Y = arrow [0].Y - triangle_width + 1;
					arrow [2].X = arrow [1].X;
					arrow [2].Y = arrow [0].Y + triangle_width - 1;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [1].X + 1, arrow [1].Y + 1, arrow [2].X + 1, arrow [2].Y + 1);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [2].X - arrow [0].X; i++) {
							dc.DrawLine (pen, arrow [2].X - i, arrow [1].Y, arrow [2].X - i, arrow [2].Y);
							arrow [1].Y += 1;
							arrow [2].Y -= 1;
						}
					}
					break;
					
				case ScrollButton.Right:
					y_middle = (int)Math.Round (rect.Height / 2.0f) - 1;
					if (y_middle == 1)
						y_middle = 2;
					
					if (rect.Width < 8) {
						triangle_width = 2;
						fill_rect = false;
					} else if (rect.Width == 11) {
						triangle_width = 3;
					} else {
						triangle_width = (int)Math.Round (rect.Width / 3.0f);
					}
					
					arrow [0].X = rect.Right - triangle_width - 1;
					arrow [0].Y = rect.Y + y_middle;
					
					if (arrow [0].X - 1 == rect.X)
						arrow [0].X += 1;
					
					arrow [1].X = arrow [0].X - triangle_width + 1;
					arrow [1].Y = arrow [0].Y - triangle_width + 1;
					arrow [2].X = arrow [1].X;
					arrow [2].Y = arrow [0].Y + triangle_width - 1;
					
					dc.DrawPolygon (pen, arrow);
					
					if ((state & ButtonState.Inactive) != 0) {
						dc.DrawLine (SystemPens.ControlLightLight, arrow [0].X + 1, arrow [0].Y + 1, arrow [2].X + 1, arrow [2].Y + 1);
						dc.DrawLine (SystemPens.ControlLightLight, arrow [0].X, arrow [0].Y + 1, arrow [2].X + 1, arrow [2].Y);
					}
					
					if (fill_rect) {
						for (int i = 0; i < arrow [0].X - arrow [1].X; i++) {
							dc.DrawLine (pen, arrow [2].X + i, arrow [1].Y, arrow [2].X + i, arrow [2].Y);
							arrow [1].Y += 1;
							arrow [2].Y -= 1;
						}
					}
					break;
			}
		}

		public  override void CPDrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor) {

		}


		public override void CPDrawSizeGrip (Graphics dc, Color backColor, Rectangle bounds)
		{
			Pen pen_dark = ResPool.GetPen(ControlPaint.Dark(backColor));
			Pen pen_light_light = ResPool.GetPen(ControlPaint.LightLight(backColor));
			
			for (int i = 2; i < bounds.Width - 2; i += 4) {
				dc.DrawLine (pen_light_light, bounds.X + i, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + i - 1);
				dc.DrawLine (pen_dark, bounds.X + i + 1, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + i);
				dc.DrawLine (pen_dark, bounds.X + i + 2, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + i + 1);
			}
		}

		private void DrawStringDisabled20 (Graphics g, string s, Font font, Rectangle layoutRectangle, Color color, TextFormatFlags flags, bool useDrawString)
		{
			CPColor cpcolor = ResPool.GetCPColor (color);

			layoutRectangle.Offset (1, 1);
			TextRenderer.DrawTextInternal (g, s, font, layoutRectangle, cpcolor.LightLight, flags, useDrawString);

			layoutRectangle.Offset (-1, -1);
			TextRenderer.DrawTextInternal (g, s, font, layoutRectangle, cpcolor.Dark, flags, useDrawString);
		}

		public  override void CPDrawStringDisabled (Graphics dc, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format)
		{
			CPColor cpcolor = ResPool.GetCPColor (color);
			
			dc.DrawString (s, font, ResPool.GetSolidBrush(cpcolor.LightLight), 
				       new RectangleF(layoutRectangle.X + 1, layoutRectangle.Y + 1, layoutRectangle.Width, layoutRectangle.Height),
				       format);
			dc.DrawString (s, font, ResPool.GetSolidBrush (cpcolor.Dark), layoutRectangle, format);
		}

#if NET_2_0
		public override void CPDrawStringDisabled (IDeviceContext dc, string s, Font font, Color color, Rectangle layoutRectangle, TextFormatFlags format)
		{
			CPColor cpcolor = ResPool.GetCPColor (color);

			layoutRectangle.Offset (1, 1);
			TextRenderer.DrawText (dc, s, font, layoutRectangle, cpcolor.LightLight, format);

			layoutRectangle.Offset (-1, -1);
			TextRenderer.DrawText (dc, s, font, layoutRectangle, cpcolor.Dark, format);
		}

		public override void CPDrawVisualStyleBorder (Graphics graphics, Rectangle bounds)
		{
			graphics.DrawRectangle (SystemPens.ControlDarkDark, bounds);
		}
#endif

		private static void DrawBorderInternal (Graphics graphics, int startX, int startY, int endX, int endY,
			int width, Color color, ButtonBorderStyle style, Border3DSide side) 
		{
			DrawBorderInternal (graphics, (float) startX, (float) startY, (float) endX, (float) endY, 
				width, color, style, side);
		}

		private static void DrawBorderInternal (Graphics graphics, float startX, float startY, float endX, float endY,
			int width, Color color, ButtonBorderStyle style, Border3DSide side) {

			Pen pen = null;

			switch (style) {
			case ButtonBorderStyle.Solid:
			case ButtonBorderStyle.Inset:
			case ButtonBorderStyle.Outset:
					pen = ThemeEngine.Current.ResPool.GetDashPen (color, DashStyle.Solid);
					break;
			case ButtonBorderStyle.Dashed:
					pen = ThemeEngine.Current.ResPool.GetDashPen (color, DashStyle.Dash);
					break;
			case ButtonBorderStyle.Dotted:
					pen = ThemeEngine.Current.ResPool.GetDashPen (color, DashStyle.Dot);
					break;
			default:
			case ButtonBorderStyle.None:
					return;
			}

			switch(style) {
			case ButtonBorderStyle.Outset: {
				Color		colorGrade;
				int		hue, brightness, saturation;
				int		brightnessSteps;
				int		brightnessDownSteps;

				ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

				brightnessDownSteps=brightness/width;
				if (brightness>127) {
					brightnessSteps=Math.Max(6, (160-brightness)/width);
				} else {
					brightnessSteps=(127-brightness)/width;
				}

				for (int i=0; i<width; i++) {
					switch(side) {
					case Border3DSide.Left:	{
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
						break;
					}

					case Border3DSide.Right: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
						break;
					}

					case Border3DSide.Top: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
						break;
					}

					case Border3DSide.Bottom: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
						break;
					}
					}
				}
				break;
			}

			case ButtonBorderStyle.Inset: {
				Color		colorGrade;
				int		hue, brightness, saturation;
				int		brightnessSteps;
				int		brightnessDownSteps;

				ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

				brightnessDownSteps=brightness/width;
				if (brightness>127) {
					brightnessSteps=Math.Max(6, (160-brightness)/width);
				} else {
					brightnessSteps=(127-brightness)/width;
				}

				for (int i=0; i<width; i++) {
					switch(side) {
					case Border3DSide.Left:	{
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
						break;
					}

					case Border3DSide.Right: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
						break;
					}

					case Border3DSide.Top: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
						break;
					}

					case Border3DSide.Bottom: {
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen = ThemeEngine.Current.ResPool.GetPen (colorGrade);
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
						break;
					}
					}
				}
				break;
			}

				/*
					I decided to have the for-loop duplicated for speed reasons;
					that way we only have to switch once (as opposed to have the
					for-loop around the switch)
				*/
			default: {
				switch(side) {
				case Border3DSide.Left:	{
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
					}
					break;
				}

				case Border3DSide.Right: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
					}
					break;
				}

				case Border3DSide.Top: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
					}
					break;
				}

				case Border3DSide.Bottom: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
					}
					break;
				}
				}
				break;
			}
			}
		}

		/*
			This function actually draws the various caption elements.
			This way we can scale them nicely, no matter what size, and they
			still look like MS's scaled caption buttons. (as opposed to scaling a bitmap)
		*/

		private void DrawCaptionHelper(Graphics graphics, Color color, Pen pen, int lineWidth, int shift, Rectangle captionRect, CaptionButton button) {
			switch(button) {
			case CaptionButton.Close: {
				if (lineWidth<2) {
					graphics.DrawLine(pen, captionRect.Left+2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
					graphics.DrawLine(pen, captionRect.Right-2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
				}

				graphics.DrawLine(pen, captionRect.Left+2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
				graphics.DrawLine(pen, captionRect.Right-2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
				return;
			}

			case CaptionButton.Help: {
				StringFormat	sf = new StringFormat();				
				Font				font = new Font("Microsoft Sans Serif", captionRect.Height, FontStyle.Bold, GraphicsUnit.Pixel);

				sf.Alignment=StringAlignment.Center;
				sf.LineAlignment=StringAlignment.Center;


				graphics.DrawString("?", font, ResPool.GetSolidBrush (color), captionRect.X+captionRect.Width/2+shift, captionRect.Y+captionRect.Height/2+shift+lineWidth/2, sf);

				sf.Dispose();				
				font.Dispose();

				return;
			}

			case CaptionButton.Maximize: {
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+2*lineWidth+shift+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift+i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
				}
				return;
			}

			case CaptionButton.Minimize: {
				/* Bottom line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth+shift, captionRect.Bottom-lineWidth+shift-i);
				}
				return;
			}

			case CaptionButton.Restore: {
				/** First 'window' **/
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift, captionRect.Top+2*lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift-i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+4*lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+5*lineWidth-lineWidth/2+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i);
				}

				/** Second 'window' **/
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+4*lineWidth+shift+1-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+4*lineWidth+shift+1-i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+4*lineWidth+shift+1, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Top+4*lineWidth+shift+1, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Bottom-lineWidth+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
				}

				return;
			}

			}
		}

		/* Generic scroll button */
		public void DrawScrollButtonPrimitive (Graphics dc, Rectangle area, ButtonState state) {
			if ((state & ButtonState.Pushed) == ButtonState.Pushed) {
				dc.FillRectangle (SystemBrushes.Control, area.X + 1,
					area.Y + 1, area.Width - 2 , area.Height - 2);

				dc.DrawRectangle (SystemPens.ControlDark, area.X,
					area.Y, area.Width, area.Height);

				return;
			}			
	
			Brush sb_control = SystemBrushes.Control;
			Brush sb_lightlight = SystemBrushes.ControlLightLight;
			Brush sb_dark = SystemBrushes.ControlDark;
			Brush sb_darkdark = SystemBrushes.ControlDarkDark;
			
			dc.FillRectangle (sb_control, area.X, area.Y, area.Width, 1);
			dc.FillRectangle (sb_control, area.X, area.Y, 1, area.Height);

			dc.FillRectangle (sb_lightlight, area.X + 1, area.Y + 1, area.Width - 1, 1);
			dc.FillRectangle (sb_lightlight, area.X + 1, area.Y + 2, 1,
				area.Height - 4);
			
			dc.FillRectangle (sb_dark, area.X + 1, area.Y + area.Height - 2,
				area.Width - 2, 1);

			dc.FillRectangle (sb_darkdark, area.X, area.Y + area.Height -1,
				area.Width , 1);

			dc.FillRectangle (sb_dark, area.X + area.Width - 2,
				area.Y + 1, 1, area.Height -3);

			dc.FillRectangle (sb_darkdark, area.X + area.Width -1,
				area.Y, 1, area.Height - 1);

			dc.FillRectangle (sb_control, area.X + 2,
				area.Y + 2, area.Width - 4, area.Height - 4);
			
		}
		
		public override void CPDrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style) {
			switch (border_style){
			case BorderStyle.Fixed3D:
				dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X, area.Y, area.X +area.Width, area.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X, area.Y, area.X, area.Y + area.Height);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X , area.Y + area.Height - 1, area.X + area.Width , 
					area.Y + area.Height - 1);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X + area.Width -1 , area.Y, area.X + area.Width -1, 
					area.Y + area.Height);

				dc.DrawLine (ResPool.GetPen (ColorActiveBorder), area.X + 1, area.Bottom - 2, area.Right - 2, area.Bottom - 2);
				dc.DrawLine (ResPool.GetPen (ColorActiveBorder), area.Right - 2, area.Top + 1, area.Right - 2, area.Bottom - 2);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), area.X + 1, area.Top + 1, area.X + 1, area.Bottom - 3);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), area.X + 1, area.Top + 1, area.Right - 3, area.Top + 1);
				break;
			case BorderStyle.FixedSingle:
				dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame), area.X, area.Y, area.Width - 1, area.Height - 1);
				break;
			case BorderStyle.None:
			default:
				break;
			}
			
		}
		#endregion	// ControlPaint


	} //class
}
