//
// Sample application for adding two images into a single TIFF file
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
public class SampleComposeImages
{			
	
	public static void Main(string[] args)
	{					
		//get the codec for tiff files		
		ImageCodecInfo info = null;
		Bitmap pages = null;		
				
		foreach(ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())		
			if(ice.MimeType=="image/tiff")		
				info = ice;		
		if (info == null) {
			Console.WriteLine ("Couldn't get codec for image/tiff");
			return;
		}
		//use the save encoder		
		Encoder enc = Encoder.SaveFlag;	
		EncoderParameters ep=new EncoderParameters(1);		
		ep.Param[0] = new EncoderParameter (enc,(long)EncoderValue.MultiFrame);
				
		pages = (Bitmap) Image.FromFile ("../../Test/System.Drawing/bitmaps/almogaver32bits.bmp");	
		pages.Save ("out.tiff", info, ep);		
		
		//save second frame		
		ep.Param[0] = new EncoderParameter (enc,(long)EncoderValue.FrameDimensionPage);		
		Bitmap bm=(Bitmap)Image.FromFile ("../../Test/System.Drawing/bitmaps/nature24bits.jpg");		
		pages.SaveAdd (bm,ep);		
		
		ep.Param[0] = new EncoderParameter (enc,(long)EncoderValue.Flush);		
		pages.SaveAdd (ep);		
	}
}


