/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : MobileListItem
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public class MobileListItem : TemplateContainer, IStateManager
	{
		private int    index;
		private string text;
		private string value;

		private object             dataItem;
		private MobileListItemType itemType;

		private const int SELECTED = 0x00;
		private const int MARKED   = 0x01; // Tracking?
		private const int SELECTD  = 0x02; // Selection dirty flag
		private const int TEXTD    = 0x03; // Text      dirty flag
		private const int VALUED   = 0x04; // Value     dirty flag

		private BitArray flags = new BitArray(5);

		public MobileListItem()
		                     : this(null, null, null)
		{
		}

		public MobileListItem(MobileListItemType type)
		                     : this(null, null, null)
		{
			this.itemType = type;
		}

		public MobileListItem(string text)
		                     : this(null, text, null)
		{
		}

		public MobileListItem(string text, string value)
		                     : this(null, text, value)
		{
		}

		public MobileListItem(object dataItem, string text, string value)
		                     : base()
		{
			this.dataItem = dataItem;
			this.text     = text;
			this.value    = value;
			this.itemType = MobileListItemType.ListItem;
		}

		internal void SetIndex(int index)
		{
			this.index = index;
		}

		public object DataItem
		{
			get
			{
				return this.dataItem;
			}
			set
			{
				this.dataItem = value;
			}
		}

		public int Index
		{
			get
			{
				return this.index;
			}
		}

		internal MobileListItemType ItemType
		{
			get
			{
				return this.itemType;
			}
		}

		public bool Selected
		{
			get
			{
				return flags[SELECTED];
			}
			set
			{
				flags[SELECTED] = value;
				if(IsTrackingViewState)
				{
					flags[SELECTD] = true;
				}
			}
		}

		internal bool IsSelectionDirty
		{
			get
			{
				return flags[SELECTD];
			}
			set
			{
				flags[SELECTD] = value;
			}
		}

		internal bool IsDirty
		{
			get
			{
				return (flags[TEXTD] || flags[VALUED]);
			}
			set
			{
				flags[TEXTD] = value;
				flags[VALUED] = value;
			}
		}

		public string Text
		{
			get
			{
				if(this.text != null)
					return this.text;
				if(this.value != null)
					return this.value;
				return String.Empty;
			}
			set
			{
				this.text = value;
				if(IsTrackingViewState)
				{
					flags[TEXTD] = true;
				}
			}
		}

		public string Value
		{
			get
			{
				if(this.value != null)
					return this.value;
				if(this.text != null)
					return this.text;
				return String.Empty;
			}
			set
			{
				this.value = value;
				if(IsTrackingViewState)
				{
					flags[VALUED] = true;
				}
			}
		}

		public static implicit operator MobileListItem(string text)
		{
			return new MobileListItem(text);
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return flags[MARKED];
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is MobileListItem)
			{
				MobileListItem other = (MobileListItem) obj;
				return (this.Text == other.Text &&
				        this.Value == other.Value);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Text.GetHashCode() + Value.GetHashCode());
		}

		public static MobileListItem FromString(string text)
		{
			return new MobileListItem(text);
		}

		public override string ToString()
		{
			return this.Text;
		}

		protected override bool OnBubbleEvent(object sender, EventArgs e)
		{
			if(e is CommandEventArgs)
			{
				CommandEventArgs cmdArgs = (CommandEventArgs)e;
				RaiseBubbleEvent(this,
				                 new ListCommandEventArgs(this, sender,
				                                          cmdArgs));
				return true;
			}
			return false;
		}

		void IStateManager.TrackViewState()
		{
			flags[MARKED] = true;
		}

		object IStateManager.SaveViewState()
		{
			object retVal = null;
			string text = (flags[TEXTD] ? this.text : null);
			string value = (flags[VALUED] ? this.value : null);
			if(text != null || value != null)
			{
				retVal = new string[] { text, value };
			}
			return retVal;
		}

		void IStateManager.LoadViewState(object state)
		{
			if(state != null)
			{
				string[] data = (string[]) state;
				this.text = data[0];
				this.value = data[1];
			}
		}
	}
}
