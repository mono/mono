using System;
using System.Drawing;

namespace System.Windows.Forms.Theming
{
	internal class ThemeElements
	{
		private static ThemeElementsDefault theme;

		static ThemeElements ()
		{
			string theme_var;

			theme_var = Environment.GetEnvironmentVariable ("MONO_THEME");

			if (theme_var == null)
				theme_var = "win32";
			else
				theme_var = theme_var.ToLower ();

			theme = new ThemeElementsDefault ();
		}

		#region Buttons
		public static void DrawButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor)
		{
			theme.DrawButton (g, bounds, state, backColor, foreColor);
		}

		public static void DrawFlatButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor, FlatButtonAppearance appearance)
		{
			theme.DrawFlatButton (g, bounds, state, backColor, foreColor, appearance);
		}

		public static void DrawPopupButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor)
		{
			theme.DrawPopupButton (g, bounds, state, backColor, foreColor);
		}
		#endregion
	}

	#region Internal Enums
	[Flags]
	internal enum ButtonThemeState
	{
		Normal = 1,
		Entered = 2,
		Pressed = 4,
		Disabled = 8,
		Default = 16
	}
	#endregion
}
