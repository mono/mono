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
	}
}

