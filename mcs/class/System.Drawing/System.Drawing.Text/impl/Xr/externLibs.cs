//
// System.Drawing.Text.XrImpl.externLibs.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
//

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Drawing.Text {
	namespace XrImpl	{
	
		[StructLayout(LayoutKind.Sequential)]
		internal struct FcFontSet {
			internal int nfont;
            internal int sfont;
            internal IntPtr fonts; // FcPattern **fonts;
		}

		internal enum FcResult : int {
		    FcResultMatch, FcResultNoMatch, FcResultTypeMismatch, FcResultNoId
		}

		class Xft {
		
			const string XftImp = "Xft";
		
			[DllImport(XftImp, EntryPoint="XftListFonts")]
			internal static extern IntPtr XftListFontFamilies (IntPtr dpy, int screen, int zero1, IntPtr FC_FAMILY_PTR, int zero2 );

		}
		
		class Fontconfig {
			const string FontconfigImp = "fontconfig";
			
			internal static string FC_FAMILY = "family";
			internal static IntPtr FC_FAMILY_PTR;
			
			static Fontconfig() {
				FC_FAMILY_PTR = Marshal.StringToHGlobalAnsi(FC_FAMILY);
			}

			[DllImport(FontconfigImp, CharSet = CharSet.Ansi)]
			internal static extern int FcPatternGetString(IntPtr fcPattern, IntPtr obj, int n, ref IntPtr val);
		
			[DllImport(FontconfigImp, CharSet = CharSet.Ansi)]
			internal static extern int FcPatternGetString(int fcPattern, IntPtr obj, int n, ref IntPtr val);
		}
	}
}
