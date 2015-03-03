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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//	Everaldo Canuto  <ecanuto@novell.com>

using System;
using System.Drawing;
using System.Reflection;

namespace System.Windows.Forms.Theming
{
	internal class ThemeElements
	{
		private static ThemeElementsDefault theme;
		public static ThemeElementsDefault CurrentTheme {
			get { return theme; }
		}

		static ThemeElements ()
		{
			string theme_var;

			theme_var = Environment.GetEnvironmentVariable ("MONO_THEME");

			if (theme_var == null)
				theme_var = "win32";
			else
				theme_var = theme_var.ToLower ();
	
			theme = LoadTheme (theme_var);

		}

		private static ThemeElementsDefault LoadTheme (string themeName) 
		{
			if (themeName == "visualstyles")
				if (Application.VisualStylesEnabled)
					return new ThemeElementsVisualStyles ();
				else
					return new ThemeElementsDefault ();
			Assembly ass = Assembly.GetExecutingAssembly ();
			string iname = typeof(ThemeElements).FullName;
			string assemblyname = iname + themeName;
			Type type = ass.GetType (assemblyname, false, true);
			if (type != null) {
				object o = ass.CreateInstance (type.FullName);
				if (o != null)
					return (ThemeElementsDefault) o;
			}
			return new ThemeElementsDefault ();
		}

		#region Buttons
		public static void DrawButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor)
		{
			theme.ButtonPainter.Draw (g, bounds, state, backColor, foreColor);
		}

		public static void DrawFlatButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor, FlatButtonAppearance appearance)
		{
			theme.ButtonPainter.DrawFlat (g, bounds, state, backColor, foreColor, appearance);
		}

		public static void DrawPopupButton (Graphics g, Rectangle bounds, ButtonThemeState state, Color backColor, Color foreColor)
		{
			theme.ButtonPainter.DrawPopup (g, bounds, state, backColor, foreColor);
		}
		#endregion
		
		#region Painters

		public virtual Default.ButtonPainter ButtonPainter {
			get { return theme.ButtonPainter; }
		}

		public static Default.LabelPainter LabelPainter	{
			get { return theme.LabelPainter; }
		}

		public static Default.LinkLabelPainter LinkLabelPainter	{
			get { return theme.LinkLabelPainter; }
		}

		public virtual Default.TabControlPainter TabControlPainter {
			get { return theme.TabControlPainter; }
		}

		public virtual Default.CheckBoxPainter CheckBoxPainter {
			get { return theme.CheckBoxPainter; }
		}

		public virtual Default.RadioButtonPainter RadioButtonPainter {
			get { return theme.RadioButtonPainter; }
		}

		public virtual Default.ToolStripPainter ToolStripPainter {
			get { return theme.ToolStripPainter; }
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

	internal enum ElementState
	{
		Normal = 1,
		Hot = 2,
		Pressed = 3,
		Disabled = 4
	}
	#endregion
}