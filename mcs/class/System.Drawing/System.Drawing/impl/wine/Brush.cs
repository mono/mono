//
// System.Drawing.Brush.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing {

	namespace Win32Impl {
		internal abstract class Brush : MarshalByRefObject, IBrush, ICloneable
		{
			abstract public object Clone ();
        
			internal IntPtr hbrush_ = IntPtr.Zero;
        
			internal Brush()
			{
			}

			void IDisposable.Dispose ()
			{
				Dispose (true);
				System.GC.SuppressFinalize (this);
			}

			public void Dispose()
			{
				Dispose (true);
				System.GC.SuppressFinalize (this);
			}

			void Dispose (bool disposing)
			{
				if( disposing) {
					Win32.DeleteObject(hbrush_);
				}
			}

			~Brush ()
			{
				Dispose (false);
			}

			Color IBrush.TextColor 
			{
				get {
					return Color.Black;
				}
			}
		}
	}
}

