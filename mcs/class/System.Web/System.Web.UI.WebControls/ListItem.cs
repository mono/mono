//
// System.Web.UI.WebControls.ListItem.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

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
		private AttributeCollection attributes;
		private string              text;
		private string              val;
		private bool marked;
		private bool selected;
		private bool dirty_t;
		private bool dirty_v;

		public ListItem(string text, string value)
		{
			this.text  = text;
			this.val   = value;
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

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection Attributes
		{
			get
			{
				if(attributes == null)
					attributes = new AttributeCollection(new StateBag(true));
				return attributes;
			}
		}

		[DefaultValue (false)]
		public bool Selected
		{
			get {
				return selected;
			}

			set {
				selected = value;
			}
		}

		internal bool Dirty
		{
			get {
				return (dirty_t && dirty_v);
			}

			set {
				dirty_t = value;
				dirty_v = value;
			}
		}

		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.EncodedInnerDefaultProperty)]
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
				if (IsTrackingViewState)
					dirty_t = true;
			}
		}

		[DefaultValue ("")]
		public string Value
		{
			get {
				if (val != null)
					return val;

				if (text != null)
					return text;

				return String.Empty;
			}
			set
			{
				val = value;
				if(IsTrackingViewState)
					dirty_v = true;
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
				return marked;
			}
		}

		internal void TrackViewState()
		{
			marked = true;
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
		}

		internal object SaveViewState()
		{
			if (dirty_t && dirty_v)
				return new Pair(Text, Value);

			if (dirty_t)
				return new Pair (Text, null);

			if (dirty_v)
				return new Pair(null, Value);

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
