//
// System.Drawing.Imaging.JPEGCodec.cs
//
// Author: 
//		Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/2003 Ximian, Inc.

// Codec is declared in libgdiplus
namespace System.Drawing.Imaging
{
  internal class JPEGCodec
  {
    internal static ImageCodecInfo CodecInfo {
      get {
	ImageCodecInfo info = new ImageCodecInfo();
	info.Flags =
	  ImageCodecFlags.Encoder |
	  ImageCodecFlags.Decoder |
	  ImageCodecFlags.Builtin |
	  ImageCodecFlags.SupportBitmap;
	info.FormatDescription = "JPEG file format";
	info.FormatID = System.Drawing.Imaging.ImageFormat.Jpeg.Guid;
	info.MimeType = "image/jpeg";
	info.Version = 1;

	byte[][] signaturePatterns = new byte[1][];
	signaturePatterns[0] = new byte[]{0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00};
	info.SignaturePatterns = signaturePatterns;

	byte[][] signatureMasks = new byte[1][];
	signatureMasks[0] = new byte[]{0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff};
	info.SignatureMasks = signatureMasks;

	return info;
      }
    }
  }
}
