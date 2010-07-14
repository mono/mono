//
// System.Web.UI.WebControls.ListItem.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005-2009 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS - no inheritance demand required because the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder(typeof(ListItemControlBuilder))]
	[TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
	[ParseChildren (true, "Text")]
	public sealed class ListItem : IAttributeAccessor, IParserAccessor, IStateManager
	{
#if NET_2_0
		public ListItem (string text, string value, bool enabled) : this (text, value)
		{
			this.enabled = enabled;
		}
#endif
		public ListItem (string text, string value)
		{
			this.text = text;
			this.value = value;
		}
	
		public ListItem (string text) : this (text, null)
		{
		}
	
		public ListItem () : this (null, null) 
		{
		}
	
		public static ListItem FromString (string text)
		{
			return new ListItem (text);
		}
	
		public override bool Equals (object o)
		{
			ListItem li = o as ListItem;
			if (li == null)
				return false;
			return li.Text == Text && li.Value == Value;
		}
	
		public override int GetHashCode ()
		{
			return Text.GetHashCode () ^ Value.GetHashCode ();
		
		}
	
		string IAttributeAccessor.GetAttribute (string key)
		{
			if (attrs == null)
				return null;

			return (string) Attributes [key];
		}
	
		void IAttributeAccessor.SetAttribute (string key, string value)
		{
			Attributes [key] = value;
		}
	
		void IParserAccessor.AddParsedSubObject (object obj)
		{
			LiteralControl lc = obj as LiteralControl;
			if (lc == null) {
				// obj.GetType() will throw a NullRef if obj is null. That's fine according to the test.
				throw new HttpException ("'ListItem' cannot have children of type " + obj.GetType ());
			}
			Text = lc.Text;
		}
	
		void IStateManager.LoadViewState (object state)
		{
			LoadViewState (state);
		}
		
		internal void LoadViewState (object state)
		{
			if (state == null)
				return;

			object [] states = (object []) state;

			if (states [0] != null) {
				sb = new StateBag (true);
				sb.LoadViewState (states[0]);
				sb.SetDirty (true);
			}
			
			if (states [1] != null)
				text = (string) states [1];
			if (states [2] != null)
				value = (string) states [2];
			if (states [3] != null)
				selected = (bool) states [3];
#if NET_2_0
			if (states [4] != null)
				enabled = (bool) states [4];
#endif
		}

		object IStateManager.SaveViewState () 
		{
			return SaveViewState ();
		}

		internal object SaveViewState ()
		{
			if (!dirty)
				return null;

#if NET_2_0
			object [] state = new object [5];
#else
			object [] state = new object [4];
#endif
			state [0] = sb != null ? sb.SaveViewState () : null;
			state [1] = (object) text;
			state [2] = (object) value;
			state [3] = (object) selected;
#if NET_2_0
			state [4] = (object) enabled;
#endif			
			return state;
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		internal void TrackViewState ()
		{
			tracking = true;
			if (sb != null) {
				sb.TrackViewState ();
				sb.SetDirty (true);
			}
		}

		public override string ToString ()
		{
			return Text;
		}
	
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AttributeCollection Attributes {
			get {
				if (attrs != null)
					return attrs;

				if (sb == null) {	
					sb = new StateBag (true);
					if (tracking)
						sb.TrackViewState ();
				}

				return attrs = new AttributeCollection (sb);
			}
		}

		bool IStateManager.IsTrackingViewState {
			get { return tracking; }
		}

		[TypeConverter ("System.Web.UI.MinimizableAttributeTypeConverter")]
		[DefaultValue(false)]
		public bool Selected {
			get { return selected; }
			set { 
				selected = value;
				if (tracking)
					SetDirty ();
			}
		}

		[Localizable (true)]
		[DefaultValue("")]
		[PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty)]
		public string Text {
			get {
				string r = text;
				if (r == null)
					r = value;
				if (r == null)
					r = String.Empty;
				return r;
			}
		
			set {
				text = value;
				if (tracking)
					SetDirty ();
			}
		}

		[Localizable (true)]
		[DefaultValue("")]
		public string Value {
			get {
				string r = value;
				if (r == null)
					r = text;
				if (r == null)
					r = String.Empty;
				return r;
			}
		
			set {
				this.value = value;
				if (tracking)
					SetDirty ();
			}
		}

		internal void SetDirty ()
		{
			dirty = true;
		}

#if NET_2_0
		[DefaultValue (true)]
		public bool Enabled
		{
			get { return enabled; }
			set {
				enabled = value;
				if (tracking)
					SetDirty ();
			}
		}
#endif

		internal bool HasAttributes {
			get { return attrs != null && attrs.Count > 0; }
		}

		string text;
		string value;
		bool selected;
		bool dirty;
#if NET_2_0
		bool enabled = true;
#endif
		bool tracking;
		StateBag sb;
		AttributeCollection attrs;
	}
}
