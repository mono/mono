/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Style
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Drawing;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class Style //: IParserAttribute, ITemplateable, IStateManager,
	                   //    ICloneable
	{
		private BooleanOption bold      = BooleanOption.NotSet;
		private BooleanOption italic    = BooleanOption.NotSet;
		private Alignment     alignment = Alignment.NotSet;
		private Color         backColor = Color.Empty;
		private Color         foreColor = Color.Empty;
		private string        fontName  = String.Empty;
		private FontSize      fontSize  = FontSize.NotSet;
		private Wrapping      wrapping  = Wrapping.NotSet;

		private bool marked = false;

		private MobileControl  control = null;
		private DeviceSpecific deviceSpecific;
		private FontInfo       font;
		private Style          referredStyle;

		public Style()
		{
		}

		public object this[object key]
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public object this[object key, bool inherit]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Alignment Alignment
		{
			get
			{
				return this.alignment;
			}
			set
			{
				this.alignment = value;
			}
		}

		public Color BackColor
		{
			get
			{
				return this.backColor;
			}
			set
			{
				this.backColor = value;
			}
		}

		public DeviceSpecific DeviceSpecific
		{
			get
			{
				return deviceSpecific;
			}
			set
			{
				deviceSpecific = value;
			}
		}

		public FontInfo Font
		{
			get
			{
				if(font == null)
				{
					font = new FontInfo(this);
				}
				return font;
			}
		}

		public Color ForeColor
		{
			get
			{
				return this.foreColor;
			}
			set
			{
				this.foreColor = value;
			}
		}

		public bool IsTemlpated
		{
			get
			{
				if(this.deviceSpecific != null)
				{
					return deviceSpecific.HasTemplates;
				} else if(ReferredStyle != null)
				{
					return ReferredStyle.IsTemlpated;
				}
				return false;
			}
		}

		internal Style ReferredStyle
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		internal BooleanOption Bold
		{
			get
			{
				return this.bold;
			}
			set
			{
				this.bold = value;
			}
		}

		internal BooleanOption Italic
		{
			get
			{
				return this.italic;
			}
			set
			{
				this.italic = value;
			}
		}

		internal string FontName
		{
			get
			{
				return this.fontName;
			}
			set
			{
				this.fontName = value;
			}
		}

		internal FontSize FontSize
		{
			get
			{
				return this.fontSize;
			}
			set
			{
				this.fontSize = value;
			}
		}

		public MobileControl Control
		{
			get
			{
				return this.control;
			}
			set
			{
				this.control = value;
			}
		}
	}
}
