//
// System.Windows.Forms.ControlStyles.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	public enum ControlStyles {
		AllPaintingInWmPaint = 8192,
		CacheText = 16384,
		ContainerControl = 1,
		DoubleBuffer = 65536,
		EnableNotifyMessage = 32768,
		FixedHeight = 64,
		FixedWidth = 32,
		Opaque = 4,
		ResizeRedraw = 16,
		Selectable = 512,
		StandardClick = 256,
		StandardDoubleClick = 4096,
		SupportsTransparentBackColor = 2045,
		UserMouse = 1024,
		UserPaint = 2
	}
}
