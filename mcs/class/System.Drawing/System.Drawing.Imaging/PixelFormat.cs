// created on 20.02.2002 at 21:18
// 
// Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//		Dennis Hayes
//		dennish@raytek.com
//
//
namespace System.Drawing.Imaging {

	[Serializable]
	public enum PixelFormat {
		Alpha = 262144,
		Canonical = 2097152,
		DontCare = 0,
		Extended = 1048576,
		Format16bppArgb1555 = 397319,
		Format16bppGrayScale = 1052676,
		Format16bppRgb555 = 135173,
		Format16bppRgb565 = 135174,
		Format1bppIndexed = 196865,
		Format24bppRgb = 137224,
		Format32bppArgb = 2498570,
		Format32bppPArgb = 925707,
		Format32bppRgb = 139273,
		Format48bppRgb = 1060876, 
		Format4bppIndexed = 197634,
		Format64bppArgb = 3424269,
		Format64bppPArgb = 1851406,
		Format8bppIndexed = 198659,
		Gdi = 131072,
		Indexed = 65536,
		Max = 15,
		PAlpha = 524288,
		Undefined = 0 //shows up in enumcheck as second "dontcare".
	}
}
