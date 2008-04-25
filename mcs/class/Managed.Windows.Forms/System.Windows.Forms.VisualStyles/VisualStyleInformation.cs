//
// VisualStyleInformation.cs
//
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;
#if !NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Windows.Forms.VisualStyles
{
#if NET_2_0
	public static
#endif
	class VisualStyleInformation
	{
#if !NET_2_0
		static readonly Color system_colors_button_highlight;
		// These are taken from System.Drawing.
		internal enum GetSysColorIndex {
			COLOR_BTNHIGHLIGHT		= 20,
		}
		// Windows values are in BGR format and without alpha
		// so we force it to opaque (or everything will be transparent) and reverse B and R
		static uint GetSysColor (GetSysColorIndex index)
		{
			uint bgr = Win32GetSysColor (index);
			return 0xFF000000 | (bgr & 0xFF) << 16 | (bgr & 0xFF00) | (bgr >> 16);
		}
		[DllImport ("user32.dll", EntryPoint = "GetSysColor", CallingConvention = CallingConvention.StdCall)]
		static extern uint Win32GetSysColor (GetSysColorIndex index);
		static VisualStyleInformation ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				system_colors_button_highlight = Color.FromArgb ((int)GetSysColor (GetSysColorIndex.COLOR_BTNHIGHLIGHT));
			else
				system_colors_button_highlight = Color.FromArgb (0xFF, 0xFF, 0xFF, 0xFF);
		}
#endif
		#region Public Static Properties
		public static string Author {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("AUTHOR");
			}
		}

		public static string ColorScheme {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				Text.StringBuilder ThemeName = new Text.StringBuilder (260);
				Text.StringBuilder ColorName = new Text.StringBuilder (260);
				Text.StringBuilder SizeName = new Text.StringBuilder (260);
				UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

				return ColorName.ToString ();
			}
		}

		public static string Company {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("COMPANY");
			}
		}

		[MonoTODO(@"Cannot get this to return the same as MS's...")]
		public static Color ControlHighlightHot {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return
#if NET_2_0
					SystemColors.ButtonHighlight
#else
					system_colors_button_highlight
#endif
					;

				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "BUTTON");

				uint retval = UXTheme.GetThemeSysColor (theme, 1621);
				UXTheme.CloseThemeData (theme);

				return System.Drawing.Color.FromArgb ((int)(0x000000FFU & retval),
					(int)(0x0000FF00U & retval) >> 8, (int)(0x00FF0000U & retval) >> 16);
			}
		}

		public static string Copyright {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("COPYRIGHT");
			}
		}

		public static string Description {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("DESCRIPTION");
			}
		}

		public static string DisplayName {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("DISPLAYNAME");
			}
		}

		public static bool IsEnabledByUser {
			get {
				if (!VisualStyleInformation.IsSupportedByOS) 
					return false;
				
				return (UXTheme.IsAppThemed () && UXTheme.IsThemeActive ());
			}
		}
 
		public static bool IsSupportedByOS {
			get {
				// Supported OS's should be NT based and at least XP (XP, 2003, Vista)
				if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version >= new Version (5, 1))) 
					return true;
				
				return false;
			}
		}
 
		public static int MinimumColorDepth {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return 0;

				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "BUTTON");
				int retval;
				
				UXTheme.GetThemeSysInt (theme, 1301, out retval);
				UXTheme.CloseThemeData (theme);

				return retval;
			}
		}

		public static string Size {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				Text.StringBuilder ThemeName = new Text.StringBuilder (260);
				Text.StringBuilder ColorName = new Text.StringBuilder (260);
				Text.StringBuilder SizeName = new Text.StringBuilder (260);
				UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

				return SizeName.ToString ();
			}
		}

		public static bool SupportsFlatMenus {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return false;

				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "BUTTON");
				bool retval;

				retval = UXTheme.GetThemeSysBool (theme, 1001) == 0 ? false : true;
				UXTheme.CloseThemeData (theme);

				return retval;
			}
		}

		[MonoTODO(@"Cannot get this to return the same as MS's...")]
		public static Color TextControlBorder {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return SystemColors.ControlDarkDark;

				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "EDIT");

				uint retval = UXTheme.GetThemeSysColor (theme, 1611);
				UXTheme.CloseThemeData (theme);

				return System.Drawing.Color.FromArgb ((int)(0x000000FFU & retval),
						     (int)(0x0000FF00U & retval) >> 8, (int)(0x00FF0000U & retval) >> 16);
			}
		}

		public static string Url {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("URL");
			}
		}

		public static string Version {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return GetData ("VERSION");
			}
		}
		#endregion
		
		#region Internal Helper Methods
		internal static string GetData (string propertyName)
		{
			Text.StringBuilder ThemeName = new Text.StringBuilder (260);
			Text.StringBuilder ColorName = new Text.StringBuilder (260);
			Text.StringBuilder SizeName = new Text.StringBuilder (260);

			UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

			Text.StringBuilder PropertyValue = new Text.StringBuilder (260);

			UXTheme.GetThemeDocumentationProperty (ThemeName.ToString(), propertyName, PropertyValue, PropertyValue.Capacity);

			return PropertyValue.ToString ();
		}
		#endregion
	}
}