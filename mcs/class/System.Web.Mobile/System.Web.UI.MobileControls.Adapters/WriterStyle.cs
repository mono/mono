
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : WriterStyle
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections;
using System.Drawing;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

namespace System.Web.UI.MobileControls.Adapters
{
	class WriterStyle
	{
		private bool layout;
		private Alignment alignment;
		private bool bold;
		private Color fontColor;
		private string fontName;
		private FontSize fontSize;
		private bool format;
		private bool italic;
		private Wrapping wrapping;

		public WriterStyle()
		{
			throw new NotImplementedException();
		}

		public WriterStyle(Style style)
		{
			throw new NotImplementedException();
		}

		public bool Layout
		{
			get
			{
				return this.layout;
			}
			set
			{
				this.layout = value;
			}
		}

		public Alignment Alignment
		{
			get
			{
				return alignment;
			}
			set
			{
				alignment = value;
			}
		}

		public bool Bold
		{
			get
			{
				return bold;
			}
			set
			{
				bold = value;
			}
		}

		public Color FontColor
		{
			get
			{
				return fontColor;
			}
			set
			{
				fontColor = value;
			}
		}

		public string FontName
		{
			get
			{
				return fontName;
			}
			set
			{
				fontName = value;
			}
		}

		public FontSize FontSize
		{
			get
			{
				return fontSize;
			}
			set
			{
				fontSize = value;
			}
		}

		public bool Format
		{
			get
			{
				return format;
			}
			set
			{
				format = value;
			}
		}

		public bool Italic
		{
			get
			{
				return italic;
			}
			set
			{
				italic = value;
			}
		}

		public Wrapping Wrapping
		{
			get
			{
				return wrapping;
			}
			set
			{
				wrapping = value;
			}
		}
	}
}
