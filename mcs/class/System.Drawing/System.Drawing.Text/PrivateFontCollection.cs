//
// System.Drawing.Text.PrivateFontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//
using System;
using System.Runtime.InteropServices;

namespace System.Drawing.Text {

	[ComVisible(false)]
	public sealed class PrivateFontCollection : FontCollection {

		// constructors
		[MonoTODO]
		public PrivateFontCollection() {
			throw new NotImplementedException ();
		}
		
		// methods
		[MonoTODO]
		[ComVisible(false)]
		public void AddFontFile(string filename) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ComVisible(false)]
		public void AddMemoryFont(IntPtr memory, int length) {
			throw new NotImplementedException ();
		}

	}

}
