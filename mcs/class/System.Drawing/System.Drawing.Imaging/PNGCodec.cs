//
// System.Drawing.Imaging.PNGCodec.cs
//
// Author: 
//		Alexandre Pigolkine (pigolkine@gmx.de)
//

// Codec is declared in libgdiplus
namespace System.Drawing.Imaging
{
  internal class PNGCodec
  {
    internal static ImageCodecInfo CodecInfo {
      get {
	ImageCodecInfo info = new ImageCodecInfo();
	info.Flags =
	  ImageCodecFlags.Encoder |
	  ImageCodecFlags.Decoder |
	  ImageCodecFlags.Builtin |
	  ImageCodecFlags.SupportBitmap;
	info.FormatDescription = "PNG file format";
	info.FormatID = System.Drawing.Imaging.ImageFormat.Png.Guid;
	info.MimeType = "image/png";
	info.Version = 1;

	byte[][] signaturePatterns = new byte[1][];
	signaturePatterns[0] = new byte[]{0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a};
	info.SignaturePatterns = signaturePatterns;

	byte[][] signatureMasks = new byte[1][];
	signatureMasks[0] = new byte[]{0xff,0xff,0xff,0xff,0xff,0xff};
	info.SignatureMasks = signatureMasks;

	return info;
      }
    }
  }
}
