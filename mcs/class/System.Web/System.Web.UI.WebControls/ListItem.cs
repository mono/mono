/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListItem
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[ControlBuilder(typeof(ListItemControlBuilder))]
	public sealed class ListItem : IStateManager, IParserAccessor, IAttributeAccessor
	{
		private static int MARKED   = (0x01 << 0);
		private static int SELECTED = (0x01 << 1);
		private static int DIRTY_T  = (0x01 << 2);
		private static int DIRTY_V  = (0x01 << 3);

		private static int selBits;

		private AttributeCollection attributes;
		private string              text;
		private string              val;

		public ListItem(string text, string value)
		{
			this.text  = text;
			this.val   = value;
			selBits    = 0x00;
			attributes = null;
		}

		public ListItem(string text): this(text, null)
		{
		}

		public ListItem(): this(null, null)
		{
		}

		public static ListItem FromString(string text)
		{
			return new ListItem(text);
		}

		public AttributeCollection Attributes
		{
			get
			{
				if(attributes == null)
					attributes = new AttributeCollection(new StateBag(true));
				return attributes;
			}
		}

		public bool Selected
		{
			get
			{
				return IsSet(SELECTED);
			}
			set
			{
				Set(SELECTED);
			}
		}

		internal bool Dirty
		{
			get
			{
				return (IsSet(DIRTY_T) && IsSet(DIRTY_V));
			}
			set
			{
				Set(DIRTY_T);
				Set(DIRTY_V);
			}
		}

		private bool IsSet(int bit)
		{
			return ( (selBits & bit) != 0x00 );
		}

		private void Set(int bit)
		{
			selBits |= bit;
		}

		public string Text
		{
			get
			{
				if(text!=null)
				{
					return text;
				}
				if(val!=null)
				{
					return val;
				}
				return String.Empty;
			}
			set
			{
				text = value;
				if(IsTrackingViewState)
				{
					Set(DIRTY_T);
				}
			}
		}

		public string Value
		{
			get
			{
				if(val!=null)
				{
					return val;
				}
				if(text!=null)
				{
					return text;
				}
				return String.Empty;
			}
			set
			{
				val = value;
				if(IsTrackingViewState)
				{
					Set(DIRTY_V);
				}
			}
		}

		string IAttributeAccessor.GetAttribute(string key)
		{
			return attributes[key];
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			attributes[key] = value;
		}

		/// <remarks>
		/// The data is parsed - object must be of type LiteralControl or DataBoundLiteralControl.
		/// In latter case, throw an exception telling that the data cannot be bind-ed.
		/// </remarks>
		void IParserAccessor.AddParsedSubObject(object obj)
		{
			if(obj is LiteralControl)
			{
				Text = ((LiteralControl)obj).Text;
				return;
			}
			if(obj is DataBoundLiteralControl)
			{
				throw new HttpException(HttpRuntime.FormatResourceString("Control_Cannot_DataBind","ListItem"));
			}
			throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type", "ListItem", obj.GetType().ToString()));
		}

		bool IsTrackingViewState
		{
			get
			{
				return IsSet(MARKED);
			}
		}

		internal void TrackViewState()
		{
			Set(MARKED);
		}

		internal void LoadViewState(object state)
		{
			if(state is Pair)
			{
				Pair tv = (Pair)state;
				if(tv.First!=null)
				{
					Text = (string)tv.First;
				}
				if(tv.Second!=null)
				{
					Value = (string)tv.Second;
				}
			}
			if(state is string)
			{
				Text = (string)state;
			}
		}

		internal object SaveViewState()
		{
			if(IsSet(DIRTY_T) && IsSet(DIRTY_V))
			{
				return new Pair(Text, Value);
			}
			if(IsSet(DIRTY_T))
			{
				return Text;
			}
			if(IsSet(DIRTY_V))
			{
				return new Pair(null, Value);
			}
			return null;
		}

		public override bool Equals (object o)
		{
			ListItem li = o as ListItem;
			if (li == null)
				return false;

			return (Text == li.Text && Value == li.Value);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override string ToString ()
		{
			return Text;
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return IsTrackingViewState;
			}
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		void IStateManager.LoadViewState(object state)
		{
			LoadViewState(state);
		}
	}
}
