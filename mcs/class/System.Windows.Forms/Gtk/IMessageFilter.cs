//
// System.Windows.Forms.IMessageFilter.cs
//
// Author:
//   Dennis hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System.Threading;
namespace System.Windows.Forms {

	public interface IMessageFilter {
		bool PreFilterMessage(ref Message m);
	}
}

