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

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[ParseChildren (true, "Text")]
#endif
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

#if NET_2_0
		private bool enabled;
#endif

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

#if NET_2_0
		public ListItem (string text, string value, bool enabled): this (text, value)
		{
			this.enabled = enabled;
		}
#endif

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

#if NET_2_0
//	    [TypeConverterAttribute (typeof (System.Web.UI.MinimizableAttributeTypeConverter))]
#endif
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

#if NET_2_0
		[Localizable (true)]
#endif
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

#if NET_2_0
		[Localizable (true)]
#endif
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
		
#if NET_2_0
		[MonoTODO ("Disable items in ListControl and subclasses")]
		[DefaultValue (true)]
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}
#endif		

		string IAttributeAccessor.GetAttribute (string key)
		{
			if (attributes == null)
				return null;

			return attributes [key];
		}

		void IAttributeAccessor.SetAttribute (string key, string value)
		{
			Attributes [key] = value;
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
