//
// System.Drawing.Imaging.BMPCodec.cs
//
// Author: 
//    Alexandre Pigolkine (pigolkine@gmx.de)
//	  Jordi Mas i Hernàndez (jmas@softcatala.org>, 2004
//    BITMAPINFOHEADER,Decode functions implemented using code/ideas from
//    CxImage (c)  07/Aug/2001 <ing.davide.pizzolato@libero.it>
//
// (C) 2002/2003 Ximian, Inc.
//
// Useful documentation about bitmaps
//
//	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/bitmaps_4v1h.asp
//	http://www.csdn.net/Dev/Format/windows/Bmp.html
//	http://www.fortunecity.com/skyscraper/windows/364/bmpffrmt.html
//
//	Header structure
//		BITMAPFILEHEADER
//		BITMAPINFOHEADER or BITMAPV4HEADER or BITMAPV5HEADER or BITMAPCOREHEADER 
//		RGBQUADS or RGBTRIPLE (optional)
//		Bitmap data
//

namespace System.Drawing.Imaging
{
  internal class BMPCodec
  {
    internal static ImageCodecInfo CodecInfo {
      get {
	ImageCodecInfo info = new ImageCodecInfo ();
	info.Flags =
	  ImageCodecFlags.Encoder |
	  ImageCodecFlags.Decoder |
	  ImageCodecFlags.Builtin |
	  ImageCodecFlags.SupportBitmap;
	
	info.FormatDescription = "BITMAP file format";
	info.FormatID = System.Drawing.Imaging.ImageFormat.Bmp.Guid;
	info.MimeType = "image/bmp";
	info.Version = 1;

	byte[][] signaturePatterns = new byte[1][];
	signaturePatterns[0] = new byte[2];
	signaturePatterns[0][0] = 0x42;
	signaturePatterns[0][1] = 0x4d;
	info.SignaturePatterns = signaturePatterns;

	byte[][] signatureMasks = new byte[1][];
	signatureMasks[0] = new byte[2];
	signatureMasks[0][0] = 0xff;
	signatureMasks[0][1] = 0xff;
	info.SignatureMasks = signatureMasks;

	return info;
      }
    }
  }
}
