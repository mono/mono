//
// System.IMessageFilter.cs
//
// Author:
//   Dennis hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	interface IMessageFilter {
		bool PreFilterMessage(ref Message m);
	}
}

