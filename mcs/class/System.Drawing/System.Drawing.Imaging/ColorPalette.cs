//
// System.Drawing.Imaging.ColorPalette.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
// Author:
//   Miguel de Icaza (miguel@ximian.com
//

using System;
using System.Drawing;
namespace System.Drawing.Imaging
{
	public sealed class ColorPalette {
		// 0x1: the color values in the array contain alpha information
		// 0x2: the color values are grayscale values.
		// 0x4: the colors in the array are halftone values.

		int flags;
		Color [] entries;

		//
		// There is no public constructor, this will be used somewhere in the
		// drawing code
		//
		internal ColorPalette ()
		{
			flags = 0;
			entries = new Color [0];
		}

		internal ColorPalette (int flags, Color[] colors) {
			this.flags = flags;
			entries = colors;
		}

		public Color [] Entries {
			get {
				return entries;
			}
		}

		public int Flags {
			get {
				return flags;
			}
		}
	}
}
