
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
 * Class     : Style
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Drawing;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class Style : IParserAccessor, ITemplateable, IStateManager,
	                     ICloneable
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
		private bool checkedStyleRef = false;

		private MobileControl  control = null;
		private DeviceSpecific deviceSpecific;
		private FontInfo       font;
		private Style          referredStyle;

		private StateBag state;

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

		protected internal StateBag State
		{
			get
			{
				if(this.state == null)
				{
					this.state = new StateBag();
					if(((IStateManager)this).IsTrackingViewState)
					{
						((IStateManager)state).TrackViewState();
					}
				}
				return this.state;
			}
		}

		internal void Refresh()
		{
			this.referredStyle = null;
			this.checkedStyleRef = false;
		}

		void IParserAccessor.AddParsedSubObject(object obj)
		{
			if(obj is DeviceSpecific)
			{
				DeviceSpecific ds = (DeviceSpecific) obj;
				if(this.DeviceSpecific == null)
					this.DeviceSpecific = ds;
				else
				{
					throw new ArgumentException("MobileControl" +
					                            "_NoMultipleDeviceSpecifics");
				}
			}
		}

		void IStateManager.LoadViewState(object state)
		{
			if(state != null)
			{
				this.Refresh();
				((IStateManager)State).LoadViewState(state);
			}
		}

		object IStateManager.SaveViewState()
		{
			if(this.state != null)
			{
				return ((IStateManager)state).SaveViewState();
			}
			return null;
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return this.marked;
			}
		}

		void IStateManager.TrackViewState()
		{
			this.marked = true;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ApplyTo(WebControls.WebControl control)
		{
			throw new NotImplementedException();
		}
	}
}
