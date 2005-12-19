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
// Copyright (c) 2004-2005 Novell, Inc.
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
//	- RadioButton !?!
//	- TabControl: TabAlignment.Left and TabAlignment.Bottom
//	- if an other control draws over a ScrollBar button you can see artefacts on the rounded edges 
//	  (maybe use theme backcolor, but that looks ugly on a white background, need to find a way to get the backcolor of the parent control)
//	- correct drawing of disabled controls (for example ComboBox... )

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace System.Windows.Forms {
	internal class ThemeClearlooks : ThemeWin32Classic {
		public override Version Version {
			get {
				return new Version( 0, 0, 0, 1 );
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
		
		static readonly Color arrow_color = Color.FromArgb( 16, 16, 16 );
		
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
		static readonly Color combobox_focus_inner_border_color = Color.FromArgb( 167, 198, 225 );
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
		
		const int SEPARATOR_HEIGHT = 7;
    		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tab
    		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items
		
		#region	Principal Theme Methods
		public ThemeClearlooks( ) {
			ColorControl = theme_back_color;
			always_draw_hotkeys = true;
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
		
		public override Size Border3DSize {
			get {
				return new Size( 3, 3 );
			}
		}
		#endregion	// Internal Methods
		
		#region ButtonBase
		// FIXME: button style flat ?
		protected override void ButtonBase_DrawButton( ButtonBase button, Graphics dc ) {
			Rectangle buttonRectangle;
			
			int width = button.ClientSize.Width;
			int height = button.ClientSize.Height;
			
			dc.FillRectangle( ResPool.GetSolidBrush( button.BackColor ), button.ClientRectangle );
			
			// set up the button rectangle
			buttonRectangle = button.ClientRectangle;
			
			Color first_gradient_color = button.has_focus ? Color.LightYellow : Color.White;
			Color second_gradient_color = Color.White;
			
			if ( ( ( button is CheckBox ) && ( ( (CheckBox)button ).check_state == CheckState.Checked ) ) ||
			    ( ( button is RadioButton ) && ( ( (RadioButton)button ).check_state == CheckState.Checked ) ) ) {
				first_gradient_color = button.is_entered ? gradient_second_color : pressed_gradient_first_color;
				second_gradient_color = button.is_entered  ? gradient_second_color : pressed_gradient_second_color;
			} else
			if ( !button.is_enabled ) {
				first_gradient_color = gradient_first_color;
				second_gradient_color = gradient_second_color;
				button.is_entered = false;
			} else
			if ( !button.is_entered ) {
				first_gradient_color = gradient_first_color;
				second_gradient_color = gradient_second_color;
			} else {
				if ( !button.is_pressed ) {
					first_gradient_color = Color.White;
					second_gradient_color = button_mouse_entered_second_gradient_color;
				} else {
					first_gradient_color = pressed_gradient_first_color;
					second_gradient_color = pressed_gradient_second_color;
				}
			}
			
			Rectangle lgbRectangle = new Rectangle( buttonRectangle.X + 3, buttonRectangle.Y + 3, button.is_pressed ? buttonRectangle.Width - 5 : buttonRectangle.Width - 6, buttonRectangle.Height - 6 );
			
			if ( button.flat_style != FlatStyle.Popup || ( ( button.flat_style == FlatStyle.Popup ) && button.is_entered ) ) {
				LinearGradientBrush lgbr;
				if ( button.flat_style == FlatStyle.Flat )
					lgbr = new LinearGradientBrush( new Point( 0, 3 ), new Point( 0, height - 3 ), second_gradient_color, first_gradient_color );
				else
					lgbr = new LinearGradientBrush( new Point( 0, 3 ), new Point( 0, height - 3 ), first_gradient_color, second_gradient_color );
				dc.FillRectangle( lgbr, lgbRectangle );
				lgbr.Dispose( );
				
				Point[] points_top = {
					new Point( 2, 2 ),
					new Point( 3, 1 ),
					new Point( width - 4, 1 ),
					new Point( width - 3 , 2 )
				};
				
				Point[] points_bottom = {
					new Point( 2, height - 3 ),
					new Point( 3, height - 2 ),
					new Point( width - 4, height - 2 ),
					new Point( width - 3, height - 3 )
				};
				
				Point[] points_top_outer = {
					new Point( 1, 1 ),
					new Point( 2, 0 ),
					new Point( width - 3, 0 ),
					new Point( width - 2 , 1 )
				};
				
				Point[] points_bottom_outer = {
					new Point( 1, height - 2 ),
					new Point( 2, height - 1 ),
					new Point( width - 3, height - 1 ),
					new Point( width - 2, height - 2 )
				};
				
				Pen pen = null; 
				
				// normal border
				if ( button.is_enabled ) { 
					bool paint_acceptbutton_black_border = false;
					Form form = button.TopLevelControl as Form;
					
					if ( form != null && ( form.AcceptButton == button as IButtonControl ) )
						paint_acceptbutton_black_border = true;
					
					Color top_color = Color.Black;
					Color bottom_color = Color.Black;
					
					if ( !paint_acceptbutton_black_border ) {
						top_color = button.is_pressed ? border_pressed_dark_color : border_normal_dark_color;
						bottom_color = button.is_pressed ? border_pressed_light_color : border_normal_light_color;
					}
					
					pen = ResPool.GetPen( top_color );
					dc.DrawLines( pen, points_top );
					pen = ResPool.GetPen( bottom_color );
					dc.DrawLines( pen, points_bottom );
					
					using ( LinearGradientBrush lgbr2 = new LinearGradientBrush( new Point( 0, 3 ), new Point( 0, height - 3 ), top_color, bottom_color ) ) {
						dc.FillRectangle( lgbr2, 1, 3, 1, height - 6 );
						dc.FillRectangle( lgbr2, width - 2, 3, 1, height - 6 );
					}
				} else {
					Point[] points_button_complete = {
						new Point( 1, 3 ),
						new Point( 3, 1 ),
						new Point( width - 4, 1 ),
						new Point( width - 2, 3 ),
						new Point( width - 2, height - 4 ),
						new Point( width - 4, height - 2 ),
						new Point( 3, height - 2 ),
						new Point( 1, height - 4 ),
						new Point( 1, 3 )
					};
					
					pen = ResPool.GetPen( pressed_inner_border_dark_color );
					dc.DrawLines( pen, points_button_complete );
				}
				
				// outer border
				pen = ResPool.GetPen( button_outer_border_dark_color );
				dc.DrawLines( pen, points_top_outer );
				pen = ResPool.GetPen( button_outer_border_light_color );
				dc.DrawLines( pen, points_bottom_outer );
				
				using ( LinearGradientBrush lgbr2 = new LinearGradientBrush( new Point( 0, 2 ), new Point( 0, height - 1 ), button_outer_border_dark_color, button_outer_border_light_color ) ) {
					dc.FillRectangle( lgbr2, 0, 2, 1, height - 4 );
					dc.FillRectangle( lgbr2, width - 1, 2, 1, height - 4 );
				}
				
				// inner border
				pen = ResPool.GetPen( button.is_pressed ? pressed_inner_border_dark_color : inner_border_dark_color );
				if ( !button.is_pressed ) {
					dc.DrawLine( pen, width - 3, 3, width - 3, height - 4 );
				}
				dc.DrawLine( pen, 3, height - 3, width - 4, height - 3 );
				pen = ResPool.GetPen( button.is_pressed ? pressed_inner_border_dark_color : Color.White );
				dc.DrawLine( pen, 2, 3, 2, height - 4 );
				dc.DrawLine( pen, 3 , 2, width - 4, 2 );
				
				// edges
				pen = ResPool.GetPen( edge_top_inner_color );
				dc.DrawLine( pen, 1, 2, 2, 1 );
				dc.DrawLine( pen, width - 3, 1, width - 2, 2 );
				
				pen = ResPool.GetPen( button_edge_top_outer_color );
				dc.DrawLine( pen, 0, 1, 1, 0 );
				dc.DrawLine( pen, width - 2, 0, width - 1, 1 );
				
				pen = ResPool.GetPen( edge_bottom_inner_color );
				dc.DrawLine( pen, 1, height - 3, 2, height - 2 );
				dc.DrawLine( pen, width - 2, height - 3, width - 3, height - 2 );
				
				pen = ResPool.GetPen( button_edge_bottom_outer_color );
				dc.DrawLine( pen, 0, height - 2, 1, height - 1 );
				dc.DrawLine( pen, width - 1, height - 2, width - 2, height - 1 );
			}
		}
		
		protected override void ButtonBase_DrawFocus( ButtonBase button, Graphics dc ) {
			
			if ( !button.is_enabled || button.flat_style == FlatStyle.Popup )
				return;
			
			Pen pen = ResPool.GetPen( button_focus_color );
			DashStyle old_dash_style = pen.DashStyle;
			pen.DashStyle = DashStyle.Dot;
			
			Rectangle focus_rect = new Rectangle( button.ClientRectangle.X + 4, button.ClientRectangle.Y + 4, button.ClientRectangle.Width - 9, button.ClientRectangle.Height - 9 );
			
			dc.DrawRectangle( pen, focus_rect );
			
			pen.DashStyle = old_dash_style;
		}
		
		protected override void ButtonBase_DrawText( ButtonBase button, Graphics dc ) {
			if ( !( button is CheckBox ) && !( button is RadioButton ) ) {
				base.ButtonBase_DrawText( button, dc );
			}
		}
		#endregion	// ButtonBase
		
		#region CheckBox
		protected override void CheckBox_DrawCheckBox( Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle ) {
			dc.FillRectangle( ResPool.GetSolidBrush( checkbox.BackColor ), checkbox.ClientRectangle );
			// render as per normal button
			if ( checkbox.appearance == Appearance.Button ) {
				DrawButtonBase( dc, checkbox.ClientRectangle, checkbox );
			} else {
				// establish if we are rendering a flat style of some sort
				if ( checkbox.FlatStyle == FlatStyle.Flat || checkbox.FlatStyle == FlatStyle.Popup ) {
					DrawFlatStyleCheckBox( dc, checkbox_rectangle, checkbox );
				} else {
					ControlPaint.DrawCheckBox( dc, checkbox_rectangle, state );
				}
			}
		}
		#endregion	// CheckBox
		
		#region ComboBox
		
		// Drawing
		// FIXME: sometimes there are some artefacts left...
		public override void DrawComboBoxEditDecorations( Graphics dc, ComboBox ctrl, Rectangle cl ) {
			
			if ( !ctrl.Focused ) {
				Pen tmp_pen = ResPool.GetPen( theme_back_color );
				dc.DrawLine( tmp_pen, cl.X + 1, cl.Y + 1, cl.X + 1, cl.Bottom - 3 );
				dc.DrawLine( tmp_pen, cl.X + 2, cl.Y + 1, cl.Right - 2, cl.Y + 1 );
				
				tmp_pen = ResPool.GetPen( ctrl.Parent.BackColor );
				dc.DrawLine( tmp_pen, cl.X, cl.Y, cl.X, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.X, cl.Bottom - 2, cl.X, cl.Bottom - 3 );
				dc.DrawLine( tmp_pen, cl.Right - 1, cl.Y, cl.Right - 1, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.Right - 1, cl.Bottom - 2, cl.Right - 1, cl.Bottom - 3 );
				
				dc.DrawLine( tmp_pen, cl.X, cl.Bottom, cl.Right, cl.Bottom );
				
				dc.DrawLine( tmp_pen, cl.X, cl.Y + cl.Height - 1, cl.X + cl.Width, cl.Y + cl.Height - 1 );
				
				Point[] points = {
					new Point( cl.X + 2, cl.Y ),
					new Point( cl.Right - 3, cl.Y ),
					new Point( cl.Right - 1, cl.Y + 2 ),
					new Point( cl.Right - 1, cl.Bottom - 4 ),
					new Point( cl.Right - 3, cl.Bottom - 2 ),
					new Point( cl.X + 2, cl.Bottom - 2 ),
					new Point( cl.X, cl.Bottom - 4 ),
					new Point( cl.X, cl.Y + 2 ),
					new Point( cl.X + 2, cl.Y )
				};
				
				dc.DrawLines( ResPool.GetPen( combobox_border_color ), points );
				
				tmp_pen = ResPool.GetPen( edge_bottom_inner_color );
				dc.DrawLine( tmp_pen, cl.X, cl.Y + 1, cl.X + 1, cl.Y );
				dc.DrawLine( tmp_pen, cl.X, cl.Bottom - 3, cl.X + 1, cl.Bottom - 2 );
				dc.DrawLine( tmp_pen, cl.Right - 2, cl.Y, cl.Right - 1, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.Right - 2, cl.Bottom - 2, cl.Right - 1, cl.Bottom - 3 );
			} else { 
				Pen tmp_pen = ResPool.GetPen( combobox_focus_inner_border_color );
				
				dc.DrawLine( tmp_pen, cl.X + 1, cl.Y + 1, cl.X + 1, cl.Bottom - 3 );
				dc.DrawLine( tmp_pen, cl.X + 2, cl.Y + 1, cl.Right - 2, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.X + 2, cl.Bottom - 3, cl.Right - 2, cl.Bottom - 3 );
				dc.DrawLine( tmp_pen, cl.Right - 2, cl.Y + 1, cl.Right - 2, cl.Bottom - 3 );
				
				tmp_pen = ResPool.GetPen( ctrl.Parent.BackColor );
				dc.DrawLine( tmp_pen, cl.X, cl.Y, cl.X, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.X, cl.Bottom - 2, cl.X, cl.Bottom - 3 );
				dc.DrawLine( tmp_pen, cl.Right - 1, cl.Y, cl.Right - 1, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.Right - 1, cl.Bottom - 2, cl.Right - 1, cl.Bottom - 3 );
				
				dc.DrawLine( tmp_pen, cl.X, cl.Bottom, cl.Right, cl.Bottom );
				
				dc.DrawLine( tmp_pen, cl.X, cl.Y + cl.Height - 1, cl.X + cl.Width, cl.Y + cl.Height - 1 );
				
				Point[] points = {
					new Point( cl.X + 2, cl.Y ),
					new Point( cl.Right - 3, cl.Y ),
					new Point( cl.Right - 1, cl.Y + 2 ),
					new Point( cl.Right - 1, cl.Bottom - 4 ),
					new Point( cl.Right - 3, cl.Bottom - 2 ),
					new Point( cl.X + 2, cl.Bottom - 2 ),
					new Point( cl.X, cl.Bottom - 4 ),
					new Point( cl.X, cl.Y + 2 ),
					new Point( cl.X + 2, cl.Y )
				};
				
				dc.DrawLines( ResPool.GetPen( combobox_focus_border_color ), points );
				
				tmp_pen = ResPool.GetPen( edge_bottom_inner_color );
				dc.DrawLine( tmp_pen, cl.X, cl.Y + 1, cl.X + 1, cl.Y );
				dc.DrawLine( tmp_pen, cl.X, cl.Bottom - 3, cl.X + 1, cl.Bottom - 2 );
				dc.DrawLine( tmp_pen, cl.Right - 2, cl.Y, cl.Right - 1, cl.Y + 1 );
				dc.DrawLine( tmp_pen, cl.Right - 2, cl.Bottom - 2, cl.Right - 1, cl.Bottom - 3 );
			}
			
			// FIXME:
			// here we need to draw the combobox button again,
			// as DrawComboBoxEditDecorations paints over a fullsize combox button
			// ComboBox code calls CPDrawComboButton first then DrawComboBoxEditDecorations
			// a fix should go to ComboBox
			
			if ( ctrl.combobox_info.show_button )
				CPDrawComboButton( dc, ctrl.combobox_info.button_rect, ctrl.combobox_info.button_status );
			else {
				// quick and ugly fix for combobox artefacts on the inner border of the right side if no button gets drawn
				Pen tmp_pen = ResPool.GetPen( Color.White );
				dc.DrawLine( tmp_pen, cl.Right - 2, cl.Y + 2, cl.Right - 2, cl.Bottom - 4 );
			}
		}
		
		public override void DrawComboListBoxDecorations( Graphics dc, ComboBox ctrl, Rectangle cl ) {
			if ( ctrl.DropDownStyle == ComboBoxStyle.Simple ) {
				DrawComboBoxEditDecorations( dc, ctrl, cl );
			} else {
				dc.DrawRectangle( ResPool.GetPen( ThemeEngine.Current.ColorWindowFrame ), cl.X, cl.Y, cl.Width - 1, cl.Height - 1 );
			}
		}		
		#endregion ComboBox
		
		#region Menus
		public override void CalcItemSize( Graphics dc, MenuItem item, int y, int x, bool menuBar ) {
			item.X = x;
			item.Y = y;
			
			if ( item.Visible == false )
				return;
			
			if ( item.Separator == true ) {
				item.Height = SEPARATOR_HEIGHT / 2;
				item.Width = -1;
				return;
			}
			
			if ( item.MeasureEventDefined ) {
				MeasureItemEventArgs mi = new MeasureItemEventArgs( dc, item.Index );
				item.PerformMeasureItem( mi );
				item.Height = mi.ItemHeight;
				item.Width = mi.ItemWidth;
				return;
			} else {		
				SizeF size;
				size =  dc.MeasureString( item.Text, ThemeEngine.Current.MenuFont );
				item.Width = (int) size.Width;
				item.Height = (int) size.Height;
				
				if ( !menuBar ) {
					if ( item.Shortcut != Shortcut.None && item.ShowShortcut ) {
						item.XTab = ThemeEngine.Current.MenuCheckSize.Width + MENU_TAB_SPACE + (int) size.Width;
						size =  dc.MeasureString( " " + item.GetShortCutText( ), ThemeEngine.Current.MenuFont );
						item.Width += MENU_TAB_SPACE + (int) size.Width;
					}
					
					item.Width += 4 + ( ThemeEngine.Current.MenuCheckSize.Width * 2 );
				} else {
					item.Width += MENU_BAR_ITEMS_SPACE;
					x += item.Width;
				}
				
				if ( item.Height < ThemeEngine.Current.MenuHeight )
					item.Height = ThemeEngine.Current.MenuHeight;
			}
		}
		
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
				e.Graphics.DrawLine( ThemeEngine.Current.ResPool.GetPen( menu_separator_color ),
						    e.Bounds.X, e.Bounds.Y + 1, e.Bounds.X + e.Bounds.Right - 4, e.Bounds.Y + 1 );
				
				e.Graphics.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.White ),
						    e.Bounds.X, e.Bounds.Y + 2, e.Bounds.X + e.Bounds.Right - 4, e.Bounds.Y + 2 );
				
				return;
			}
			
			if ( !item.MenuBar )
				rect_text.X += ThemeEngine.Current.MenuCheckSize.Width;
			
			if ( item.BarBreak ) { /* Draw vertical break bar*/
				Rectangle rect = e.Bounds;
				rect.Y++;
	        		rect.Width = 3;
	        		rect.Height = item.MenuHeight - 6;
				
				e.Graphics.DrawLine( ThemeEngine.Current.ResPool.GetPen( menu_separator_color ),
						    rect.X, rect.Y , rect.X, rect.Y + rect.Height );
				
				e.Graphics.DrawLine( ThemeEngine.Current.ResPool.GetPen( ThemeEngine.Current.ColorControlLight ),
						    rect.X + 1, rect.Y , rect.X + 1, rect.Y + rect.Height );
			}
			
			Color color_text = ThemeEngine.Current.ColorMenuText;
			Color color_back;
			
			/* Draw background */
			Rectangle rect_back = e.Bounds;
			rect_back.X++;
			rect_back.Width -= 2;
			
			if ( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected ) {
				color_text = ThemeEngine.Current.ColorHighlightText;
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
				color_text = ThemeEngine.Current.ColorMenuText;
				color_back = item.MenuBar ? theme_back_color : menu_background_color;
				
				e.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( color_back ), rect_back );
			}
			
			if ( item.Enabled ) {
				e.Graphics.DrawString( item.Text, e.Font,
						      ThemeEngine.Current.ResPool.GetSolidBrush( color_text ),
						      rect_text, string_format );
				
				if ( !item.MenuBar && item.Shortcut != Shortcut.None && item.ShowShortcut ) {
					string str = item.GetShortCutText( );
					Rectangle rect = rect_text;
					rect.X = item.XTab;
					rect.Width -= item.XTab;
					
					e.Graphics.DrawString( str, e.Font, ThemeEngine.Current.ResPool.GetSolidBrush( color_text ),
							      rect, string_format_menu_shortcut );
				}
			} else {
				ControlPaint.DrawStringDisabled( e.Graphics, item.Text, e.Font,
								Color.Black, rect_text, string_format );
			}
			
			/* Draw arrow */
			if ( item.MenuBar == false && item.IsPopup ) {
				int cx = ThemeEngine.Current.MenuCheckSize.Width;
				int cy = ThemeEngine.Current.MenuCheckSize.Height;
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
				int cx = ThemeEngine.Current.MenuCheckSize.Width;
				int cy = ThemeEngine.Current.MenuCheckSize.Height;
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
			
			dc.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush
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
					item.PerformDrawItem( new DrawItemEventArgs( dc, ThemeEngine.Current.MenuFont,
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
			
			if ( barpos_pixels == 0 )
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
		protected override void RadioButton_DrawButton( RadioButton radio_button, Graphics dc, ButtonState state, Rectangle radiobutton_rectangle ) {
			SolidBrush sb = new SolidBrush( radio_button.BackColor );
			dc.FillRectangle( sb, radio_button.ClientRectangle );
			sb.Dispose( );
			
			if ( radio_button.appearance == Appearance.Button ) {
				if ( radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup ) {
					DrawFlatStyleButton( dc, radio_button.ClientRectangle, radio_button );
				} else {
					DrawButtonBase( dc, radio_button.ClientRectangle, radio_button );
				}
			} else {
				// establish if we are rendering a flat style of some sort
				if ( radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup ) {
					DrawFlatStyleRadioButton( dc, radiobutton_rectangle, radio_button );
				} else {
					ControlPaint.DrawRadioButton( dc, radiobutton_rectangle, state );
				}
			}
		}
		
		protected override void RadioButton_DrawFocus( RadioButton radio_button, Graphics dc, Rectangle text_rectangle ) {
			if ( radio_button.Focused && radio_button.appearance != Appearance.Button ) {
				if ( radio_button.FlatStyle != FlatStyle.Flat && radio_button.FlatStyle != FlatStyle.Popup ) {
					DrawInnerFocusRectangle( dc, text_rectangle, radio_button.BackColor );
				} 
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
				
				/* Background */
				switch ( bar.thumb_moving ) {
					case ScrollBar.ThumbMoving.None: {
							ScrollBar_Vertical_Draw_ThumbMoving_None( scrollbutton_height, bar, clip, dc );
							break;
						}
					case ScrollBar.ThumbMoving.Forward: {
							ScrollBar_Vertical_Draw_ThumbMoving_Forward( scrollbutton_height, bar, thumb_pos, clip, dc );
							break;
						}
						
					case ScrollBar.ThumbMoving.Backwards: {
							ScrollBar_Vertical_Draw_ThumbMoving_Backwards( scrollbutton_height, bar, thumb_pos, clip, dc );
							break;
						}
						
					default:
						break;
				}
				
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
				
				/* Background */					
				switch ( bar.thumb_moving ) {
					case ScrollBar.ThumbMoving.None: {
							ScrollBar_Horizontal_Draw_ThumbMoving_None( scrollbutton_width, bar, clip, dc );
							break;
						}
						
					case ScrollBar.ThumbMoving.Forward: {
							ScrollBar_Horizontal_Draw_ThumbMoving_Forward( scrollbutton_width, thumb_pos, bar, clip, dc );
							break;
						}
						
					case ScrollBar.ThumbMoving.Backwards: {
							ScrollBar_Horizontal_Draw_ThumbMoving_Backwards( scrollbutton_width, thumb_pos, bar, clip, dc );
							break;
						}
				}
				
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
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_None( int scrollbutton_height, ScrollBar bar, Rectangle clip, Graphics dc ) {
			Rectangle r = new Rectangle( 0,
						    scrollbutton_height, bar.ClientRectangle.Width, bar.ClientRectangle.Height - ( scrollbutton_height * 2 ) );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.X, intersect.Bottom - 1 );
				dc.DrawLine( pen, intersect.Right - 1, intersect.Y, intersect.Right - 1, intersect.Bottom - 1 );
			}
		}
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_Forward( int scrollbutton_height, ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc ) {
			Rectangle r = new Rectangle( 0,	 scrollbutton_height,
						    bar.ClientRectangle.Width, thumb_pos.Y  - scrollbutton_height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.X, intersect.Bottom - 1 );
				dc.DrawLine( pen, intersect.Right - 1, intersect.Y, intersect.Right - 1, intersect.Bottom - 1 );
			}
			
			r.X = 0;
			r.Y = thumb_pos.Y + thumb_pos.Height;
			r.Width = bar.ClientRectangle.Width;
			r.Height = bar.ClientRectangle.Height -	 ( thumb_pos.Y + thumb_pos.Height ) - scrollbutton_height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.X, intersect.Bottom - 1 );
				dc.DrawLine( pen, intersect.Right - 1, intersect.Y, intersect.Right - 1, intersect.Bottom - 1 );
			}
		}
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_Backwards( int scrollbutton_height, ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc ) {
			Rectangle r = new Rectangle( 0,	 scrollbutton_height,
						    bar.ClientRectangle.Width, thumb_pos.Y - scrollbutton_height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.X, intersect.Bottom - 1 );
				dc.DrawLine( pen, intersect.Right - 1, intersect.Y, intersect.Right - 1, intersect.Bottom - 1 );
			}
			
			r.X = 0;
			r.Y = thumb_pos.Y + thumb_pos.Height;
			r.Width = bar.ClientRectangle.Width;
			r.Height = bar.ClientRectangle.Height -	 ( thumb_pos.Y + thumb_pos.Height ) - scrollbutton_height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.X, intersect.Bottom - 1 );
				dc.DrawLine( pen, intersect.Right - 1, intersect.Y, intersect.Right - 1, intersect.Bottom - 1 );
			}
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_None( int scrollbutton_width, ScrollBar bar, Rectangle clip, Graphics dc ) {
			Rectangle r = new Rectangle( scrollbutton_width,
						    0, bar.ClientRectangle.Width - ( scrollbutton_width * 2 ), bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.Right - 1, intersect.Y );
				dc.DrawLine( pen, intersect.X, intersect.Bottom - 1, intersect.Right - 1, intersect.Bottom - 1 );
			}
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_Forward( int scrollbutton_width, Rectangle thumb_pos, ScrollBar bar, Rectangle clip, Graphics dc ) {
			Rectangle r = new Rectangle( scrollbutton_width,  0,
						    thumb_pos.X - scrollbutton_width, bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.Right - 1, intersect.Y );
				dc.DrawLine( pen, intersect.X, intersect.Bottom - 1, intersect.Right - 1, intersect.Bottom - 1 );
			}
			
			r.X = thumb_pos.X + thumb_pos.Width;
			r.Y = 0;
			r.Width = bar.ClientRectangle.Width -  ( thumb_pos.X + thumb_pos.Width ) - scrollbutton_width;
			r.Height = bar.ClientRectangle.Height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.Right - 1, intersect.Y );
				dc.DrawLine( pen, intersect.X, intersect.Bottom - 1, intersect.Right - 1, intersect.Bottom - 1 );
			}
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_Backwards( int scrollbutton_width, Rectangle thumb_pos, ScrollBar bar, Rectangle clip, Graphics dc ) {
			Rectangle r = new Rectangle( scrollbutton_width,  0,
						    thumb_pos.X - scrollbutton_width, bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.Right - 1, intersect.Y );
				dc.DrawLine( pen, intersect.X, intersect.Bottom - 1, intersect.Right - 1, intersect.Bottom - 1 );
			}
			
			r.X = thumb_pos.X + thumb_pos.Width;
			r.Y = 0;
			r.Width = bar.ClientRectangle.Width -  ( thumb_pos.X + thumb_pos.Width ) - scrollbutton_width;
			r.Height = bar.ClientRectangle.Height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty ) {
				dc.FillRectangle( ResPool.GetSolidBrush( scrollbar_background_color ), intersect );
				Pen pen = ResPool.GetPen( scrollbar_border_color );
				dc.DrawLine( pen, intersect.X, intersect.Y, intersect.Right - 1, intersect.Y );
				dc.DrawLine( pen, intersect.X, intersect.Bottom - 1, intersect.Right - 1, intersect.Bottom - 1 );
			}
		}
		#endregion	// ScrollBar
		
		#region StatusBar
		protected override void DrawStatusBarPanel( Graphics dc, Rectangle area, int index,
							   SolidBrush br_forecolor, StatusBarPanel panel ) {
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
		
		// FIXME: regions near the borders don't get filles with the correct backcolor
		// TODO: TabAlignment.Left and TabAlignment.Bottom
		public override void DrawTabControl( Graphics dc, Rectangle area, TabControl tab ) {
			dc.FillRectangle( ResPool.GetSolidBrush( tab.Parent.BackColor ), area ); 
			Rectangle panel_rect = GetTabPanelRectExt( tab );
			
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
			
			if ( tab.Alignment == TabAlignment.Top ) {
				for ( int r = tab.TabPages.Count; r > 0; r-- ) {
					for ( int i = tab.SliderPos; i < tab.TabPages.Count; i++ ) {
						if ( i == tab.SelectedIndex )
							continue;
						if ( r != tab.TabPages[ i ].Row )
							continue;
						Rectangle rect = tab.GetTabRect( i );
						if ( !rect.IntersectsWith( area ) )
							continue;
						DrawTab( dc, tab.TabPages[ i ], tab, rect, false );
					}
				}
			} else {
				for ( int r = 0; r < tab.TabPages.Count; r++ ) {
					for ( int i = tab.SliderPos; i < tab.TabPages.Count; i++ ) {
						if ( i == tab.SelectedIndex )
							continue;
						if ( r != tab.TabPages[ i ].Row )
							continue;
						Rectangle rect = tab.GetTabRect( i );
						if ( !rect.IntersectsWith( area ) )
							continue;
						DrawTab( dc, tab.TabPages[ i ], tab, rect, false );
					}
				}
			}
			
			if ( tab.SelectedIndex != -1 && tab.SelectedIndex >= tab.SliderPos ) {
				Rectangle rect = tab.GetTabRect( tab.SelectedIndex );
				if ( rect.IntersectsWith( area ) )
					DrawTab( dc, tab.TabPages[ tab.SelectedIndex ], tab, rect, true );
			}
			
			if ( tab.ShowSlider ) {
				Rectangle right = GetTabControlRightScrollRect( tab );
				Rectangle left = GetTabControlLeftScrollRect( tab );
				CPDrawScrollButton( dc, right, ScrollButton.Right, tab.RightSliderState );
				CPDrawScrollButton( dc, left, ScrollButton.Left, tab.LeftSliderState );
			}
		}
		
		protected override int DrawTab( Graphics dc, TabPage page, TabControl tab, Rectangle bounds, bool is_selected ) {
			int FlatButtonSpacing = 8;
			Rectangle interior;
			int res = bounds.Width;
			
			if ( page.BackColor != tab_selected_gradient_second_color )
				page.BackColor = tab_selected_gradient_second_color;
			
			// we can't fill the background right away because the bounds might be adjusted if the tab is selected
			
			if ( tab.Appearance == TabAppearance.Buttons || tab.Appearance == TabAppearance.FlatButtons ) {
				dc.FillRectangle( ResPool.GetSolidBrush( tab_selected_gradient_second_color ), bounds );
				
				// Separators
				if ( tab.Appearance == TabAppearance.FlatButtons ) {
					int width = bounds.Width;
					bounds.Width += ( FlatButtonSpacing - 2 );
					res = bounds.Width;
					CPDrawBorder3D( dc, bounds, Border3DStyle.Etched, Border3DSide.Right );
					bounds.Width = width;
				}
				
				if ( is_selected ) {
					CPDrawBorder3D( dc, bounds, Border3DStyle.Sunken, Border3DSide.All );
				} else if ( tab.Appearance != TabAppearance.FlatButtons ) {
					CPDrawBorder3D( dc, bounds, Border3DStyle.Raised, Border3DSide.All );
				}
				
				interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 4, bounds.Height - 4 );
				
                                
				StringFormat string_format = new StringFormat( );
				string_format.Alignment = StringAlignment.Center;
				string_format.LineAlignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
				
				interior.Y++;
				dc.DrawString( page.Text, page.Font, ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
				interior.Y--;
			} else {
				Pen border_pen = ResPool.GetPen( tab_border_color );
				
				Color tab_first_color = is_selected ? tab_selected_gradient_first_color : tab_not_selected_gradient_first_color;
				Color tab_second_color = is_selected ? tab_selected_gradient_second_color : tab_not_selected_gradient_second_color;
				
				switch ( tab.Alignment ) {
					case TabAlignment.Top:
						
						interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 3 );
						
						using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2 ), new Point( bounds.Left + 2, bounds.Bottom ), tab_first_color, tab_second_color ) ) {
							dc.FillRectangle( lgbr, interior );
						}
						
						// edges
						Pen tmp_pen = ResPool.GetPen( tab_edge_color );
						dc.DrawLine( tmp_pen, bounds.Left, bounds.Top + 1, bounds.Left + 1, bounds.Top );
						dc.DrawLine( tmp_pen, bounds.Right - 1, bounds.Top, bounds.Right, bounds.Top + 1 );
						
						// inner border
						tmp_pen = ResPool.GetPen( Color.White );
						dc.DrawLine( tmp_pen, bounds.Left + 1, bounds.Bottom - 2, bounds.Left + 1, bounds.Top + 1 );
						dc.DrawLine( tmp_pen, bounds.Left + 2, bounds.Top + 1, bounds.Right - 1, bounds.Top + 1 );
						
						// border
						tmp_pen = ResPool.GetPen( border_pressed_dark_color );
						dc.DrawLine( tmp_pen, bounds.Left, bounds.Top + 2, bounds.Left + 2, bounds.Top );
						dc.DrawLine( tmp_pen, bounds.Left + 2, bounds.Top, bounds.Right - 2, bounds.Top );
						dc.DrawLine( tmp_pen, bounds.Right - 2, bounds.Top, bounds.Right, bounds.Top + 2 );
						
						using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left, bounds.Top + 2 ), new Point( bounds.Left, bounds.Bottom - 1 ), border_pressed_dark_color, border_pressed_light_color ) ) {
							int diff = is_selected ? 4 : 3;
							dc.FillRectangle( lgbr, bounds.Left, bounds.Top + 2, 1, bounds.Height - diff );
							dc.FillRectangle( lgbr, bounds.Right, bounds.Top + 2, 1, bounds.Height - diff );
						}
						
						if ( page.Focused ) {
							tmp_pen = ResPool.GetPen( tab_focus_color );
							dc.DrawLine( tmp_pen, bounds.Left + 1, bounds.Top  + 2, bounds.Right - 1, bounds.Top + 2 );
							dc.DrawLine( tmp_pen, bounds.Left + 2, bounds.Top + 1, bounds.Right - 2, bounds.Top + 1 );
							
							tmp_pen = ResPool.GetPen( tab_top_border_focus_color );
							dc.DrawLine( tmp_pen, bounds.Left, bounds.Top + 2, bounds.Left + 2, bounds.Top );
							dc.DrawLine( tmp_pen, bounds.Left + 2, bounds.Top, bounds.Right - 2, bounds.Top );
							dc.DrawLine( tmp_pen, bounds.Right - 2, bounds.Top, bounds.Right, bounds.Top + 2 );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty ) {
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							interior.Y++;
							dc.DrawString( page.Text, page.Font, ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.Y--;
						}
						
						break;
						
					case TabAlignment.Bottom:
						
						interior = new Rectangle( bounds.Left + 3, bounds.Top, bounds.Width - 3, bounds.Height );
						
						using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 3, bounds.Top ), new Point( bounds.Left + 3, bounds.Bottom ), tab_first_color, tab_second_color ) ) {
							dc.FillRectangle( lgbr, interior );
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom - 3, bounds.Left + 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Bottom, bounds.Right - 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Bottom, bounds.Right, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Bottom - 3, bounds.Right, bounds.Top );
						
						if ( page.Focused ) {
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left - 1 , bounds.Bottom, bounds.Right - 1, bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Bottom - 1, bounds.Right , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Bottom - 2, bounds.Right , bounds.Bottom - 2 );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty ) {
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							interior.Y++;
							dc.DrawString( page.Text, page.Font, ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.Y--;
						}
						
						break;
						
					case TabAlignment.Left:
						
						interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
						
						using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2 ), new Point( bounds.Right, bounds.Top + 2 ), tab_first_color, tab_second_color ) ) {
							dc.FillRectangle( lgbr, interior );
						}
						
						dc.DrawLine( border_pen, bounds.Right, bounds.Top, bounds.Left + 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Top, bounds.Left, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Top + 3, bounds.Left, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom - 3, bounds.Left + 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Bottom, bounds.Right, bounds.Bottom );
						
						if ( page.Focused ) {
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left , bounds.Top + 1, bounds.Left , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left + 1 , bounds.Top, bounds.Left + 1 , bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left + 2 , bounds.Top, bounds.Left + 2 , bounds.Bottom );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty ) {
							StringFormat string_format = new StringFormat( );
							// Flip the text around
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							string_format.FormatFlags = StringFormatFlags.DirectionVertical;
							int wo = interior.Width / 2;
							int ho = interior.Height / 2;
							dc.TranslateTransform( interior.X + wo, interior.Y + ho );
							dc.RotateTransform( 180 );
							dc.DrawString( page.Text, page.Font, ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.ControlText ), 0, 0, string_format );
							dc.ResetTransform( );
						}
						
						break;
						
					default:
						// TabAlignment.Right
						
						interior = new Rectangle( bounds.Left, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
						
						using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left, bounds.Top + 2 ), new Point( bounds.Right, bounds.Top + 2 ), tab_second_color, tab_first_color ) ) {
							dc.FillRectangle( lgbr, interior );
						}
						
						int l_diff = is_selected ? 2 : 0;
						
						// edges
						tmp_pen = ResPool.GetPen( tab_edge_color );
						dc.DrawLine( tmp_pen, bounds.Right - 2, bounds.Top, bounds.Right - 1, bounds.Top + 1 );
						dc.DrawLine( tmp_pen, bounds.Right - 1, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom );
						
						// inner border
						tmp_pen = ResPool.GetPen( Color.White );
						dc.DrawLine( tmp_pen, bounds.Left + l_diff, bounds.Top + 1, bounds.Right - 2, bounds.Top + 1 );
						dc.DrawLine( tmp_pen, bounds.Right - 2, bounds.Top + 2, bounds.Right - 2, bounds.Bottom - 2 );
						
						// border
						tmp_pen = ResPool.GetPen( border_pressed_dark_color );
						dc.DrawLine( tmp_pen, bounds.Right - 3, bounds.Top, bounds.Right - 1, bounds.Top + 2 );
						dc.DrawLine( tmp_pen, bounds.Right - 1, bounds.Top + 2, bounds.Right - 1, bounds.Bottom - 2 );
						dc.DrawLine( tmp_pen, bounds.Right - 1, bounds.Bottom - 2, bounds.Right - 3, bounds.Bottom );
						
						using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left, bounds.Top + 2 ), new Point( bounds.Right - 2, bounds.Top + 2 ), border_pressed_light_color, border_pressed_dark_color ) ) {
							int diff = is_selected ? 4 : 2;
							
							dc.FillRectangle( lgbr, bounds.Left + l_diff, bounds.Top, bounds.Width - diff, 1 );
							dc.FillRectangle( lgbr, bounds.Left + l_diff, bounds.Bottom, bounds.Width - diff, 1 );
						}
						
						if ( page.Focused ) {
							tmp_pen = ResPool.GetPen( tab_focus_color );
							dc.DrawLine( tmp_pen, bounds.Right - 3, bounds.Top + 1, bounds.Right - 3, bounds.Bottom - 1 );
							dc.DrawLine( tmp_pen, bounds.Right - 2, bounds.Top + 2, bounds.Right - 2, bounds.Bottom - 2 );
							
							tmp_pen = ResPool.GetPen( tab_top_border_focus_color );
							dc.DrawLine( tmp_pen, bounds.Right - 3, bounds.Top, bounds.Right - 1, bounds.Top + 2 );
							dc.DrawLine( tmp_pen, bounds.Right - 1, bounds.Top + 2, bounds.Right - 1, bounds.Bottom - 2 );
							dc.DrawLine( tmp_pen, bounds.Right - 1, bounds.Bottom - 2, bounds.Right - 3, bounds.Bottom );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty ) {
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							string_format.FormatFlags = StringFormatFlags.DirectionVertical;
							interior.X++;
							dc.DrawString( page.Text, page.Font, ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.X--;
						}
						
						break;
				}
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
			
			if ( ( state & ButtonState.Checked ) != 0 ) {
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControlLightLight, ColorControlLight ), rectangle );
			} else
				dc.FillRectangle( ResPool.GetSolidBrush( Color.White ), rectangle );
			
			if ( ( state & ButtonState.Flat ) != 0 ) {
				first_color = gradient_first_color;
				second_color = combobox_button_second_gradient_color;
			} else {
				if ( ( state & ( ButtonState.Pushed | ButtonState.Checked ) ) != 0 ) {
					first_color = pressed_gradient_first_color;
					second_color = pressed_gradient_second_color;
					pushed = true;
				}
			}
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rectangle.X, rectangle.Y ), new Point( rectangle.X, rectangle.Bottom ), first_color, second_color ) ) {
				dc.FillRectangle( lgbr, rectangle.X + 2, rectangle.Y, rectangle.Width - 2, rectangle.Height - 1 );
			}
			
			// inner border
			Pen tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : Color.White );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y - 1, rectangle.Right, rectangle.Y - 1 );
			dc.DrawLine( tmp_pen, rectangle.X + 1, rectangle.Y - 1, rectangle.X + 1, rectangle.Bottom - 1 );
			
			tmp_pen = ResPool.GetPen( pushed ? pressed_inner_border_dark_color : inner_border_dark_color );
			dc.DrawLine( tmp_pen, rectangle.Right, rectangle.Y, rectangle.Right, rectangle.Bottom - 1 );
			dc.DrawLine( tmp_pen, rectangle.X + 2, rectangle.Bottom - 1, rectangle.Right, rectangle.Bottom - 1 );
			
			// border
			Point[] points = new Point[] {
				new Point( rectangle.X, rectangle.Y - 2 ),
				new Point( rectangle.Right - 1, rectangle.Y - 2 ),
				new Point( rectangle.Right + 1, rectangle.Y ),
				new Point( rectangle.Right + 1, rectangle.Bottom - 2 ),
				new Point( rectangle.Right - 1, rectangle.Bottom ),
				new Point( rectangle.X, rectangle.Bottom ),
				new Point( rectangle.X, rectangle.Y - 2 )
			};
			
			dc.DrawPolygon( ResPool.GetPen( pushed ? border_pressed_dark_color : border_normal_dark_color ), points );
			
			rect = new Rectangle( rectangle.X + rectangle.Width / 4, rectangle.Y + rectangle.Height / 4, rectangle.Width / 2, rectangle.Height / 2 );
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
			P3 = new Point( rect.Right, centerY );
			
			arrow[ 0 ] = P1;
			arrow[ 1 ] = P2;
			arrow[ 2 ] = P3;
			
			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			/* Draw the arrow */
			if ( ( state & ButtonState.Inactive ) != 0 ) {
				using ( Pen pen = new Pen( SystemColors.ControlLightLight, 2 ) ) {
					dc.DrawLines( pen, arrow );
				}
				
				/* Move away from the shadow */
				P1.X -= 1;		P1.Y -= 1;
				P2.X -= 1;		P2.Y -= 1;
				P3.X -= 1;		P3.Y -= 1;
				
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
			
			if ( area.Width < 12 || area.Height < 12 ) /* Cannot see a thing at smaller sizes */
				return;
			
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
			
			Point[]	arrow = new Point[ 4 ];
			
			switch ( scroll_button_type ) {
				case ScrollButton.Down:
					centerY += shift + 1;
					arrow[ 0 ] = new Point( centerX - 4, centerY - 2 );
					arrow[ 1 ] = new Point( centerX, centerY + 2 );
					arrow[ 2 ] = new Point( centerX + 4, centerY - 2 );
					arrow[ 3 ] = new Point( centerX - 4, centerY - 2 );
					break;
				case ScrollButton.Up:
					centerY -= shift;
					arrow[ 0 ] = new Point( centerX - 4, centerY + 2 );
					arrow[ 1 ] = new Point( centerX, centerY - 2 );
					arrow[ 2 ] = new Point( centerX + 4, centerY + 2 );
					arrow[ 3 ] = new Point( centerX - 4, centerY + 2 );
					break;
				case ScrollButton.Left:
					centerX -= shift;
					arrow[ 0 ] = new Point( centerX + 2, centerY - 4 );
					arrow[ 1 ] = new Point( centerX + 2, centerY + 4 );
					arrow[ 2 ] = new Point( centerX - 2, centerY );
					arrow[ 3 ] = new Point( centerX + 2, centerY - 4 );
					break;
				case ScrollButton.Right:
					centerX += shift + 1;
					arrow[ 0 ] = new Point( centerX - 2, centerY - 4 );
					arrow[ 1 ] = new Point( centerX + 2, centerY );
					arrow[ 2 ] = new Point( centerX - 2, centerY + 4 );
					arrow[ 3 ] = new Point( centerX - 2, centerY - 4 );
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
		
		public override void CPDrawBorder3D( Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides ) {
			CPDrawBorder3D( graphics, rectangle, style, sides, ColorControl );
		}
		
		private void CPDrawBorder3D( Graphics dc, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color ) {
			// currently we don't take care of Border3DStyle or Border3DSide
			
			// FIXME: temporary fix for artefacts, it should use the backcolor of the parent control
			dc.DrawLine( ResPool.GetPen( ColorControl ), rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 1 );
			dc.DrawLine( ResPool.GetPen( ColorControl ), rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1 );
			
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
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 2 ), new Point( 0, rectangle.Height - 1 ), button_outer_border_dark_color, button_outer_border_light_color ) ) {
				dc.FillRectangle( lgbr, rectangle.X, rectangle.Y + 2, 1, rectangle.Height - 4 );
				dc.FillRectangle( lgbr, rectangle.Right - 1, rectangle.Y + 2, 1, rectangle.Height - 4 );
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
	} //class
}

