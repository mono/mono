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

	internal interface IBrush : IDisposable  {

		object Clone ();

		// this property helps to draw text using brush color
		Color TextColor 
		{
			get ;
		}
	}
}

