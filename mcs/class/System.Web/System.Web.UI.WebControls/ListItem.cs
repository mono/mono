//
// System.Web.UI.WebControls.ListItem.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
	public sealed class ListItem : IAttributeAccessor, IParserAccessor, IStateManager {
	
		public ListItem (string text, string value)
		{
			Text = text;
			Value = value;
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
			Triplet t = (Triplet) state;
			if (!(t.First is bool))
				text = (string) t.First;
			if (!(t.Second is bool))
				value = (string) t.Second;
			sb = (StateBag) t.Third;
		}
	

		object IStateManager.SaveViewState () 
		{
			return SaveViewState ();
		}

		internal object SaveViewState ()
		{
			Triplet t = new Triplet ();
			t.First = text_dirty ? (object) text : (object) false;
			t.Second = value_dirty ? (object) value : (object) false;
			t.Third = sb;
			return t;
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		internal void TrackViewState ()
		{
			tracking = true;
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
			get {
				return tracking;
			}
		
		}

		[DefaultValue(false)]
		public bool Selected {
			get {
				return selected;
			}
		
			set {
				selected = value;
			}
		}

		[DefaultValue("")]
		[PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty)]
		public string Text {
			get {
				string r = text;
				if (r == null)
					r = value;
				if (r == null)
					r = "";
				return r;
			}
		
			set {
				text = value;
				text_dirty = tracking;
			}
		}

		[DefaultValue("")]
		public string Value {
			get {
				string r = value;
				if (r == null)
					r = text;
				if (r == null)
					r = "";
				return r;
			}
		
			set {
				this.value = value;
				value_dirty = tracking;
			}
		}

		
		internal bool Dirty {
			get {
				return value_dirty || text_dirty;
			}
			
			set {
				value_dirty = text_dirty = value;
			}
		}

		string text;
		string value;
		bool selected;
		bool tracking;
		bool text_dirty;
		bool value_dirty;
		StateBag sb;
		AttributeCollection attrs;
	}
}
