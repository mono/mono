//
// System.Drawing.Brush.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing {

	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable {

		internal IntPtr nativeObject;
		abstract public object Clone ();
        
		internal Brush()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing for now.
		}

		~Brush ()
		{
			Dispose (false);
		}
	}
}

