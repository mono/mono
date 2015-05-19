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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Geoff Norton (gnorton@novell.com)
//
//


using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal class Cursor {
		internal static CarbonCursor defcur = new CarbonCursor (StdCursor.Default);

                internal static Bitmap DefineStdCursorBitmap (StdCursor id) {
			// FIXME
			return new Bitmap (16, 16);
                }
                internal static IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			CarbonCursor cc = new CarbonCursor (bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);

			return (IntPtr) GCHandle.Alloc (cc);
                }
		internal static IntPtr DefineStdCursor (StdCursor id) {
			CarbonCursor cc = new CarbonCursor (id);
		
			return (IntPtr) GCHandle.Alloc (cc);
		}
		internal static void SetCursor (IntPtr cursor) {
			if (cursor == IntPtr.Zero) {
				defcur.SetCursor ();
				return;
			}

			CarbonCursor cc = (CarbonCursor) ((GCHandle) cursor).Target;

			cc.SetCursor ();
		}
	}

	internal struct CarbonCursor {
		private Bitmap bmp;
		private Bitmap mask;
		private Color cursor_color;
		private Color mask_color;
		private int hot_x;
		private int hot_y;
		private StdCursor id;
		private bool standard;

                public CarbonCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			this.id = StdCursor.Default;
			this.bmp = bitmap;
			this.mask = mask;
			this.cursor_color = cursor_pixel;
			this.mask_color = mask_pixel;
			this.hot_x = xHotSpot;
			this.hot_y = yHotSpot;
			standard = true;
		}

		public CarbonCursor (StdCursor id) {
			this.id = id;
			this.bmp = null;
			this.mask = null;
			this.cursor_color = Color.Black;
			this.mask_color = Color.Black;
			this.hot_x = 0;
			this.hot_y = 0;
			standard = true;
		}

		public StdCursor StdCursor {
			get {
				return id;
			}
		}

		public Bitmap Bitmap {
			get { 
				return bmp;
			}
		}

		public Bitmap Mask {
			get { 
				return mask;
			}
		}

		public Color CursorColor {
			get {
				return cursor_color;
			}
		}

		public Color MaskColor {
			get {
				return mask_color;
			}
		}

		public int HotSpotX {
			get { 
				return hot_x;
			}
		}

		public int HotSpotY {
			get { 
				return hot_y;
			}
		}

		public void SetCursor () {
			if (standard)
				SetStandardCursor ();
			else	
				SetCustomCursor ();
		}

		public void SetCustomCursor () {
			throw new NotImplementedException ("We dont support custom cursors yet");
		}

		public void SetStandardCursor () {
			switch (id) {
				case StdCursor.AppStarting:
					SetThemeCursor (ThemeCursor.kThemeSpinningCursor);
					break;
				case StdCursor.Arrow:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.Cross:
					SetThemeCursor (ThemeCursor.kThemeCrossCursor);
					break;
				case StdCursor.Default:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.Hand:
					SetThemeCursor (ThemeCursor.kThemeOpenHandCursor);
					break;
				case StdCursor.Help:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.HSplit:
					SetThemeCursor (ThemeCursor.kThemeResizeLeftRightCursor);
					break;
				case StdCursor.IBeam:
					SetThemeCursor (ThemeCursor.kThemeIBeamCursor);
					break;
				case StdCursor.No:
					SetThemeCursor (ThemeCursor.kThemeNotAllowedCursor);
					break;
				case StdCursor.NoMove2D:
					SetThemeCursor (ThemeCursor.kThemeNotAllowedCursor);
					break;
				case StdCursor.NoMoveHoriz:
					SetThemeCursor (ThemeCursor.kThemeNotAllowedCursor);
					break;
				case StdCursor.NoMoveVert:
					SetThemeCursor (ThemeCursor.kThemeNotAllowedCursor);
					break;
				case StdCursor.PanEast:
					SetThemeCursor (ThemeCursor.kThemeResizeRightCursor);
					break;
				case StdCursor.PanNE:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.PanNorth:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.PanNW:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.PanSE:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.PanSouth:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.PanSW:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.PanWest:
					SetThemeCursor (ThemeCursor.kThemeResizeLeftCursor);
					break;
				case StdCursor.SizeAll:
					SetThemeCursor (ThemeCursor.kThemeResizeLeftRightCursor);
					break;
				case StdCursor.SizeNESW:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.SizeNS:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.SizeNWSE:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.SizeWE:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.UpArrow:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.VSplit:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
				case StdCursor.WaitCursor:
					SetThemeCursor (ThemeCursor.kThemeSpinningCursor);
					break;
				default:
					SetThemeCursor (ThemeCursor.kThemeArrowCursor);
					break;
			}
			return;
		}

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetThemeCursor (ThemeCursor cursor);

	}
}
