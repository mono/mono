
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
 * Namespace : System.Web.UI.MobileControls
 * Class     : FontInfo
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
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
