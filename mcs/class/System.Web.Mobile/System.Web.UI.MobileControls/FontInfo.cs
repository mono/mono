/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : FontInfo
 * Author    : Gaurav Vaish
 *
 * Copyright : 2002 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class FontInfo
	{
		private Style style;

		internal FontInfo(Style style)
		{
			this.style = style;
		}

		public BooleanOption Bold
		{
			get
			{
				if(style != null)
					return style.Bold;
				return BooleanOption.False;
			}
			set
			{
				if(style != null)
					style.Bold = value;
			}
		}

		public BooleanOption Italic
		{
			get
			{
				if(style != null)
					return style.Italic;
				return BooleanOption.False;
			}
			set
			{
				if(style != null)
					style.Italic = value;
			}
		}

		public string Name
		{
			get
			{
				if(style != null)
					return style.FontName;
				return String.Empty;
			}
			set
			{
				if(style != null)
					style.FontName = value;
			}
		}

		public FontSize Size
		{
			get
			{
				if(style != null)
					return style.FontSize;
				return FontSize.Normal;
			}
			set
			{
				if(style != null)
					style.FontSize = value;
			}
		}
		
		[MonoTODO]
		public override string ToString()
		{
			//string retVal = String.Empty;
			throw new NotImplementedException();
		}
	}
}
