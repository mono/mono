//
// System.Drawing.Text.XrImpl.FontCollection.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
//

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

// FIXME: this factory is in the System.Drawing namespace to keep code in System.Drawing/Factories.cs as it is.
// May be the following changes are needed:
//    - implement a new Factories in the System.Drawing.Text or
//    - modify code in existing Factories.cs to operate not only on System.Drawing namespace
//

namespace System.Drawing {
	namespace XrImpl {

		internal class FontCollectionFactory : System.Drawing.Text.IFontCollectionFactory {

			System.Drawing.Text.IFontCollection System.Drawing.Text.IFontCollectionFactory.InstalledFontCollection(){
				return new System.Drawing.Text.XrImpl.InstalledFontCollection();
			}

			System.Drawing.Text.IFontCollection System.Drawing.Text.IFontCollectionFactory.PrivateFontCollection() {
				return new System.Drawing.Text.XrImpl.PrivateFontCollection();
			}
		}
	}
}

namespace System.Drawing.Text {
	namespace XrImpl	{

		internal sealed class InstalledFontCollection : IFontCollection
		{
			ArrayList families;
			
			public void Dispose () {
			}

			public InstalledFontCollection(){
				families = new ArrayList();
				IntPtr fontSetPtr = Xft.XftListFontFamilies(IntPtr.Zero, 0, 0, Fontconfig.FC_FAMILY_PTR, 0);
				if( fontSetPtr != IntPtr.Zero) {
					FcFontSet fontSet = (FcFontSet)Marshal.PtrToStructure( fontSetPtr, typeof(FcFontSet));
					if( fontSet.nfont > 0) {
						int[] fontFamilyNamePtrs = new int[fontSet.nfont];
						Marshal.Copy( fontSet.fonts, fontFamilyNamePtrs, 0, fontFamilyNamePtrs.Length);
						IntPtr namePtr = IntPtr.Zero;
						for( int i = 0; i < fontFamilyNamePtrs.Length; i++) {
							int res = Fontconfig.FcPatternGetString(fontFamilyNamePtrs[i], Fontconfig.FC_FAMILY_PTR, 0, ref namePtr);
							string name = Marshal.PtrToStringAnsi(namePtr);
							//Console.WriteLine("InstalledFont : " + name);
							families.Add( new FontFamily(name));
						}
					}
				}
			}
			
			public FontFamily[] Families {
				get { return (FontFamily[])families.ToArray(typeof(FontFamily)); }
			}
		}
		
		internal sealed class PrivateFontCollection : IFontCollection
		{
			public void Dispose () {
			}

			public PrivateFontCollection(){
			}
			
			public FontFamily[] Families {
				get { throw new NotImplementedException(); }
			}
		}
	}
}
