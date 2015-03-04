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

namespace System.Windows.Forms.VisualStyles
{
	public static class VisualStyleInformation
	{
		#region Public Static Properties
		public static string Author {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationAuthor;
			}
		}

		public static string ColorScheme {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationColorScheme;
			}
		}

		public static string Company {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationCompany;
			}
		}

		[MonoTODO(@"Cannot get this to return the same as MS's...")]
		public static Color ControlHighlightHot {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return SystemColors.ButtonHighlight;

				return VisualStyles.VisualStyleInformationControlHighlightHot;
			}
		}

		public static string Copyright {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationCopyright;
			}
		}

		public static string Description {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationDescription;
			}
		}

		public static string DisplayName {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationDisplayName;
			}
		}

		public static bool IsEnabledByUser {
			get {
				if (!VisualStyleInformation.IsSupportedByOS) 
					return false;
				
				return (VisualStyles.UxThemeIsAppThemed () && VisualStyles.UxThemeIsThemeActive ());
			}
		}
 
		public static bool IsSupportedByOS {
			get {
				return VisualStyles.VisualStyleInformationIsSupportedByOS;
			}
		}
 
		public static int MinimumColorDepth {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return 0;

				return VisualStyles.VisualStyleInformationMinimumColorDepth;
			}
		}

		public static string Size {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationSize;
			}
		}

		public static bool SupportsFlatMenus {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return false;

				return VisualStyles.VisualStyleInformationSupportsFlatMenus;
			}
		}

		[MonoTODO(@"Cannot get this to return the same as MS's...")]
		public static Color TextControlBorder {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return SystemColors.ControlDarkDark;

				return VisualStyles.VisualStyleInformationTextControlBorder;
			}
		}

		public static string Url {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationUrl;
			}
		}

		public static string Version {
			get {
				if (!VisualStyleRenderer.IsSupported)
					return string.Empty;

				return VisualStyles.VisualStyleInformationVersion;
			}
		}
		#endregion

		#region Private Static Properties
		static IVisualStyles VisualStyles {
			get {
				return VisualStylesEngine.Instance;
			}
		}
		#endregion
	}
}
