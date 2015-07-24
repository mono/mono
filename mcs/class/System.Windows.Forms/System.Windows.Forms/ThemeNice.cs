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
//		- You can activate this Theme with export MONO_THEME=nice


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace System.Windows.Forms
{
	internal class ThemeNice : ThemeWin32Classic
	{
		public override Version Version
		{
			get {
				return new Version( 0, 0, 0, 3 );
			}
		}
		
		static readonly Color NormalColor = Color.LightGray;
		static readonly Color MouseOverColor = Color.DarkGray;
		static readonly Color PressedColor = Color.Gray;
		static readonly Color FocusColor = Color.FromArgb( System.Convert.ToInt32( "0xff00c0ff", 16 ) );
		static readonly Color FocusColorLight = Color.FromArgb(250, 253, 255);
		static readonly Color LightColor = Color.LightGray;
		static readonly Color BorderColor = MouseOverColor;
		static readonly Color NiceBackColor  = Color.FromArgb( System.Convert.ToInt32( "0xffefebe7", 16 ) );
		
		static Bitmap size_grip_bmp = CreateSizegripDot();
		
		static Blend NormalBlend;
		static Blend FlatBlend;
		
		#region	Principal Theme Methods
		public ThemeNice( )
		{
			ColorControl = NiceBackColor;
			
			FlatBlend = new Blend ();
			FlatBlend.Factors = new float []{0.0f, 0.992f, 1.0f};
			FlatBlend.Positions = new float []{0.0f, 0.68f, 1.0f};
			
			NormalBlend = new Blend ();
			NormalBlend.Factors = new float []{0.0f, 0.008f, 1.0f};
			NormalBlend.Positions = new float []{0.0f, 0.32f, 1.0f};
		}
		
		public override Color DefaultControlBackColor
		{
			get { return NiceBackColor; }
		}
		
		public override Color DefaultWindowBackColor
		{
			get { return NiceBackColor; }
		}
		
		public override Color ColorControl {
			get { return NiceBackColor;}
		}
		
		static Bitmap CreateSizegripDot()
		{
			Bitmap bmp = new Bitmap( 4, 4 );
			using ( Graphics dc = Graphics.FromImage( bmp ) )
			{
				SmoothingMode old_smoothing_mode = dc.SmoothingMode;
				dc.SmoothingMode = SmoothingMode.AntiAlias;
				
				using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 4, 4 ), PressedColor, Color.White ) ) {
					Blend bl = new Blend ();
					bl.Factors = new float []{0.0f, 0.992f, 1.0f};
					bl.Positions = new float []{0.0f, 0.68f, 1.0f};
					lgbr.Blend = bl;
					dc.FillEllipse( lgbr, new Rectangle( 0, 0, 4, 4 ) );
				}
				
				dc.SmoothingMode = old_smoothing_mode;
			}
			
			return bmp;
		}
		
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
		protected override void ButtonBase_DrawButton (ButtonBase button, Graphics dc)
		{
			bool check_or_radio = false;
			bool check_or_radio_checked = false;
			
			Color use_color = NormalColor;
			Color first_color = Color.White;
			
			dc.FillRectangle (ResPool.GetSolidBrush (button.BackColor), button.ClientRectangle);
			
			if (button is CheckBox) {
				check_or_radio = true;
				check_or_radio_checked = ((CheckBox)button).Checked;
			} else if (button is RadioButton) {
				check_or_radio = true;
				check_or_radio_checked = ((RadioButton)button).Checked;
			}
			
			if (button.has_focus && !check_or_radio && button.is_enabled)
				first_color = FocusColorLight;
			
			if (button.is_enabled) {
				if (button.FlatStyle == FlatStyle.Popup) {
					if (!button.is_pressed && button.is_entered && !check_or_radio_checked)
						use_color = MouseOverColor;
				} else if (button.FlatStyle == FlatStyle.Flat) {
					if (button.is_entered && !button.is_pressed && !check_or_radio_checked)
						use_color = MouseOverColor;
				} else {
					if (!button.is_pressed && button.is_entered && !check_or_radio_checked)
						use_color = MouseOverColor;
				}
			}
			
			Rectangle buttonRectangle;
			
			int height = button.ClientSize.Height;
			
			// set up the button rectangle
			buttonRectangle = button.ClientRectangle;
			
			// Fill button with a nice linear gradient brush
			Rectangle lgbRectangle = Rectangle.Inflate (buttonRectangle, -1, -1);
			
			if (button.FlatStyle != FlatStyle.Popup || ((button.FlatStyle == FlatStyle.Popup) && button.is_entered)) {
				LinearGradientBrush lgbr;
				if (!button.is_pressed && !check_or_radio_checked) {
					if (button.FlatStyle == FlatStyle.Flat) {
						lgbr = new LinearGradientBrush (new Point (0, 0), new Point (0, height - 1), use_color, first_color);
						lgbr.Blend = FlatBlend;
					} else {
						lgbr = new LinearGradientBrush (new Point (0, 0), new Point (0, height - 1), first_color, use_color);
						lgbr.Blend = NormalBlend;
					}
				} else {
					lgbr = new LinearGradientBrush (new Point (0, 0), new Point (0, height - 1), PressedColor, MouseOverColor);
				}
				dc.FillRectangle (lgbr, lgbRectangle);
				lgbr.Dispose ();
				
				if (button.has_focus && !check_or_radio)
					return; 
				
				Internal_DrawButton(dc, buttonRectangle, BorderColor);
			}
		}
		
		private void Internal_DrawButton(Graphics dc, Rectangle area, Color border_color)
		{
			Point[] points = new Point [] {
				new Point (area.X + 1, area.Y),
				new Point (area.Right - 2, area.Y),
				new Point (area.Right - 2, area.Y + 1),
				new Point (area.Right - 1, area.Y + 1),
				new Point (area.Right - 1, area.Bottom - 2),
				new Point (area.Right - 2, area.Bottom - 2),
				new Point (area.Right - 2, area.Bottom - 1),
				new Point (area.X + 1, area.Bottom - 1),
				new Point (area.X + 1, area.Bottom - 2),
				new Point (area.X, area.Bottom - 2),
				new Point (area.X, area.Y + 1),
				new Point (area.X + 1, area.Y + 1),
				new Point (area.X + 1, area.Y)
			};
			
			Pen pen = ResPool.GetPen (border_color);
			dc.DrawPolygon (pen, points);
		}
		
		protected override void ButtonBase_DrawFocus( ButtonBase button, Graphics dc )
		{
			if ((button is RadioButton) || (button is CheckBox))
				return; 
			
			Internal_DrawButton(dc, button.ClientRectangle, FocusColor);
		}
		
		protected override void ButtonBase_DrawText( ButtonBase button, Graphics dc )
		{
			if ( !( button is CheckBox ) && !(button is RadioButton ) )
			{
				base.ButtonBase_DrawText( button, dc );
			}
		}
		#endregion	// ButtonBase
		
		#region Menus
		public override void DrawMenuBar (Graphics dc, Menu menu, Rectangle rect)
		{
			if (menu.Height == 0)
				CalcMenuBarSize (dc, menu, rect.Width);
			
			bool keynav = (menu as MainMenu).tracker.Navigating;
			HotkeyPrefix hp = MenuAccessKeysUnderlined || keynav ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
			string_format_menu_menubar_text.HotkeyPrefix = hp;
			string_format_menu_text.HotkeyPrefix = hp;
			
			rect.Height = menu.Height;
			dc.FillRectangle (ResPool.GetSolidBrush (NiceBackColor), rect);
			
			for (int i = 0; i < menu.MenuItems.Count; i++) {
				MenuItem item = menu.MenuItems [i];
				Rectangle item_rect = item.bounds;
				item_rect.X += rect.X;
				item_rect.Y += rect.Y;
				item.MenuHeight = menu.Height;
				item.PerformDrawItem (new DrawItemEventArgs (dc, MenuFont, item_rect, i, item.Status));	
			}	
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
				e.Graphics.DrawLine (ResPool.GetPen (ColorControlDark),
						     e.Bounds.X, e.Bounds.Y, e.Bounds.X + e.Bounds.Width, e.Bounds.Y);
				
				e.Graphics.DrawLine (ResPool.GetPen (ColorControlLight),
						     e.Bounds.X, e.Bounds.Y + 1, e.Bounds.X + e.Bounds.Width, e.Bounds.Y + 1);
				
				return;
			}
			
			if (!item.MenuBar)
				rect_text.X += MenuCheckSize.Width;
			
			if (item.BarBreak) { /* Draw vertical break bar*/
				Rectangle rect = e.Bounds;
				rect.Y++;
				rect.Width = 3;
				rect.Height = item.MenuHeight - 6;
				
				e.Graphics.DrawLine (ResPool.GetPen (ColorControlDark),
					rect.X, rect.Y , rect.X, rect.Y + rect.Height);
				
				e.Graphics.DrawLine (ResPool.GetPen (ColorControlLight),
					rect.X + 1, rect.Y , rect.X +1, rect.Y + rect.Height);
			}
			
			Color color_text = ColorMenuText;
			Color color_back = NiceBackColor;
			
			/* Draw background */
			Rectangle rect_back = e.Bounds;
			rect_back.X++;
			rect_back.Width -=2;
			
			if (((e.State & DrawItemState.Selected) == DrawItemState.Selected) || ((e.State & DrawItemState.HotLight) == DrawItemState.HotLight)) {
					using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (rect_back.X, rect_back.Y), new Point (rect_back.Right, rect_back.Y), Color.White, NormalColor))//NormalColor, Color.White ) )
						e.Graphics.FillRectangle (lgbr, rect_back);
					
					e.Graphics.DrawRectangle (ResPool.GetPen (BorderColor), rect_back.X, rect_back.Y, rect_back.Width, rect_back.Height - 1);
			} else {
				e.Graphics.FillRectangle (ResPool.GetSolidBrush (NiceBackColor), rect_back);
			}
			
			if (item.Enabled) {
				e.Graphics.DrawString (item.Text, e.Font,
					ResPool.GetSolidBrush (color_text),
					rect_text, string_format);
				
				if (!item.MenuBar && item.Shortcut != Shortcut.None && item.ShowShortcut) {
					string str = item.GetShortCutText ();
					Rectangle rect = rect_text;
					rect.X = item.XTab;
					rect.Width -= item.XTab;
					
					e.Graphics.DrawString (str, e.Font, ResPool.GetSolidBrush (color_text),
						rect, string_format_menu_shortcut);
				}
				
			} else {
				ControlPaint.DrawStringDisabled (e.Graphics, item.Text, e.Font, 
					Color.Black, rect_text, string_format);
			}
			
			/* Draw arrow */
			if (item.MenuBar == false && item.IsPopup || item.MdiList) {
				
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
			
			dc.FillRectangle (ResPool.GetSolidBrush
					  (NiceBackColor), cliparea);
			
			/* Draw menu borders */
			dc.DrawLine (ResPool.GetPen (ColorHighlightText),
				     rect.X, rect.Y, rect.X + rect.Width, rect.Y);
			
			dc.DrawLine (ResPool.GetPen (ColorHighlightText),
				     rect.X, rect.Y, rect.X, rect.Y + rect.Height);
			
			dc.DrawLine (ResPool.GetPen (ColorControlDark),
				     rect.X + rect.Width - 1 , rect.Y , rect.X + rect.Width - 1, rect.Y + rect.Height);
			
			dc.DrawLine (ResPool.GetPen (ColorControlDarkDark),
				     rect.X + rect.Width, rect.Y , rect.X + rect.Width, rect.Y + rect.Height);
			
			dc.DrawLine (ResPool.GetPen (ColorControlDark),
				     rect.X , rect.Y + rect.Height - 1 , rect.X + rect.Width - 1, rect.Y + rect.Height -1);
			
			dc.DrawLine (ResPool.GetPen (ColorControlDarkDark),
				     rect.X , rect.Y + rect.Height, rect.X + rect.Width - 1, rect.Y + rect.Height);
			
			for (int i = 0; i < menu.MenuItems.Count; i++)
				if (cliparea.IntersectsWith (menu.MenuItems [i].bounds)) {
					MenuItem item = menu.MenuItems [i];
					item.MenuHeight = menu.Height;
					item.PerformDrawItem (new DrawItemEventArgs (dc, MenuFont,
										     item.bounds, i, item.Status));
				}
		}
		#endregion // Menus
		
		#region ProgressBar
		public override void DrawProgressBar( Graphics dc, Rectangle clip_rect, ProgressBar ctrl )
		{
			Rectangle	client_area = ctrl.client_area;
			int		barpos_pixels;
			Rectangle bar = ctrl.client_area;
			
			barpos_pixels = ( ( ctrl.Value - ctrl.Minimum ) * client_area.Width ) / ( ctrl.Maximum - ctrl.Minimum );
			
			bar.Width = barpos_pixels;
//			bar.Height += 1;
			
			// Draw bar background
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( client_area.Left, client_area.Top ), new Point( client_area.Left, client_area.Bottom ), LightColor, Color.White ) )
			{
				lgbr.Blend = FlatBlend;
				dc.FillRectangle( lgbr, client_area );
			}
			
			// Draw bar
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( bar.Location, new Point( bar.X, bar.Bottom ), Color.White, PressedColor ) )
			{
				lgbr.Blend = NormalBlend;
				dc.FillRectangle( lgbr, bar );
			}
			
			/* Draw border */
			dc.DrawRectangle( ResPool.GetPen( BorderColor ), ctrl.ClientRectangle.X, ctrl.ClientRectangle.Y, ctrl.ClientRectangle.Width - 1, ctrl.ClientRectangle.Height - 1 );
			dc.DrawRectangle( ResPool.GetPen( LightColor ), ctrl.ClientRectangle.X + 1, ctrl.ClientRectangle.Y + 1, ctrl.ClientRectangle.Width - 2, ctrl.ClientRectangle.Height - 2 );
		}
		#endregion	// ProgressBar
		
		#region ScrollBar
		protected override void ScrollBar_DrawThumb( ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc )
		{
			if ( bar.Enabled && thumb_pos.Width > 0 && thumb_pos.Height > 0 && clip.IntersectsWith( thumb_pos ) )
				DrawScrollBarThumb( dc, thumb_pos, bar );
		}
		#endregion	// ScrollBar
		
		#region StatusBar
		protected override void DrawStatusBarPanel( Graphics dc, Rectangle area, int index,
							   Brush br_forecolor, StatusBarPanel panel )
		{
			int border_size = 3; // this is actually const, even if the border style is none
			
			area.Height -= border_size;
			if ( panel.BorderStyle != StatusBarPanelBorderStyle.None )
			{
				Internal_DrawButton( dc, area, BorderColor );
			}
			
			if ( panel.Style == StatusBarPanelStyle.OwnerDraw )
			{
				StatusBarDrawItemEventArgs e = new StatusBarDrawItemEventArgs(
					dc, panel.Parent.Font, area, index, DrawItemState.Default,
					panel, panel.Parent.ForeColor, panel.Parent.BackColor );
				panel.Parent.OnDrawItemInternal( e );
				return;
			}
			
			int left = area.Left;
			if ( panel.Icon != null )
			{
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
			
			if ( text[ 0 ] == '\t' )
			{
				string_format.Alignment = StringAlignment.Center;
				text = text.Substring( 1 );
				if ( text[ 0 ] == '\t' )
				{
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
		
		public override void DrawTabControl( Graphics dc, Rectangle area, TabControl tab )
		{
			// Do we need to fill the back color? It can't be changed...
			dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), area );
			Rectangle panel_rect = TabControlGetPanelRect( tab );
			
			if ( tab.Appearance == TabAppearance.Normal )
			{
				CPDrawBorder( dc, panel_rect, BorderColor, 1, ButtonBorderStyle.Solid, BorderColor, 1, ButtonBorderStyle.Solid,
					     BorderColor, 1, ButtonBorderStyle.Solid, BorderColor, 1, ButtonBorderStyle.Solid );
			}
			
			if ( tab.Alignment == TabAlignment.Top )
			{
				for ( int r = tab.TabPages.Count; r > 0; r-- )
				{
					for ( int i = tab.SliderPos; i < tab.TabPages.Count; i++ )
					{
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
			else
			{
				for ( int r = 0; r < tab.TabPages.Count; r++ )
				{
					for ( int i = tab.SliderPos; i < tab.TabPages.Count; i++ )
					{
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
			
			if ( tab.SelectedIndex != -1 && tab.SelectedIndex >= tab.SliderPos )
			{
				Rectangle rect = tab.GetTabRect( tab.SelectedIndex );
				if ( rect.IntersectsWith( area ) )
					DrawTab( dc, tab.TabPages[ tab.SelectedIndex ], tab, rect, true );
			}
			
			if ( tab.ShowSlider )
			{
				Rectangle right = TabControlGetRightScrollRect( tab );
				Rectangle left = TabControlGetLeftScrollRect( tab );
				CPDrawScrollButton( dc, right, ScrollButton.Right, tab.RightSliderState );
				CPDrawScrollButton( dc, left, ScrollButton.Left, tab.LeftSliderState );
			}
		}
		
		protected virtual int DrawTab( Graphics dc, TabPage page, TabControl tab, Rectangle bounds, bool is_selected )
		{
			int FlatButtonSpacing = 8;
			Rectangle interior;
			int res = bounds.Width;
			
			// we can't fill the background right away because the bounds might be adjusted if the tab is selected
			
			if ( tab.Appearance == TabAppearance.Buttons || tab.Appearance == TabAppearance.FlatButtons )
			{
				dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), bounds );
				
				// Separators
				if ( tab.Appearance == TabAppearance.FlatButtons )
				{
					int width = bounds.Width;
					bounds.Width += ( FlatButtonSpacing - 2 );
					res = bounds.Width;
					CPDrawBorder3D( dc, bounds, Border3DStyle.Etched, Border3DSide.Right );
					bounds.Width = width;
				}
				
				if ( is_selected )
				{
					CPDrawBorder3D( dc, bounds, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				}
				else if ( tab.Appearance != TabAppearance.FlatButtons )
				{
					CPDrawBorder3D( dc, bounds, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				}
				
				interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 4, bounds.Height - 4 );
				
				StringFormat string_format = new StringFormat( );
				string_format.Alignment = StringAlignment.Center;
				string_format.LineAlignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
				
				interior.Y++;
				dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
				interior.Y--;
			}
			else
			{
				Pen border_pen = ResPool.GetPen( BorderColor );
				
				dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), bounds );
				
				switch ( tab.Alignment )
				{
					case TabAlignment.Top:
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2  ), new Point( bounds.Left + 2, bounds.Bottom ), Color.White, LightColor ) )
							{
								lgbr.Blend = NormalBlend;
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom, bounds.Left, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Top + 3, bounds.Left + 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Top, bounds.Right - 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Top, bounds.Right, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Top + 3, bounds.Right, bounds.Bottom );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left - 1 , bounds.Top, bounds.Right - 1, bounds.Top );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Top + 1, bounds.Right , bounds.Top + 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Top + 2, bounds.Right , bounds.Top + 2 );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							interior.Y++;
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.Y--;
						}
						
						break;
						
					case TabAlignment.Bottom:
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 3, bounds.Top, bounds.Width - 3, bounds.Height );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 3, bounds.Top  ), new Point( bounds.Left + 3, bounds.Bottom  ), Color.White, LightColor ) )
							{
								lgbr.Blend = NormalBlend;
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom - 3, bounds.Left + 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Bottom, bounds.Right - 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Bottom, bounds.Right, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Bottom - 3, bounds.Right, bounds.Top );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left - 1 , bounds.Bottom, bounds.Right - 1, bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Bottom - 1, bounds.Right , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Bottom - 2, bounds.Right , bounds.Bottom - 2 );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							interior.Y++;
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.Y--;
						}
						
						break;
						
					case TabAlignment.Left:
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2  ), new Point( bounds.Right, bounds.Top + 2 ), LightColor, Color.White ) )
							{
								lgbr.Blend = FlatBlend;
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Right, bounds.Top, bounds.Left + 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Top, bounds.Left, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Top + 3, bounds.Left, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom - 3, bounds.Left + 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Bottom, bounds.Right, bounds.Bottom );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left , bounds.Top + 1, bounds.Left , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left + 1 , bounds.Top, bounds.Left + 1 , bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left + 2 , bounds.Top, bounds.Left + 2 , bounds.Bottom );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
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
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), 0, 0, string_format );
							dc.ResetTransform( );
						}
						
						break;
						
					default:
						// TabAlignment.Right
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2  ), new Point( bounds.Right, bounds.Top + 2 ), Color.White, LightColor ) )
							{
								lgbr.Blend = NormalBlend;
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Top, bounds.Right - 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Top, bounds.Right, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Top + 3, bounds.Right, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Bottom - 3, bounds.Right - 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Bottom, bounds.Left, bounds.Bottom );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Right , bounds.Top + 1, bounds.Right , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Right - 1 , bounds.Top, bounds.Right - 1 , bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Right - 2 , bounds.Top, bounds.Right - 2 , bounds.Bottom );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							string_format.FormatFlags = StringFormatFlags.DirectionVertical;
							interior.X++;
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.X--;
						}
						
						break;
				}
			}
			
			return res;
		}
		
		#region ToolBar
		public  override void DrawToolBar (Graphics dc, Rectangle clip_rectangle, ToolBar control) 
		{
			StringFormat format = new StringFormat ();
			format.Trimming = StringTrimming.EllipsisWord;
			format.LineAlignment = StringAlignment.Center;
			if (control.TextAlign == ToolBarTextAlign.Underneath)
				format.Alignment = StringAlignment.Center;
			else
				format.Alignment = StringAlignment.Near;
			
			dc.FillRectangle (ResPool.GetSolidBrush( NiceBackColor ), clip_rectangle);
			
			foreach (ToolBarItem item in control.items)
				if (item.Button.Visible && clip_rectangle.IntersectsWith (item.Rectangle))
					DrawToolBarButton (dc, control, item, format);
			
			format.Dispose ();
		}
		
		protected override void DrawToolBarButton (Graphics dc, ToolBar control, ToolBarItem item, StringFormat format)
		{
			bool is_flat = control.Appearance == ToolBarAppearance.Flat;
			
			if (item.Button.Style != ToolBarButtonStyle.Separator)
				DoDrawToolBarButton (dc, item, is_flat);
			
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
			default:
				DrawToolBarButtonContents (dc, control, item, format);
				break;
			}
		}
		
		const Border3DSide all_sides = Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom;
		
		void DoDrawToolBarButton (Graphics dc, ToolBarItem item, bool is_flat)
		{
			Color use_color = NormalColor;
			Color first_color = Color.White;
			
			if (is_flat) {
				if (item.Button.Pushed || item.Pressed) {
					first_color = PressedColor;
					use_color = MouseOverColor;
				} else
				if (item.Hilight)
					use_color = MouseOverColor;
				else
					return;
			} else {
				if (item.Button.Pushed || item.Pressed) {
					first_color = PressedColor;
					use_color = MouseOverColor;
				} else
				if (item.Hilight)
					use_color = MouseOverColor;
			}
			
			Rectangle buttonRectangle = item.Rectangle;
			
			Rectangle lgbRectangle = Rectangle.Inflate (buttonRectangle, -1, -1);
			
			using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (buttonRectangle.X, buttonRectangle.Y), new Point (buttonRectangle.X, buttonRectangle.Bottom - 1), first_color, use_color)) {
				lgbr.Blend = NormalBlend;
				dc.FillRectangle (lgbr, lgbRectangle);
			}
			
			Internal_DrawButton (dc, buttonRectangle, BorderColor);
		}
		
		protected override void DrawToolBarSeparator (Graphics dc, ToolBarItem item)
		{
			Rectangle area = item.Rectangle;
			int offset = (int) ResPool.GetPen (ColorControl).Width + 1;
			dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X + 1, area.Y, area.X + 1, area.Bottom);
			dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X + offset, area.Y, area.X + offset, area.Bottom);
		}
		
		protected override void DrawToolBarDropDownArrow (Graphics dc, ToolBarItem item, bool is_flat)
		{
			Rectangle rect = item.Rectangle;
			rect.X = item.Rectangle.Right - ToolBarDropDownWidth;
			rect.Width = ToolBarDropDownWidth;
			
			if (item.DDPressed) {
				CPDrawBorder3D (dc, rect, Border3DStyle.SunkenOuter, all_sides);
				CPDrawBorder3D (dc, rect, Border3DStyle.SunkenInner, Border3DSide.Bottom | Border3DSide.Right);
			} else if (item.Button.Pushed || item.Pressed)
				CPDrawBorder3D (dc, rect, Border3DStyle.Sunken, all_sides);
			else if (is_flat) {
				if (item.Hilight)
					CPDrawBorder3D (dc, rect, Border3DStyle.RaisedOuter, all_sides);
			} else
				CPDrawBorder3D (dc, rect, Border3DStyle.Raised, all_sides);
			
			PointF [] vertices = new PointF [3];
			PointF ddCenter = new PointF (rect.X + (rect.Width/2.0f), rect.Y + (rect.Height/2.0f));
			vertices [0].X = ddCenter.X - ToolBarDropDownArrowWidth / 2.0f + 0.5f;
			vertices [0].Y = ddCenter.Y;
			vertices [1].X = ddCenter.X + ToolBarDropDownArrowWidth / 2.0f + 0.5f;
			vertices [1].Y = ddCenter.Y;
			vertices [2].X = ddCenter.X + 0.5f; // 0.5 is added for adjustment
			vertices [2].Y = ddCenter.Y + ToolBarDropDownArrowHeight;
			dc.FillPolygon (SystemBrushes.ControlText, vertices);
		}
		
		protected override void DrawToolBarButtonContents (Graphics dc, ToolBar control, ToolBarItem item, StringFormat format)
		{
			if (item.Button.Image != null) {
				int x = item.ImageRectangle.X + ToolBarImageGripWidth;
				int y = item.ImageRectangle.Y + ToolBarImageGripWidth;
				if (item.Button.Enabled)
					dc.DrawImage (item.Button.Image, x, y);
				else 
					CPDrawImageDisabled (dc, item.Button.Image, x, y, ColorControl);
			}
			
			if (item.Button.Enabled)
				dc.DrawString (item.Button.Text, control.Font, ResPool.GetSolidBrush (ColorControlText), item.TextRectangle, format);
			else
				CPDrawStringDisabled (dc, item.Button.Text, control.Font, ColorControlLight, item.TextRectangle, format);
		}

		#endregion	// ToolBar
		
//		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
//			CPDrawBorder3D(graphics, rectangle, style, sides, ColorControl);
//		}
		
		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color)
		{
			Pen		penTopLeft;
			Pen		penTopLeftInner;
			Pen		penBottomRight;
			Pen		penBottomRightInner;
			Rectangle	rect= new Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			
			if ((style & Border3DStyle.Adjust) != 0) {
				rect.Y -= 2;
				rect.X -= 2;
				rect.Width += 4;
				rect.Height += 4;
			}
			
			penTopLeft = penTopLeftInner = penBottomRight = penBottomRightInner = ResPool.GetPen (control_color);
			
			CPColor cpcolor = ResPool.GetCPColor (control_color);
			
			switch (style) {
			case Border3DStyle.Raised:
				penTopLeftInner = ResPool.GetPen (cpcolor.LightLight);
				penBottomRight = ResPool.GetPen (cpcolor.Dark);
				penBottomRightInner = ResPool.GetPen (BorderColor);
				break;
			case Border3DStyle.Sunken:
				penTopLeft = ResPool.GetPen (BorderColor);
				penTopLeftInner = ResPool.GetPen (cpcolor.Dark);
				penBottomRight = ResPool.GetPen (cpcolor.LightLight);
				break;
			case Border3DStyle.Etched:
				penTopLeft = penBottomRightInner = ResPool.GetPen (BorderColor);
				penTopLeftInner = penBottomRight = ResPool.GetPen (cpcolor.LightLight);
				break;
			case Border3DStyle.RaisedOuter:
				penBottomRight = ResPool.GetPen (cpcolor.Dark);
				break;
			case Border3DStyle.SunkenOuter:
				penTopLeft = ResPool.GetPen (BorderColor);
				penBottomRight = ResPool.GetPen (cpcolor.LightLight);
				break;
			case Border3DStyle.RaisedInner:
				penTopLeft = ResPool.GetPen (cpcolor.LightLight);
				penBottomRight = ResPool.GetPen (BorderColor);
				break;
			case Border3DStyle.SunkenInner:
				penTopLeft = ResPool.GetPen (cpcolor.Dark);
				break;
			case Border3DStyle.Flat:
				penTopLeft = penBottomRight = ResPool.GetPen (BorderColor);
				break;
			case Border3DStyle.Bump:
				penTopLeftInner = penBottomRight = ResPool.GetPen (cpcolor.Dark);
				break;
			default:
				break;
			}
			
			if ((sides & Border3DSide.Middle) != 0) {
				graphics.FillRectangle (ResPool.GetSolidBrush (control_color), rect);
			}
			
			if ((sides & Border3DSide.Left) != 0) {
				graphics.DrawLine (penTopLeft, rect.Left, rect.Bottom - 2, rect.Left, rect.Top);
				graphics.DrawLine (penTopLeftInner, rect.Left + 1, rect.Bottom - 2, rect.Left + 1, rect.Top);
			}
			
			if ((sides & Border3DSide.Top) != 0) {
				graphics.DrawLine (penTopLeft, rect.Left, rect.Top, rect.Right - 2, rect.Top);
				graphics.DrawLine (penTopLeftInner, rect.Left + 1, rect.Top + 1, rect.Right - 3, rect.Top + 1);
			}
			
			if ((sides & Border3DSide.Right) != 0) {
				graphics.DrawLine (penBottomRight, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine (penBottomRightInner, rect.Right - 2, rect.Top + 1, rect.Right - 2, rect.Bottom - 2);
			}
			
			if ((sides & Border3DSide.Bottom) != 0) {
				graphics.DrawLine (penBottomRight, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine (penBottomRightInner, rect.Left + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
			}
		}
		
		public override void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			dc.FillRectangle (ResPool.GetSolidBrush (NiceBackColor), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);
			
			Color first_color = Color.White;
			Color second_color = NormalColor;
			
			if ((state & ButtonState.Flat) == ButtonState.Flat) {
				first_color = NormalColor;
				second_color = Color.White;
			} else
			if (((state & ButtonState.Flat) == ButtonState.Flat) &&
			    (((state & ButtonState.Checked) == ButtonState.Checked) || ((state & ButtonState.Pushed) == ButtonState.Pushed))) {
				first_color = PressedColor;
				second_color = Color.White;
			} else
			if (((state & ButtonState.Checked) == ButtonState.Checked) || ((state & ButtonState.Pushed) == ButtonState.Pushed)) {
				second_color = PressedColor;
			}
			
			Rectangle lgbRectangle = Rectangle.Inflate (rectangle, -1, -1);
			
			using (LinearGradientBrush lgbr = new LinearGradientBrush (new Point (rectangle.X, rectangle.Y), new Point (rectangle.X, rectangle.Bottom - 1), first_color, second_color)) {
				if ((state & ButtonState.Flat) == ButtonState.Flat) {
					lgbr.Blend = FlatBlend;
				} else {
					lgbr.Blend = NormalBlend;
				}
				dc.FillRectangle (lgbr, lgbRectangle);
			}
			
			Internal_DrawButton(dc, rectangle, BorderColor);
		}
		
		public override void CPDrawComboButton( Graphics dc, Rectangle rectangle, ButtonState state )
		{
			Point[]			arrow = new Point[ 3 ];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;
			
			Color first_color = Color.White;
			Color second_color = NormalColor;
			
//			rectangle.Width += 1;
			
			if ( ( state & ButtonState.Checked ) != 0 )
			{
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControlLightLight, ColorControlLight ), rectangle );
			}
			else
				dc.FillRectangle( ResPool.GetSolidBrush( Color.White ), rectangle );
			
			if ( ( state & ButtonState.Flat ) != 0 )
			{
				first_color = NormalColor;
				second_color = Color.White;
			}
			else
			{
				if ( ( state & ( ButtonState.Pushed | ButtonState.Checked ) ) != 0 )
				{
					first_color = Color.White;
					second_color = PressedColor;
				}
//				else
//				{
//					CPDrawBorder3D( graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl );
//				}
			}
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rectangle.X + 1, rectangle.Y + 1 ), new Point( rectangle.X + 1, rectangle.Bottom - 2 ), first_color, second_color ) )
			{
				lgbr.Blend = NormalBlend;
				dc.FillRectangle( lgbr, rectangle.X + 2, rectangle.Y + 1, rectangle.Width - 4, rectangle.Height - 3 );
			}
			
			Internal_DrawButton(dc, new Rectangle(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2), BorderColor);
			
			rect = new Rectangle( rectangle.X + rectangle.Width / 4, rectangle.Y + rectangle.Height / 4, rectangle.Width / 2, rectangle.Height / 2 );
			centerX = rect.Left + rect.Width / 2;
			centerY = rect.Top + rect.Height / 2;
			shiftX = Math.Max( 1, rect.Width / 8 );
			shiftY = Math.Max( 1, rect.Height / 8 );
			
			if ( ( state & ButtonState.Pushed ) != 0 )
			{
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
			if ( ( state & ButtonState.Inactive ) != 0 )
			{
				using ( Pen pen = new Pen( SystemColors.ControlLightLight, 2 ) )
				{
					dc.DrawLines( pen, arrow );
				}
				
				/* Move away from the shadow */
				P1.X -= 1;		P1.Y -= 1;
				P2.X -= 1;		P2.Y -= 1;
				P3.X -= 1;		P3.Y -= 1;
				
				arrow[ 0 ] = P1;
				arrow[ 1 ] = P2;
				arrow[ 2 ] = P3;
				
				using ( Pen pen = new Pen( SystemColors.ControlDark, 2 ) )
				{
					dc.DrawLines( pen, arrow );
				}
			}
			else
			{
				using ( Pen pen = new Pen( SystemColors.ControlText, 2 ) )
				{
					dc.DrawLines( pen, arrow );
				}
			}
			
			dc.SmoothingMode = old_smoothing_mode;
		}
		
		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton( Graphics dc, Rectangle area, ScrollButton scroll_button_type, ButtonState state )
		{
			bool enabled = ( state == ButtonState.Inactive ) ? false: true;
			
			DrawScrollButtonPrimitive( dc, area, state, scroll_button_type );
			
			Pen pen = null;
			
			if ( enabled )
				pen = ResPool.GetSizedPen( arrow_color, 2 );
			else
				pen = ResPool.GetSizedPen( ColorGrayText, 2 );
			
			/* Paint arrows */
			
			int centerX = area.Left + area.Width / 2;
			int centerY = area.Top + area.Height / 2;
			
			int shift = 0;
			
			if ( ( state & ButtonState.Pushed ) != 0 )
				shift = 1;
			
			int min_3 = 3;
			int min_2 = 2;
			if ( area.Width < 12 || area.Height < 12 ) {
				min_3 = 2;
				min_2 = 1;
			}
			
			Point[]	arrow = new Point [3];
			
			switch (scroll_button_type) {
			case ScrollButton.Down:
				centerY += shift;
				arrow [0] = new Point (centerX - min_3, centerY - min_2);
				arrow [1] = new Point (centerX, centerY + min_2);
				arrow [2] = new Point (centerX + min_3, centerY - min_2);
				break;
			case ScrollButton.Up:
				centerY -= shift;
				arrow [0] = new Point (centerX - min_3, centerY + min_2);
				arrow [1] = new Point (centerX, centerY - min_2);
				arrow [2] = new Point (centerX + min_3, centerY + min_2);
				break;
			case ScrollButton.Left:
				centerX -= shift;
				arrow [0] = new Point (centerX + min_2, centerY - min_3);
				arrow [1] = new Point (centerX - min_2, centerY);
				arrow [2] = new Point (centerX + min_2, centerY + min_3);
				break;
			case ScrollButton.Right:
				centerX += shift;
				arrow [0] = new Point (centerX - min_2, centerY - min_3);
				arrow [1] = new Point (centerX + min_2, centerY);
				arrow [2] = new Point (centerX - min_2, centerY + min_3);
				break;
			default:
				break;
			}

			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			dc.DrawLines (pen, arrow);
			
			dc.SmoothingMode = old_smoothing_mode;
		}
		
		public override void CPDrawSizeGrip( Graphics dc, Color backColor, Rectangle bounds )
		{
			Point pt = new Point( bounds.Right - 4, bounds.Bottom - 4 );
			
			dc.DrawImage( size_grip_bmp, pt );
			dc.DrawImage( size_grip_bmp, pt.X, pt.Y - 5 );
			dc.DrawImage( size_grip_bmp, pt.X, pt.Y - 10 );
			dc.DrawImage( size_grip_bmp, pt.X - 5, pt.Y );
			dc.DrawImage( size_grip_bmp, pt.X - 10, pt.Y );
			dc.DrawImage( size_grip_bmp, pt.X - 5, pt.Y - 5 );
		}
		
		private void DrawScrollBarThumb( Graphics dc, Rectangle area, ScrollBar bar )
		{
			LinearGradientBrush lgbr = null;
			
			if ( bar.vert )
				lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.Right, area.Y ), Color.White, NormalColor );
			else
				lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.X, area.Bottom ), Color.White, NormalColor );
			
			lgbr.Blend = NormalBlend;
			
			if ( bar.vert )
			{
				dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
				Internal_DrawButton (dc, area, BorderColor);
				
				// draw grip lines only if stere is enough space
				if ( area.Height > 20 )
				{
					int mid_y = area.Y + ( area.Height / 2 );
					int mid_x = area.X + ( area.Width / 2 );
					
					Pen lpen = ResPool.GetSizedPen( MouseOverColor, 2 );
					dc.DrawLine( lpen, mid_x - 3, mid_y, mid_x + 3, mid_y );
					dc.DrawLine( lpen, mid_x - 3, mid_y - 4, mid_x + 3, mid_y - 4 );
					dc.DrawLine( lpen, mid_x - 3, mid_y + 4, mid_x + 3, mid_y + 4 );
					
					Pen spen = ResPool.GetPen( Color.White );
					dc.DrawLine( spen, mid_x - 3, mid_y - 1, mid_x + 3, mid_y - 1 );
					dc.DrawLine( spen, mid_x - 3, mid_y - 5, mid_x + 3, mid_y - 5 );
					dc.DrawLine( spen, mid_x - 3, mid_y + 3, mid_x + 3, mid_y + 3 );
				}
			}
			else
			{
				dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
				Internal_DrawButton (dc, area, BorderColor);
				
				// draw grip lines only if stere is enough space
				if ( area.Width > 20 )
				{
					int mid_x = area.X +  ( area.Width / 2 );
					int mid_y = area.Y +  ( area.Height / 2 );
					
					Pen lpen = ResPool.GetSizedPen( MouseOverColor, 2 );
					dc.DrawLine( lpen, mid_x, mid_y - 3, mid_x, mid_y + 3 );
					dc.DrawLine( lpen, mid_x - 4, mid_y - 3, mid_x - 4, mid_y + 3 );
					dc.DrawLine( lpen, mid_x + 4, mid_y - 3, mid_x + 4, mid_y + 3 );
					
					Pen spen = ResPool.GetPen( Color.White );
					dc.DrawLine( spen, mid_x - 1, mid_y - 3, mid_x - 1, mid_y + 3 );
					dc.DrawLine( spen, mid_x - 5, mid_y - 3, mid_x - 5, mid_y + 3 );
					dc.DrawLine( spen, mid_x + 3, mid_y - 3, mid_x + 3, mid_y + 3 );
				}
			}
			
			lgbr.Dispose( );
		}
		
		/* Nice scroll button */
		public void DrawScrollButtonPrimitive( Graphics dc, Rectangle area, ButtonState state, ScrollButton scroll_button_type )
		{
			Pen pen = ResPool.GetPen( BorderColor );
			
			dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), area );
			
			Color use_color;
			
			if ( ( state & ButtonState.Pushed ) == ButtonState.Pushed )
				use_color = PressedColor;
			else
				use_color = NormalColor;
			
			Point[] points = null;
			
			LinearGradientBrush lgbr = null;
			
			switch ( scroll_button_type )
			{
				case ScrollButton.Left:
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.Right - 1, area.Y ), use_color, Color.White );
					lgbr.Blend = FlatBlend;
					dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
					
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
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.Right - 1, area.Y ), Color.White, use_color );
					lgbr.Blend = NormalBlend;
					dc.FillRectangle( lgbr, area.X, area.Y + 1, area.Width - 1, area.Height - 2 );
					
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
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.X, area.Bottom - 1 ), use_color, Color.White );
					lgbr.Blend = FlatBlend;
					dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
					
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
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.X, area.Bottom - 1 ), Color.White, use_color );
					lgbr.Blend = NormalBlend;
					dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
					
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
		public override void DrawGroupBox (Graphics dc,  Rectangle area, GroupBox box) 
		{
			StringFormat	text_format;
			SizeF		size;
			int		width;
			int		y;
			Rectangle	rect;
			
			rect = box.ClientRectangle;
			
			dc.FillRectangle (ResPool.GetSolidBrush (box.BackColor), rect);
			
			text_format = new StringFormat();
			text_format.HotkeyPrefix = HotkeyPrefix.Show;
			
			size = dc.MeasureString (box.Text, box.Font);
			width = (int) size.Width;
			
			if (width > box.Width - 16)
				width = box.Width - 16;
			
			y = box.Font.Height / 2;
			
			Pen pen = ResPool.GetPen( BorderColor );
			
			/* Draw group box*/
			Point[] points = new Point[] {
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
			if (box.Enabled) {
				dc.DrawString (box.Text, box.Font, ResPool.GetSolidBrush (box.ForeColor), 10, 0, text_format);
			} else {
				CPDrawStringDisabled (dc, box.Text, box.Font, box.ForeColor, 
						      new RectangleF (10, 0, width,  box.Font.Height), text_format);
			}
			text_format.Dispose ();
		}
		#endregion
	} //class
}
