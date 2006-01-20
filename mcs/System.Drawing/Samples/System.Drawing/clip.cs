// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Autor Jordi Mas <jordi@ximian.com>	
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
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

//
public class ClipSample
{		
	public static void Main () 
	{
		Bitmap bmp = new Bitmap (600, 500);
		Graphics dc = Graphics.FromImage (bmp);        					
		RectangleF[] rects = dc.Clip.GetRegionScans (new Matrix());
		
		for (int i = 0; i < rects.GetLength(0); i++)		
			Console.WriteLine ("clip: " + rects[i].ToString());
			
		Console.WriteLine ("VisibleClipBounds: " + dc.VisibleClipBounds);	 	
		Console.WriteLine ("IsVisible Point 650, 650: " + dc.IsVisible (650,650));
		Console.WriteLine ("IsVisible Point 0, 0: " + dc.IsVisible (0.0f, 0.0f));		
		
		Console.WriteLine ("IsVisible Rectangle (20,20,100,100): " + dc.IsVisible (new Rectangle (20,20,100,100)));
		Console.WriteLine ("IsVisible Rectangle (1000, 1000,100,100): " + dc.IsVisible (new RectangleF (1000, 1000,100,100)));		
        	
	}	

}


