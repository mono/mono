//
// System.Windows.Forms.ScrollEventType.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	[Flags]
	public enum ScrollEventType {
		EndScroll = 8,
		First = 6,
		LargeDecrement = 2,
		LargeIncrement = 3,
		Last = 7,
		SmallDecrement = 0,
		ThumbPosition = 4,
		SmallIncrement = 1,
		ThumbTrack = 5
	}
}
