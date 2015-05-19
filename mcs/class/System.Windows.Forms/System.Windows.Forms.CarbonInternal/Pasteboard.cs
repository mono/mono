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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Geoff Norton (gnorton@novell.com)
//
//


using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms.CarbonInternal {
	internal class Pasteboard {
		private static IntPtr primary_pbref;
		private static IntPtr app_pbref;

		private static IntPtr internal_format;

		static Pasteboard () {
			PasteboardCreate (XplatUICarbon.__CFStringMakeConstantString("com.apple.pasteboard.clipboard"), ref primary_pbref);
			PasteboardCreate (IntPtr.Zero, ref app_pbref);
			internal_format = XplatUICarbon.__CFStringMakeConstantString ("com.novell.mono.mwf.pasteboard");
		}

		internal static object Retrieve (IntPtr pbref, int key) {
			UInt32 count = 0;

			key = (int)internal_format;

			PasteboardGetItemCount (pbref, ref count);
			for (int i = 1; i <= count; i++) {
				UInt32 itemid = 0;

				PasteboardGetItemIdentifier (pbref, (UInt32)i, ref itemid);
				//FIXME: We should get all the flavors and enumerate but we're cheating for now
				if (itemid == 0xFACE) {
					IntPtr pbdata = IntPtr.Zero;

					PasteboardCopyItemFlavorData (pbref, (UInt32)0xFACE, (UInt32)key, ref pbdata);
					if (pbdata != IntPtr.Zero) {
						GCHandle handle = (GCHandle) Marshal.ReadIntPtr (CFDataGetBytePtr (pbdata));
						
						return handle.Target;
					}
				}
			}
			return null;
		}

		internal static void Store (IntPtr pbref, object data, int key) {
			IntPtr gcdata = (IntPtr) GCHandle.Alloc (data);
			IntPtr pbdata = CFDataCreate (IntPtr.Zero, ref gcdata, Marshal.SizeOf (typeof (IntPtr)));

			key = (int)internal_format;

			PasteboardClear (pbref);
			PasteboardPutItemFlavor (pbref, (UInt32)0xFACE, (UInt32)key, pbdata, 0);
		}

		internal static IntPtr Primary {
			get { return primary_pbref; }
		}
		
		internal static IntPtr Application {
			get { return app_pbref; }
		}

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern IntPtr CFDataCreate (IntPtr allocator, ref IntPtr buf, Int32 length);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern IntPtr CFDataGetBytePtr (IntPtr data);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int PasteboardClear (IntPtr pbref);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int PasteboardCreate (IntPtr str, ref IntPtr pbref);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int PasteboardCopyItemFlavorData (IntPtr pbref, UInt32 itemid, UInt32 key, ref IntPtr data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int PasteboardGetItemCount (IntPtr pbref, ref UInt32 count);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int PasteboardGetItemIdentifier (IntPtr pbref, UInt32 itemindex, ref UInt32 itemid);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int PasteboardPutItemFlavor (IntPtr pbref, UInt32 itemid, UInt32 key, IntPtr data, UInt32 flags);
	}
}
