//
// System.Drawing.Bitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
// Just to get things to compile
//
namespace System.Drawing {

	public sealed class Graphics : MarshalByRefObject, IDisposable {

		public void Dispose ()
		{
		}

		public void FillRectangle (Brush b, Rectangle r)
		{
			throw new NotImplementedException ();
		}
	}
}
