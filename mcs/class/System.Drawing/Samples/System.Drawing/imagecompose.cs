//
// Sample application for adding two images into a single TIFF file
//
// Author:
//   Jordi Mas i Hernàndez, jordi@ximian.com
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


