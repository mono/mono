//
// System.Drawing.Bitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
// Just to get things to compile
//
namespace System.Drawing {

	[MonoTODO]
	public sealed class Graphics : MarshalByRefObject, IDisposable {

		public void Dispose ()
		{
		}

		[MonoTODO]
		public void FillRectangle (Brush b, Rectangle r)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHdc (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IntPtr GetHdc ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ReleaseHdc (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString(string str, Font fnt, Brush br, PointF pt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString(string str, Font fnt, Brush br, RectangleF rc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString(string str, Font fnt, Brush br, PointF pt, StringFormat sf)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString(string str, Font fnt, Brush br, RectangleF rc, StringFormat sf)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString(string str, Font fnt, Brush br, float X, float Y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString(string str, Font fnt, Brush br, float X, float Y, StringFormat sf)
		{
			throw new NotImplementedException ();
		}
	}
}

