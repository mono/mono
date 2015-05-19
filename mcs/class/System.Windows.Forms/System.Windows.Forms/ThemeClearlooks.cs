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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//	Alexander Olk, alex.olk@googlemail.com
//
//	based on ThemeWin32Classic
//
//		- You can activate this Theme with export MONO_THEME=clearlooks
//
// This theme tries to match clearlooks theme
//
// TODO:	
//	- if an other control draws over a ScrollBar button you can see artefacts on the rounded edges 
//	  (maybe use theme backcolor, but that looks ugly on a white background, need to find a way to get the backcolor of the parent control)
//	- more correct drawing of disabled controls

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace System.Windows.Forms {
	internal class ThemeClearlooks : ThemeWin32Classic {
		public override Version Version {
			get {
				return new Version( 0, 0, 0, 2 );
			}
		}
		
		static readonly Color theme_back_color = Color.FromArgb( 239, 235, 231 );
		
		static readonly Color gradient_first_color = Color.FromArgb( 250, 248, 247 );
		static readonly Color gradient_second_color = Color.FromArgb( 226, 219, 212 );
		static readonly Color gradient_second_color_nr2 = Color.FromArgb( 234, 229, 224 );
		static readonly Color pressed_gradient_first_color = Color.FromArgb( 217, 207, 202 );
		static readonly Color pressed_gradient_second_color = Color.FromArgb( 199, 193, 187 );
		static readonly Color border_normal_dark_color = Color.FromArgb( 129, 117, 106 );
		static readonly Color border_normal_light_color = Color.FromArgb( 158, 145, 131 );
		static readonly Color border_pressed_dark_color = Color.FromArgb( 109, 103, 98 );
		static readonly Color border_pressed_light_color = Color.FromArgb( 120, 114, 107 );
		static readonly Color button_outer_border_dark_color = Color.FromArgb( 232, 226, 220 );
		static readonly Color button_outer_border_light_color = Color.FromArgb( 250, 248, 247 ); 
		static readonly Color inner_border_dark_color = Color.FromArgb( 226, 219, 212 );
		static readonly Color pressed_inner_border_dark_color = Color.FromArgb( 192, 181, 169 );
		static readonly Color edge_top_inner_color = Color.FromArgb( 206, 200, 194 );
		static readonly Color edge_bottom_inner_color = Color.FromArgb( 215, 209, 202 );
		static readonly Color button_edge_top_outer_color = Color.FromArgb( 237, 233, 228 );
		static readonly Color button_edge_bottom_outer_color = Color.FromArgb( 243, 239, 236 );
		static readonly Color button_focus_color = Color.FromArgb( 101, 94, 86 );
		static readonly Color button_mouse_entered_second_gradient_color = Color.FromArgb( 230, 226, 219 );
		
		static readonly Color scrollbar_background_color = Color.FromArgb( 209, 200, 191 );
		static readonly Color scrollbar_border_color = Color.FromArgb( 170, 156, 143 );
		static readonly Color scrollbar_gradient_first_color = Color.FromArgb( 248, 247, 245 );
		static readonly Color scrollbar_gradient_second_color = Color.FromArgb( 234, 229, 224 );
		
		new static readonly Color arrow_color = Color.FromArgb( 16, 16, 16 );
		
		static readonly Color tab_border_color = Color.FromArgb( 166, 151, 138 );
		static readonly Color tab_not_selected_gradient_first_color = Color.FromArgb( 227, 223, 220 );
		static readonly Color tab_not_selected_gradient_second_color = Color.FromArgb( 214, 209, 204 );
		static readonly Color tab_selected_gradient_first_color = Color.FromArgb( 243, 239, 236 );
		static readonly Color tab_selected_gradient_second_color = Color.FromArgb( 234, 228, 223 );
		static readonly Color tab_edge_color = Color.FromArgb( 200, 196, 191 );
		static readonly Color tab_inner_border_color = Color.FromArgb( 221, 212, 205 );
		static readonly Color tab_top_border_focus_color = Color.FromArgb( 70, 91, 110 );
		static readonly Color tab_focus_color = Color.FromArgb( 105, 147, 185 );
		
		static readonly Color menuitem_gradient_first_color = Color.FromArgb( 98, 140, 178 );
		static readonly Color menuitem_gradient_second_color = Color.FromArgb( 81, 113, 142 );
		static readonly Color menuitem_border_color = Color.FromArgb( 80, 112, 141 );
		static readonly Color menu_separator_color = Color.FromArgb( 219, 211, 203 );
		static readonly Color menu_background_color = Color.FromArgb( 248, 245, 242 );
		static readonly Color menu_border_color = Color.FromArgb( 141, 122, 104 );
		static readonly Color menu_inner_border_color = Color.FromArgb( 236, 228, 221 );
		
		static readonly Color combobox_border_color = Color.FromArgb( 159, 146, 132 );
		static readonly Color combobox_focus_border_color = Color.FromArgb( 70, 91, 110 );
		//static readonly Color combobox_focus_inner_border_color = Color.FromArgb( 167, 198, 225 );
		static readonly Color combobox_button_second_gradient_color = Color.FromArgb( 226, 220, 213 );
		
		static readonly Color progressbar_edge_dot_color = Color.FromArgb( 219, 212, 205 );
		static readonly Color progressbar_inner_border_color = Color.FromArgb( 139, 176, 209 );
		static readonly Color progressbar_first_gradient_color = Color.FromArgb( 104, 146, 184 );
		static readonly Color progressbar_second_gradient_color = Color.FromArgb( 91, 133, 172 );
		
		static readonly Color checkbox_inner_boder_color = Color.FromArgb( 237, 234, 231 );
		static readonly Color checkbox_pressed_inner_boder_color = Color.FromArgb( 203, 196, 189 );
		static readonly Color checkbox_pressed_backcolor = Color.FromArgb( 212, 207, 202 );
		
		static readonly Color trackbar_second_gradient_color = Color.FromArgb( 114, 154, 190 );
		static readonly Color trackbar_third_gradient_color = Color.FromArgb( 130, 168, 202 );
		static readonly Color trackbar_inner_first_gradient_color = Color.FromArgb( 238, 233, 229 );
		static readonly Color trackbar_inner_second_gradient_color = Color.FromArgb( 223, 215, 208 );
		static readonly Color trackbar_inner_pressed_second_gradient_color = Color.FromArgb( 224, 217, 210 );
		
		//static readonly Color disabled_color_foreground = Color.FromArgb( 182, 180, 173 );
		
		static readonly Color active_caption = Color.FromArgb( 85, 152, 215 );
		
		static readonly Color radio_button_border_circle_color = Color.FromArgb( 126, 118, 105 );
		static readonly Color radio_button_dot_color = Color.FromArgb( 94, 160, 221 );
		
		const int SEPARATOR_HEIGHT = 7;
		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tab
		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items
		
		static Color control_parent_backcolor;
		
		#region	Principal Theme Methods
		public ThemeClearlooks( ) {
			ColorControl = theme_back_color;
		}
		
		public override Color DefaultControlBackColor {
			get { return theme_back_color; }
		}
		
		public override Color DefaultWindowBackColor {
			get { return theme_back_color; }			
		}
		
		public override Color ColorControl {
			get { return theme_back_color;}
		}
		
		public override Color ColorHighlight {
			get { return menuitem_gradient_first_color; }
		}
		
		public override Color ColorActiveCaption {
			get { return active_caption; }
		}
		
		public override Size Border3DSize {
			get {
				return new Size( 3, 3 );
			}
		}
		
		public override Color ColorInfo {
			get { return Color.FromArgb (255, 255, 191); }
		}
		
		static readonly Color info_border_color = Color.FromArgb (218, 178, 85);
		static readonly Color info_second_color = Color.FromArgb (220, 220, 160);

		public override Image Images(UIIcon index, int size) {
			switch (index) {
			case UIIcon.PlacesRecentDocuments:
				if (XplatUI.RunningOnUnix)
					return MimeIconEngine.GetIconForMimeTypeAndSize( "recently/recently", new Size(size, size) );
				else
					return base.Images (UIIcon.PlacesRecentDocuments, size);
			case UIIcon.PlacesDesktop:
				if (XplatUI.RunningOnUnix)
					return MimeIconEngine.GetIconForMimeTypeAndSize( "desktop/desktop", new Size(size, size) );
				else
					return base.Images (UIIcon.PlacesDesktop, size);
			case UIIcon.PlacesPersonal:
				if (XplatUI.RunningOnUnix)
					return MimeIconEngine.GetIconForMimeTypeAndSize( "directory/home", new Size(size, size) );
				else
					return base.Images (UIIcon.PlacesPersonal, size);
			case UIIcon.PlacesMyComputer:
				if (XplatUI.RunningOnUnix)
					return MimeIconEngine.GetIconForMimeTypeAndSize( "workplace/workplace", new Size(size, size) );
				else
					return base.Images (UIIcon.PlacesMyComputer, size);
			case UIIcon.PlacesMyNetwork:
				if (XplatUI.RunningOnUnix)
					return MimeIconEngine.GetIconForMimeTypeAndSize( "network/network", new Size(size, size) );
				else
					return base.Images (UIIcon.PlacesMyNetwork, size);
				
				// Icons for message boxes
			case UIIcon.MessageBoxError:		return base.Images (UIIcon.MessageBoxError, size);
			case UIIcon.MessageBoxInfo:		return base.Images (UIIcon.MessageBoxInfo, size);
			case UIIcon.MessageBoxQuestion:		return base.Images (UIIcon.MessageBoxQuestion, size);
			case UIIcon.MessageBoxWarning:		return base.Images (UIIcon.MessageBoxWarning, size);
				
				// misc Icons
			case UIIcon.NormalFolder:
				if (XplatUI.RunningOnUnix)
					return MimeIconEngine.GetIconForMimeTypeAndSize( "inode/directory", new Size(size, size) );
				else
					return base.Images (UIIcon.NormalFolder, size);
				
			default: {
					throw new ArgumentException("Invalid Icon type requested", "index");
				}
			}
		}
		#endregion	// Internal Methods
		
		#region ButtonBase
		protected override void ButtonBase_DrawButton( ButtonBase button, Graphics dc ) {
			dc.FillRectangle( ResPool.GetSolidBrush( button.BackColor ), button.ClientRectangle );
			
			Color first_gradient_color = gradient_first_color;
			Color second_gradient_color = gradient_second_color;
			
			if (((button is CheckBox) && (((CheckBox)button).check_state == CheckState.Checked)) ||
			    ((button is RadioButton) && (((RadioButton)button).check_state == CheckState.Checked))) {
				first_gradient_color = button.is_entered ? gradient_second_color : pressed_gradient_first_color;
				second_gradient_color = button.is_entered  ? gradient_second_color : pressed_gradient_second_color;
			} else
			if (!button.is_enabled) {
				button.is_entered = false;
			} else
			if (button.is_entered) {
				if (!button.is_pressed) {
					first_gradient_color = Color.White;
					second_gradient_color = button_mouse_entered_second_gradient_color;
				} else {
					first_gradient_color = pressed_gradient_first_color;
					second_gradient_color = pressed_gradient_second_color;
				}
			}
			
			bool paint_acceptbutton_black_border = false;
			Form form = button.TopLevelControl as Form;
			
			if (form != null && (form.AcceptButton == button as IButtonControl))
				paint_acceptbutton_black_border = true;
			
			CL_Draw_Button(dc, button.ClientRectangle, button.FlatStyle,
					  button.is_entered, button.is_enabled, button.is_pressed,
					  first_gradient_color, second_gradient_color,
					  paint_acceptbutton_black_border);
		}

		private void CL_Draw_Button(Graphics dc, Rectangle buttonRectangle, FlatStyle flat_style,
					       bool is_entered, bool is_enabled, bool is_pressed,
					       Color first_gradient_color, Color second_gradient_color,
					       bool paint_acceptbutton_black_border)
		{
			Rectangle lgbRectangle = new Rectangle (buttonRectangle.X + 3, buttonRectangle.Y + 3,
								is_pressed ? buttonRectangle.Width - 5 : buttonRectangle.Width - 6,
								buttonRectangle.Height - 6);
			
			if (flat_style != FlatStyle.Popup || ((flat_style == FlatStyle.Popup) && is_entered)) {
				LinearGradientBrush lgbr;
				if (flat_style == FlatStyle.Flat) {
					lgbr = new LinearGradientBrush (new Point (buttonRectangle.X, buttonRectangle.Y + 3),
								       new Point (buttonRectangle.X, buttonRectangle.Bottom - 3),
								       second_gradient_color, first_gradient_color);
				} else {
					lgbr = new LinearGradientBrush  (new Point (buttonRectangle.X, buttonRectangle.Y + 3),
									new Point (buttonRectangle.X, buttonRectangle.Bottom - 3),
									first_gradient_color, second_gradient_color);
				}
				dc.FillRectangle (lgbr, lgbRectangle);
				lgbr.Dispose ();
				
				Point[] points_top = {
					new Point (buttonRectangle.X + 2, buttonRectangle.Y + 2),
					new Point (buttonRectangle.X + 3, buttonRectangle.Y + 1),
					new Point (buttonRectangle.Right - 4, buttonRectangle.Y + 1),
					new Point (buttonRectangle.Right - 3 , buttonRectangle.Y + 2)
				};
				
				Point[] points_bottom = {
					new Point (buttonRectangle.X + 2, buttonRectangle.Bottom - 3),
					new Point (buttonRectangle.X + 3, buttonRectangle.Bottom - 2),
					new Point (buttonRectangle.Right - 4, buttonRectangle.Bottom - 2),
					new Point (buttonRectangle.Right - 3, buttonRectangle.Bottom - 3)
				};
				
				Point[] points_top_outer = {
					new Point (buttonRectangle.X + 1, buttonRectangle.Y + 1),
					new Point (buttonRectangle.X + 2, buttonRectangle.Y),
					new Point (buttonRectangle.Right - 3, buttonRectangle.Y),
					new Point (buttonRectangle.Right - 2 , buttonRectangle.Y + 1)
				};
				
				Point[] points_bottom_outer = {
					new Point (buttonRectangle.X + 1, buttonRectangle.Bottom - 2),
					new Point (buttonRectangle.X + 2, buttonRectangle.Bottom - 1),
					new Point (buttonRectangle.Right - 3, buttonRectangle.Bottom - 1),
					new Point (buttonRectangle.Right - 2, buttonRectangle.Bottom - 2)
				};
				
				Pen pen = null; 
				
				// normal border
				if (is_enabled) { 
					Color top_color = Color.Black;
					Color bottom_color = Color.Black;
					
					if (!paint_acceptbutton_black_border) {
						top_color = is_pressed ? border_pressed_dark_color : border_normal_dark_color;
						bottom_color = is_pressed ? border_pressed_light_color : border_normal_light_color;
					}
					
					pen = ResPool.GetPen (top_color);
					dc.DrawLines (pen, points_top);
					pen = ResPool.GetPen (bottom_color);
					dc.DrawLines (pen, points_bottom);
					
					using (LinearGradientBrush lgbr2 = new LinearGradientBrush (new Point (buttonRectangle.X, buttonRectangle.Y + 3),
												    new Point (buttonRectangle.X, buttonRectangle.Bottom - 3),
												    top_color, bottom_color)) {
						using (Pen lgbrpen = new Pen (lgbr2)) {
							dc.DrawLine (lgbrpen, buttonRectangle.X + 1, buttonRectangle.Y + 3, buttonRectangle.X + 1, buttonRectangle.Bottom - 3);
							dc.DrawLine (lgbrpen, buttonRectangle.Right - 2, buttonRectangle.Y + 3, buttonRectangle.Right - 2, buttonRectangle.Bottom - 3);
						}
					}
				} else {
					Point[] points_button_complete = {
						new Point (buttonRectangle.X + 1, buttonRectangle.Y + 3),
						new Point (buttonRectangle.X + 3, buttonRectangle.Y + 1),
						new Point (buttonRectangle.Right - 4, buttonRectangle.Y + 1),
						new Point (buttonRectangle.Right - 2, buttonRectangle.Y + 3),
						new Point (buttonRectangle.Right - 2, buttonRectangle.Bottom - 4),
						new Point (buttonRectangle.Right - 4, buttonRectangle.Bottom - 2),
						new Point (buttonRectangle.X + 3, buttonRectangle.Bottom - 2),
						new Point (buttonRectangle.X + 1, buttonRectangle.Bottom - 4),
						new Point (buttonRectangle.X + 1, buttonRectangle.Y + 3)
					};
					
					pen = ResPool.GetPen (pressed_inner_border_dark_color);
					dc.DrawLines (pen, points_button_complete);
				}
				
				// outer border
				pen = ResPool.GetPen (button_outer_border_dark_color);
				dc.DrawLines (pen, points_top_outer);
				pen = ResPool.GetPen (button_outer_border_light_color);
				dc.DrawLines (pen, points_bottom_outer);
				
				using (LinearGradientBrush lgbr2 = new LinearGradientBrush (new Point (buttonRectangle.X, buttonRectangle.Y + 2),
											    new Point (buttonRectangle.X, buttonRectangle.Bottom - 1),
											    button_outer_border_dark_color, button_outer_border_light_color)) {
					using (Pen lgbrpen = new Pen(lgbr2)) {
						dc.DrawLine (lgbrpen, buttonRectangle.X, buttonRectangle.Y + 2, buttonRectangle.X, buttonRectangle.Bottom - 3);
						dc.DrawLine (lgbrpen, buttonRectangle.Right - 1, buttonRectangle.Y + 2, buttonRectangle.Right - 1, buttonRectangle.Bottom - 3);
					}
				}
				
				// inner border
				pen = ResPool.GetPen (is_pressed ? pressed_inner_border_dark_color : inner_border_dark_color);
				if (!is_pressed) {
					dc.DrawLine (pen, buttonRectangle.Right - 3, buttonRectangle.Y + 3, buttonRectangle.Right - 3, buttonRectangle.Bottom - 4);
				}
				dc.DrawLine (pen, buttonRectangle.X + 3, buttonRectangle.Bottom - 3, buttonRectangle.Right - 4, buttonRectangle.Bottom - 3);
				pen = ResPool.GetPen (is_pressed ? pressed_inner_border_dark_color : Color.White);
				dc.DrawLine (pen, buttonRectangle.X + 2, buttonRectangle.Y + 3, buttonRectangle.X + 2, buttonRectangle.Bottom - 4);
				dc.DrawLine (pen, buttonRectangle.X + 3 , buttonRectangle.Y + 2, buttonRectangle.Right - 4, buttonRectangle.Y + 2);
				
				// edges
				pen = ResPool.GetPen (edge_top_inner_color);
				dc.DrawLine (pen, buttonRectangle.X + 1, buttonRectangle.Y + 2, buttonRectangle.X + 2, buttonRectangle.Y + 1);
				dc.DrawLine (pen, buttonRectangle.Right - 3, buttonRectangle.Y + 1, buttonRectangle.Right - 2, buttonRectangle.Y + 2);
				
				pen = ResPool.GetPen (button_edge_top_outer_color);
				dc.DrawLine (pen, buttonRectangle.X, buttonRectangle.Y + 1, buttonRectangle.X + 1, buttonRectangle.Y);
				dc.DrawLine (pen, buttonRectangle.Right - 2, buttonRectangle.Y, buttonRectangle.Right - 1, buttonRectangle.Y + 1);
				
				pen = ResPool.GetPen (edge_bottom_inner_color);
				dc.DrawLine (pen, buttonRectangle.X + 1, buttonRectangle.Bottom - 3, buttonRectangle.X + 2, buttonRectangle.Bottom - 2);
				dc.DrawLine (pen, buttonRectangle.Right - 2, buttonRectangle.Bottom - 3, buttonRectangle.Right - 3, buttonRectangle.Bottom - 2);
				
				pen = ResPool.GetPen (button_edge_bottom_outer_color);
				dc.DrawLine (pen, buttonRectangle.X, buttonRectangle.Bottom - 2, buttonRectangle.X + 1, buttonRectangle.Bottom - 1);
				dc.DrawLine (pen, buttonRectangle.Right - 1, buttonRectangle.Bottom - 2, buttonRectangle.Right - 2, buttonRectangle.Bottom - 1);
			}
		}
		
		protected override void ButtonBase_DrawFocus( ButtonBase button, Graphics dc ) {
			
			if ( !button.is_enabled || button.FlatStyle == FlatStyle.Popup )
				return;
			
			Pen pen = ResPool.GetPen( button_focus_color );
			DashStyle old_dash_style = pen.DashStyle;
			pen.DashStyle = DashStyle.Dot;
			
			Rectangle focus_rect = new Rectangle( button.ClientRectangle.X + 4, button.ClientRectangle.Y + 4, button.ClientRectangle.Width - 9, button.ClientRectangle.Height - 9 );
			
			dc.DrawRectangle( pen, focus_rect );
			
			pen.DashStyle = old_dash_style;
		}
		
		// FIXME: remove if libgdiplus DrawOrMeasureString is fixed
		protected override void ButtonBase_DrawText( ButtonBase button, Graphics dc ) {
			if ( !( button is CheckBox ) && !( button is RadioButton ) ) {
				Rectangle buttonRectangle = button.ClientRectangle;
				Rectangle text_rect = Rectangle.Inflate( buttonRectangle, -4, -4 );
				
				string text = button.Text;
				
				if ( text.Length > 1 ) {
					SizeF sizef = dc.MeasureString( text, button.Font, text_rect.Width, button.text_format );
					
					if ( (int)sizef.Width > text_rect.Width - 3 ) {
						for ( int i = 1; i < text.Length + 1; i++ ) {
							sizef = dc.MeasureString( text.Substring( 0, i ), button.Font, text_rect.Width, button.text_format );
							
							if ( (int)sizef.Width > text_rect.Width - 3 ) {
								text = text.Substring( 0, i - 1 );
								break;
							}
						}
					}
				}
				
				if ( button.is_pressed ) {
					text_rect.X++;
					text_rect.Y++;
				}
				
				if ( button.is_enabled ) {					
					dc.DrawString( text, button.Font, ResPool.GetSolidBrush( button.ForeColor ), text_rect, button.text_format );
				} else {
					if ( button.FlatStyle == FlatStyle.Flat || button.FlatStyle == FlatStyle.Popup ) {
						dc.DrawString( text, button.Font, ResPool.GetSolidBrush( ControlPaint.DarkDark( this.ColorControl ) ), text_rect, button.text_format );
					} else {
						CPDrawStringDisabled( dc, text, button.Font, ColorControlText, text_rect, button.text_format );
					}
				}
			}
		}
		#endregion	// ButtonBase
		
		#region Menus
		public override void DrawMenuItem( MenuItem item, DrawItemEventArgs e ) {
			StringFormat string_format;
			Rectangle rect_text = e.Bounds;
			
			if ( item.Visible == false )
				return; 
			
			if ( item.MenuBar ) {
				string_format = string_format_menu_menubar_text;
			} else {
				string_format = string_format_menu_text;
			}
			
			if ( item.Separator ) {
				e.Graphics.DrawLine( ResPool.GetPen( menu_separator_color ),
						    e.Bounds.X, e.Bounds.Y + 1, e.Bounds.X + e.Bounds.Right - 4, e.Bounds.Y + 1 );
				
				e.Graphics.DrawLine( ResPool.GetPen( Color.White ),
						    e.Bounds.X, e.Bounds.Y + 2, e.Bounds.X + e.Bounds.Right - 4, e.Bounds.Y + 2 );
				
				return;
			}
			
			if ( !item.MenuBar )
				rect_text.X += MenuCheckSize.Width;
			
			if ( item.BarBreak ) { /* Draw vertical break bar*/
				Rectangle rect = e.Bounds;
				rect.Y++;
	        		rect.Width = 3;
	        		rect.Height = item.MenuHeight - 6;
				
				e.Graphics.DrawLine( ResPool.GetPen( menu_separator_color ),
						    rect.X, rect.Y , rect.X, rect.Y + rect.Height );
				
				e.Graphics.DrawLine( ResPool.GetPen( ColorControlLight ),
						    rect.X + 1, rect.Y , rect.X + 1, rect.Y + rect.Height );
			}
			
			Color color_text = ColorMenuText;
			Color color_back;
			
			/* Draw background */
			Rectangle rect_back = e.Bounds;
			rect_back.X++;
			rect_back.Width -= 2;
			
			if ( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected ) {
				color_text = ColorHighlightText;
				color_back = item.MenuBar ? theme_back_color : menu_background_color;
				
				using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rect_back.X, rect_back.Y + 1 ), new Point( rect_back.X, rect_back.Bottom - 1 ), menuitem_gradient_first_color, menuitem_gradient_second_color ) ) {
					e.Graphics.FillRectangle( lgbr, rect_back.X + 1, rect_back.Y + 1, rect_back.Width - 1, rect_back.Height - 1 );
				}
				
				rect_back.Height--;
				Pen tmp_pen = ResPool.GetPen( menuitem_border_color );
				e.Graphics.DrawLine( tmp_pen, rect_back.X + 1, rect_back.Y, rect_back.Right - 1, rect_back.Y );
				e.Graphics.DrawLine( tmp_pen, rect_back.Right, rect_back.Y + 1, rect_back.Right, rect_back.Bottom - 1 );
				e.Graphics.DrawLine( tmp_pen, rect_back.Right - 1, rect_back.Bottom, rect_back.X + 1, rect_back.Bottom );
				e.Graphics.DrawLine( tmp_pen, rect_back.X, rect_back.Bottom - 1, rect_back.X, rect_back.Y + 1 );
			} else {
				color_text = ColorMenuText;
				color_back = item.MenuBar ? theme_back_color : menu_background_color;
				
				e.Graphics.FillRectangle( ResPool.GetSolidBrush( color_back ), rect_back );
			}
			
			if ( item.Enabled ) {
				e.Graphics.DrawString( item.Text, e.Font,
						      ResPool.GetSolidBrush( color_text ),
						      rect_text, string_format );
				
				if ( !item.MenuBar && item.Shortcut != Shortcut.None && item.ShowShortcut ) {
					string str = item.GetShortCutText( );
					Rectangle rect = rect_text;
					rect.X = item.XTab;
					rect.Width -= item.XTab;
					
					e.Graphics.DrawString( str, e.Font, ResPool.GetSolidBrush( color_text ),
							      rect, string_format_menu_shortcut );
				}
			} else {
				ControlPaint.DrawStringDisabled( e.Graphics, item.Text, e.Font,
								Color.Black, rect_text, string_format );
			}
			
			/* Draw arrow */
			if ( item.MenuBar == false && item.IsPopup ) {
				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				using ( Bitmap	bmp = new Bitmap( cx, cy ) ) {
					using ( Graphics dc = Graphics.FromImage( bmp ) ) {
						SmoothingMode old_smoothing_mode = dc.SmoothingMode;
						dc.SmoothingMode = SmoothingMode.AntiAlias;
						
						Rectangle rect_arrow = new Rectangle( 0, 0, cx, cy );
						ControlPaint.DrawMenuGlyph( dc, rect_arrow, MenuGlyph.Arrow );
						bmp.MakeTransparent( );
						
						if ( item.Enabled ) {
							e.Graphics.DrawImage( bmp, e.Bounds.X + e.Bounds.Width - cx,
									     e.Bounds.Y + ( ( e.Bounds.Height - cy ) / 2 ) );
						} else {
							ControlPaint.DrawImageDisabled( e.Graphics, bmp, e.Bounds.X + e.Bounds.Width - cx,
										       e.Bounds.Y + ( ( e.Bounds.Height - cy ) / 2 ),  color_back );
						}
						
						dc.SmoothingMode = old_smoothing_mode;
					}
				}
			}
			
			/* Draw checked or radio */
			if ( item.MenuBar == false && item.Checked ) {
				
				Rectangle area = e.Bounds;
				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				using ( Bitmap bmp = new Bitmap( cx, cy ) ) {
					using ( Graphics gr = Graphics.FromImage( bmp ) ) {
						Rectangle rect_arrow = new Rectangle( 0, 0, cx, cy );
						
						if ( item.RadioCheck )
							ControlPaint.DrawMenuGlyph( gr, rect_arrow, MenuGlyph.Bullet );
						else
							ControlPaint.DrawMenuGlyph( gr, rect_arrow, MenuGlyph.Checkmark );
						
						bmp.MakeTransparent( );
						e.Graphics.DrawImage( bmp, area.X, e.Bounds.Y + ( ( e.Bounds.Height - cy ) / 2 ) );
					}
				}
			}
		}
		
		public override void DrawPopupMenu( Graphics dc, Menu menu, Rectangle cliparea, Rectangle rect ) {
			
			dc.FillRectangle( ResPool.GetSolidBrush
					 ( menu_background_color ), cliparea );
			
			/* Draw menu borders */
			dc.DrawRectangle( ResPool.GetPen( menu_border_color ), rect.X, rect.Y, rect.Width - 1, rect.Height - 1 );
			
			// inner border
			Pen tmp_pen = ResPool.GetPen( Color.White );
			dc.DrawLine( tmp_pen, rect.X + 1, rect.Y + 1, rect.Right - 2, rect.Y + 1 );
			dc.DrawLine( tmp_pen, rect.X + 1, rect.Y + 2, rect.X + 1, rect.Bottom - 2 );
			
			tmp_pen = ResPool.GetPen( menu_inner_border_color );
			dc.DrawLine( tmp_pen, rect.Right - 2, rect.Y + 2, rect.Right - 2, rect.Bottom - 2 );
			dc.DrawLine( tmp_pen, rect.Right - 3, rect.Bottom - 2, rect.X + 2, rect.Bottom - 2 );
			
			for ( int i = 0; i < menu.MenuItems.Count; i++ )
				if ( cliparea.IntersectsWith( menu.MenuItems[ i ].bounds ) ) {
					MenuItem item = menu.MenuItems[ i ];
					item.MenuHeight = menu.Height;
					item.PerformDrawItem( new DrawItemEventArgs( dc, MenuFont,
										    item.bounds, i, item.Status ) );
				}
		}
		#endregion // Menus
		
		#region ProgressBar
		public override void DrawProgressBar( Graphics dc, Rectangle clip_rect, ProgressBar ctrl ) {
			Rectangle	client_area = ctrl.client_area;
			int		barpos_pixels;
			Rectangle bar = ctrl.client_area;
			
			barpos_pixels = ( ( ctrl.Value - ctrl.Minimum ) * client_area.Width ) / ( ctrl.Maximum - ctrl.Minimum );
			
			bar.Width = barpos_pixels + 1;
			
			// Draw bar background
			dc.FillRectangle( ResPool.GetSolidBrush( menu_separator_color ), ctrl.ClientRectangle.X + 1, ctrl.ClientRectangle.Y + 1, ctrl.ClientRectangle.Width - 2, ctrl.ClientRectangle.Height - 2 );
			
			/* Draw border */
			Pen tmp_pen = ResPool.GetPen( progressbar_edge_dot_color );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.X, ctrl.ClientRectangle.Y, ctrl.ClientRectangle.X, ctrl.ClientRectangle.Y + 1 );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.X, ctrl.ClientRectangle.Bottom - 1, ctrl.ClientRectangle.X, ctrl.ClientRectangle.Bottom - 2 );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.Right - 1, ctrl.ClientRectangle.Y, ctrl.ClientRectangle.Right - 1, ctrl.ClientRectangle.Y + 1 );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.Right - 1, ctrl.ClientRectangle.Bottom - 1, ctrl.ClientRectangle.Right - 1, ctrl.ClientRectangle.Bottom - 2 );
			
			tmp_pen = ResPool.GetPen( scrollbar_border_color );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.X + 1, ctrl.ClientRectangle.Y, ctrl.ClientRectangle.Right - 2, ctrl.ClientRectangle.Y );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.Right - 1, ctrl.ClientRectangle.Y + 1, ctrl.ClientRectangle.Right - 1, ctrl.ClientRectangle.Bottom - 2 );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.X + 1, ctrl.ClientRectangle.Bottom - 1, ctrl.ClientRectangle.Right - 2, ctrl.ClientRectangle.Bottom - 1 );
			dc.DrawLine( tmp_pen, ctrl.ClientRectangle.X, ctrl.ClientRectangle.Y + 1, ctrl.ClientRectangle.X, ctrl.ClientRectangle.Bottom - 2 );
			
			if ( barpos_pixels <= 0 )
				return;
			
			if ((bar.Width - 2) <= 0 || (bar.Height - 1) <= 0)
				return;
			
			// Draw bar
			dc.DrawRectangle( ResPool.GetPen( combobox_focus_border_color ), bar.X - 1, bar.Y - 1, bar.Width, bar.Height + 1 );
			tmp_pen = ResPool.GetPen( progressbar_inner_border_color );
			dc.DrawLine( tmp_pen, bar.X, bar.Y, bar.Right - 2, bar.Y );
			dc.DrawLine( tmp_pen, bar.X, bar.Y, bar.X, bar.Bottom - 1 );
			
			using ( Bitmap bmp = new Bitmap( bar.Width - 2, bar.Height - 1 ) ) {
				using ( Graphics gr = Graphics.FromImage( bmp ) ) {
					gr.FillRectangle( ResPool.GetSolidBrush( tab_focus_color ), 0, 0, bmp.Width, bmp.Height );
					
					LinearGradientBrush lgbr = new LinearGradientBrush( new Rectangle( 0, 0, bmp.Height, bmp.Height ), progressbar_first_gradient_color, progressbar_second_gradient_color, 0.0f, true );
					
					lgbr.RotateTransform( 45.0f, MatrixOrder.Append );
					
					float pen_width = bmp.Height / 2;
					
					Pen pen = new Pen( lgbr, pen_width );
					
					int add = bmp.Height + (int)pen.Width / 2;
					
					int x_top = 0;
					int x_bottom = - bmp.Height;
					
					while ( x_bottom < bmp.Width ) {
						gr.DrawLine( pen, x_top, 0, x_bottom, bmp.Height );
						x_top += add;
						x_bottom += add;
					}
					pen.Dispose( );
					lgbr.Dispose( );
				}
				
				dc.DrawImage( bmp, bar.X + 1, bar.Y + 1 );
			}
		}
		#endregion	// ProgressBar
		
		#region RadioButton
		
		// renders a radio button with the Flat and Popup FlatStyle
		protected override void DrawFlatStyleRadioButton (Graphics dc, Rectangle rectangle, RadioButton radio_button)
		{
			if (radio_button.Enabled) {
				// draw the outer flatstyle arcs
				if (radio_button.FlatStyle == FlatStyle.Flat) {
					dc.DrawArc (ResPool.GetPen (radio_button.ForeColor), rectangle, 0, 359);
					
					// fill in the area depending on whether or not the mouse is hovering
					if (radio_button.is_entered && radio_button.Capture) {
						dc.FillPie (ResPool.GetSolidBrush (ControlPaint.Light (radio_button.BackColor)), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
					} else {
						dc.FillPie (ResPool.GetSolidBrush (ControlPaint.LightLight (radio_button.BackColor)), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
					}
				} else {
					// must be a popup radio button
					// fill the control
					dc.FillPie (ResPool.GetSolidBrush (ControlPaint.LightLight (radio_button.BackColor)), rectangle, 0, 359);
					
					if (radio_button.is_entered || radio_button.Capture) {
						// draw the popup 3d button knob
						dc.DrawArc (ResPool.GetPen (ControlPaint.Light (radio_button.BackColor)), rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 0, 359);
						
						dc.DrawArc (ResPool.GetPen (ControlPaint.Dark (radio_button.BackColor)), rectangle, 135, 180);
						dc.DrawArc (ResPool.GetPen (ControlPaint.LightLight (radio_button.BackColor)), rectangle, 315, 180);
						
					} else {
						// just draw lighter flatstyle outer circle
						dc.DrawArc (ResPool.GetPen (ControlPaint.Dark (this.ColorControl)), rectangle, 0, 359);						
					}										
				}
			} else {
				// disabled
				// fill control background color regardless of actual backcolor
				dc.FillPie (ResPool.GetSolidBrush (this.ColorControl), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
				// draw the ark as control dark
				dc.DrawArc (ResPool.GetPen (ControlPaint.Dark(this.ColorControl)), rectangle, 0, 359);
			}
			
			// draw the check
			if (radio_button.Checked) {
				SmoothingMode old_smoothing_mode = dc.SmoothingMode;
				dc.SmoothingMode = SmoothingMode.AntiAlias;
				
				CL_Draw_RadioButton_Dot (dc, rectangle, true, false);
				
				dc.SmoothingMode = old_smoothing_mode;
			}
		}
		#endregion	// RadioButton
		
		#region ScrollBar
		public override void DrawScrollBar( Graphics dc, Rectangle clip, ScrollBar bar ) {
			int		scrollbutton_width = bar.scrollbutton_width;
			int		scrollbutton_height = bar.scrollbutton_height;
			Rectangle	first_arrow_area;
			Rectangle	second_arrow_area;			
			Rectangle	thumb_pos;
			
			thumb_pos = bar.ThumbPos;
			
			if ( bar.vert ) {
				first_arrow_area = new Rectangle( 0, 0, bar.Width, scrollbutton_height + 1 );
				bar.FirstArrowArea = first_arrow_area;
				
				second_arrow_area = new Rectangle( 0, bar.ClientRectangle.Height - scrollbutton_height - 1, bar.Width, scrollbutton_height + 1 );
				bar.SecondArrowArea = second_arrow_area;
				
				thumb_pos.Width = bar.Width;
				bar.ThumbPos = thumb_pos;

				Pen pen;
				/* Background, upper track */
				Rectangle UpperTrack = new Rectangle (0, 0, bar.ClientRectangle.Width, bar.ThumbPos.Top);
				if (clip.IntersectsWith (UpperTrack))
					dc.FillRectangle (ResPool.GetSolidBrush (scrollbar_background_color), UpperTrack);
					pen = ResPool.GetPen (scrollbar_border_color);
					dc.DrawLine (pen, UpperTrack.X, UpperTrack.Y, UpperTrack.X, UpperTrack.Bottom - 1);
					dc.DrawLine (pen, UpperTrack.Right - 1, UpperTrack.Y, UpperTrack.Right - 1, UpperTrack.Bottom - 1);

				/* Background, lower track */
				Rectangle LowerTrack = new Rectangle (0, bar.ThumbPos.Bottom, bar.ClientRectangle.Width, bar.ClientRectangle.Height - bar.ThumbPos.Bottom);
				if (clip.IntersectsWith (LowerTrack))
					dc.FillRectangle (ResPool.GetSolidBrush (scrollbar_background_color), LowerTrack);
					pen = ResPool.GetPen (scrollbar_border_color);
					dc.DrawLine (pen, LowerTrack.X, LowerTrack.Y, LowerTrack.X, LowerTrack.Bottom - 1);
					dc.DrawLine (pen, LowerTrack.Right - 1, LowerTrack.Y, LowerTrack.Right - 1, LowerTrack.Bottom - 1);

				/* Buttons */
				if ( clip.IntersectsWith( first_arrow_area ) )
					CPDrawScrollButton( dc, first_arrow_area, ScrollButton.Up, bar.firstbutton_state );
				if ( clip.IntersectsWith( second_arrow_area ) )
					CPDrawScrollButton( dc, second_arrow_area, ScrollButton.Down, bar.secondbutton_state );
			} else {
				first_arrow_area = new Rectangle( 0, 0, scrollbutton_width + 1, bar.Height );
				bar.FirstArrowArea = first_arrow_area;
				
				second_arrow_area = new Rectangle( bar.ClientRectangle.Width - scrollbutton_width - 1, 0, scrollbutton_width + 1, bar.Height );
				bar.SecondArrowArea = second_arrow_area;
				
				thumb_pos.Height = bar.Height;
				bar.ThumbPos = thumb_pos;

				Pen pen;
				//Background, left track
				Rectangle LeftTrack = new Rectangle (0, 0, bar.ThumbPos.Left, bar.ClientRectangle.Height);
				if (clip.IntersectsWith (LeftTrack))
					dc.FillRectangle (ResPool.GetSolidBrush (scrollbar_background_color), LeftTrack);
					pen = ResPool.GetPen (scrollbar_border_color);
					dc.DrawLine (pen, LeftTrack.X, LeftTrack.Y, LeftTrack.Right - 1, LeftTrack.Y);
					dc.DrawLine (pen, LeftTrack.X, LeftTrack.Bottom - 1, LeftTrack.Right - 1, LeftTrack.Bottom - 1);

				//Background, right track
				Rectangle RightTrack = new Rectangle (bar.ThumbPos.Right, 0, bar.ClientRectangle.Width - bar.ThumbPos.Right, bar.ClientRectangle.Height);
				if (clip.IntersectsWith (RightTrack))
					dc.FillRectangle (ResPool.GetSolidBrush (scrollbar_background_color), RightTrack);
					pen = ResPool.GetPen (scrollbar_border_color);
					dc.DrawLine (pen, RightTrack.X, RightTrack.Y, RightTrack.Right - 1, RightTrack.Y);
					dc.DrawLine (pen, RightTrack.X, RightTrack.Bottom - 1, RightTrack.Right - 1, RightTrack.Bottom - 1);					
				
				/* Buttons */
				if ( clip.IntersectsWith( first_arrow_area ) )
					CPDrawScrollButton( dc, first_arrow_area, ScrollButton.Left, bar.firstbutton_state );
				if ( clip.IntersectsWith( second_arrow_area ) )
					CPDrawScrollButton( dc, second_arrow_area, ScrollButton.Right, bar.secondbutton_state );
			}
			
			/* Thumb */
			ScrollBar_DrawThumb( bar, thumb_pos, clip, dc );				
		}
		
		protected override void ScrollBar_DrawThumb( ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc ) {
			if ( bar.Enabled && thumb_pos.Width > 0 && thumb_pos.Height > 0 && clip.IntersectsWith( thumb_pos ) )
				DrawScrollBarThumb( dc, thumb_pos, bar );
		}
		#endregion	// ScrollBar
		
		#region StatusBar
		protected override void DrawStatusBarPanel( Graphics dc, Rectangle area, int index,
							   Brush br_forecolor, StatusBarPanel panel ) {
			int border_size = 3; // this is actually const, even if the border style is none
			
			area.Height -= border_size;
			if ( panel.BorderStyle != StatusBarPanelBorderStyle.None ) {
				dc.DrawRectangle( ResPool.GetPen( pressed_inner_border_dark_color ), area );
			}
			
			if ( panel.Style == StatusBarPanelStyle.OwnerDraw ) {
				StatusBarDrawItemEventArgs e = new StatusBarDrawItemEventArgs(
					dc, panel.Parent.Font, area, index, DrawItemState.Default,
					panel, panel.Parent.ForeColor, panel.Parent.BackColor );
				panel.Parent.OnDrawItemInternal( e );
				return;
			}
			
			int left = area.Left;
			if ( panel.Icon != null ) {
				left += 2;
				dc.DrawIcon( panel.Icon, left, area.Top );
				left += panel.Icon.Width;
			}
			
			if ( panel.Text == String.Empty )
				return;
			
			string text = panel.Text;
			StringFormat string_format = new StringFormat( );
			string_format.Trimming = StringTrimming.Character;
			string_format.FormatFlags = StringFormatFlags.NoWrap;
			
			if ( text[ 0 ] == '\t' ) {
				string_format.Alignment = StringAlignment.Center;
				text = text.Substring( 1 );
				if ( text[ 0 ] == '\t' ) {
					string_format.Alignment = StringAlignment.Far;
					text = text.Substring( 1 );
				}
			}
			
			int x = left + border_size;
			int y = border_size + 2;
			Rectangle r = new Rectangle( x, y,
						    area.Right - x - border_size,
						    area.Bottom - y - border_size );
			
			dc.DrawString( text, panel.Parent.Font, br_forecolor, r, string_format );
		}
		#endregion	// StatusBar
		
		// FIXME: regions near the borders don't get filled with the correct backcolor
		// TODO: TabAlignment.Left and TabAlignment.Bottom
		public override void DrawTabControl( Graphics dc, Rectangle area, TabControl tab ) {
			if (tab.Parent != null)
				dc.FillRectangle( ResPool.GetSolidBrush( tab.Parent.BackColor ), area );
			else
				dc.FillRectangle( ResPool.GetSolidBrush( tab.BackColor ), area );
			Rectangle panel_rect = TabControlGetPanelRect( tab );
			
			if ( tab.Appearance == TabAppearance.Normal ) {
				
				switch ( tab.Alignment ) {
					case TabAlignment.Top:
						// inner border...
						Pen pen = ResPool.GetPen( Color.White );
						
						dc.DrawLine( pen, panel_rect.Left + 1, panel_rect.Top, panel_rect.Left + 1, panel_rect.Bottom - 1 );
						dc.DrawLine( pen, panel_rect.Left + 2, panel_rect.Top , panel_rect.Right - 2, panel_rect.Top );
						
						pen = ResPool.GetPen( tab_inner_border_color );
						dc.DrawLine( pen, panel_rect.Right - 2, panel_rect.Top + 1, panel_rect.Right - 2, panel_rect.Bottom - 2 );
						dc.DrawLine( pen, panel_rect.Right - 2, panel_rect.Bottom - 1, panel_rect.Left + 2, panel_rect.Bottom - 1 );
						
						// border
						pen = ResPool.GetPen( tab_border_color );
						
						dc.DrawLine( pen, panel_rect.Left, panel_rect.Top - 1, panel_rect.Right - 1, panel_rect.Top - 1 );
						dc.DrawLine( pen, panel_rect.Right - 1, panel_rect.Top - 1, panel_rect.Right - 1, panel_rect.Bottom - 2 );
						dc.DrawLine( pen, panel_rect.Right - 1, panel_rect.Bottom - 2, panel_rect.Right - 3, panel_rect.Bottom );
						dc.DrawLine( pen, panel_rect.Right - 3, panel_rect.Bottom, panel_rect.Left + 2, panel_rect.Bottom );
						dc.DrawLine( pen, panel_rect.Left + 2, panel_rect.Bottom, panel_rect.Left, panel_rect.Bottom - 2 );
						dc.DrawLine( pen, panel_rect.Left, panel_rect.Bottom - 2, panel_rect.Left, panel_rect.Top - 1 );
						break;
						
						// FIXME: the size of the tab page is to big to draw the upper inner white border
					case TabAlignment.Right:
						// inner border...
						pen = ResPool.GetPen( Color.White );
						
						dc.DrawLine( pen, panel_rect.Left + 1, panel_rect.Top + 1, panel_rect.Left + 1, panel_rect.Bottom - 1 );
						dc.DrawLine( pen, panel_rect.Left + 2, panel_rect.Top + 1 , panel_rect.Right - 2, panel_rect.Top + 1 );
						
						pen = ResPool.GetPen( tab_inner_border_color );
						dc.DrawLine( pen, panel_rect.Right - 2, panel_rect.Top + 1, panel_rect.Right - 2, panel_rect.Bottom - 2 );
						dc.DrawLine( pen, panel_rect.Right - 2, panel_rect.Bottom - 1, panel_rect.Left + 2, panel_rect.Bottom - 1 );
						
						// border
						pen = ResPool.GetPen( tab_border_color );
						
						dc.DrawLine( pen, panel_rect.Left + 2, panel_rect.Top, panel_rect.Right - 1, panel_rect.Top );
						dc.DrawLine( pen, panel_rect.Right - 1, panel_rect.Top, panel_rect.Right - 1, panel_rect.Bottom );
						dc.DrawLine( pen, panel_rect.Right - 1, panel_rect.Bottom, panel_rect.Left + 2, panel_rect.Bottom );
						dc.DrawLine( pen, panel_rect.Left + 2, panel_rect.Bottom, panel_rect.Left, panel_rect.Bottom - 2 );
						dc.DrawLine( pen, panel_rect.Left, panel_rect.Bottom - 2, panel_rect.Left, panel_rect.Top + 2 );
						dc.DrawLine( pen, panel_rect.Left, panel_rect.Top + 2, panel_rect.Left + 2, panel_rect.Top );
						break;
				}
			}
			
			if (tab.Alignment == TabAlignment.Top) {
				for (int r = tab.TabPages.Count; r > 0; r--) {
					for (int i = tab.SliderPos; i < tab.TabPages.Count; i++) {
						if (i == tab.SelectedIndex)
							continue;
						if (r != tab.TabPages [i].Row)
							continue;
						Rectangle rect = tab.GetTabRect (i);
						if (!rect.IntersectsWith (area))
							continue;
						DrawTab (dc, tab.TabPages [i], tab, rect, false);
					}
				}
			} else {
				for (int r = 0; r < tab.TabPages.Count; r++) {
					for (int i = tab.SliderPos; i < tab.TabPages.Count; i++) {
						if (i == tab.SelectedIndex)
							continue;
						if (r != tab.TabPages [i].Row)
							continue;
						Rectangle rect = tab.GetTabRect (i);
						if (!rect.IntersectsWith (area))
							continue;
						DrawTab (dc, tab.TabPages [i], tab, rect, false);
					}
				}
			}
			
			if (tab.SelectedIndex != -1 && tab.SelectedIndex >= tab.SliderPos) {
				Rectangle rect = tab.GetTabRect (tab.SelectedIndex);
				if (rect.IntersectsWith (area))
					DrawTab (dc, tab.TabPages [tab.SelectedIndex], tab, rect, true);
			}
			
			if (tab.ShowSlider) {
				Rectangle right = TabControlGetRightScrollRect (tab);
				Rectangle left = TabControlGetLeftScrollRect (tab);
				CPDrawScrollButton (dc, right, ScrollButton.Right, tab.RightSliderState);
				CPDrawScrollButton (dc, left, ScrollButton.Left, tab.LeftSliderState);
			}
		}
		
		protected virtual int DrawTab (Graphics dc, TabPage page, TabControl tab, Rectangle bounds, bool is_selected)
		{
			int FlatButtonSpacing = 8;
			Rectangle interior;
			int res = bounds.Width;
			
			if (page.BackColor != tab_selected_gradient_second_color)
				page.BackColor = tab_selected_gradient_second_color;
			
			// we can't fill the background right away because the bounds might be adjusted if the tab is selected
			
			StringFormat string_format = new StringFormat ();
			
			if (tab.Appearance == TabAppearance.Buttons || tab.Appearance == TabAppearance.FlatButtons) {
				dc.FillRectangle (ResPool.GetSolidBrush (tab_selected_gradient_second_color), bounds);
				
				// Separators
				if (tab.Appearance == TabAppearance.FlatButtons) {
					int width = bounds.Width;
					bounds.Width += (FlatButtonSpacing - 2);
					res = bounds.Width;
					CPDrawBorder3D (dc, bounds, Border3DStyle.Etched, Border3DSide.Right);
					bounds.Width = width;
				}
				
				if (is_selected) {
					CPDrawBorder3D (dc, bounds, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				} else if (tab.Appearance != TabAppearance.FlatButtons) {
					CPDrawBorder3D (dc, bounds, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				}
				
				interior = new Rectangle (bounds.Left + 2, bounds.Top + 2, bounds.Width - 4, bounds.Height - 4);
				
				string_format.Alignment = StringAlignment.Center;
				string_format.LineAlignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
			} else {
				Color tab_first_color = is_selected ? tab_selected_gradient_first_color : tab_not_selected_gradient_first_color;
				Color tab_second_color = is_selected ? tab_selected_gradient_second_color : tab_not_selected_gradient_second_color;
				
				switch (tab.Alignment) {
				case TabAlignment.Top:
					
					Rectangle tab_interior = new Rectangle (bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 3);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left + 2, bounds.Top + 2), new Point (bounds.Left + 2, bounds.Bottom), tab_first_color, tab_second_color)) {
						dc.FillRectangle (lgbr, tab_interior);
					}
					
					// edges
					Pen tmp_pen = ResPool.GetPen (tab_edge_color);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Top + 1, bounds.Left + 1, bounds.Top);
					dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Top, bounds.Right, bounds.Top + 1);
					
					// inner border
					tmp_pen = ResPool.GetPen (Color.White);
					dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Bottom - 2, bounds.Left + 1, bounds.Top + 1);
					dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top + 1, bounds.Right - 1, bounds.Top + 1);
					
					// border
					tmp_pen = ResPool.GetPen (border_pressed_dark_color);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Top + 2, bounds.Left + 2, bounds.Top);
					dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top, bounds.Right - 2, bounds.Top);
					dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Top, bounds.Right, bounds.Top + 2);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left, bounds.Top + 2), new Point (bounds.Left, bounds.Bottom - 1), border_pressed_dark_color, border_pressed_light_color)) {
						int diff = is_selected ? 3 : 2;
						using (Pen lgbrpen = new Pen (lgbr)) {
							dc.DrawLine (lgbrpen, bounds.Left, bounds.Top + 2, bounds.Left, bounds.Bottom - diff);
							dc.DrawLine (lgbrpen, bounds.Right, bounds.Top + 2, bounds.Right, bounds.Bottom - diff);
						}
					}
					
					if (page.Focused) {
						tmp_pen = ResPool.GetPen (tab_focus_color);
						dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Top  + 2, bounds.Right - 1, bounds.Top + 2);
						dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top + 1, bounds.Right - 2, bounds.Top + 1);
						
						tmp_pen = ResPool.GetPen (tab_top_border_focus_color);
						dc.DrawLine (tmp_pen, bounds.Left, bounds.Top + 2, bounds.Left + 2, bounds.Top);
						dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top, bounds.Right - 2, bounds.Top);
						dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Top, bounds.Right, bounds.Top + 2);
					}
					
					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
					
					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;
					
					break;
					
				case TabAlignment.Bottom:
					
					tab_interior = new Rectangle (bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 3);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left + 2, bounds.Top + 2), new Point (bounds.Left + 2, bounds.Bottom - 1), tab_first_color, tab_second_color)) {
						dc.FillRectangle (lgbr, tab_interior);
					}
					
					// edges
					tmp_pen = ResPool.GetPen (tab_edge_color);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Bottom - 1, bounds.Left + 1, bounds.Bottom);
					dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Bottom, bounds.Right, bounds.Bottom - 1);
					
					// inner border
					tmp_pen = ResPool.GetPen (Color.White);
					dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Bottom - 2, bounds.Left + 1, bounds.Top + 2);
					dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
					
					// border
					tmp_pen = ResPool.GetPen (border_pressed_dark_color);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Bottom - 2, bounds.Left + 2, bounds.Bottom);
					dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Bottom, bounds.Right - 2, bounds.Bottom);
					dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Bottom, bounds.Right, bounds.Bottom - 2);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left, bounds.Top + 2), new Point (bounds.Left, bounds.Bottom - 1), border_pressed_light_color, border_pressed_dark_color)) {
						int diff = is_selected ? 3 : 2;
						using (Pen lgbrpen = new Pen (lgbr)) {
							dc.DrawLine (lgbrpen, bounds.Left, bounds.Top + 2, bounds.Left, bounds.Bottom - 1 - diff);
							dc.DrawLine (lgbrpen, bounds.Right, bounds.Top + 2, bounds.Left, bounds.Bottom - 1 - diff);
						}
					}
					
					if (page.Focused) {
						tmp_pen = ResPool.GetPen (tab_focus_color);
						dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Bottom - 2, bounds.Right - 1, bounds.Bottom - 2);
						dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom - 1);
						
						tmp_pen = ResPool.GetPen (tab_top_border_focus_color);
						dc.DrawLine (tmp_pen, bounds.Left, bounds.Bottom - 2, bounds.Left + 2, bounds.Bottom);
						dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Bottom, bounds.Right - 2, bounds.Bottom);
						dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Bottom, bounds.Right, bounds.Bottom - 2);
					}
					
					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
					
					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;
					
					break;
					
				case TabAlignment.Left:
					
					int w_diff = is_selected ? 2 : 0;
					
					tab_interior = new Rectangle (bounds.Left + 2, bounds.Top + 2, bounds.Width - 2 - w_diff, bounds.Height - 2);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left + 2, bounds.Top + 2), new Point (bounds.Right - w_diff, bounds.Top + 2), tab_first_color, tab_second_color)) {
						dc.FillRectangle (lgbr, tab_interior);
					}
					
					// edges
					tmp_pen = ResPool.GetPen (tab_edge_color);
					dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Top, bounds.Left, bounds.Top + 1);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Bottom - 1, bounds.Left + 1, bounds.Bottom);
					
					// inner border
					tmp_pen = ResPool.GetPen (Color.White);
					dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top + 1, bounds.Right - 3, bounds.Top + 1);
					dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Top + 2, bounds.Left + 1, bounds.Bottom - 2);
					
					// border
					tmp_pen = ResPool.GetPen (border_pressed_dark_color);
					dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top, bounds.Left, bounds.Top + 2);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Top + 2, bounds.Left, bounds.Bottom - 2);
					dc.DrawLine (tmp_pen, bounds.Left, bounds.Bottom - 2, bounds.Left + 2, bounds.Bottom);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left, bounds.Top + 2), new Point (bounds.Right - 2, bounds.Top + 2), border_pressed_dark_color, border_pressed_light_color)) {
						int diff = is_selected ? 3 : 1;
						
						using (Pen lgbrpen = new Pen (lgbr)) {
							dc.DrawLine (lgbrpen, bounds.Left + 2, bounds.Top, bounds.Right - diff, bounds.Left + 2);
							dc.DrawLine (lgbrpen, bounds.Left + 2, bounds.Bottom, bounds.Right - diff, bounds.Left + 2);
						}
					}
					
					if (page.Focused) {
						tmp_pen = ResPool.GetPen (tab_focus_color);
						dc.DrawLine (tmp_pen, bounds.Left + 3, bounds.Top + 1, bounds.Left + 3, bounds.Bottom - 1);
						dc.DrawLine (tmp_pen, bounds.Left + 2, bounds.Top + 2, bounds.Left + 2, bounds.Bottom - 2);
						
						tmp_pen = ResPool.GetPen (tab_top_border_focus_color);
						dc.DrawLine (tmp_pen, bounds.Left + 3, bounds.Top, bounds.Left + 1, bounds.Top + 2);
						dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Top + 2, bounds.Left + 1, bounds.Bottom - 2);
						dc.DrawLine (tmp_pen, bounds.Left + 1, bounds.Bottom - 2, bounds.Left + 3, bounds.Bottom);
					}
					
					interior = new Rectangle (bounds.Left + 2, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
					
					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;
					string_format.FormatFlags = StringFormatFlags.DirectionVertical;
					
					break;
					
				default:
					// TabAlignment.Right
					
					tab_interior = new Rectangle (bounds.Left, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left, bounds.Top + 2), new Point (bounds.Right, bounds.Top + 2), tab_second_color, tab_first_color)) {
						dc.FillRectangle (lgbr, tab_interior);
					}
					
					int l_diff = is_selected ? 2 : 0;
					
					// edges
					tmp_pen = ResPool.GetPen (tab_edge_color);
					dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Top, bounds.Right - 1, bounds.Top + 1);
					dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom);
					
					// inner border
					tmp_pen = ResPool.GetPen (Color.White);
					dc.DrawLine (tmp_pen, bounds.Left + l_diff, bounds.Top + 1, bounds.Right - 2, bounds.Top + 1);
					dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Top + 2, bounds.Right - 2, bounds.Bottom - 2);
					
					// border
					tmp_pen = ResPool.GetPen (border_pressed_dark_color);
					dc.DrawLine (tmp_pen, bounds.Right - 3, bounds.Top, bounds.Right - 1, bounds.Top + 2);
					dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Top + 2, bounds.Right - 1, bounds.Bottom - 2);
					dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Bottom - 2, bounds.Right - 3, bounds.Bottom);
					
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (bounds.Left, bounds.Top + 2), new Point (bounds.Right - 2, bounds.Top + 2), border_pressed_light_color, border_pressed_dark_color)) {
						int diff = is_selected ? 3 : 1;
						
						using (Pen lgbrpen = new Pen (lgbr)) {
							dc.DrawLine (lgbrpen, bounds.Left + l_diff, bounds.Top, bounds.Right - diff, bounds.Top);
							dc.DrawLine (lgbrpen, bounds.Left + l_diff, bounds.Bottom, bounds.Right - diff, bounds.Bottom);
						}
					}
					
					if (page.Focused) {
						tmp_pen = ResPool.GetPen (tab_focus_color);
						dc.DrawLine (tmp_pen, bounds.Right - 3, bounds.Top + 1, bounds.Right - 3, bounds.Bottom - 1);
						dc.DrawLine (tmp_pen, bounds.Right - 2, bounds.Top + 2, bounds.Right - 2, bounds.Bottom - 2);
						
						tmp_pen = ResPool.GetPen (tab_top_border_focus_color);
						dc.DrawLine (tmp_pen, bounds.Right - 3, bounds.Top, bounds.Right - 1, bounds.Top + 2);
						dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Top + 2, bounds.Right - 1, bounds.Bottom - 2);
						dc.DrawLine (tmp_pen, bounds.Right - 1, bounds.Bottom - 2, bounds.Right - 3, bounds.Bottom);
					}
					
					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
					
					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;
					string_format.FormatFlags = StringFormatFlags.DirectionVertical;
					
					break;
				}
			}
			
			if (tab.DrawMode == TabDrawMode.Normal && page.Text != null) {
				if (tab.Alignment == TabAlignment.Left) {
					int wo = interior.Width / 2;
					int ho = interior.Height / 2;
					dc.TranslateTransform (interior.X + wo, interior.Y + ho);
					dc.RotateTransform (180);
					dc.DrawString (page.Text, page.Font, ResPool.GetSolidBrush (SystemColors.ControlText), 0, 0, string_format);
					dc.ResetTransform ();
				} else {
					dc.DrawString (page.Text, page.Font,
						       ResPool.GetSolidBrush (SystemColors.ControlText),
						       interior, string_format);
				}
			} else if (page.Text != null) {
				DrawItemState state = DrawItemState.None;
				if (page == tab.SelectedTab)
					state |= DrawItemState.Selected;
				DrawItemEventArgs e = new DrawItemEventArgs (dc,
									     tab.Font, bounds, tab.IndexForTabPage (page),
									     state, page.ForeColor, page.BackColor);
				tab.OnDrawItemInternal (e);
				return res;
			}
			
			return res;
		}		
		
		public override void CPDrawComboButton( Graphics dc, Rectangle rectangle, ButtonState state ) {
			Point[]			arrow = new Point[ 3 ];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;
			
			bool pushed = false;
			
			Color first_color = Color.White;
			Color second_color = combobox_button_second_gradient_color;
			
			dc.FillRectangle( ResPool.GetSolidBrush( Color.White ), rectangle );
			
			if ( state == ButtonState.Pushed ) {
				first_color = pressed_gradient_first_color;
				second_color = pressed_gradient_second_color;
				pushed = true;
			}
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rectangle.X, rectangle.Y + 2 ), new Point( rectangle.X, rectangle.Bottom - 2 ), first_color, second_color ) ) {
				dc.FillRectangle( lgbr, rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4 );
			}
			
			// inner borders
			Pen tmp_pen = ResPool.GetPen( !pushed ? Color.White : pressed_inner_border_dark_color );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y + 1, rectangle.Right - 2, rectangle.Y + 1 );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 2 );
			
			tmp_pen = ResPool.GetPen( !pushed ? inner_border_dark_color : pressed_inner_border_dark_color  );
			dc.DrawLine( tmp_pen, rectangle.Right - 2, rectangle.Y + 2, rectangle.Right - 2, rectangle.Bottom - 2 );
			dc.DrawLine( tmp_pen, rectangle.X + 2, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 2 );
			
			// border
			Point[] points = new Point[] {
				new Point( rectangle.X, rectangle.Y ),
				new Point( rectangle.Right - 3, rectangle.Y ),
				new Point( rectangle.Right - 1, rectangle.Y + 2 ),
				new Point( rectangle.Right - 1, rectangle.Bottom - 3 ),
				new Point( rectangle.Right - 3, rectangle.Bottom - 1 ),
				new Point( rectangle.X, rectangle.Bottom - 1 ),
				new Point( rectangle.X, rectangle.Y )
			};
			
			dc.DrawPolygon( ResPool.GetPen( pushed ? border_pressed_dark_color : border_normal_dark_color ), points );
			
			// edges on right side
			tmp_pen = ResPool.GetPen( control_parent_backcolor );
			dc.DrawLine( tmp_pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Y + 1 );
			dc.DrawLine( tmp_pen, rectangle.Right - 1, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 2 );
			
			tmp_pen = ResPool.GetPen( edge_bottom_inner_color );
			dc.DrawLine( tmp_pen, rectangle.Right - 2, rectangle.Y, rectangle.Right - 1, rectangle.Y + 1 );
			dc.DrawLine( tmp_pen, rectangle.Right - 2, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 2 );
			
			rect = new Rectangle( rectangle.X + 1 + rectangle.Width / 4, rectangle.Y + rectangle.Height / 4, rectangle.Width / 2 - 1, rectangle.Height / 2 );
			centerX = rect.Left + rect.Width / 2;
			centerY = rect.Top + rect.Height / 2;
			shiftX = Math.Max( 1, rect.Width / 8 );
			shiftY = Math.Max( 1, rect.Height / 8 );
			
			if ( ( state & ButtonState.Pushed ) != 0 ) {
				shiftX--;
				shiftY--;
			}
			
			rect.Y -= shiftY;
			centerY -= shiftY;
			
			P1 = new Point( rect.Left, centerY );
			P2 = new Point( centerX, rect.Bottom );
			P3 = new Point( rect.Right - 1, centerY );
			
			arrow[ 0 ] = P1;
			arrow[ 1 ] = P2;
			arrow[ 2 ] = P3;
			
			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			/* Draw the arrow */
			if ( state == ButtonState.Inactive ) {
				/* Move away from the shadow */
				P1.X += 1;		P1.Y += 1;
				P2.X += 1;		P2.Y += 1;
				P3.X += 1;		P3.Y += 1;
				
				arrow[ 0 ] = P1;
				arrow[ 1 ] = P2;
				arrow[ 2 ] = P3;
				
				using ( Pen pen = new Pen( SystemColors.ControlLightLight, 2 ) ) {
					dc.DrawLines( pen, arrow );
				}
				
				P1 = new Point( rect.Left, centerY );
				P2 = new Point( centerX, rect.Bottom );
				P3 = new Point( rect.Right - 1, centerY );
				
				arrow[ 0 ] = P1;
				arrow[ 1 ] = P2;
				arrow[ 2 ] = P3;
				
				using ( Pen pen = new Pen( SystemColors.ControlDark, 2 ) ) {
					dc.DrawLines( pen, arrow );
				}
			} else {
				using ( Pen pen = new Pen( SystemColors.ControlText, 2 ) ) {
					dc.DrawLines( pen, arrow );
				}
			}
			
			dc.SmoothingMode = old_smoothing_mode;
		}
		
		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton( Graphics dc, Rectangle area, ScrollButton scroll_button_type, ButtonState state ) {
			bool enabled = ( state == ButtonState.Inactive ) ? false: true;
			
			DrawScrollButtonPrimitive( dc, area, state, scroll_button_type );
			
			Color color_arrow;
			
			if ( enabled )
				color_arrow = arrow_color;
			else
				color_arrow = ColorGrayText;
			
			/* Paint arrows */
			
			int centerX = area.Left + area.Width / 2;
			int centerY = area.Top + area.Height / 2;
			
			int shift = 0;
			
			if ( ( state & ButtonState.Pushed ) != 0 )
				shift = 1;
			
			int min_4 = 4;
			int min_2 = 2;
			if ( area.Width < 12 || area.Height < 12 ) {
				min_4 = 3;
				min_2 = 1;
			}
			
			Point[]	arrow = new Point[ 4 ];
			
			switch (scroll_button_type) {
			case ScrollButton.Down:
				centerY += shift + 1;
				arrow [0] = new Point (centerX - min_4, centerY - min_2);
				arrow [1] = new Point (centerX, centerY + min_2);
				arrow [2] = new Point (centerX + min_4, centerY - min_2);
				arrow [3] = new Point (centerX - min_4, centerY - min_2);
				break;
			case ScrollButton.Up:
				centerY -= shift;
				arrow [0] = new Point (centerX - min_4, centerY + min_2);
				arrow [1] = new Point (centerX, centerY - min_2);
				arrow [2] = new Point (centerX + min_4, centerY + min_2);
				arrow [3] = new Point (centerX - min_4, centerY + min_2);
				break;
			case ScrollButton.Left:
				centerX -= shift;
				arrow [0] = new Point (centerX + min_2, centerY - min_4);
				arrow [1] = new Point (centerX + min_2, centerY + min_4);
				arrow [2] = new Point (centerX - min_2, centerY);
				arrow [3] = new Point (centerX + min_2, centerY - min_4);
				break;
			case ScrollButton.Right:
				centerX += shift + 1;
				arrow [0] = new Point (centerX - min_2, centerY - min_4);
				arrow [1] = new Point (centerX + min_2, centerY);
				arrow [2] = new Point (centerX - min_2, centerY + min_4);
				arrow [3] = new Point (centerX - min_2, centerY - min_4);
				break;
			default:
				break;
			}
			
			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			dc.FillPolygon( ResPool.GetSolidBrush( color_arrow ), arrow );
			dc.SmoothingMode = old_smoothing_mode;
		}
		
		public override void CPDrawSizeGrip( Graphics dc, Color backColor, Rectangle bounds ) {
			Point pt1 = new Point( bounds.Right - 3, bounds.Bottom );
			Point pt2 = new Point( bounds.Right, bounds.Bottom - 3 );
			
			// diagonals
			Pen tmp_pen = ResPool.GetPen( Color.White );
			for ( int i = 0; i < 4; i++ ) {
				dc.DrawLine( tmp_pen, pt1.X - i * 4, pt1.Y, pt2.X, pt2.Y - i * 4 );
			}
			
			pt1.X += 1;
			pt2.Y += 1;
			
			tmp_pen = ResPool.GetPen( pressed_inner_border_dark_color );
			for ( int i = 0; i < 4; i++ ) {
				dc.DrawLine( tmp_pen, pt1.X - i * 4, pt1.Y, pt2.X, pt2.Y - i * 4 );
			}
		}
		
		private void DrawScrollBarThumb( Graphics dc, Rectangle area, ScrollBar bar ) {
			LinearGradientBrush lgbr = null;
			
			if ( bar.vert )
				lgbr = new LinearGradientBrush( new Point( area.X + 2, area.Y + 2 ), new Point( area.Right - 2, area.Y + 2 ), scrollbar_gradient_first_color, scrollbar_gradient_second_color );
			else
				lgbr = new LinearGradientBrush( new Point( area.X + 2, area.Y + 2 ), new Point( area.X + 2, area.Bottom - 2 ), scrollbar_gradient_first_color, scrollbar_gradient_second_color );
			
			dc.FillRectangle( lgbr, area.X + 2, area.Y + 2, area.Width - 4, area.Height - 4 );
			
			lgbr.Dispose( );
			
			// outer border
			Pen pen = ResPool.GetPen( border_normal_dark_color );
			
			dc.DrawRectangle( pen, area.X, area.Y, area.Width - 1, area.Height - 1 );
			
			// inner border
			pen = ResPool.GetPen( Color.White );
			dc.DrawLine( pen, area.X + 1, area.Bottom - 2, area.X + 1, area.Y + 1 );
			dc.DrawLine( pen, area.X + 2, area.Y + 1, area.Right - 2, area.Y + 1 );
			
			pen = ResPool.GetPen( inner_border_dark_color );
			dc.DrawLine( pen, area.Right - 2, area.Y + 2, area.Right - 2, area.Bottom - 2 );
			dc.DrawLine( pen, area.X + 2, area.Bottom - 2, area.Right - 3, area.Bottom - 2 );
			
			if ( bar.vert ) {
				if ( area.Height > 12 ) {
					int mid_y = area.Y + ( area.Height / 2 );
					int mid_x = area.X + ( area.Width / 2 );
					
					pen = ResPool.GetPen( pressed_inner_border_dark_color );
					dc.DrawLine( pen, mid_x - 3, mid_y, mid_x + 3, mid_y );
					dc.DrawLine( pen, mid_x - 3, mid_y - 3, mid_x + 3, mid_y - 3 );
					dc.DrawLine( pen, mid_x - 3, mid_y + 3, mid_x + 3, mid_y + 3 );
					
					Pen spen = ResPool.GetPen( Color.White );
					dc.DrawLine( spen, mid_x - 3, mid_y + 1, mid_x + 3, mid_y + 1 );
					dc.DrawLine( spen, mid_x - 3, mid_y - 2, mid_x + 3, mid_y - 2 );
					dc.DrawLine( spen, mid_x - 3, mid_y + 4, mid_x + 3, mid_y + 4 );
				}
			} else {
				// draw grip lines only if there is enough space
				if ( area.Width > 12 ) {
					int mid_x = area.X +  ( area.Width / 2 );
					int mid_y = area.Y +  ( area.Height / 2 );
					
					pen = ResPool.GetPen( pressed_inner_border_dark_color );
					dc.DrawLine( pen, mid_x, mid_y - 3, mid_x, mid_y + 3 );
					dc.DrawLine( pen, mid_x - 3, mid_y - 3, mid_x - 3, mid_y + 3 );
					dc.DrawLine( pen, mid_x + 3, mid_y - 3, mid_x + 3, mid_y + 3 );
					
					Pen spen = ResPool.GetPen( Color.White );
					dc.DrawLine( spen, mid_x + 1, mid_y - 3, mid_x + 1, mid_y + 3 );
					dc.DrawLine( spen, mid_x - 2, mid_y - 3, mid_x - 2, mid_y + 3 );
					dc.DrawLine( spen, mid_x + 4, mid_y - 3, mid_x + 4, mid_y + 3 );
				}
			}
		}
		
		public void DrawScrollButtonPrimitive( Graphics dc, Rectangle area, ButtonState state, ScrollButton scroll_button_type ) {
			Pen pen = ResPool.GetPen( border_normal_dark_color );
			
			Color first_gradient_color = gradient_first_color; 
			Color second_gradient_color = gradient_second_color_nr2;
			
			bool pushed = false;
			
			if ( ( state & ButtonState.Pushed ) == ButtonState.Pushed ) {
				first_gradient_color = pressed_gradient_first_color;
				second_gradient_color = pressed_gradient_second_color;
				pushed = true;
			}
			
			Point[] points = null;
			
			LinearGradientBrush lgbr = null;
			
			switch ( scroll_button_type ) {
				case ScrollButton.Left:
					// FIXME: temporary fix for artefacts, it should use the backcolor of the parent control
					dc.DrawLine( ResPool.GetPen( ColorControl ), area.X, area.Y, area.X, area.Bottom - 1 );
					
					lgbr = new LinearGradientBrush( new Point( area.X + 2, area.Y + 2 ), new Point( area.X + 2, area.Bottom - 2 ), first_gradient_color, second_gradient_color );
					dc.FillRectangle( lgbr, area.X + 2, area.Y + 2, area.Width - 4, area.Height - 2 );
					
					Pen tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : Color.White );
					dc.DrawLine( tmp_pen, area.X + 1, area.Y + 2, area.X + 1, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Y + 1, area.Right - 2, area.Y + 1 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : inner_border_dark_color );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Y + 2, area.Right - 2, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Bottom - 2, area.Right - 3, area.Bottom - 2 );
					
					tmp_pen = ResPool.GetPen( edge_top_inner_color );
					dc.DrawLine( tmp_pen, area.X, area.Y + 1, area.X + 1, area.Y );
					dc.DrawLine( tmp_pen, area.X, area.Bottom - 2, area.X + 1, area.Bottom - 1 );
					
					points = new Point[] {
						new Point( area.X + 2, area.Y ),
						new Point( area.Right - 1, area.Y ),
						new Point( area.Right - 1, area.Bottom - 1 ),
						new Point( area.X + 2, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 3 ),
						new Point( area.X, area.Y + 2 ),
						new Point( area.X + 2, area.Y )
					};
					dc.DrawPolygon( pen, points );
					break;
				case ScrollButton.Right:
					// FIXME: temporary fix for artefacts, it should use the backcolor of the parent control
					dc.DrawLine( ResPool.GetPen( ColorControl ), area.Right - 1, area.Y, area.Right - 1, area.Bottom - 1 );
					
					lgbr = new LinearGradientBrush( new Point( area.X + 2, area.Y + 2 ), new Point( area.X + 2, area.Bottom - 2 ), first_gradient_color, second_gradient_color );
					dc.FillRectangle( lgbr, area.X + 2, area.Y + 2, area.Width - 4, area.Height - 2 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : Color.White );
					dc.DrawLine( tmp_pen, area.X + 1, area.Y + 1, area.X + 1, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Y + 1, area.Right - 2, area.Y + 1 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : inner_border_dark_color );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Y + 2, area.Right - 2, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Bottom - 2, area.Right - 3, area.Bottom - 2 );
					
					tmp_pen = ResPool.GetPen( edge_top_inner_color );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Y, area.Right - 1, area.Y + 1 );
					dc.DrawLine( tmp_pen, area.Right - 1, area.Bottom - 2, area.Right - 2, area.Bottom - 1 );
					
					points = new Point[] {
						new Point( area.X, area.Y ),
						new Point( area.Right - 3, area.Y ),
						new Point( area.Right - 1, area.Y + 2 ),
						new Point( area.Right - 1, area.Bottom - 3 ),
						new Point( area.Right - 3, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 1 ),
						new Point( area.X, area.Y ),
					};
					dc.DrawPolygon( pen, points );
					break;
				case ScrollButton.Up:
					// FIXME: temporary fix for artefacts, it should use the backcolor of the parent control
					dc.DrawLine( ResPool.GetPen( ColorControl ), area.X, area.Y, area.Right - 1, area.Y );
					
					lgbr = new LinearGradientBrush( new Point( area.X + 2, area.Y ), new Point( area.Right - 2, area.Y ), first_gradient_color, second_gradient_color );
					dc.FillRectangle( lgbr, area.X + 2, area.Y + 2, area.Width - 4, area.Height - 4 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : Color.White );
					dc.DrawLine( tmp_pen, area.X + 1, area.Y + 1, area.X + 1, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Y + 1, area.Right - 2, area.Y + 1 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : inner_border_dark_color );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Y + 2, area.Right - 2, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Bottom - 2, area.Right - 3, area.Bottom - 2 );
					
					tmp_pen = ResPool.GetPen( edge_top_inner_color );
					dc.DrawLine( tmp_pen, area.X, area.Y + 1, area.X + 1, area.Y );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Y, area.Right - 1, area.Y + 1 );
					
					points = new Point[] {
						new Point( area.X + 2, area.Y ),
						new Point( area.Right - 3, area.Y ),
						new Point( area.Right - 1, area.Y + 2 ),
						new Point( area.Right - 1, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 1 ),
						new Point( area.X, area.Y + 2 ),
						new Point( area.X + 2, area.Y )
					};
					dc.DrawPolygon( pen, points );
					break;
				case ScrollButton.Down:
					// FIXME: temporary fix for artefacts, it should use the backcolor of the parent control
					dc.DrawLine( ResPool.GetPen( ColorControl ), area.X, area.Bottom - 1, area.Right - 1, area.Bottom - 1 );
					
					lgbr = new LinearGradientBrush( new Point( area.X + 2, area.Y ), new Point( area.Right - 2, area.Y ), first_gradient_color, second_gradient_color );
					dc.FillRectangle( lgbr, area.X + 2, area.Y + 2, area.Width - 4, area.Height - 4 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : Color.White );
					dc.DrawLine( tmp_pen, area.X + 1, area.Y + 1, area.X + 1, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Y + 1, area.Right - 2, area.Y + 1 );
					
					tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : inner_border_dark_color );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Y + 2, area.Right - 2, area.Bottom - 2 );
					dc.DrawLine( tmp_pen, area.X + 2, area.Bottom - 2, area.Right - 3, area.Bottom - 2 );
					
					tmp_pen = ResPool.GetPen( edge_top_inner_color );
					dc.DrawLine( tmp_pen, area.X, area.Bottom - 2, area.X + 1, area.Bottom - 1 );
					dc.DrawLine( tmp_pen, area.Right - 2, area.Bottom - 1, area.Right - 1, area.Bottom - 2 );
					
					points = new Point[] {
						new Point( area.X, area.Y ),
						new Point( area.Right - 1, area.Y ),
						new Point( area.Right - 1, area.Bottom - 3 ),
						new Point( area.Right - 3, area.Bottom - 1 ),
						new Point( area.X + 2, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 3 ),
						new Point( area.X, area.Y )
					};
					dc.DrawPolygon( pen, points );
					break;
			}
			
			lgbr.Dispose( );
		}
		
		#region ToolBar

		#endregion // ToolBar

		#region GroupBox
		public override void DrawGroupBox( Graphics dc,  Rectangle area, GroupBox box ) {
			StringFormat	text_format;
			SizeF		size;
			int		width;
			int		y;
			Rectangle	rect;
			
			rect = box.ClientRectangle;
			
			dc.FillRectangle( ResPool.GetSolidBrush( box.BackColor ), rect );
			
			text_format = new StringFormat( );
			text_format.HotkeyPrefix = HotkeyPrefix.Show;
			
			size = dc.MeasureString( box.Text, box.Font );
			width = (int) size.Width;
			
			if ( width > box.Width - 16 )
				width = box.Width - 16;
			
			y = box.Font.Height / 2;
			
			Pen pen = ResPool.GetPen( pressed_inner_border_dark_color );
			
			/* Draw group box*/
			Point[] points = {
				new Point( 8 + width, y ),
				new Point( box.Width - 3, y ),
				new Point( box.Width - 1, y + 2 ),
				new Point( box.Width - 1, box.Height - 3 ),
				new Point( box.Width - 3, box.Height - 1 ),
				new Point( 2, box.Height - 1 ),
				new Point( 0, box.Height - 3 ),
				new Point( 0, y + 2 ),
				new Point( 2, y ),
				new Point( 8, y )
			};
			dc.DrawLines( pen, points );
			
			/* Text */
			if ( box.Enabled ) {
				dc.DrawString( box.Text, box.Font, ResPool.GetSolidBrush( box.ForeColor ), 10, 0, text_format );
			} else {
				CPDrawStringDisabled( dc, box.Text, box.Font, box.ForeColor, 
						     new RectangleF( 10, 0, width,  box.Font.Height ), text_format );
			}
			text_format.Dispose( );	
		}
		#endregion
		
		#region	TrackBar
		private void DrawTrackBar_Vertical( Graphics dc, Rectangle clip_rectangle, TrackBar tb,
						   ref Rectangle thumb_pos, ref Rectangle thumb_area,
						   float ticks, int value_pos, bool mouse_value ) {			
			
			Point toptick_startpoint = new Point( );
			Point bottomtick_startpoint = new Point( );
			Point channel_startpoint = new Point( );
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;
			Rectangle area = tb.ClientRectangle;
			
			switch ( tb.TickStyle ) 	{
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
			thumb_area.Width = 4;
			
			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ( tb.Maximum - tb.Minimum );
			
			/* Convert thumb position from mouse position to value*/
			if ( mouse_value ) {
				
				if ( value_pos >= channel_startpoint.Y )
					value_pos = (int)( ( (float) ( value_pos - channel_startpoint.Y ) ) / pixels_betweenticks );
				else
					value_pos = 0;			
				
				if ( value_pos + tb.Minimum > tb.Maximum )
					value_pos = tb.Maximum - tb.Minimum;
                                
				tb.Value = value_pos + tb.Minimum;
			}		
			
			thumb_pos.Width = 13;
			thumb_pos.Height = 29;
			
			thumb_pos.Y = channel_startpoint.Y + (int) ( pixels_betweenticks * (float) value_pos ) - ( thumb_pos.Height / 3 );
			
			if ( thumb_pos.Y < channel_startpoint.Y )
				thumb_pos.Y = channel_startpoint.Y;
			
			if ( thumb_pos.Y > thumb_area.Bottom - 29 )
				thumb_pos.Y = thumb_area.Bottom - 29;
			
			/* Draw channel */
			// bottom
			Pen pen = ResPool.GetPen( tab_top_border_focus_color );
			dc.DrawLine( pen, channel_startpoint.X, thumb_pos.Y + 29, channel_startpoint.X, thumb_area.Bottom );
			dc.DrawLine( pen, channel_startpoint.X, thumb_area.Bottom, channel_startpoint.X + 4, thumb_area.Bottom );
			dc.DrawLine( pen, channel_startpoint.X + 4, thumb_pos.Y + 29, channel_startpoint.X + 4, thumb_area.Bottom );
			
			pen = ResPool.GetPen( menuitem_gradient_first_color );
			dc.DrawLine( pen, channel_startpoint.X + 1, thumb_pos.Y + 28, channel_startpoint.X + 1, thumb_area.Bottom - 1 );
			pen = ResPool.GetPen( trackbar_second_gradient_color );
			dc.DrawLine( pen, channel_startpoint.X + 2, thumb_pos.Y + 28, channel_startpoint.X + 2, thumb_area.Bottom - 1 );
			pen = ResPool.GetPen( trackbar_third_gradient_color );
			dc.DrawLine( pen, channel_startpoint.X + 3, thumb_pos.Y + 28, channel_startpoint.X + 3, thumb_area.Bottom - 1 );
			
			// top
			pen = ResPool.GetPen( pressed_inner_border_dark_color );
			dc.DrawLine( pen, channel_startpoint.X + 1, channel_startpoint.Y + 1, channel_startpoint.X + 1, thumb_pos.Y );
			dc.DrawRectangle( ResPool.GetPen( scrollbar_background_color ), channel_startpoint.X + 2, channel_startpoint.Y + 1, 1, thumb_pos.Y );
			
			pen = ResPool.GetPen( scrollbar_border_color );
			dc.DrawLine( pen, channel_startpoint.X, channel_startpoint.Y, channel_startpoint.X, thumb_pos.Y );
			dc.DrawLine( pen, channel_startpoint.X, channel_startpoint.Y, channel_startpoint.X + 4, channel_startpoint.Y );
			dc.DrawLine( pen, channel_startpoint.X + 4, channel_startpoint.Y, channel_startpoint.X + 4, thumb_pos.Y );
			
			/* Draw thumb */
			thumb_pos.X = channel_startpoint.X - 4;
			
			// inner border
			pen = ResPool.GetPen( Color.White );
			dc.DrawLine( pen, thumb_pos.X + 1, thumb_pos.Y + 1, thumb_pos.X + 1, thumb_pos.Bottom - 2 );
			dc.DrawLine( pen, thumb_pos.X + 2, thumb_pos.Y + 1, thumb_pos.Right - 2, thumb_pos.Y + 1 );
			
			pen = ResPool.GetPen( menu_separator_color );
			dc.DrawLine( pen, thumb_pos.X + 2, thumb_pos.Bottom - 2, thumb_pos.Right - 2, thumb_pos.Bottom - 2 );
			dc.DrawLine( pen, thumb_pos.Right - 2, thumb_pos.Y + 2, thumb_pos.Right - 2, thumb_pos.Bottom - 2 );
			
			// outer border
			Point[] points = {
				new Point( thumb_pos.X + 2, thumb_pos.Y ),
				new Point( thumb_pos.Right - 3 , thumb_pos.Y ),
				new Point( thumb_pos.Right - 1, thumb_pos.Y + 2 ),
				new Point( thumb_pos.Right - 1, thumb_pos.Bottom - 3 ),
				new Point( thumb_pos.Right - 3, thumb_pos.Bottom - 1 ),
				new Point( thumb_pos.X + 2, thumb_pos.Bottom - 1 ),
				new Point( thumb_pos.X, thumb_pos.Bottom - 3 ),
				new Point( thumb_pos.X, thumb_pos.Y + 2 ),
				new Point( thumb_pos.X + 2, thumb_pos.Y )
			};
			
			dc.DrawLines( ResPool.GetPen( border_normal_dark_color ), points );
			
			Color first_gradient_color = mouse_value ? button_edge_bottom_outer_color : trackbar_inner_first_gradient_color;
			Color second_gradient_color = mouse_value ? trackbar_inner_pressed_second_gradient_color : trackbar_inner_second_gradient_color;
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( thumb_pos.X, thumb_pos.Y + 2 ), new Point( thumb_pos.X, thumb_pos.Bottom - 2 ), first_gradient_color, second_gradient_color ) ) {
				dc.FillRectangle( lgbr, thumb_pos.X + 2, thumb_pos.Y + 2, thumb_pos.Width - 4, thumb_pos.Height - 4 );
			}
			
			// outer egdes
			pen = ResPool.GetPen( edge_top_inner_color );
			dc.DrawLine( pen, thumb_pos.X, thumb_pos.Y + 1, thumb_pos.X + 1, thumb_pos.Y );
			dc.DrawLine( pen, thumb_pos.Right - 2, thumb_pos.Y, thumb_pos.Right - 1, thumb_pos.Y + 1 );
			
			pen = ResPool.GetPen( edge_bottom_inner_color );
			dc.DrawLine( pen, thumb_pos.X, thumb_pos.Bottom - 2, thumb_pos.X + 1, thumb_pos.Bottom - 1 );
			dc.DrawLine( pen, thumb_pos.Right - 1, thumb_pos.Bottom - 2, thumb_pos.Right - 2, thumb_pos.Bottom - 1 );
			
			// draw grip lines
			pen = ResPool.GetPen( pressed_inner_border_dark_color );
			dc.DrawLine( pen, thumb_pos.X + 4, thumb_pos.Y + 11, thumb_pos.X + 8, thumb_pos.Y + 11 );
			dc.DrawLine( pen, thumb_pos.X + 4, thumb_pos.Y + 14, thumb_pos.X + 8, thumb_pos.Y + 14 );
			dc.DrawLine( pen, thumb_pos.X + 4, thumb_pos.Y + 17, thumb_pos.X + 8, thumb_pos.Y + 17 );
			
			pen = ResPool.GetPen( Color.White );
			dc.DrawLine( pen, thumb_pos.X + 4, thumb_pos.Y + 12, thumb_pos.X + 8, thumb_pos.Y + 12 );
			dc.DrawLine( pen, thumb_pos.X + 4, thumb_pos.Y + 15, thumb_pos.X + 8, thumb_pos.Y + 15 );
			dc.DrawLine( pen, thumb_pos.X + 4, thumb_pos.Y + 18, thumb_pos.X + 8, thumb_pos.Y + 18 );
			
			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			/* Draw ticks*/
			thumb_area.X = thumb_pos.X;
			thumb_area.Y = channel_startpoint.Y;
			thumb_area.Width = thumb_pos.Width;
			
			Region outside = new Region( area );
			outside.Exclude( thumb_area );			
			
			if ( outside.IsVisible( clip_rectangle ) ) {				
				if ( pixels_betweenticks > 0 && ( ( tb.TickStyle & TickStyle.BottomRight ) == TickStyle.BottomRight ||
				    ( ( tb.TickStyle & TickStyle.Both ) == TickStyle.Both ) ) ) {	
					
					for ( float inc = 0; inc < ( pixel_len + 1 ); inc += pixels_betweenticks ) 	{					
						if ( inc == 0 || ( inc +  pixels_betweenticks ) >= pixel_len + 1 )
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + bottomtick_startpoint.X , area.Y + bottomtick_startpoint.Y  + inc, 
								    area.X + bottomtick_startpoint.X  + 3, area.Y + bottomtick_startpoint.Y + inc );
						else
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + bottomtick_startpoint.X, area.Y + bottomtick_startpoint.Y  + inc, 
								    area.X + bottomtick_startpoint.X  + 2, area.Y + bottomtick_startpoint.Y + inc );
					}
				}
				
				if ( pixels_betweenticks > 0 &&  ( ( tb.TickStyle & TickStyle.TopLeft ) == TickStyle.TopLeft ||
				    ( ( tb.TickStyle & TickStyle.Both ) == TickStyle.Both ) ) ) {
					
					pixel_len = thumb_area.Height - 11;
					pixels_betweenticks = pixel_len / ticks;
					
					for ( float inc = 0; inc < ( pixel_len + 1 ); inc += pixels_betweenticks ) {					
						if ( inc == 0 || ( inc +  pixels_betweenticks ) >= pixel_len + 1 )
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + toptick_startpoint.X  - 3 , area.Y + toptick_startpoint.Y + inc, 
								    area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y + inc );
						else
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + toptick_startpoint.X  - 2, area.Y + toptick_startpoint.Y + inc, 
								    area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y  + inc );
					}			
				}
			}
			
			outside.Dispose( );
			
		}
		
		private void DrawTrackBar_Horizontal( Graphics dc, Rectangle clip_rectangle, TrackBar tb,
						     ref Rectangle thumb_pos, ref Rectangle thumb_area,
						     float ticks, int value_pos, bool mouse_value ) {			
			Point toptick_startpoint = new Point( );
			Point bottomtick_startpoint = new Point( );
			Point channel_startpoint = new Point( );
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;
			Rectangle area = tb.ClientRectangle;
			
			switch ( tb.TickStyle ) {
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
			thumb_area.Height = 4;
			
			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ( tb.Maximum - tb.Minimum );
			
			/* Convert thumb position from mouse position to value*/
			if ( mouse_value ) {			
				if ( value_pos >= channel_startpoint.X )
					value_pos = (int)( ( (float) ( value_pos - channel_startpoint.X ) ) / pixels_betweenticks );
				else
					value_pos = 0;				
				
				if ( value_pos + tb.Minimum > tb.Maximum )
					value_pos = tb.Maximum - tb.Minimum;
                                
				tb.Value = value_pos + tb.Minimum;
			}			
			
			thumb_pos.Width = 29;
			thumb_pos.Height = 13;
			
			thumb_pos.X = channel_startpoint.X + (int) ( pixels_betweenticks * (float) value_pos ) - ( thumb_pos.Width / 3 );
			
			if ( thumb_pos.X < channel_startpoint.X )
				thumb_pos.X = channel_startpoint.X;
			
			if ( thumb_pos.X > thumb_area.Right - 29 )
				thumb_pos.X = thumb_area.Right - 29;
			
			/* Draw channel */
			// left side
			Pen pen = ResPool.GetPen( tab_top_border_focus_color );
			dc.DrawLine( pen, channel_startpoint.X, channel_startpoint.Y, thumb_pos.X, channel_startpoint.Y );
			dc.DrawLine( pen, channel_startpoint.X, channel_startpoint.Y, channel_startpoint.X, channel_startpoint.Y + 4 );
			dc.DrawLine( pen, channel_startpoint.X, channel_startpoint.Y + 4, thumb_pos.X, channel_startpoint.Y + 4 );
			
			pen = ResPool.GetPen( menuitem_gradient_first_color );
			dc.DrawLine( pen, channel_startpoint.X + 1, channel_startpoint.Y + 1, thumb_pos.X, channel_startpoint.Y + 1 );
			pen = ResPool.GetPen( trackbar_second_gradient_color );
			dc.DrawLine( pen, channel_startpoint.X + 1, channel_startpoint.Y + 2, thumb_pos.X, channel_startpoint.Y + 2 );
			pen = ResPool.GetPen( trackbar_third_gradient_color );
			dc.DrawLine( pen, channel_startpoint.X + 1, channel_startpoint.Y + 3, thumb_pos.X, channel_startpoint.Y + 3 );
			
			// right side
			pen = ResPool.GetPen( pressed_inner_border_dark_color );
			dc.DrawLine( pen, thumb_pos.X + 29, channel_startpoint.Y + 1, thumb_area.Right - 1, channel_startpoint.Y + 1 );
			dc.DrawRectangle( ResPool.GetPen( scrollbar_background_color ), thumb_pos.X + 29, channel_startpoint.Y + 2, thumb_area.Right - thumb_pos.X - 30, 1 );
			
			pen = ResPool.GetPen( scrollbar_border_color );
			dc.DrawLine( pen, thumb_pos.X + 29, channel_startpoint.Y, thumb_area.Right, channel_startpoint.Y );
			dc.DrawLine( pen, thumb_area.Right, channel_startpoint.Y, thumb_area.Right, channel_startpoint.Y + 4 );
			dc.DrawLine( pen, thumb_pos.X + 29, channel_startpoint.Y + 4, thumb_area.Right, channel_startpoint.Y + 4 );
			
			/* Draw thumb */
			
			thumb_pos.Y = channel_startpoint.Y - 4;
			
			// inner border
			pen = ResPool.GetPen( Color.White );
			dc.DrawLine( pen, thumb_pos.X + 1, thumb_pos.Y + 1, thumb_pos.X + 1, thumb_pos.Bottom - 2 );
			dc.DrawLine( pen, thumb_pos.X + 2, thumb_pos.Y + 1, thumb_pos.Right - 2, thumb_pos.Y + 1 );
			
			pen = ResPool.GetPen( menu_separator_color );
			dc.DrawLine( pen, thumb_pos.X + 2, thumb_pos.Bottom - 2, thumb_pos.Right - 2, thumb_pos.Bottom - 2 );
			dc.DrawLine( pen, thumb_pos.Right - 2, thumb_pos.Y + 2, thumb_pos.Right - 2, thumb_pos.Bottom - 2 );
			
			// outer border
			Point[] points = {
				new Point( thumb_pos.X + 2, thumb_pos.Y ),
				new Point( thumb_pos.Right - 3 , thumb_pos.Y ),
				new Point( thumb_pos.Right - 1, thumb_pos.Y + 2 ),
				new Point( thumb_pos.Right - 1, thumb_pos.Bottom - 3 ),
				new Point( thumb_pos.Right - 3, thumb_pos.Bottom - 1 ),
				new Point( thumb_pos.X + 2, thumb_pos.Bottom - 1 ),
				new Point( thumb_pos.X, thumb_pos.Bottom - 3 ),
				new Point( thumb_pos.X, thumb_pos.Y + 2 ),
				new Point( thumb_pos.X + 2, thumb_pos.Y )
			};
			
			dc.DrawLines( ResPool.GetPen( border_normal_dark_color ), points );
			
			Color first_gradient_color = mouse_value ? button_edge_bottom_outer_color : trackbar_inner_first_gradient_color;
			Color second_gradient_color = mouse_value ? trackbar_inner_pressed_second_gradient_color : trackbar_inner_second_gradient_color;
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( thumb_pos.X, thumb_pos.Y + 2 ), new Point( thumb_pos.X, thumb_pos.Bottom - 2 ), first_gradient_color, second_gradient_color ) ) {
				dc.FillRectangle( lgbr, thumb_pos.X + 2, thumb_pos.Y + 2, thumb_pos.Width - 4, thumb_pos.Height - 4 );
			}
			
			// outer egdes
			pen = ResPool.GetPen( edge_top_inner_color );
			dc.DrawLine( pen, thumb_pos.X, thumb_pos.Y + 1, thumb_pos.X + 1, thumb_pos.Y );
			dc.DrawLine( pen, thumb_pos.Right - 2, thumb_pos.Y, thumb_pos.Right - 1, thumb_pos.Y + 1 );
			
			pen = ResPool.GetPen( edge_bottom_inner_color );
			dc.DrawLine( pen, thumb_pos.X, thumb_pos.Bottom - 2, thumb_pos.X + 1, thumb_pos.Bottom - 1 );
			dc.DrawLine( pen, thumb_pos.Right - 1, thumb_pos.Bottom - 2, thumb_pos.Right - 2, thumb_pos.Bottom - 1 );
			
			// draw grip lines
			pen = ResPool.GetPen( pressed_inner_border_dark_color );
			dc.DrawLine( pen, thumb_pos.X + 11, thumb_pos.Y + 4, thumb_pos.X + 11, thumb_pos.Y + 8 );
			dc.DrawLine( pen, thumb_pos.X + 14, thumb_pos.Y + 4, thumb_pos.X + 14, thumb_pos.Y + 8 );
			dc.DrawLine( pen, thumb_pos.X + 17, thumb_pos.Y + 4, thumb_pos.X + 17, thumb_pos.Y + 8 );
			
			pen = ResPool.GetPen( Color.White );
			dc.DrawLine( pen, thumb_pos.X + 12, thumb_pos.Y + 4, thumb_pos.X  + 12, thumb_pos.Y + 8 );
			dc.DrawLine( pen, thumb_pos.X + 15, thumb_pos.Y + 4, thumb_pos.X + 15, thumb_pos.Y + 8 );
			dc.DrawLine( pen, thumb_pos.X + 18, thumb_pos.Y + 4, thumb_pos.X + 18, thumb_pos.Y + 8 );
			
			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			/* Draw ticks*/
			thumb_area.Y = thumb_pos.Y;
			thumb_area.X = channel_startpoint.X;
			thumb_area.Height = thumb_pos.Height;
			Region outside = new Region( area );
			outside.Exclude( thumb_area );			
			
			if ( outside.IsVisible( clip_rectangle ) ) {				
				if ( pixels_betweenticks > 0 && ( ( tb.TickStyle & TickStyle.BottomRight ) == TickStyle.BottomRight ||
				    ( ( tb.TickStyle & TickStyle.Both ) == TickStyle.Both ) ) ) {				
					
					for ( float inc = 0; inc < ( pixel_len + 1 ); inc += pixels_betweenticks ) {					
						if ( inc == 0 || ( inc +  pixels_betweenticks ) >= pixel_len + 1 )
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y, 
								    area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y + 3 );
						else
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y, 
								    area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y + 2 );
					}
				}
				
				if ( pixels_betweenticks > 0 && ( ( tb.TickStyle & TickStyle.TopLeft ) == TickStyle.TopLeft ||
				    ( ( tb.TickStyle & TickStyle.Both ) == TickStyle.Both ) ) ) {
					
					for ( float inc = 0; inc < ( pixel_len + 1 ); inc += pixels_betweenticks ) {					
						if ( inc == 0 || ( inc +  pixels_betweenticks ) >= pixel_len + 1 )
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y - 3, 
								    area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y );
						else
							dc.DrawLine( ResPool.GetPen( pen_ticks_color ), area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y - 2, 
								    area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y );
					}			
				}
			}
			
			outside.Dispose( );			
		}
		
		public override void DrawTrackBar( Graphics dc, Rectangle clip_rectangle, TrackBar tb ) {
			int		value_pos;
			bool		mouse_value;
			float		ticks = ( tb.Maximum - tb.Minimum ) / tb.tickFrequency; /* N of ticks draw*/
			Rectangle	area;
			Rectangle	thumb_pos = tb.ThumbPos;
			Rectangle	thumb_area = tb.ThumbArea;
			
			if ( tb.thumb_pressed ) {
				value_pos = tb.thumb_mouseclick;
				mouse_value = true;
			} else {
				value_pos = tb.Value - tb.Minimum;
				mouse_value = false;
			}
			
			area = tb.ClientRectangle;
			
			/* Control Background */
			if ( tb.BackColor == DefaultControlBackColor ) {
				dc.FillRectangle( ResPool.GetSolidBrush( ColorControl ), clip_rectangle );
			} else {
				dc.FillRectangle( ResPool.GetSolidBrush( tb.BackColor ), clip_rectangle );
			}
			
			if ( tb.Orientation == Orientation.Vertical ) {
				DrawTrackBar_Vertical( dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
						      ticks, value_pos, mouse_value );
				
			} else {
				DrawTrackBar_Horizontal( dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
							ticks, value_pos, mouse_value );
			}
			
			// TODO: draw better focus rectangle
			if ( tb.Focused ) {
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControl, Color.Black ), area.X, area.Y, area.Width - 1, 1 );
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControl, Color.Black ), area.X, area.Y + area.Height - 1, area.Width - 1, 1 );
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControl, Color.Black ), area.X, area.Y, 1, area.Height - 1 );
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControl, Color.Black ), area.X + area.Width - 1, area.Y, 1, area.Height - 1 );
			}
			
			tb.ThumbPos = thumb_pos;
			tb.ThumbArea = thumb_area;
		}
		#endregion	// TrackBar
		
		#region ListView
		// draws the ListViewItem of the given index
		protected override void DrawListViewItem( Graphics dc, ListView control, ListViewItem item ) {				
			Rectangle rect_checkrect = item.CheckRectReal;
			Rectangle rect_iconrect = item.GetBounds( ItemBoundsPortion.Icon );
			Rectangle full_rect = item.GetBounds( ItemBoundsPortion.Entire );
			Rectangle text_rect = item.GetBounds( ItemBoundsPortion.Label );			
			
			if ( control.CheckBoxes ) {
				if ( control.StateImageList == null ) {
					// Make sure we've got at least a line width of 1
					int check_wd = Math.Max( 3, rect_checkrect.Width / 6 );
					int scale = Math.Max( 1, rect_checkrect.Width / 12 );
					
					// set the checkbox background
					dc.FillRectangle( this.ResPool.GetSolidBrush( this.ColorWindow ),
							 rect_checkrect );
					// define a rectangle inside the border area
					Rectangle rect = new Rectangle( rect_checkrect.X + 2,
								       rect_checkrect.Y + 2,
								       rect_checkrect.Width - 4,
								       rect_checkrect.Height - 4 );
					Pen pen = new Pen( this.ColorWindowText, 2 );
					dc.DrawRectangle( pen, rect );
					
					// Need to draw a check-mark
					if ( item.Checked ) {
						pen.Width = 1;
						// adjustments to get the check-mark at the right place
						rect.X ++; rect.Y ++;
						// following logic is taken from DrawFrameControl method
						for ( int i = 0; i < check_wd; i++ ) {
							dc.DrawLine( pen, rect.Left + check_wd / 2,
								    rect.Top + check_wd + i,
								    rect.Left + check_wd / 2 + 2 * scale,
								    rect.Top + check_wd + 2 * scale + i );
							dc.DrawLine( pen,
								    rect.Left + check_wd / 2 + 2 * scale,
								    rect.Top + check_wd + 2 * scale + i,
								    rect.Left + check_wd / 2 + 6 * scale,
								    rect.Top + check_wd - 2 * scale + i );
						}
					}
				} else {
					if ( item.Checked && control.StateImageList.Images.Count > 1 )
						control.StateImageList.Draw( dc,
									    rect_checkrect.Location, 1 );
					else if ( ! item.Checked && control.StateImageList.Images.Count > 0 )
						control.StateImageList.Draw( dc,
									    rect_checkrect.Location, 0 );
				}
			}
			
			// Item is drawn as a special case, as it is not just text
			if ( control.View == View.LargeIcon ) {
				if ( item.ImageIndex > -1 &&
				    control.LargeImageList != null &&
				    item.ImageIndex < control.LargeImageList.Images.Count ) {
					// center image
					Point image_location = rect_iconrect.Location;
					Image image = control.LargeImageList.Images[ item.ImageIndex ];
					if ( image.Width < rect_iconrect.Width ) {
						int icon_rect_middle = rect_iconrect.Width / 2;
						int image_middle = image.Width / 2;
						image_location.X = image_location.X + icon_rect_middle - image_middle;
					}
					control.LargeImageList.Draw( dc, image_location,
								    item.ImageIndex );
				}
			} else {
				if ( item.ImageIndex > -1 &&
				    control.SmallImageList != null &&
				    item.ImageIndex < control.SmallImageList.Images.Count )
					control.SmallImageList.Draw( dc, rect_iconrect.Location,
								    item.ImageIndex );
			}
			
			// draw the item text			
			// format for the item text
			StringFormat format = new StringFormat( );
			format.LineAlignment = StringAlignment.Center;
			if ( control.View == View.LargeIcon )
				format.Alignment = StringAlignment.Center; 
			else
				format.Alignment = StringAlignment.Near;
			
			if ( !control.LabelWrap )
				format.FormatFlags = StringFormatFlags.NoWrap;
			
			if ( item.Selected ) {
				if ( control.View == View.Details ) {
					if ( control.FullRowSelect ) {
						// fill the entire rect excluding the checkbox						
						full_rect.Location = item.GetBounds (ItemBoundsPortion.Label).Location;
						dc.FillRectangle( this.ResPool.GetSolidBrush
								 ( this.ColorHighlight ), full_rect );
					} else {
						Size text_size = Size.Ceiling( dc.MeasureString( item.Text,
												item.Font ) );
						text_rect.Width = text_size.Width;
						dc.FillRectangle( this.ResPool.GetSolidBrush
								 ( this.ColorHighlight ), text_rect );
					}
				} else {
					/*Size text_size = Size.Ceiling (dc.MeasureString (item.Text,
					 item.Font));
					 Point loc = text_rect.Location;
					 loc.X += (text_rect.Width - text_size.Width) / 2;
					 text_rect.Width = text_size.Width;*/
					dc.FillRectangle( this.ResPool.GetSolidBrush( this.ColorHighlight ),
							 text_rect );
				}
			} else
				dc.FillRectangle( ResPool.GetSolidBrush( item.BackColor ), text_rect );
			
			if ( item.Text != null && item.Text.Length > 0 ) {
				
				if ( control.View != View.LargeIcon ) {
					if ( item.Selected )
						dc.DrawString( item.Text, item.Font, this.ResPool.GetSolidBrush
							      ( this.ColorHighlightText ), text_rect, format );
					else
						dc.DrawString( item.Text, item.Font, this.ResPool.GetSolidBrush
							      ( item.ForeColor ), text_rect, format );
				} else {
					// ListView CalcTextSize says wrapping is done for two lines only !?!
					// text is centered for the complete text_rect but it should be centered per available row/line
					
					// calculate how much lines we get out of text_rect and current item.Font
					int nr_lines = text_rect.Height / item.Font.Height;
					int rest = text_rect.Height % item.Font.Height;
					int line_height = item.Font.Height + ( rest > 1 ? 2 : 0 );
					
					Rectangle[] text_rects = new Rectangle[ nr_lines ];
					
					for ( int i = 0; i < nr_lines; i++ ) {
						text_rects[ i ].X = text_rect.X;
						text_rects[ i ].Y = text_rect.Y + i * line_height;
						text_rects[ i ].Width = text_rect.Width;
						text_rects[ i ].Height = line_height;
					}
					
					string[] lines = new string[ nr_lines ];
					
					string text = item.Text;
					
					int line_nr = 0;
					int current_pos = 0;
					for ( int k = 1; k <= text.Length; k++ ) {
						lines[ line_nr ] = text.Substring( current_pos, k - current_pos );
						
						// FIXME: Graphics.MeasureString returns wrong results if there is a 
						//        space char in the string
						SizeF sizef = dc.MeasureString( lines[ line_nr ], item.Font, text_rect.Width, format );
						
						if ( (int)sizef.Width > text_rect.Width - 3 ) {
							lines[ line_nr ] = lines[ line_nr ].Remove( lines[ line_nr ].Length - 1, 1 );
							k--;
							current_pos = k;
							line_nr++;
							if ( line_nr == nr_lines )
								break;
						}
					}
					
					int j = 0;
					foreach ( Rectangle t_rect in text_rects ) {
						if ( item.Selected )
							dc.DrawString( lines[ j ], item.Font, this.ResPool.GetSolidBrush
								      ( this.ColorHighlightText ), t_rect, format );
						else
							dc.DrawString( lines[ j ], item.Font, this.ResPool.GetSolidBrush
								      ( item.ForeColor ), t_rect, format );
						j++;
					}
				} 
			}
			
			if ( control.View == View.Details && control.Columns.Count > 0 ) {
				// draw subitems for details view
				ListViewItem.ListViewSubItemCollection subItems = item.SubItems;
				int count = ( control.Columns.Count < subItems.Count ? 
					control.Columns.Count : subItems.Count );
				
				if ( count > 0 ) {
					ColumnHeader col;
					ListViewItem.ListViewSubItem subItem;
					Rectangle sub_item_rect = text_rect; 
					
					// set the format for subitems
					format.FormatFlags = StringFormatFlags.NoWrap;
					format.Alignment = StringAlignment.Near;
					
					// 0th subitem is the item already drawn
					for ( int index = 1; index < count; index++ ) {
						subItem = subItems[ index ];
						col = control.Columns[ index ];
						sub_item_rect.X = col.Rect.Left;
						sub_item_rect.Width = col.Wd;
						sub_item_rect.X -= control.h_marker;
						
						SolidBrush sub_item_back_br = null;
						SolidBrush sub_item_fore_br = null;
						Font sub_item_font = null;
						
						if ( item.UseItemStyleForSubItems ) {
							sub_item_back_br = this.ResPool.GetSolidBrush
							( item.BackColor );
							sub_item_fore_br = this.ResPool.GetSolidBrush
							( item.ForeColor );
							sub_item_font = item.Font;
						} else {
							sub_item_back_br = this.ResPool.GetSolidBrush
							( subItem.BackColor );
							sub_item_fore_br = this.ResPool.GetSolidBrush
							( subItem.ForeColor );
							sub_item_font = subItem.Font;
						}
						
						// In case of fullrowselect, background is filled
						// for the entire rect above
						if ( item.Selected && control.FullRowSelect ) {
							if ( subItem.Text != null && subItem.Text.Length > 0 )
								dc.DrawString( subItem.Text, sub_item_font,
									      this.ResPool.GetSolidBrush
									      ( this.ColorHighlightText ),
									      sub_item_rect, format );
						} else {
							dc.FillRectangle( sub_item_back_br, sub_item_rect );
							if ( subItem.Text != null && subItem.Text.Length > 0 )
								dc.DrawString( subItem.Text, sub_item_font,
									      sub_item_fore_br,
									      sub_item_rect, format );
						}
						sub_item_rect.X += col.Wd;
					}
				}
			}
			
			if ( item.Focused ) {				
				if ( item.Selected )
					CPDrawFocusRectangle( dc, text_rect, ColorHighlightText, ColorHighlight );
				else
					CPDrawFocusRectangle( dc, text_rect, control.ForeColor, control.BackColor );
			}
			
			format.Dispose( );
		}
		#endregion ListView
		
		#region DateTimePicker
		public override void DrawDateTimePicker (Graphics dc,  Rectangle clip_rectangle, DateTimePicker dtp) {
			// if not showing the numeric updown control then render border
			if (!dtp.ShowUpDown && clip_rectangle.IntersectsWith (dtp.ClientRectangle)) {
				// draw the outer border
				Rectangle button_bounds = dtp.ClientRectangle;
				CPDrawBorder3D (dc, button_bounds, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, dtp.BackColor);
				
				// deflate by the border width
				if (clip_rectangle.IntersectsWith (dtp.drop_down_arrow_rect)) {
					button_bounds.Inflate (-2,-2);
					ButtonState state = dtp.is_drop_down_visible ? ButtonState.Pushed : ButtonState.Normal;
					Rectangle button_rect = new Rectangle(dtp.drop_down_arrow_rect.X, dtp.drop_down_arrow_rect.Y + 1,
									      dtp.drop_down_arrow_rect.Width - 1, dtp.drop_down_arrow_rect.Height - 2);
					this.CPDrawComboButton ( 
						dc, 
						button_rect, 
						state);
				}
			}
			
			// render the date part
			if (clip_rectangle.IntersectsWith (dtp.date_area_rect)) {
				// fill the background
				Rectangle date_area_rect = new Rectangle( dtp.date_area_rect.X + 1, dtp.date_area_rect.Y + 1,
									 dtp.date_area_rect.Width - 2,  dtp.date_area_rect.Height - 2);
				dc.FillRectangle (ResPool.GetSolidBrush (ColorWindow), date_area_rect);
				
				// fill the currently highlighted area
				if (dtp.hilight_date_area != Rectangle.Empty) {
					dc.FillRectangle (ResPool.GetSolidBrush (ColorHighlight), dtp.hilight_date_area);
				}
				
				// draw the text part
				// TODO: if date format is CUstom then we need to draw the dates as separate parts
				StringFormat text_format = new StringFormat();
				text_format.LineAlignment = StringAlignment.Center;
				text_format.Alignment = StringAlignment.Near;					
				dc.DrawString (dtp.Text, dtp.Font, ResPool.GetSolidBrush (dtp.ForeColor), Rectangle.Inflate(dtp.date_area_rect, -1, -1), text_format);
				text_format.Dispose ();
			}
		}
		#endregion // DateTimePicker
		
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
				dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), bottom_rect);
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
					Font bold_font = new Font (mc.Font.FontFamily, mc.Font.Size, mc.Font.Style | FontStyle.Bold);
					Rectangle today_rect = new Rectangle (
						today_offset + client_rectangle.X,
						Math.Max(client_rectangle.Bottom - date_cell_size.Height, 0),
						Math.Max(client_rectangle.Width - today_offset, 0),
						date_cell_size.Height);
					dc.DrawString ("Today: " + DateTime.Now.ToShortDateString(), bold_font, ResPool.GetSolidBrush (mc.ForeColor), today_rect, text_format);
					text_format.Dispose ();
					bold_font.Dispose ();
				}				
			}
			
			// finally paint the borders of the calendars as required
			for (int i = 0; i <= mc.CalendarDimensions.Width; i++) {
				if (i == 0 && clip_rectangle.X == client_rectangle.X) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.X, client_rectangle.Y, 1, client_rectangle.Height));
				} else if (i == mc.CalendarDimensions.Width && clip_rectangle.Right == client_rectangle.Right) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.Right-1, client_rectangle.Y, 1, client_rectangle.Height));
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X + (month_size.Width*i) + (calendar_spacing.Width * (i-1)) + 1,
						client_rectangle.Y,
						calendar_spacing.Width,
						client_rectangle.Height);
					if (i < mc.CalendarDimensions.Width && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), rect);
					}
				}
			}
			for (int i = 0; i <= mc.CalendarDimensions.Height; i++) {
				if (i == 0 && clip_rectangle.Y == client_rectangle.Y) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.X, client_rectangle.Y, client_rectangle.Width, 1));
				} else if (i == mc.CalendarDimensions.Height && clip_rectangle.Bottom == client_rectangle.Bottom) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.X, client_rectangle.Bottom-1, client_rectangle.Width, 1));
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X,
						client_rectangle.Y + (month_size.Height*i) + (calendar_spacing.Height*(i-1)) + 1,
						client_rectangle.Width,
						calendar_spacing.Height);
					if (i < mc.CalendarDimensions.Height && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), rect);
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
			
			// set up some standard string formating variables
			StringFormat text_format = new StringFormat();
			text_format.LineAlignment = StringAlignment.Center;
			text_format.Alignment = StringAlignment.Center;
			
			
			// draw the title back ground
			DateTime this_month = current_month.AddMonths (row*mc.CalendarDimensions.Width+col);
			Rectangle title_rect = new Rectangle(rectangle.X, rectangle.Y, title_size.Width, title_size.Height);
			if (title_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), title_rect);
				// draw the title				
				string title_text = this_month.ToString ("MMMM yyyy");
				dc.DrawString (title_text, mc.Font, ResPool.GetSolidBrush (mc.TitleForeColor), title_rect, text_format);
				
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
				dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), day_name_rect);
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
					dc.DrawString (((DayOfWeek)i).ToString().Substring(0, 3), mc.Font, ResPool.GetSolidBrush (mc.TitleBackColor), day_rect, text_format);
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
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), row_rect);
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
							text_format);
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
			text_format.Dispose ();
		}
		
		// draws the pervious or next button
		private void DrawMonthCalendarButton (Graphics dc, Rectangle rectangle, MonthCalendar mc, Size title_size, int x_offset, Size button_size, bool is_previous) 
		{
			bool is_clicked = false;
			Rectangle button_rect;
			Rectangle arrow_rect = new Rectangle (rectangle.X, rectangle.Y, 4, 7);
			Point[] arrow_path = new Point[3];
			// prepare the button
			if (is_previous) 
			{
				is_clicked = mc.is_previous_clicked;
				button_rect = new Rectangle (
					rectangle.X + 1 + x_offset,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));
				arrow_rect.X = button_rect.X + ((button_rect.Width - arrow_rect.Width)/2);
				arrow_rect.Y = button_rect.Y + ((button_rect.Height - arrow_rect.Height)/2);
				if (is_clicked) {
					arrow_rect.Offset(1,1);
				}
				arrow_path[0] = new Point (arrow_rect.Right, arrow_rect.Y);
				arrow_path[1] = new Point (arrow_rect.X, arrow_rect.Y + arrow_rect.Height/2);
				arrow_path[2] = new Point (arrow_rect.Right, arrow_rect.Bottom);
			}
			else
			{
				is_clicked = mc.is_next_clicked;
				button_rect = new Rectangle (
					rectangle.Right - 1 - x_offset - button_size.Width,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));
				arrow_rect.X = button_rect.X + ((button_rect.Width - arrow_rect.Width)/2);
				arrow_rect.Y = button_rect.Y + ((button_rect.Height - arrow_rect.Height)/2);
				if (is_clicked) {
					arrow_rect.Offset(1,1);
				}
				arrow_path[0] = new Point (arrow_rect.X, arrow_rect.Y);
				arrow_path[1] = new Point (arrow_rect.Right, arrow_rect.Y + arrow_rect.Height/2);
				arrow_path[2] = new Point (arrow_rect.X, arrow_rect.Bottom);				
			}
			
			// fill the background
			dc.FillRectangle (ResPool.GetSolidBrush(mc.TitleBackColor), button_rect);
			
			// draw the button
			Color first_gradient_color = is_clicked ? pressed_gradient_first_color : gradient_first_color;
			Color second_gradient_color = is_clicked ? pressed_gradient_second_color : gradient_second_color;
			
			CL_Draw_Button (dc, button_rect, FlatStyle.Standard,
					   false, true, is_clicked, 
					   first_gradient_color, second_gradient_color,
					   false);
				
			// draw the arrow
			SmoothingMode old_smooting_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			dc.FillPolygon (SystemBrushes.ControlText, arrow_path);
			dc.SmoothingMode = old_smooting_mode;
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
			
			
			if (date == mc.SelectionStart && date == mc.SelectionEnd) {
				// see if the date is in the start of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, -3, -3);				
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 0, 359);
			} else if (date == mc.SelectionStart) {
				// see if the date is in the start of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, -3, -3);				
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 90, 180);
				// fill the other side as a straight rect
				if (date < mc.SelectionEnd) 
				{
					// use rectangle instead of rectangle to go all the way to edge of rect
					selection_rect.X = (int) Math.Floor((double)(rectangle.X + rectangle.Width / 2));
					selection_rect.Width = Math.Max(rectangle.Right - selection_rect.X, 0);
					dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
				}
			} else if (date == mc.SelectionEnd) {
				// see if it is the end of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, -3, -3);
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 270, 180);
				// fill the other side as a straight rect
				if (date > mc.SelectionStart) {
					selection_rect.X = rectangle.X;
					selection_rect.Width = rectangle.Width - (rectangle.Width / 2);
					dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
				}
			} else if (date > mc.SelectionStart && date < mc.SelectionEnd) {
				// now see if it's in the middle
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, 0, -3);
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
			}
			
			// set up some standard string formating variables
			StringFormat text_format = new StringFormat();
			text_format.LineAlignment = StringAlignment.Center;
			text_format.Alignment = StringAlignment.Center;
			
			
			// establish if it's a bolded font
			Font font;
			if (mc.IsBoldedDate (date)) {
				font = new Font (mc.Font.FontFamily, mc.Font.Size, mc.Font.Style | FontStyle.Bold);
			} else {
				font = mc.Font;
			}
			
			// just draw the date now
			dc.DrawString (date.Day.ToString(), font, ResPool.GetSolidBrush (date_color), rectangle, text_format);
			
			// today circle if needed
			if (mc.ShowTodayCircle && date == DateTime.Now.Date) {
				DrawTodayCircle (dc, interior);
			}
			
			// draw the selection grid
			if (mc.is_date_clicked && mc.clicked_date == date) {				
				using (Pen pen = new Pen (Color.Black, 1) ) {
					pen.DashStyle = DashStyle.Dot;
					dc.DrawRectangle (pen, interior);
				}
			}
			text_format.Dispose ();
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
			
			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			using (Pen pen = new Pen (circle_color, 2)) {
				dc.DrawArc (pen, lhs_circle_rect, 90, 180);
				dc.DrawArc (pen, rhs_circle_rect, 270, 180);					
				dc.DrawCurve (pen, curve_points);
				dc.DrawLine (ResPool.GetPen (circle_color), curve_points [2], new Point (curve_points [2].X, lhs_circle_rect.Y));
			}
			
			dc.SmoothingMode = old_smoothing_mode;
		}
		#endregion 	// MonthCalendar
		
		public override void CPDrawBorder3D( Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides ) {
			CPDrawBorder3D( graphics, rectangle, style, sides, ColorControl );
		}
		
		public override void CPDrawBorder3D( Graphics dc, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color ) {
			// currently we don't take care of Border3DStyle or Border3DSide
			
			Pen tmp_pen = ResPool.GetPen( edge_bottom_inner_color );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y + 2, rectangle.X + 2, rectangle.Y + 1 );
			dc.DrawLine( tmp_pen, rectangle.Right - 3, rectangle.Y + 1, rectangle.Right - 2, rectangle.Y + 2 );
			dc.DrawLine( tmp_pen, rectangle.Right - 3, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 3 );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Bottom - 3, rectangle.X + 2, rectangle.Bottom - 2 );
			
			tmp_pen = ResPool.GetPen( theme_back_color );
			dc.DrawLine( tmp_pen, rectangle.X + 2, rectangle.Y + 2, rectangle.Right - 3, rectangle.Y + 2 );
			dc.DrawLine( tmp_pen, rectangle.X + 2, rectangle.Y + 3, rectangle.X + 2, rectangle.Bottom - 3 );
			
			tmp_pen = ResPool.GetPen( Color.White );
			dc.DrawLine( tmp_pen, rectangle.X + 3, rectangle.Bottom - 3, rectangle.Right - 3, rectangle.Bottom - 3 );
			dc.DrawLine( tmp_pen, rectangle.Right - 3, rectangle.Y + 3, rectangle.Right - 3, rectangle.Bottom - 3 );
			
			Point[] points = {
				new Point( rectangle.X + 3, rectangle.Y + 1 ),
				new Point( rectangle.Right - 4, rectangle.Y + 1 ),
				new Point( rectangle.Right - 2, rectangle.Y + 3 ),
				new Point( rectangle.Right - 2, rectangle.Bottom - 4 ),
				new Point( rectangle.Right - 4, rectangle.Bottom - 2 ),
				new Point( rectangle.X + 3, rectangle.Bottom - 2 ),
				new Point( rectangle.X + 1, rectangle.Bottom - 4 ),
				new Point( rectangle.X + 1, rectangle.Y + 3 ),
				new Point( rectangle.X + 3, rectangle.Y + 1 )
			};
			
			dc.DrawLines( ResPool.GetPen( combobox_border_color ), points );
			
			Point[] points_top_outer = {
				new Point( rectangle.X + 1, rectangle.Y + 1 ),
				new Point( rectangle.X + 2, rectangle.Y ),
				new Point( rectangle.Right - 3, rectangle.Y ),
				new Point( rectangle.Right - 2 , rectangle.Y + 1 )
			};
			
			Point[] points_bottom_outer = {
				new Point( rectangle.X + 1, rectangle.Bottom - 2 ),
				new Point( rectangle.X + 2, rectangle.Bottom - 1 ),
				new Point( rectangle.Right - 3, rectangle.Bottom - 1 ),
				new Point( rectangle.Right - 2, rectangle.Bottom - 2 )
			};
			
			// outer border
			tmp_pen = ResPool.GetPen( button_outer_border_dark_color );
			dc.DrawLines( tmp_pen, points_top_outer );
			tmp_pen = ResPool.GetPen( button_outer_border_light_color );
			dc.DrawLines( tmp_pen, points_bottom_outer );
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rectangle.X, rectangle.Y + 2 ), new Point( rectangle.X, rectangle.Bottom - 3 ), button_outer_border_dark_color, button_outer_border_light_color ) ) {
				using (Pen lgbrpen = new Pen (lgbr)) {
					dc.DrawLine (lgbrpen, rectangle.X, rectangle.Y + 2, rectangle.X, rectangle.Bottom - 3 );
					dc.DrawLine (lgbrpen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom - 3 );
				}
			}
			
			tmp_pen = ResPool.GetPen( button_edge_top_outer_color );
			dc.DrawLine( tmp_pen, rectangle.X, rectangle.Y + 1, rectangle.X + 1, rectangle.Y );
			dc.DrawLine( tmp_pen, rectangle.Right - 2, rectangle.Y, rectangle.Right - 1, rectangle.Y + 1 );
			
			tmp_pen = ResPool.GetPen( button_edge_bottom_outer_color );
			dc.DrawLine( tmp_pen, rectangle.X, rectangle.Bottom - 2, rectangle.X + 1, rectangle.Bottom - 1 );
			dc.DrawLine( tmp_pen, rectangle.Right - 1, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 1 );
		}
		
		public override void CPDrawBorder( Graphics dc, Rectangle bounds, Color leftColor, int leftWidth,
						  ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
						  Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
						  int bottomWidth, ButtonBorderStyle bottomStyle ) {
			dc.DrawRectangle( ResPool.GetPen( combobox_border_color ), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1 );
		}
		
		// TODO: inactive...
		public override void CPDrawCheckBox( Graphics dc, Rectangle rectangle, ButtonState state ) {
			
			bool pushed = ( state & ButtonState.Pushed ) != 0;
			
			int lineWidth;
			Rectangle rect;
			int scale;
			
			// background
			dc.FillRectangle( ResPool.GetSolidBrush( pushed ? checkbox_pressed_backcolor : Color.White ), rectangle );
			
			// border
			dc.DrawRectangle( ResPool.GetPen( scrollbar_border_color ), rectangle );
			
			Color inner_border_color = pushed ? checkbox_pressed_inner_boder_color : checkbox_inner_boder_color;
			
			Pen tmp_pen = ResPool.GetPen( inner_border_color );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y + 1, rectangle.Right - 1, rectangle.Y + 1 );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y + 2, rectangle.X + 1, rectangle.Bottom - 1 );
			
			/* Make sure we've got at least a line width of 1 */
			lineWidth = Math.Max( 3, rectangle.Width / 6 );
			scale = Math.Max( 1, rectangle.Width / 12 );
			
			// define a rectangle inside the border area
			rect = new Rectangle( rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4 );
			if ( ( state & ButtonState.Inactive ) != 0 ) {
				tmp_pen = SystemPens.ControlDark;
			} else {
				tmp_pen = SystemPens.ControlText;
			}
			
			if ( ( state & ButtonState.Checked ) != 0 ) { 
				/* Need to draw a check-mark */
				for ( int i=0; i < lineWidth; i++ ) {
					dc.DrawLine( tmp_pen, rect.Left + lineWidth / 2, rect.Top + lineWidth + i, rect.Left + lineWidth / 2 + 2 * scale, rect.Top + lineWidth + 2 * scale + i );
					dc.DrawLine( tmp_pen, rect.Left + lineWidth / 2 + 2 * scale, rect.Top + lineWidth + 2 * scale + i, rect.Left + lineWidth / 2 + 6 * scale, rect.Top + lineWidth - 2 * scale + i );
				}
			}
		}
		
		public  override void CPDrawStringDisabled( Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
							   StringFormat format ) {			
			
			graphics.DrawString( s, font, ResPool.GetSolidBrush( ColorGrayText), layoutRectangle, format );			
			
		}
		
		public override void CPDrawButton (Graphics dc, Rectangle buttonRectangle, ButtonState state)
		{
			bool is_enabled = true;
			FlatStyle flat_style = FlatStyle.Standard;
			bool is_pressed = false;
			
			if ((state & ButtonState.Pushed) != 0) {
				is_pressed = true;
			}
			
//			if ((state & ButtonState.Checked)!=0) {
//				dfcs |= DrawFrameControlStates.Checked;
//			}
			
			if ((state & ButtonState.Flat) != 0) {
				flat_style = FlatStyle.Flat;
			}
			
			if ((state & ButtonState.Inactive) != 0) {
				is_enabled = false;
			}
			
			Color first_gradient_color = gradient_first_color;
			Color second_gradient_color = gradient_second_color;
			
			if (is_pressed) {
				first_gradient_color = pressed_gradient_first_color;
				second_gradient_color = pressed_gradient_second_color;
			}
			
			CL_Draw_Button (dc, buttonRectangle, flat_style,
					false, is_enabled, is_pressed,
					first_gradient_color, second_gradient_color,
					false);
		}
		
		public override void CPDrawRadioButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			bool is_checked = false;
			bool is_inactive = false;
			
			if ((state & ButtonState.Checked) != 0) {
				is_checked = true;
			}
			
			if ((state & ButtonState.Inactive) != 0) {
				is_inactive = true;
			}
			
			SmoothingMode old_smooting_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			dc.FillPie (ResPool.GetSolidBrush (this.ColorWindow), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
			
			dc.DrawArc (ResPool.GetPen (radio_button_border_circle_color), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
			
			CL_Draw_RadioButton_Dot (dc, rectangle, is_checked, is_inactive);
			
			dc.SmoothingMode = old_smooting_mode;
		}

		private void CL_Draw_RadioButton_Dot (Graphics dc,  Rectangle rectangle, bool is_checked, bool is_inactive)
		{
			if (is_checked) {
				int lineWidth = Math.Max (1, Math.Min (rectangle.Width, rectangle.Height) / 4);
				
				SolidBrush buttonBrush;
				
				if (is_inactive) {
					buttonBrush = SystemBrushes.ControlDark as SolidBrush;
				} else {
					buttonBrush = ResPool.GetSolidBrush (radio_button_dot_color);
				}
				dc.FillPie (buttonBrush, rectangle.X + lineWidth, rectangle.Y + lineWidth, rectangle.Width - lineWidth * 2, rectangle.Height - lineWidth * 2, 0, 359);
				
				// the white shiny dott
				buttonBrush = ResPool.GetSolidBrush (ColorWindow);
				dc.FillPie (buttonBrush, rectangle.X + lineWidth + lineWidth / 2, rectangle.Y + lineWidth + lineWidth / 2, (rectangle.Width - lineWidth * 2) / 3, (rectangle.Height - lineWidth * 2) / 3, 0, 359);
			}
		}

		#region ToolTip
		public override void DrawToolTip (Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control)
		{
			dc.FillRectangle(ResPool.GetSolidBrush (ColorInfo), control.ClientRectangle);
			dc.DrawRectangle(ResPool.GetPen (info_border_color), 0, 0, control.Width-1, control.Height-1);
	
			TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
			TextRenderer.DrawTextInternal (dc, control.Text, control.Font, control.ClientRectangle, this.ColorInfoText, flags, false);
		}
		#endregion	// ToolTip

		#region BalloonWindow
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

		private const int balloon_iconsize = 48;
		private const int balloon_bordersize = 8; 
		
		public override void DrawBalloonWindow (Graphics dc, Rectangle clip, NotifyIcon.BalloonWindow control) 
		{
			Brush solidbrush = ResPool.GetSolidBrush(this.ColorInfoText);
			Rectangle rect = control.ClientRectangle;
			int iconsize = (control.Icon == ToolTipIcon.None) ? 0 : balloon_iconsize;
			
			// Rectangle borders and background.
			dc.FillRectangle (ResPool.GetSolidBrush (ColorInfo), rect);
			dc.FillRectangle (ResPool.GetSolidBrush (info_second_color), new Rectangle (rect.X, rect.Y, (balloon_iconsize/2)+balloon_bordersize, rect.Height));
			dc.DrawRectangle (ResPool.GetPen (info_border_color), 0, 0, rect.Width - 1, rect.Height - 1);

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
				dc.DrawImage (image, new Rectangle (balloon_bordersize, (2*balloon_bordersize)+7, iconsize, iconsize));
			
			// Title
			Rectangle titlerect = new Rectangle (rect.X + balloon_bordersize + (balloon_iconsize/2) + balloon_bordersize, 
												rect.Y + balloon_bordersize, 
												rect.Width - ((3 * balloon_bordersize) + (balloon_iconsize/2)), 
												rect.Height - (2 * balloon_bordersize));
			
			Font titlefont = new Font (control.Font.FontFamily, control.Font.Size, control.Font.Style | FontStyle.Bold, control.Font.Unit);
			dc.DrawString (control.Title, titlefont, solidbrush, titlerect, control.Format);
			
			// Text
			Rectangle textrect = new Rectangle (rect.X + (2 * balloon_bordersize) + balloon_iconsize, 
												rect.Y + balloon_bordersize, 
												rect.Width - ((2 * balloon_bordersize) + balloon_iconsize), 
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
			
			if (textsize.Height < balloon_iconsize)
				textsize.Height = balloon_iconsize;
			
			Rectangle rect = new Rectangle ();
			rect.Height = (int) (titlesize.Height + textsize.Height + (3 * balloon_bordersize));
			rect.Width = (int) ((titlesize.Width > textsize.Width) ? titlesize.Width : textsize.Width) + (3 * balloon_bordersize) + balloon_iconsize;
			rect.X = deskrect.Width - rect.Width - 2;
			rect.Y = deskrect.Height - rect.Height - 2;
			
			return rect;
		}
		#endregion	// BalloonWindow
		
	} //class
}


