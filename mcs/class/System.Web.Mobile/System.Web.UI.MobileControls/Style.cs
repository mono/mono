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
	}
}
