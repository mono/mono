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
