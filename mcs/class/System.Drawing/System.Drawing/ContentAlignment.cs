//
// System.Drawing.ContentAlignment.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//

using System;
using System.ComponentModel;

namespace System.Drawing 
{
	[Serializable]
	[Editor ("System.Drawing.Design.ContentAlignmentEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public enum ContentAlignment {
		TopLeft      = 0x001,
		TopCenter    = 0x002,
		TopRight     = 0x004,
		MiddleLeft   = 0x010,
		MiddleCenter = 0x020,
		MiddleRight  = 0x040,
		BottomLeft   = 0x100,
		BottomCenter = 0x200,
		BottomRight  = 0x400
	}
}
