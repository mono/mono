//
// System.Web.UI.HtmlControls.HtmlSelect.cs
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Web.UI.WebControls;
using System.Web.Util;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls 
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("ServerChange")]
	[ValidationProperty ("Value")]
	[ControlBuilder (typeof (HtmlSelectBuilder))]
	[SupportsEventValidation]
	public class HtmlSelect : HtmlContainerControl, IPostBackDataHandler, IParserAccessor
	{
		static readonly object EventServerChange = new object ();

		DataSourceView _boundDataSourceView;
		bool requiresDataBinding;
		bool _initialized;
		object datasource;
		ListItemCollection items;

		public HtmlSelect () : base ("select")
		{
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Data")]
		public virtual string DataMember {
			get {
				string member = Attributes["datamember"];

				if (member == null) {
					return (String.Empty);
				}

				return (member);
			}
			set {
				if (value == null) {
					Attributes.Remove ("datamember");
				} else {
					Attributes["datamember"] = value;
				}
			}
		}
		
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Data")]
		public virtual object DataSource {
			get {
				return (datasource);
			}
			set {
				if ((value != null) &&
				    !(value is IEnumerable) &&
				    !(value is IListSource)) {
					throw new ArgumentException ();
				}

				datasource = value;
			}
		}

		[DefaultValue ("")]
		public virtual string DataSourceID {
			get {
				return ViewState.GetString ("DataSourceID", "");
			}
			set {
				if (DataSourceID == value)
					return;
				ViewState ["DataSourceID"] = value;
				if (_boundDataSourceView != null)
					_boundDataSourceView.DataSourceViewChanged -= OnDataSourceViewChanged;
				_boundDataSourceView = null;
				OnDataPropertyChanged ();
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Data")]
		public virtual string DataTextField {
			get {
				string text = Attributes["datatextfield"];

				if (text == null) {
					return (String.Empty);
				}

				return (text);
			}
			set {
				if (value == null) {
					Attributes.Remove ("datatextfield");
				} else {
					Attributes["datatextfield"] = value;
				}
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Data")]
		public virtual string DataValueField {
			get {
				string value = Attributes["datavaluefield"];

				if (value == null) {
					return (String.Empty);
				}

				return (value);
			}
			set {
				if (value == null) {
					Attributes.Remove ("datavaluefield");
				} else {
					Attributes["datavaluefield"] = value;
				}
			}
		}

		public override string InnerHtml {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public override string InnerText {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		protected bool IsBoundUsingDataSourceID {
			get {
				return (DataSourceID.Length != 0);
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public ListItemCollection Items {
			get {
				if (items == null) {
					items = new ListItemCollection ();
					if (IsTrackingViewState)
						((IStateManager) items).TrackViewState ();
				}

				return (items);
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public bool Multiple {
			get {
				string multi = Attributes["multiple"];

				if (multi == null) {
					return (false);
				}

				return (true);
			}
			set {
				if (value == false) {
					Attributes.Remove ("multiple");
				} else {
					Attributes["multiple"] = "multiple";
				}
			}
		}
		
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public string Name {
			get {
				return (UniqueID);
			}
			set {
				/* Do nothing */
			}
		}

		protected bool RequiresDataBinding {
			get { return requiresDataBinding; }
			set { requiresDataBinding = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public virtual int SelectedIndex {
			get {
				/* Make sure Items has been initialised */
				ListItemCollection listitems = Items;

				for (int i = 0; i < listitems.Count; i++) {
					if (listitems[i].Selected) {
						return (i);
					}
				}

				/* There is always a selected item in
				 * non-multiple mode, if the size is
				 * <= 1
				 */
				if (!Multiple && Size <= 1) {
					/* Select the first item */
					if (listitems.Count > 0) {
						/* And make it stick
						 * if there is
						 * anything in the
						 * list
						 */
						listitems[0].Selected = true;
					}
					
					return (0);
				}
				
				return (-1);
			}
			set {
				ClearSelection ();

				if (value == -1 || items == null) {
					return;
				}

				if (value < 0 || value >= items.Count) {
					throw new ArgumentOutOfRangeException ("value");
				}

				items[value].Selected = true;
			}
		}

		/* "internal infrastructure" according to the docs,
		 * but has some documentation in 2.0
		 */
		protected virtual int[] SelectedIndices {
			get {
				ArrayList selected = new ArrayList ();

				int count = Items.Count;

				for (int i = 0; i < count; i++) {
					if (Items [i].Selected) {
						selected.Add (i);
					}
				}
				
				return ((int[])selected.ToArray (typeof (int)));
			}
		}
		

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Size {
			get {
				string size = Attributes["size"];

				if (size == null) {
					return (-1);
				}

				return (Int32.Parse (size, Helpers.InvariantCulture));
			}
			set {
				if (value == -1) {
					Attributes.Remove ("size");
				} else {
					Attributes["size"] = value.ToString ();
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Value {
			get {
				int sel = SelectedIndex;

				if (sel >= 0 && sel < Items.Count) {
					return (Items[sel].Value);
				}
				
				return (String.Empty);
			}
			set {
				int sel = Items.IndexOf (value);

				if (sel >= 0) {
					SelectedIndex = sel;
				}
			}
		}

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerChange {
			add {
				Events.AddHandler (EventServerChange, value);
			}
			remove {
				Events.RemoveHandler (EventServerChange, value);
			}
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (!(obj is ListItem)) {
				throw new HttpException ("HtmlSelect can only contain ListItem");
			}

			Items.Add ((ListItem)obj);
			
			base.AddParsedSubObject (obj);
		}

		/* "internal infrastructure" according to the docs,
		 * but has some documentation in 2.0
		 */
		protected virtual void ClearSelection ()
		{
			if (items == null) {
				return;
			}

			int count = items.Count;
			for (int i = 0; i < count; i++) {
				items[i].Selected = false;
			}
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return (base.CreateControlCollection ());
		}

		protected void EnsureDataBound ()
		{
			if (IsBoundUsingDataSourceID && RequiresDataBinding)
				DataBind ();
		}

		protected virtual IEnumerable GetData ()
		{
			if (DataSource != null && IsBoundUsingDataSourceID)
				throw new HttpException ("Control bound using both DataSourceID and DataSource properties.");

			if (DataSource != null)
				return DataSourceResolver.ResolveDataSource (DataSource, DataMember);

			if (!IsBoundUsingDataSourceID)
				return null;

			IEnumerable result = null;

			DataSourceView boundDataSourceView = ConnectToDataSource ();
			boundDataSourceView.Select (DataSourceSelectArguments.Empty, delegate (IEnumerable data) { result = data; });

			return result;
		}

		protected override void LoadViewState (object savedState)
		{
			object first = null;
			object second = null;

			Pair pair = savedState as Pair;
			if (pair != null) {
				first = pair.First;
				second = pair.Second;
			}

			base.LoadViewState (first);

			if (second != null) {
				IStateManager manager = Items as IStateManager;
				manager.LoadViewState (second);
			}
		}

		protected override void OnDataBinding (EventArgs e)
		{
			base.OnDataBinding (e);

			/* Make sure Items has been initialised */
			ListItemCollection listitems = Items;

			listitems.Clear ();
			
			IEnumerable list = GetData ();
			if (list == null)
				return;
			
			foreach (object container in list) {
				string text = null;
				string value = null;

				if (DataTextField == String.Empty &&
				    DataValueField == String.Empty) {
					text = container.ToString ();
					value = text;
				} else {
					if (DataTextField != String.Empty) {
						text = DataBinder.Eval (container, DataTextField).ToString ();
					}

					if (DataValueField != String.Empty) {
						value = DataBinder.Eval (container, DataValueField).ToString ();
					} else {
						value = text;
					}

					if (text == null &&
					    value != null) {
						text = value;
					}
				}

				if (text == null) {
					text = String.Empty;
				}
				if (value == null) {
					value = String.Empty;
				}

				ListItem item = new ListItem (text, value);
				listitems.Add (item);
			}
			RequiresDataBinding = false;
			IsDataBound = true;
		}

		protected virtual void OnDataPropertyChanged ()
		{
			if (_initialized)
			RequiresDataBinding = true;
		}

		protected virtual void OnDataSourceViewChanged (object sender,
								EventArgs e)
		{
			RequiresDataBinding = true;
		}

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);

			Page.PreLoad += new EventHandler (OnPagePreLoad);
		}

		protected virtual void OnPagePreLoad (object sender, EventArgs e)
		{
			Initialize ();
		}

		protected internal override void OnLoad (EventArgs e)
		{
			if (!_initialized)
				Initialize ();

			base.OnLoad (e);
		}

		void Initialize ()
		{
			_initialized = true;

			if (!IsDataBound)
				RequiresDataBinding = true;

			if (IsBoundUsingDataSourceID)
				ConnectToDataSource ();
		}

		bool IsDataBound{
			get {
				return ViewState.GetBool ("_DataBound", false);
			}
			set {
				ViewState ["_DataBound"] = value;
			}
		}

		DataSourceView ConnectToDataSource ()
		{
			if (_boundDataSourceView != null)
				return _boundDataSourceView;

			/* verify that the data source exists and is an IDataSource */
			object ctrl = null;
			Page page = Page;
			if (page != null)
				ctrl = page.FindControl (DataSourceID);

			if (ctrl == null || !(ctrl is IDataSource)) {
				string format;

				if (ctrl == null)
				  	format = "DataSourceID of '{0}' must be the ID of a control of type IDataSource.  A control with ID '{1}' could not be found.";
				else
				  	format = "DataSourceID of '{0}' must be the ID of a control of type IDataSource.  '{1}' is not an IDataSource.";

				throw new HttpException (String.Format (format, ID, DataSourceID));
			}

			_boundDataSourceView = ((IDataSource)ctrl).GetView (String.Empty);
			_boundDataSourceView.DataSourceViewChanged += OnDataSourceViewChanged;
			return _boundDataSourceView;
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			EnsureDataBound ();
			base.OnPreRender (e);

			Page page = Page;
			if (page != null && !Disabled) {
				page.RegisterRequiresPostBack (this);
				page.RegisterEnabledControl (this);
			}
		}

		protected virtual void OnServerChange (EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[EventServerChange];

			if (handler != null) {
				handler (this, e);
			}
		}
		
		protected override void RenderAttributes (HtmlTextWriter w)
		{
			Page page = Page;
			if (page != null)
				page.ClientScript.RegisterForEventValidation (UniqueID);

			/* If there is no "name" attribute,
			 * LoadPostData doesn't work...
			 */
			w.WriteAttribute ("name", Name);
			Attributes.Remove ("name");
			
			/* Don't render the databinding attributes */
			Attributes.Remove ("datamember");
			Attributes.Remove ("datatextfield");
			Attributes.Remove ("datavaluefield");
			
			base.RenderAttributes (w);
		}
		
		protected internal override void RenderChildren (HtmlTextWriter w)
		{
			base.RenderChildren (w);

			if (items == null)
				return;
			
			w.WriteLine ();

			bool done_sel = false;
			
			int count = items.Count;
			for (int i = 0; i < count; i++) {
				ListItem item = items[i];
				w.Indent++;
				
				/* Write the <option> elements this
				 * way so that the output HTML matches
				 * the ms version (can't make
				 * HtmlTextWriterTag.Option an inline
				 * element, cos that breaks other
				 * stuff.)
				 */
				w.WriteBeginTag ("option");
				if (item.Selected && !done_sel) {

					w.WriteAttribute ("selected", "selected");

					if (!Multiple) {
						done_sel = true;
					}
				}
				
				w.WriteAttribute ("value", item.Value, true);
				if (item.HasAttributes) {
					AttributeCollection attrs = item.Attributes;
					foreach (string key in attrs.Keys)
						w.WriteAttribute (key, HttpUtility.HtmlAttributeEncode (attrs [key]));
				}
				w.Write (HtmlTextWriter.TagRightChar);
				
				w.Write (HttpUtility.HtmlEncode(item.Text));
				w.WriteEndTag ("option");
				w.WriteLine ();

				w.Indent--;
			}
		}

		protected override object SaveViewState ()
		{
			object first = null;
			object second = null;

			first = base.SaveViewState ();

			IStateManager manager = items as IStateManager;
			if (manager != null) {
				second = manager.SaveViewState ();
			}

			if (first == null && second == null)
				return (null);

			return new Pair (first, second);
		}

		/* "internal infrastructure" according to the docs,
		 * but has some documentation in 2.0
		 */
		protected virtual void Select (int[] selectedIndices)
		{
			if (items == null) {
				return;
			}

			ClearSelection ();
			
			int count = items.Count;
			foreach (int i in selectedIndices) {
				if (i >= 0 && i < count) {
					items[i].Selected = true;
				}
			}
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();

			IStateManager manager = items as IStateManager;
			if (manager != null) {
				manager.TrackViewState ();
			}
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}

		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			/* postCollection contains the values that are
			 * selected
			 */

			string[] values = postCollection.GetValues (postDataKey);
			bool changed = false;

			if (values != null) {
				if (Multiple) {
					/* We have a set of
					 * selections.  We can't just
					 * set the new list, because
					 * we need to know if the set
					 * has changed from last time
					 */
					int value_len = values.Length;
					int[] old_sel = SelectedIndices;
					int[] new_sel = new int[value_len];
					int old_sel_len = old_sel.Length;
					
					for (int i = 0; i < value_len; i++) {
						new_sel[i] = Items.IndexOf (values[i]);
						if (old_sel_len != value_len ||
						    old_sel[i] != new_sel[i]) {
							changed = true;
						}
					}

					if (changed) {
						Select (new_sel);
					}
				} else {
					/* Just take the first one */
					int sel = Items.IndexOf (values[0]);

					if (sel != SelectedIndex) {
						SelectedIndex = sel;
						changed = true;
					}
				}
			}

			if (changed)
				ValidateEvent (postDataKey, String.Empty);
			return (changed);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
	}
}
