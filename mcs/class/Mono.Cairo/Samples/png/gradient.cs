//
//
//	Mono.Cairo drawing samples using image (png) as drawing surface
//	Author: Hisham Mardam Bey <hisham@hisham.cc>
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
using Cairo;
	
public class CairoTest
{	
	static readonly double  M_PI = 3.14159265358979323846;

	
	
	static void draw (Cairo.Context gr, int width, int height)
	{
		gr.Scale (width, height);
		gr.LineWidth = 0.04;
		LinearGradient pat;		
		
		pat = new LinearGradient (0.0, 0.0,  0.0, 1.0);
		pat.AddColorStop (1, new Color (0, 0, 0, 1) );
		pat.AddColorStop (0, new Color (1, 1, 1, 1) );
		gr.Rectangle ( new PointD (0, 0),
			       1, 1
			       );
		
		gr.Pattern =  pat;
		gr.Fill ();
		pat.Destroy ();

		RadialGradient pat2 = new RadialGradient (0.45, 0.4, 0.1,
				     0.4,  0.4, 0.5);
		
		pat2.AddColorStop (0, new Color (1, 1, 1, 1) );
		pat2.AddColorStop (1, new Color (0, 0, 0, 1) );
		gr.Pattern =  pat2;
		gr.Arc (0.5, 0.5, 0.3, 0, 2 * M_PI);
		gr.Fill ();
		pat2.Destroy ();
	}
		
	static void Main ()
	{		
		Surface s = new ImageSurface (Format.ARGB32, 500, 500);
		Cairo.Context g = new Cairo.Context (s);

		draw (g, 500, 500);
		
		s.WriteToPng ("gradient.png");
	}
}
