//
// System.Drawing.Text.FontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//
using System;

namespace System.Drawing.Text {

	public abstract class FontCollection : IDisposable {

		// methods
		[MonoTODO]
		public void Dispose() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose(bool disposing) {
			throw new NotImplementedException ();
		}

		// properties
		[MonoTODO]
		public FontFamily[] Families {
			get { throw new NotImplementedException (); }
		}

	}

}
