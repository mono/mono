//
// System.Drawing.Text.InstalledFontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//         Alexandre Pigolkine ( pigolkine@gmx.de)
//
using System;
using System.Drawing;

namespace System.Drawing.Text {

	public sealed class InstalledFontCollection : FontCollection {
		internal static IFontCollectionFactory	factory_ = Factories.GetFontCollectionFactory();
		// constructors
		[MonoTODO]
		public InstalledFontCollection() {
			implementation_ = factory_.InstalledFontCollection();
		}
	}
}
