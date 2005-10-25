//
// System.Web.UI.WebControls.Repeater.cs
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

// Helpful resources while implementing this class:
//
// _Developing Microsoft ASP.NET Server Controls and Components_ (Kothari, Datye)
//    Chapters 16 and 20 (especially listing 20-3 on page 559)
//
// "Building DataBound Templated Custom ASP.NET Server Controls" (Mitchell) on MSDN
//    Right now, http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnaspp/html/databoundtemplatedcontrols.asp
//    works, but with msdn we all know that urls have a very short lifetime :-)
//

using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("ItemCommand")]
	[DefaultProperty ("DataSource")]
	[Designer ("System.Web.UI.Design.WebControls.RepeaterDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren (true)]
	[PersistChildren (false)]
	public class Repeater : Control, INamingContainer {

		object dataSource;
#if NET_2_0
		IDataSource boundDataSource;
		private bool initialized;
		private bool requiresDataBinding;
		private DataSourceSelectArguments selectArguments;
		private IEnumerable data;
#endif

		// See Kothari, listing 20-3
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void CreateChildControls ()
		{
			// We are recreating the children from viewstate
			Controls.Clear();

			// Build the children from the viewstate
			if (ViewState ["Items"] != null)
				CreateControlHierarchy (false);
		}
		
		// See Kothari, listing 20-3
		protected override void OnDataBinding (EventArgs e)
		{
			base.OnDataBinding (EventArgs.Empty);

			Controls.Clear ();
			ClearChildViewState ();
			TrackViewState ();

			CreateControlHierarchy (true);

			ChildControlsCreated = true;
		}

		void DoItem (int i, ListItemType t, object d, bool databind)
		{
			RepeaterItem itm = CreateItem (i, t);

			if (t == ListItemType.Item || t == ListItemType.AlternatingItem)
				items.Add (itm);
			
			itm.DataItem = d;
			RepeaterItemEventArgs e = new RepeaterItemEventArgs (itm);
			InitializeItem (itm);
			
			//
			// It is very important that this be called *before* data
			// binding. Otherwise, we won't save our state in the viewstate.
			//
			Controls.Add (itm);
			OnItemCreated (e);

			if (databind) {
				itm.DataBind ();
				OnItemDataBound (e);
			}
		}
		
		protected virtual void CreateControlHierarchy (bool useDataSource)
		{
			IEnumerable ds;
			items = new ArrayList ();
			itemscol = null;
			
			if (useDataSource) {
#if NET_2_0
				if (IsBoundUsingDataSourceID) {
					ds = GetData ();
				}
				else
#endif
					ds = DataSourceResolver.ResolveDataSource (DataSource, DataMember);
			}
			else {
				// Optimize (shouldn't need all this memory ;-)
				ds = new object [(int) ViewState ["Items"]];
			}

			// If there is no datasource, then we don't show anything. the "Items"
			// viewstate won't get set, so on postback, we won't get here
			if (ds == null)
				return;

			if (HeaderTemplate != null)
				DoItem (-1, ListItemType.Header, null, useDataSource);

			int idx = 0;
			foreach (object o in ds) {
				if (idx != 0 && SeparatorTemplate != null)
					DoItem (idx - 1, ListItemType.Separator, null, useDataSource);

				DoItem (idx, idx % 2 == 0 ? ListItemType.Item : ListItemType.AlternatingItem, o, useDataSource);
				idx ++;
			}
			
			if (FooterTemplate != null)
				DoItem (-1, ListItemType.Footer, null, useDataSource);

			ViewState ["Items"] = idx;
		}
		
		// Why does this get overriden?
		public override void DataBind ()
		{
			// In all the examples I've seen online, this does base.OnDataBinding and
			// then does all the create child controls stuff. But from stack traces on
			// windows, this doesn't seem to be the case here.
			OnDataBinding (EventArgs.Empty);

#if NET_2_0
			RequiresDataBinding = false;
#endif
		}
		
		protected virtual RepeaterItem CreateItem (int itemIndex, ListItemType itemType)
		{
			return new RepeaterItem (itemIndex, itemType);
		}
		
		protected virtual void InitializeItem (RepeaterItem item)
		{
			ITemplate t = null;
			
			switch (item.ItemType) {
			case ListItemType.Header:
				t = HeaderTemplate;
				break;
			case ListItemType.Footer:
				t = FooterTemplate;
				break;	
			case ListItemType.Item:
				t = ItemTemplate;
				break;
			case ListItemType.AlternatingItem:
				t = AlternatingItemTemplate;
				if (t == null)
					t = ItemTemplate;
				break;
			case ListItemType.Separator:
				t = SeparatorTemplate;
				break;
			}

			if (t != null)
				t.InstantiateIn (item);			
		}
		

		protected override bool OnBubbleEvent (object sender, EventArgs e)
		{
			RepeaterCommandEventArgs rcea = e as RepeaterCommandEventArgs;
			if (rcea != null) {
				OnItemCommand (rcea);
				return true;
			}

			return false;
		}

	
		public override ControlCollection Controls {
			get {
				EnsureChildControls ();
				return base.Controls;
			}
			
		}

		RepeaterItemCollection itemscol;
		ArrayList items;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		public virtual RepeaterItemCollection Items {
			get {
				if (itemscol == null)
					itemscol = new RepeaterItemCollection (items);
				return itemscol;
			}
		}
		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataMember {
			get {
				return ViewState.GetString ("DataMember", "");
			}
			set {
				if (value == null)
					ViewState.Remove ("DataMember");
				else
					ViewState ["DataMember"] = value;

#if NET_2_0
				if (!Initialized)
					OnDataPropertyChanged ();
#endif
			}
		}

		[Bindable(true)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual object DataSource {
			get {
				return dataSource;
			}
			
			set {
				if (value == null || value is IListSource || value is IEnumerable) {
#if NET_2_0
// FIXME - can't duplicate in a test case ? LAMESPEC ?
// can't duplicate in a test case
//					if ((dataSourceId != null) && (dataSourceId.Length != 0))
//						throw new HttpException (Locale.GetText ("DataSourceID is already set."));

					dataSource = value;

					if (!Initialized)
						OnDataPropertyChanged ();
#else
					dataSource = value;
#endif
				} else
					throw new ArgumentException (String.Format (
					    "An invalid data source is being used for {0}. A valid data source must implement either IListSource or IEnumerable",
					    ID));
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[IDReferenceProperty (typeof (DataSourceControl))]
		public virtual string DataSourceID
		{
			get {
				return ViewState.GetString ("DataSourceID", "");
			}
			set {
				if (dataSource != null)
			  		throw new HttpException ("Only one of DataSource and DataSourceID can be specified.");
				ViewState ["DataSourceID"] = value;

				if (!Initialized)
					OnDataPropertyChanged ();
			}
		}

		[Browsable (true)]
		[MonoTODO]
		public virtual new bool EnableTheming
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif		

		ITemplate alt_itm_tmpl;
		[Browsable(false)]
		[DefaultValue(null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("")]
		public virtual ITemplate AlternatingItemTemplate {
			get {
				return alt_itm_tmpl;
			}
			set {
				alt_itm_tmpl = value;
			}
		}		
		
		ITemplate footer_tmpl;
		[Browsable(false)]
		[DefaultValue(null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("")]
		public virtual ITemplate FooterTemplate {
			get {
				return footer_tmpl;
			}
			set {
				footer_tmpl = value;
			}
		}

		ITemplate header_tmpl;
		[Browsable(false)]
		[DefaultValue(null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("")]
		public virtual ITemplate HeaderTemplate {
			get {
				return header_tmpl;
			}
			set {
				header_tmpl = value;
			}
		}

		ITemplate item_tmpl;
		[Browsable(false)]
		[DefaultValue(null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("")]
		public virtual ITemplate ItemTemplate {
			get {
				return item_tmpl;
			}
			set {
				item_tmpl = value;
			}
		}

		ITemplate separator_tmpl;
		[Browsable(false)]
		[DefaultValue(null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("")]
		public virtual ITemplate SeparatorTemplate {
			get {
				return separator_tmpl;
			}
			set {
				separator_tmpl = value;
			}
		}

		
		protected virtual void OnItemCommand (RepeaterCommandEventArgs e)
		{
			RepeaterCommandEventHandler h = (RepeaterCommandEventHandler) Events [ItemCommandEvent];
			if (h != null)
				h (this, e);
		}

		static readonly object ItemCommandEvent = new object ();

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event RepeaterCommandEventHandler ItemCommand {
			add { Events.AddHandler (ItemCommandEvent, value); }
			remove { Events.RemoveHandler (ItemCommandEvent, value); }
		}

		
		protected virtual void OnItemCreated (RepeaterItemEventArgs e)
		{
			RepeaterItemEventHandler h = (RepeaterItemEventHandler) Events [ItemCreatedEvent];
			if (h != null)
				h (this, e);
		}

		static readonly object ItemCreatedEvent = new object ();

		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public event RepeaterItemEventHandler ItemCreated {
			add { Events.AddHandler (ItemCreatedEvent, value); }
			remove { Events.RemoveHandler (ItemCreatedEvent, value); }
		}
		
		protected virtual void OnItemDataBound (RepeaterItemEventArgs e) 
		{
			RepeaterItemEventHandler h = (RepeaterItemEventHandler) Events [ItemDataBoundEvent];
			if (h != null)
				h (this, e);
		}
		
		static readonly object ItemDataBoundEvent = new object ();

		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public event RepeaterItemEventHandler ItemDataBound {
			add { Events.AddHandler (ItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (ItemDataBoundEvent, value); }
		}

#if NET_2_0
		protected bool Initialized {
			get { return initialized; }
		}

		protected bool IsBoundUsingDataSourceID
		{
			get { return (DataSourceID.Length != 0); }
		}

		protected bool RequiresDataBinding
		{
			get { return requiresDataBinding; }
			set { requiresDataBinding = value; }
		}

		protected DataSourceSelectArguments SelectArguments
		{
			get {
				/* i know this seems weird - i mean, why
				 * don't we call
				 * CreateDataSourceSelectArguments here?  i
				 * have no idea. ask MS */
				if (selectArguments == null)
					selectArguments = new DataSourceSelectArguments ();
				return selectArguments;
			}
		}

		protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			if (selectArguments == null)
				selectArguments = new DataSourceSelectArguments ();
			return selectArguments;
		}

		protected void EnsureDataBound ()
		{
			if (IsBoundUsingDataSourceID && RequiresDataBinding)
				DataBind ();
		}

		private void SelectCallback (IEnumerable data)
		{
			this.data = data;
		}

		protected virtual IEnumerable GetData ()
		{
			IEnumerable result;
			if (DataSourceID.Length == 0)
				return null;

			DataSourceView dsv = boundDataSource.GetView (String.Empty);
			dsv.Select (SelectArguments, new DataSourceViewSelectCallback (SelectCallback));

			result = data;
			data = null;
			return result;
		}

		[MonoTODO]
		protected virtual void OnDataPropertyChanged ()
		{
		}

		[MonoTODO]
		protected virtual void OnDataSourceViewChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}

		[MonoTODO]
		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		protected internal override void OnLoad (EventArgs e)
		{
			if ((Page != null) && !Page.IsPostBack)
				RequiresDataBinding = true;

			initialized = true;
			base.OnLoad (e);

			if (IsBoundUsingDataSourceID)
				ConnectToDataSource ();
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			EnsureDataBound ();
			base.OnPreRender (e);
		}

		void ConnectToDataSource ()
		{
			/* verify that the data source exists and is an IDataSource */
			object ctrl = null;
			if (Page != null)
				ctrl = Page.FindControl (DataSourceID);

			if (ctrl == null || !(ctrl is IDataSource)) {
				string format;

				if (ctrl == null)
				  	format = "DataSourceID of '{0}' must be the ID of a control of type IDataSource.  A control with ID '{1}' could not be found.";
				else
				  	format = "DataSourceID of '{0}' must be the ID of a control of type IDataSource.  '{1}' is not an IDataSource.";

				throw new HttpException (String.Format (format, ID, DataSourceID));
			}

			boundDataSource = (IDataSource)ctrl;
		}
#endif
	}
}
