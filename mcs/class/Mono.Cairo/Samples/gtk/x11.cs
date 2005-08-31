//
//
//	X11 interface
//	Authors: Hisham Mardam Bey <hisham.mardambey@gmail.com>
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
using System.Runtime.InteropServices;


public class X11Test
{
        static readonly double  M_PI = 3.14159265358979323846;
	
	static void draw (Cairo.Graphics gr, int width, int height)
	{
		double xc = 0.5;
		double yc = 0.5;
		double radius = 0.4;
		double angle1 = 45.0  * (M_PI/180.0);  /* angles are specified */
		double angle2 = 180.0 * (M_PI/180.0);  /* in radians           */
		
		gr.Scale (width, height);
		gr.LineWidth = 0.04;

		
		gr.Arc (xc, yc, radius, angle1, angle2);
		gr.Stroke ();
		
		/* draw helping lines */
		gr.Color = new Color(1, 0.2, 0.2, 0.6);
		gr.Arc (xc, yc, 0.05, 0, 2*M_PI);
		gr.Fill ();
		gr.LineWidth = 0.03;
		gr.Arc (xc, yc, radius, angle1, angle1);
		gr.LineTo (new PointD(xc, yc));
		gr.Arc (xc, yc, radius, angle2, angle2);
		gr.LineTo (new PointD(xc, yc));
		gr.Stroke ();
		
	}	
	
	static void Main (string [] args)
	{
		Window win = new Window (640, 480);
		
		win.Show ();
		
		Cairo.Surface s = Cairo.Surface.CreateForXlib (win.Display,
			       win.XWindow,
			       X11.XDefaultVisual (win.Display, win.Screen),
			       (int)win.Width, (int)win.Height);

		
		Cairo.Graphics g = new Cairo.Graphics (s);
		
		draw (g, 640, 480);
		
		IntPtr xev = new IntPtr ();
		
		while (true) {
			
			X11.XNextEvent (win.Display, xev);
			
		}
		
	}
						
	public class Window {
		
		protected IntPtr display;
		protected int screen;
		protected IntPtr window;
		protected IntPtr gc;
		protected uint width = 0;
		protected uint height = 0;
		
		public Window () {
		}
		
		public Window (uint width, uint height)
		{
			this.width = width;
			this.height = height;
			
			display = X11.XOpenDisplay (0);
						
			IntPtr root = X11.XDefaultRootWindow (display);
			screen = X11.XDefaultScreen (display);
			
			window = X11.XCreateSimpleWindow (display, root, 0, 0,
					      width, height, (ulong)0,
					      X11.XWhitePixel (display, screen),
					      X11.XWhitePixel (display, screen));									
		}
		
		public int Show ()
		{
			return X11.XMapWindow(display, window);
		}
	
		public int Clear ()
		{
			return X11.XClearWindow (display, window);
		}
		
		public uint Width {
			get { return width; }
			set { width = value; }
		}
		
		public uint Height {
			get { return height; }
			set { height = value; }
		}
		
		public IntPtr Display {
			get { return display; }
		}
		
		public IntPtr XWindow {
			get { return window; }
		}
		
		public int Screen {
			get { return screen; }
		}
		
		public void Close ()
		{
			 X11.XCloseDisplay(display);
		}
						
	}

	
	public class X11
	{
		[DllImport("X11")]
		  internal static extern IntPtr XDefaultVisual (IntPtr display,
								int screen_number);
		
		[DllImport("X11")]
		  internal static extern IntPtr XDefaultRootWindow (IntPtr display);
		
		[DllImport("X11")]
		  internal static extern int XDefaultScreen (IntPtr display);
		
		[DllImport("X11")]
		  internal static extern IntPtr XCreateSimpleWindow (IntPtr display,
							      IntPtr window,
							      int x,
							      int y,
							      uint width,
							      uint height,
							      ulong border_width,
							      ulong border,
							      ulong background);
		[DllImport("X11")]
		  internal static extern int XMapWindow (IntPtr display,
							 IntPtr window);
		
		[DllImport("X11")]
		  internal static extern int XClearWindow (IntPtr display,
							   IntPtr window);
		
		[DllImport("X11")]
		  internal static extern IntPtr XOpenDisplay (int display_name);

		[DllImport("X11")]
		  internal static extern IntPtr XScreenOfDisplay (IntPtr display,
								  int screen_number);
						
		[DllImport("X11")]
		  internal static extern int XCloseDisplay(IntPtr display);
		
		[DllImport("X11")]
		  internal static extern ulong XBlackPixel (IntPtr display,
							    int screen);
		
		[DllImport("X11")]
		  internal static extern ulong XWhitePixel (IntPtr display,
							    int screen);
		
		[DllImport("X11")]
		  internal static extern int XNextEvent (IntPtr display,
							 IntPtr event_return);
	}

		  

}
