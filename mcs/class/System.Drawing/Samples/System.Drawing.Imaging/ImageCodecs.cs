//
// Sample application for encoder/decoder
//
// Author:
//   Jordi Mas i Hernàndez, jordi@ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


