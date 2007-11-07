//
// System.Drawing.carbonFunctions.cs
//
// Authors:
//      Geoff Norton (gnorton@customerdna.com>
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
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
#undef UseQDContext

using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Drawing {

	[SuppressUnmanagedCodeSecurity]
	internal class Carbon {
		internal static Hashtable contextReference = new Hashtable ();
		internal static Hashtable contextMap = new Hashtable ();

#if EnableClipping
		internal static Type handle_type;
		internal static FieldInfo handle_children_field;
		internal static FieldInfo handle_client_rectangle_field;
		internal static FieldInfo handle_x_field;
		internal static FieldInfo handle_y_field;
		internal static FieldInfo handle_width_field;
		internal static FieldInfo handle_height_field;
		internal static FieldInfo handle_whole_window_field;
		internal static FieldInfo handle_client_window_field;
		internal static MethodInfo get_handle;
		internal static MethodInfo get_clipping_rectangles;

		static Carbon () {
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				if (String.Equals (asm.GetName ().Name, "System.Windows.Forms")) {
					handle_type = asm.GetType ("System.Windows.Forms.Hwnd");
					if (handle_type != null) {
						get_handle = handle_type.GetMethod ("ObjectFromHandle");
						get_clipping_rectangles = handle_type.GetMethod ("GetClippingRectangles");
						handle_children_field = handle_type.GetField ("children", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_client_rectangle_field = handle_type.GetField ("client_rectangle", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_x_field = handle_type.GetField ("x", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_y_field = handle_type.GetField ("y", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_width_field = handle_type.GetField ("width", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_height_field = handle_type.GetField ("height", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_whole_window_field = handle_type.GetField ("whole_window", BindingFlags.NonPublic | BindingFlags.Instance);
						handle_client_window_field = handle_type.GetField ("client", BindingFlags.NonPublic | BindingFlags.Instance);
					}
				}
			}
		}
#endif

		internal static CarbonContext GetCGContextForNSView (IntPtr handle) {
			IntPtr context = IntPtr.Zero;
			Rect view_bounds = new Rect ();

			context = objc_msgSend (objc_msgSend (objc_getClass ("NSGraphicsContext"), sel_registerName ("currentContext")), sel_registerName ("graphicsPort"));
			objc_msgSend_stret (ref view_bounds, handle, sel_registerName ("bounds"));
			return new CarbonContext (IntPtr.Zero, context, (int)view_bounds.size.width, (int)view_bounds.size.height);
		}

		internal static CarbonContext GetCGContextForView (IntPtr handle) {
			IntPtr context = IntPtr.Zero;
			IntPtr port = IntPtr.Zero;
			IntPtr window = IntPtr.Zero;

			QDRect window_bounds = new QDRect ();
			Rect view_bounds = new Rect ();

			window = GetControlOwner (handle);
			port = GetWindowPort (window);
			
			context = GetContext (port);

			GetWindowBounds (window, 32, ref window_bounds);
			HIViewGetBounds (handle, ref view_bounds);

			HIViewConvertRect (ref view_bounds, handle, IntPtr.Zero);

			CGContextTranslateCTM (context, view_bounds.origin.x, (window_bounds.bottom - window_bounds.top) - (view_bounds.origin.y + view_bounds.size.height));

#if EnableClipping
			if (get_handle != null) {
				// Create the original rect path and clip to it
				IntPtr clip_path = CGPathCreateMutable ();
				Rect rc_clip = new Rect (0, 0, view_bounds.size.width, view_bounds.size.height);
#if DebugClipping
				Console.WriteLine ("--CLIP: {0}x{1}", view_bounds.size.width, view_bounds.size.height);
#endif
				CGPathAddRect (clip_path, IntPtr.Zero, rc_clip);
				CGContextBeginPath (context);
				object handle_object = get_handle.Invoke (null, new object [] {handle});
				if (handle_object != null) {
					IntPtr whole_window = (IntPtr) handle_whole_window_field.GetValue (handle_object);

					if (handle == whole_window) {
#if EnableNCClipping
#if DebugClipping
						Console.WriteLine ("\tNCCLIP:");
#endif
						Rect clip_rect = new Rect ();
						Rectangle client_rect = (Rectangle) handle_client_rectangle_field.GetValue (handle_object);
						clip_rect.origin.x = (int) client_rect.X;
						clip_rect.origin.y = (int) client_rect.Y;
						clip_rect.size.width = (int) client_rect.Width;
						clip_rect.size.height = (int) client_rect.Height;
#if DebugClipping
						Console.WriteLine ("\tnc xor: {0}x{1} @ {2}x{3}", clip_rect.size.width, clip_rect.size.height, clip_rect.origin.x, clip_rect.origin.y);
#endif
						CGPathAddRect (clip_path, IntPtr.Zero, clip_rect);
						CGContextAddPath (context, clip_path);
						CGContextEOClip (context);
#if DebugClipping
						Console.WriteLine ("\tEOClip client_window");
#endif
#endif
					} else {
						ArrayList handle_children = (ArrayList) handle_children_field.GetValue (handle_object);
						ArrayList handle_clips = (ArrayList) get_clipping_rectangles.Invoke (handle_object, new object [0]); 
						int count = handle_children.Count;
#if EnableSiblingClipping
						count += handle_clips.Count;
#endif
						if (count > 0) {
#if DebugClipping
							Console.WriteLine ("\tCLIP:");
#endif
							Rect [] clip_rects = new Rect [count];
							for (int i = 0; i < handle_children.Count; i++) {
								clip_rects [i] = new Rect ();
								clip_rects [i].origin.x = (int) handle_x_field.GetValue (handle_children [i]);
								clip_rects [i].origin.y = view_bounds.size.height - (int) handle_y_field.GetValue (handle_children [i]) - (int) handle_height_field.GetValue (handle_children [i]);
								clip_rects [i].size.width = (int) handle_width_field.GetValue (handle_children [i]);
								clip_rects [i].size.height = (int) handle_height_field.GetValue (handle_children [i]);
#if DebugClipping
								Console.WriteLine ("\tc xor: {0}x{1} @ {2}x{3}", clip_rects [i].size.width, clip_rects [i].size.height, clip_rects [i].origin.x, clip_rects [i].origin.y);
#endif
							}

#if EnableSiblingClipping
							for (int i = 0; i < handle_clips.Count; i++) {
								clip_rects [handle_children.Count+i] = new Rect ();
								clip_rects [handle_children.Count+i].origin.x = ((Rectangle) handle_clips [i]).X;
								clip_rects [handle_children.Count+i].origin.y = ((Rectangle) handle_clips [i]).Y;
								clip_rects [handle_children.Count+i].size.width = ((Rectangle) handle_clips [i]).Width;
								clip_rects [handle_children.Count+i].size.height = ((Rectangle) handle_clips [i]).Height;
#if DebugClipping
								Console.WriteLine ("\ts xor: {0}x{1} @ {2}x{3}", clip_rects [i].size.width, clip_rects [i].size.height, clip_rects [i].origin.x, clip_rects [i].origin.y);
#endif
							}
#endif
							CGPathAddRects (clip_path, IntPtr.Zero, clip_rects, count);
							CGContextAddPath (context, clip_path);
							CGContextEOClip (context);
#if DebugClipping
							Console.WriteLine ("\tEOClip");
#endif
						} else {
#if DebugClipping
							Console.WriteLine ("\tClip");
#endif
							CGContextAddPath (context, clip_path);
							CGContextClip (context);
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
			Console.WriteLine ("\t{0:X}: {1}x{2}", handle, (int)view_bounds.size.width, (int)view_bounds.size.height);
#endif
			return new CarbonContext (port, context, (int)view_bounds.size.width, (int)view_bounds.size.height);
		}

		internal static IntPtr GetContext (IntPtr port) {
			IntPtr context = IntPtr.Zero;

#if UseQDContext
			if (contextMap [port] != null)
				context = (IntPtr) contextMap [port];

			if (context == IntPtr.Zero) {
				QDBeginCGContext (port, ref context);
				contextMap [port] = context;
			}

			if (contextReference [port] != null)
				contextReference [port] = ((int) contextReference [port]) + 1;
			else
				contextReference [port] = 1;
#else
			CreateCGContextForPort (port, ref context);

			contextMap [port] = context;
#endif

			return context;
		}

		internal static void ReleaseContext (IntPtr port) {
			IntPtr context = IntPtr.Zero;
			
#if UseQDContext
			if (contextReference [port] != null) {
				contextReference [port] = ((int) contextReference [port]) - 1;

				if (0 >= (int) contextReference [port]) {
					context = (IntPtr) contextMap [port];
					QDEndCGContext (port, ref context);
	
					contextMap [port] = null;
					contextReference [port] = null;
				}
			}
#else
			context = (IntPtr) contextMap [port];
			CFRelease (context);
#endif
		}

		#region Cocoa Methods
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_getClass(string className); 
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector, string argument);  
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern void objc_msgSend_stret(ref Rect arect, IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern IntPtr sel_registerName(string selectorName);         
		#endregion

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetBounds (IntPtr vHnd, ref Rect r);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertRect (ref Rect r, IntPtr a, IntPtr b);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlOwner (IntPtr aView);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowBounds (IntPtr wHnd, uint reg, ref QDRect rect);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CreateCGContextForPort (IntPtr port, ref IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CFRelease (IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void QDBeginCGContext (IntPtr port, ref IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void QDEndCGContext (IntPtr port, ref IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextClipToRect (IntPtr context, Rect clip);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextClipToRects (IntPtr context, Rect [] clip_rects, int count);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextTranslateCTM (IntPtr context, float tx, float ty);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextScaleCTM (IntPtr context, float x, float y);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextFlush (IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextSynchronize (IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr CGPathCreateMutable ();
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGPathAddRects (IntPtr path, IntPtr _void, Rect [] rects, int count);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGPathAddRect (IntPtr path, IntPtr _void, Rect rect);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextBeginPath (IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextAddPath (IntPtr context, IntPtr path);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextClip (IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextEOClip (IntPtr context);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextEOFillPath (IntPtr context);
	}

	internal struct CGSize {
		public float width;
		public float height;
	}

	internal struct CGPoint {
		public float x;
		public float y;
	}

	internal struct Rect {
		public Rect (float x, float y, float width, float height) {
			this.origin.x = x;
			this.origin.y = y;
			this.size.width = width;
			this.size.height = height;
		}

		public CGPoint origin;
		public CGSize size;
	}

	internal struct QDRect
	{
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct CarbonContext
	{
		public IntPtr port;
		public IntPtr ctx;
		public int width;
		public int height;

		public CarbonContext (IntPtr port, IntPtr ctx, int width, int height)
		{
			this.port = port;
			this.ctx = ctx;
			this.width = width;
			this.height = height;
		}
	}
}
