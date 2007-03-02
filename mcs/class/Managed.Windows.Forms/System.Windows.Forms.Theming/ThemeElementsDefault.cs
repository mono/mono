using System;
using System.Drawing;

namespace System.Windows.Forms.Theming
{
	internal class ThemeElementsDefault
	{
		public ThemeElementsDefault ()
		{
		}

		protected SystemResPool ResPool { get { return ThemeEngine.Current.ResPool; } }
		
		#region Buttons
		#region Standard Button
		public virtual void DrawButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor)
		{
			bool is_themecolor = backColor.ToArgb () == ThemeEngine.Current.ColorControl.ToArgb () || backColor == Color.Empty ? true : false;
			CPColor cpcolor = is_themecolor ? CPColor.Empty : ResPool.GetCPColor (backColor);
			Pen pen;
			
			switch (state) {
				case ButtonThemeState.Normal:
				case ButtonThemeState.Entered:
				case ButtonThemeState.Disabled:
					pen = is_themecolor ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
					g.DrawLine (pen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 2);
					g.DrawLine (pen, bounds.X + 1, bounds.Y, bounds.Right - 2, bounds.Y);

					pen = is_themecolor ? SystemPens.Control : ResPool.GetPen (backColor);
					g.DrawLine (pen, bounds.X + 1, bounds.Y + 1, bounds.X + 1, bounds.Bottom - 3);
					g.DrawLine (pen, bounds.X + 2, bounds.Y + 1, bounds.Right - 3, bounds.Y + 1);

					pen = is_themecolor ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
					g.DrawLine (pen, bounds.X + 1, bounds.Bottom - 2, bounds.Right - 2, bounds.Bottom - 2);
					g.DrawLine (pen, bounds.Right - 2, bounds.Y + 1, bounds.Right - 2, bounds.Bottom - 3);

					pen = is_themecolor ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
					g.DrawLine (pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
					g.DrawLine (pen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 2);
					break;
				case ButtonThemeState.Pressed:
					g.DrawRectangle (ResPool.GetPen (foreColor), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

					bounds.Inflate (-1, -1);
					pen = is_themecolor ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
					g.DrawRectangle (pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
					break;
				case ButtonThemeState.Default:
					g.DrawRectangle (ResPool.GetPen (foreColor), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

					bounds.Inflate (-1, -1);
					pen = is_themecolor ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
					g.DrawLine (pen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 2);
					g.DrawLine (pen, bounds.X + 1, bounds.Y, bounds.Right - 2, bounds.Y);

					pen = is_themecolor ? SystemPens.Control : ResPool.GetPen (backColor);
					g.DrawLine (pen, bounds.X + 1, bounds.Y + 1, bounds.X + 1, bounds.Bottom - 3);
					g.DrawLine (pen, bounds.X + 2, bounds.Y + 1, bounds.Right - 3, bounds.Y + 1);

					pen = is_themecolor ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
					g.DrawLine (pen, bounds.X + 1, bounds.Bottom - 2, bounds.Right - 2, bounds.Bottom - 2);
					g.DrawLine (pen, bounds.Right - 2, bounds.Y + 1, bounds.Right - 2, bounds.Bottom - 3);

					pen = is_themecolor ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);
					g.DrawLine (pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
					g.DrawLine (pen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 2);
					break;
			}
		}
		#endregion

		#region FlatStyle Button
		public virtual void DrawFlatButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor, FlatButtonAppearance appearance)
		{
			bool is_themecolor = backColor.ToArgb () == ThemeEngine.Current.ColorControl.ToArgb () || backColor == Color.Empty ? true : false;
			CPColor cpcolor = is_themecolor ? CPColor.Empty : ResPool.GetCPColor (backColor);
			Pen pen;
			
			switch (state) {
				case ButtonThemeState.Normal:
				case ButtonThemeState.Disabled:
					// This will just use the BackColor
					break;
				case ButtonThemeState.Entered:
					if (appearance.MouseOverBackColor != Color.Empty)
						g.FillRectangle (ResPool.GetSolidBrush (appearance.MouseOverBackColor), bounds);
					break;
				case ButtonThemeState.Pressed:
					if (appearance.MouseDownBackColor != Color.Empty)
						g.FillRectangle (ResPool.GetSolidBrush (appearance.MouseDownBackColor), bounds);
					break;
				case ButtonThemeState.Default:
					if (appearance.CheckedBackColor != Color.Empty)
						g.FillRectangle (ResPool.GetSolidBrush (appearance.CheckedBackColor), bounds);
					break;
			}
			
			if (appearance.BorderColor == Color.Empty)
				pen = is_themecolor ? SystemPens.ControlDarkDark : ResPool.GetSizedPen (cpcolor.DarkDark, appearance.BorderSize);
			else
				pen = ResPool.GetSizedPen (appearance.BorderColor, appearance.BorderSize);
				
			bounds.Width -= 1;
			bounds.Height -= 1;
			g.DrawRectangle (pen, bounds);

			if (state == ButtonThemeState.Default || state == ButtonThemeState.Pressed) {
				bounds.Inflate (-1, -1);
				g.DrawRectangle (pen, bounds);
			}
		}
		#endregion

		#region Popup Button
		public virtual void DrawPopupButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor)
		{
			bool is_themecolor = backColor.ToArgb () == ThemeEngine.Current.ColorControl.ToArgb () || backColor == Color.Empty ? true : false;
			CPColor cpcolor = is_themecolor ? CPColor.Empty : ResPool.GetCPColor (backColor);
			Pen pen;

			switch (state) {
				case ButtonThemeState.Normal:
				case ButtonThemeState.Disabled:
				case ButtonThemeState.Pressed:
				case ButtonThemeState.Default:
					pen = is_themecolor ? SystemPens.ControlDarkDark : ResPool.GetPen (cpcolor.DarkDark);

					bounds.Width -= 1;
					bounds.Height -= 1;
					g.DrawRectangle (pen, bounds);

					if (state == ButtonThemeState.Default || state == ButtonThemeState.Pressed) {
						bounds.Inflate (-1, -1);
						g.DrawRectangle (pen, bounds);
					}
					break;
				case ButtonThemeState.Entered:
					pen = is_themecolor ? SystemPens.ControlLightLight : ResPool.GetPen (cpcolor.LightLight);
					g.DrawLine (pen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 2);
					g.DrawLine (pen, bounds.X + 1, bounds.Y, bounds.Right - 2, bounds.Y);

					pen = is_themecolor ? SystemPens.ControlDark : ResPool.GetPen (cpcolor.Dark);
					g.DrawLine (pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
					g.DrawLine (pen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 2);
					break;
			}
		}
		#endregion
		#endregion
	}
}
