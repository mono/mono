//
// Sample application for encoder/decoder
//
// Author:
//   Jordi Mas i Hernàndez, jordi@ximian.com
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

//
public class SampleImageCodecs
{	
	public static void DumpCodeInfo (ImageCodecInfo codec)
	{
		Console.WriteLine ("Clsid:" + codec.Clsid);
		Console.WriteLine ("FormatID:" + codec.FormatID);			
		Console.WriteLine ("Codec:" + codec.CodecName);
		Console.WriteLine ("DllName:" + codec.DllName);
		Console.WriteLine ("Extension:" + codec.FilenameExtension);
		Console.WriteLine ("Format:" + codec.FormatDescription);
		Console.WriteLine ("MimeType:" + codec.MimeType);
		Console.WriteLine ("Flags:" + codec.Flags);			
		Console.WriteLine ("Version:" + codec.Version);			
	}
	
	public static void Main(string[] args)
	{	
		ImageCodecInfo[] decoders =  ImageCodecInfo.GetImageDecoders();			
		ImageCodecInfo[] encoders =  ImageCodecInfo.GetImageEncoders();			
	
		Console.WriteLine ("Encoders ********");
		
		for (int i = 0; i < encoders.Length; i++) {
			DumpCodeInfo (encoders[i]);
			Console.WriteLine ("---");
		}

		Console.WriteLine ("Decoders ********");
		
		for (int i = 0; i < decoders.Length; i++) {
			DumpCodeInfo (decoders[i]);
			Console.WriteLine ("---");
		}
	}

}


