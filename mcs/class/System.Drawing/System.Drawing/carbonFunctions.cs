//
// System.Drawing.carbonFunctions.cs
//
// Authors:
//      Geoff Norton (gnorton@customerdna.com>
//
// Copyright (C) 2004 Novell, Inc. (http://www.novell.com)
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

#define EnableClipping
#define EnableSiblingClipping
#undef EnableNCClipping
#undef DebugClipping
#undef DebugDrawing

using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Drawing {

	[SuppressUnmanagedCodeSecurity]
	internal class Carbon {
#if EnableClipping
		internal static Type hwnd_type;
		internal static FieldInfo hwnd_children_field;
		internal static FieldInfo hwnd_client_rectangle_field;
		internal static FieldInfo hwnd_x_field;
		internal static FieldInfo hwnd_y_field;
		internal static FieldInfo hwnd_width_field;
		internal static FieldInfo hwnd_height_field;
		internal static FieldInfo hwnd_whole_window_field;
		internal static FieldInfo hwnd_client_window_field;
		internal static MethodInfo get_hwnd;
		internal static MethodInfo get_clipping_rectangles;

		static Carbon () {
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				if (String.Equals (asm.GetName ().Name, "System.Windows.Forms")) {
					hwnd_type = asm.GetType ("System.Windows.Forms.Hwnd");
					if (hwnd_type != null) {
						get_hwnd = hwnd_type.GetMethod ("ObjectFromHandle");
						get_clipping_rectangles = hwnd_type.GetMethod ("GetClippingRectangles");
						hwnd_children_field = hwnd_type.GetField ("children", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_client_rectangle_field = hwnd_type.GetField ("client_rectangle", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_x_field = hwnd_type.GetField ("x", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_y_field = hwnd_type.GetField ("y", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_width_field = hwnd_type.GetField ("width", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_height_field = hwnd_type.GetField ("height", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_whole_window_field = hwnd_type.GetField ("whole_window", BindingFlags.NonPublic | BindingFlags.Instance);
						hwnd_client_window_field = hwnd_type.GetField ("client", BindingFlags.NonPublic | BindingFlags.Instance);
					}
				}
			}
		}
#endif

		internal static CarbonContext GetCGContextForNSView (IntPtr hwnd) {
			IntPtr cgContext = IntPtr.Zero;

			cgContext = objc_msgSend (objc_msgSend (objc_getClass ("NSGraphicsContext"), sel_registerName ("currentContext")), sel_registerName ("graphicsPort"));
			HIRect rect = new HIRect ();
			objc_msgSend_stret (ref rect, hwnd, sel_registerName ("bounds"));
			return new CarbonContext (cgContext, (int)rect.size.width, (int)rect.size.height);
		}

		internal static CarbonContext GetCGContextForView (IntPtr hwnd) {
			IntPtr cgContext = IntPtr.Zero;
			// Grab the window we're in
			IntPtr window = Carbon.GetControlOwner (hwnd);
			// Get the port of the window
			IntPtr port = Carbon.GetWindowPort (window);
			// Create a CGContext ref
			Carbon.CreateCGContextForPort (port, ref cgContext);

			// Get the bounds of the window
			QRect wBounds = new QRect ();
			Carbon.GetWindowBounds (window, 32, ref wBounds);
			
			// Get the bounds of the view
			HIRect vBounds = new HIRect ();
			Carbon.HIViewGetBounds (hwnd, ref vBounds);
			
			// Convert the view local bounds to window coordinates
			Carbon.HIViewConvertRect (ref vBounds, hwnd, IntPtr.Zero);

			Carbon.CGContextTranslateCTM (cgContext, vBounds.origin.x, (wBounds.bottom-wBounds.top)-(vBounds.origin.y+vBounds.size.height));

#if EnableClipping
			if (get_hwnd != null) {
			// Create the original rect path and clip to it
				IntPtr clip_path = CGPathCreateMutable ();
				HIRect rc_clip = new HIRect (0, 0, vBounds.size.width, vBounds.size.height);
#if DebugClipping
				Console.WriteLine ("--CLIP: {0}x{1}", vBounds.size.width, vBounds.size.height);
#endif
				CGPathAddRect (clip_path, IntPtr.Zero, rc_clip);
				CGContextBeginPath (cgContext);
				object hwnd_object = get_hwnd.Invoke (null, new object [] {hwnd});
				if (hwnd_object != null) {
					IntPtr whole_window = (IntPtr) hwnd_whole_window_field.GetValue (hwnd_object);

					if (hwnd == whole_window) {
#if EnableNCClipping
#if DebugClipping
						Console.WriteLine ("\tNCCLIP:");
#endif
						HIRect clip_rect = new HIRect ();
						Rectangle client_rect = (Rectangle) hwnd_client_rectangle_field.GetValue (hwnd_object);
						clip_rect.origin.x = (int) client_rect.X;
						clip_rect.origin.y = (int) client_rect.Y;
						clip_rect.size.width = (int) client_rect.Width;
						clip_rect.size.height = (int) client_rect.Height;
#if DebugClipping
						Console.WriteLine ("\tnc xor: {0}x{1} @ {2}x{3}", clip_rect.size.width, clip_rect.size.height, clip_rect.origin.x, clip_rect.origin.y);
#endif
						CGPathAddRect (clip_path, IntPtr.Zero, clip_rect);
						CGContextAddPath (cgContext, clip_path);
						CGContextEOClip (cgContext);
#if DebugClipping
						Console.WriteLine ("\tEOClip client_window");
#endif
#endif
					} else {
						ArrayList hwnd_children = (ArrayList) hwnd_children_field.GetValue (hwnd_object);
						ArrayList hwnd_clips = (ArrayList) get_clipping_rectangles.Invoke (hwnd_object, new object [0]); 
						int count = hwnd_children.Count;
#if EnableSiblingClipping
						count += hwnd_clips.Count;
#endif
						if (count > 0) {
#if DebugClipping
							Console.WriteLine ("\tCLIP:");
#endif
							HIRect [] clip_rects = new HIRect [count];
							for (int i = 0; i < hwnd_children.Count; i++) {
								clip_rects [i] = new HIRect ();
								clip_rects [i].origin.x = (int) hwnd_x_field.GetValue (hwnd_children [i]);
								clip_rects [i].origin.y = vBounds.size.height - (int) hwnd_y_field.GetValue (hwnd_children [i]) - (int) hwnd_height_field.GetValue (hwnd_children [i]);
								clip_rects [i].size.width = (int) hwnd_width_field.GetValue (hwnd_children [i]);
								clip_rects [i].size.height = (int) hwnd_height_field.GetValue (hwnd_children [i]);
#if DebugClipping
								Console.WriteLine ("\tc xor: {0}x{1} @ {2}x{3}", clip_rects [i].size.width, clip_rects [i].size.height, clip_rects [i].origin.x, clip_rects [i].origin.y);
#endif
							}

#if EnableSiblingClipping
							for (int i = 0; i < hwnd_clips.Count; i++) {
								clip_rects [hwnd_children.Count+i] = new HIRect ();
								clip_rects [hwnd_children.Count+i].origin.x = ((Rectangle) hwnd_clips [i]).X;
								clip_rects [hwnd_children.Count+i].origin.y = ((Rectangle) hwnd_clips [i]).Y;
								clip_rects [hwnd_children.Count+i].size.width = ((Rectangle) hwnd_clips [i]).Width;
								clip_rects [hwnd_children.Count+i].size.height = ((Rectangle) hwnd_clips [i]).Height;
#if DebugClipping
								Console.WriteLine ("\ts xor: {0}x{1} @ {2}x{3}", clip_rects [i].size.width, clip_rects [i].size.height, clip_rects [i].origin.x, clip_rects [i].origin.y);
#endif
							}
#endif
							CGPathAddRects (clip_path, IntPtr.Zero, clip_rects, count);
							CGContextAddPath (cgContext, clip_path);
							CGContextEOClip (cgContext);
#if DebugClipping
							Console.WriteLine ("\tEOClip");
#endif
						} else {
#if DebugClipping
							Console.WriteLine ("\tClip");
#endif
							CGContextAddPath (cgContext, clip_path);
							CGContextClip (cgContext);
						}
					}
#if DebugClipping
					Console.WriteLine ("--ENDCLIP:");
#endif
				}
#endif
			}

#if DebugDrawing
			Console.WriteLine ("--DRAW:");
			Console.WriteLine ("\t{0:X}: {1}x{2}", hwnd, (int)vBounds.size.width, (int)vBounds.size.height);
#endif
			return new CarbonContext (cgContext, (int)vBounds.size.width, (int)vBounds.size.height);
		}
		#region Cocoa Methods
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_getClass(string className); 
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector, string argument);  
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern void objc_msgSend_stret(ref HIRect arect, IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern IntPtr sel_registerName(string selectorName);         
		#endregion

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetBounds (IntPtr vHnd, ref HIRect r);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertRect (ref HIRect r, IntPtr a, IntPtr b);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlOwner (IntPtr aView);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowBounds (IntPtr wHnd, uint reg, ref QRect rect);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CFRelease (IntPtr cgContext);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextClipToRect (IntPtr cgContext, HIRect clip);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextClipToRects (IntPtr cgContext, HIRect [] clip_rects, int count);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CreateCGContextForPort (IntPtr port, ref IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextTranslateCTM (IntPtr cgc, float tx, float ty);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextScaleCTM (IntPtr cgc, float x, float y);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextFlush (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextSynchronize (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr CGPathCreateMutable ();
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGPathAddRects (IntPtr path, IntPtr _void, HIRect [] rects, int count);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGPathAddRect (IntPtr path, IntPtr _void, HIRect rect);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextBeginPath (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextAddPath (IntPtr cgc, IntPtr path);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextClip (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextEOClip (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextEOFillPath (IntPtr cgc);
	}

	internal struct CGSize {
		public float width;
		public float height;
	}

	internal struct CGPoint {
		public float x;
		public float y;
	}

	internal struct HIRect {
		public HIRect (float x, float y, float width, float height) {
			this.origin.x = x;
			this.origin.y = y;
			this.size.width = width;
			this.size.height = height;
		}

		public CGPoint origin;
		public CGSize size;
	}

	internal struct QRect
	{
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct CarbonContext
	{
		public IntPtr ctx;
		public int width;
		public int height;

		public CarbonContext (IntPtr ctx, int width, int height)
		{
			this.ctx = ctx;
			this.width = width;
			this.height = height;
		}
	}
}
