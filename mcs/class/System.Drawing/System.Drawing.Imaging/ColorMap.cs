//
// System.Drawing.Imaging.ColorMap.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.IO;
using System.Reflection;

namespace System.Drawing.Imaging {

	public sealed class ColorMap {

		private Color newColor;
		private Color oldColor;

		// constructors
		public ColorMap() {
		}

		// properties
		public Color NewColor {
			get { return newColor; }
			set { newColor = value; }
		}

		public Color OldColor {
			get { return oldColor; }
			set { oldColor = value; }
		}
	}

}
