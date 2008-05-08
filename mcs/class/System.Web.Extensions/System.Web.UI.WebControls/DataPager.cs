//
// System.Web.UI.WebControls.DataPager
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
#if NET_3_5
using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ThemeableAttribute(true)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DataPager : Control, IAttributeAccessor, INamingContainer, ICompositeControlDesignerAccessor
	{
		const int NO_PAGEABLE_ITEM_CONTAINER = 0;
		const int NO_DATABOUND_CONTROL = 1;
		const int NO_PAGED_CONTAINER_ID = 2;
		const int CONTROL_NOT_PAGEABLE = 3;
		const int NO_NAMING_CONTAINER = 4;

		const int STATE_BASE_STATE = 0;
		const int STATE_TOTAL_ROW_COUNT = 1;
		const int STATE_MAXIMUM_ROWS = 2;
		const int STATE_START_ROW_INDEX = 3;
		
		string[] _exceptionMessages = {
			"No IPageableItemContainer was found. Verify that either the DataPager is inside an IPageableItemContainer or PagedControlID is set to the control ID of an IPageableItemContainer",
			"There is no data-bound control associated with the DataPager control.",
			"Control with id '{0}' cannot be found in the page",
			"Control '{0}' is not pageable",
			"DataPager has no naming container"
		};
		
		IPageableItemContainer _pageableContainer;
		DataPagerFieldCollection _fields;
		AttributeCollection _attributes;
		
		int _totalRowCount;
		int _startRowIndex;
		int _maximumRows = 10;
		
		bool _initDone;
		bool _needNewContainerSetup = true;
		bool _createPagerFieldsRunning;
		
		public DataPager()
		{
			_fields = new DataPagerFieldCollection (this);
		}

		protected virtual void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (ID != null)
				writer.AddAttribute (HtmlTextWriterAttribute.Id, ID);

			if (_attributes != null && _attributes.Count > 0) {
				foreach (string attr in _attributes.Keys)
					writer.AddAttribute (attr, _attributes [attr]);
			}
		}

		protected virtual void ConnectToEvents (IPageableItemContainer container)
		{
			if (container == null)
				throw new ArgumentNullException ("container");

			container.TotalRowCountAvailable += new EventHandler <PageEventArgs> (OnTotalRowCountAvailable);
		}

		protected virtual void CreatePagerFields ()
		{
			// In theory (on multi-core or SMP machines), OnTotalRowCountAvailable may
			// be called asynchronously to this method (since it is a delegate reacting
			// to event in the container), so we want to protect ourselves from data
			// corruption here. Lock would be an overkill, since we really want to
			// create the list only once anyway.
			_createPagerFieldsRunning = true;
			
			ControlCollection controls = Controls;
			controls.Clear ();

			DataPagerFieldItem control;
			
			foreach (DataPagerField dpf in _fields) {
				control = new DataPagerFieldItem (dpf, this);
				if (dpf.Visible) {
					dpf.CreateDataPagers (control, _startRowIndex, _maximumRows, _totalRowCount, _fields.IndexOf (dpf));
					control.DataBind ();
				}
				controls.Add (control);
			}

			_createPagerFieldsRunning = false;
		}

		public override void DataBind()
		{
			OnDataBinding (EventArgs.Empty);
			EnsureChildControls ();
			DataBindChildren ();
		}

		protected virtual IPageableItemContainer FindPageableItemContainer ()
		{
			string pagedControlID = PagedControlID;
			IPageableItemContainer ret = null;
			
			if (!String.IsNullOrEmpty (pagedControlID)) {
				Control ctl = FindControl (pagedControlID, 0);
				if (ret == null)
					throw new InvalidOperationException (String.Format (_exceptionMessages [NO_PAGED_CONTAINER_ID], pagedControlID));

				ret = ctl as IPageableItemContainer;
				if (ret == null)
					throw new InvalidOperationException (String.Format (_exceptionMessages [CONTROL_NOT_PAGEABLE], pagedControlID));

				return ret;
			}

			// No ID set, try to find a container that's pageable
			Control container = NamingContainer;
			Page page = Page;

			while (container != page) {
				if (container == null)
					throw new InvalidOperationException (_exceptionMessages [NO_NAMING_CONTAINER]);

				ret = container as IPageableItemContainer;
				if (ret != null)
					return ret;

				container = container.NamingContainer;
			}

			return ret;
		}

		protected internal override void LoadControlState (object savedState)
		{
			object[] state = savedState as object[];
			object tmp;
			
			if (state != null) {
				base.LoadControlState (state [STATE_BASE_STATE]);

				if ((tmp = state [STATE_TOTAL_ROW_COUNT]) != null)
					_totalRowCount = (int) tmp;

				if ((tmp = state [STATE_MAXIMUM_ROWS]) != null)
					_maximumRows = (int) tmp;

				if ((tmp = state [STATE_START_ROW_INDEX]) != null)
					_startRowIndex = (int) tmp;
			}

			if (_pageableContainer == null) {
				_pageableContainer = FindPageableItemContainer ();
				if (_pageableContainer == null)
					throw new InvalidOperationException (_exceptionMessages [NO_DATABOUND_CONTROL]);
				ConnectToEvents (_pageableContainer);
			}
			
			SetUpForNewContainer (false);
		}

		protected override void LoadViewState (object savedState)
		{
			Pair state = savedState as Pair;

			if (state == null)
				return;

			base.LoadViewState (state.First);
			object myState = state.Second;
			if (myState != null)
				((IStateManager) Fields).LoadViewState (myState);
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			DataPagerFieldCommandEventArgs args = e as DataPagerFieldCommandEventArgs;

			if (args != null) {
				DataPagerFieldItem item = args.Item;
				DataPagerField field = item != null ? item.PagerField : null;
				
				if (field != null) {
					field.HandleEvent (args);
					return true;
				}
			}

			return false;
		}

		void SetUpForNewContainer (bool dataBind)
		{
			if (!_needNewContainerSetup)
				return;
			
			ConnectToEvents (_pageableContainer);
			_pageableContainer.SetPageProperties (_startRowIndex, _maximumRows, false);
			_needNewContainerSetup = false;
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);

			Page page = Page;
			if (page != null)
				page.RegisterRequiresControlState (this);
			
			// It might return null here - there is no guarantee all the controls on the
			// page are already initialized by the time this method is loaded. Do not
			// throw for that reason.
			_pageableContainer = FindPageableItemContainer ();
			if (_pageableContainer != null)
				// Do not re-bind the data here - not all the controls might be
				// initialized (that includes the container may be bound to)
				SetUpForNewContainer (false);

			_initDone = true;
		}

		protected internal override void OnLoad (EventArgs e)
		{
			if (_pageableContainer == null)
				_pageableContainer = FindPageableItemContainer ();
			
			if (_pageableContainer == null)
				throw new InvalidOperationException (_exceptionMessages [NO_PAGEABLE_ITEM_CONTAINER]);

			SetUpForNewContainer (false);
			base.OnLoad (e);
		}

		protected virtual void OnTotalRowCountAvailable (object sender, PageEventArgs e)
		{
			_totalRowCount = e.TotalRowCount;
			_maximumRows = e.MaximumRows;
			_startRowIndex = e.StartRowIndex;

			// Sanity checks: if the total row count is less than the current start row
			// index, we must adjust and rebind the associated container control
			if (_totalRowCount > 0 && (_totalRowCount <= _startRowIndex)) {
				// Adjust the container's start row index to the new maximum rows
				// count, but do not touch our index - we aren't a "view", so we
				// don't want/need to change the start index.
				int tmp = _startRowIndex - _maximumRows;
				if (tmp < 0 || tmp >= _totalRowCount)
					tmp = 0;

				// Trigger the databinding, which will call us again, with adjusted
				// data, so that we can recreate the pager fields.
				_pageableContainer.SetPageProperties (tmp, _maximumRows, true);
			} else if (!_createPagerFieldsRunning)
				// No adjustments necessary, re-create the pager fields
				CreatePagerFields ();
		}

		protected virtual void RecreateChildControls ()
		{
			// This is used only by VS designer
			throw new NotImplementedException ();
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			RenderBeginTag (writer);
			RenderContents (writer);
			writer.RenderEndTag ();
		}

		public virtual void RenderBeginTag (HtmlTextWriter writer)
		{
			AddAttributesToRender (writer);
			writer.RenderBeginTag (TagKey);
		}

		protected virtual void RenderContents (HtmlTextWriter writer)
		{
			// Nothing special to render, just child controls
			base.Render (writer);
		}

		protected internal override object SaveControlState ()
		{
			object[] ret = new object [4];

			ret [STATE_BASE_STATE] = base.SaveControlState ();
			ret [STATE_TOTAL_ROW_COUNT] = _totalRowCount <= 0 ? 0 : _totalRowCount;
			ret [STATE_MAXIMUM_ROWS] = _maximumRows <= 0 ? 0 : _maximumRows;
			ret [STATE_START_ROW_INDEX] = _startRowIndex <= 0 ? 0 : _startRowIndex;

			return ret;
		}

		protected override object SaveViewState ()
		{
			Pair ret = new Pair ();

			ret.First = base.SaveViewState ();
			ret.Second = _fields != null ? ((IStateManager) _fields).SaveViewState () : null;

			return ret;
		}

		public virtual void SetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
			if (_pageableContainer == null)
				throw new InvalidOperationException (_exceptionMessages [NO_DATABOUND_CONTROL]);

			_startRowIndex = startRowIndex;
			_maximumRows = maximumRows;

			_pageableContainer.SetPageProperties (startRowIndex, maximumRows, databind);
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			if (_fields != null)
				((IStateManager) _fields).TrackViewState ();
		}

		[BrowsableAttribute(false)]
		public AttributeCollection Attributes {
			get {
				if (_attributes == null)
					_attributes = new AttributeCollection (new StateBag ());
				
				return _attributes;
			}
		}

		public override ControlCollection Controls {
			get {
				EnsureChildControls ();
				return base.Controls;
			}
		}

		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		public virtual DataPagerFieldCollection Fields {
			get { return _fields; }
		}

		[BrowsableAttribute(false)]
		public int MaximumRows {
			get { return _maximumRows; }
		}

		[ThemeableAttribute(false)]
		public virtual string PagedControlID {
			get {
				string ret = ViewState ["PagedControlID"] as string;
				if (ret == null)
					return String.Empty;

				return ret;
			}
			
			set { ViewState ["PagedControlID"] = value; }
		}

		public int PageSize {
			get { return _maximumRows; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("value");

				if (value == _maximumRows)
					return;
				
				_maximumRows = value;
				if (_initDone) {
					// We have a source and the page size has changed, update
					// the container
					CreatePagerFields ();

					// Environment has changed, let the container know that it
					// needs to rebind.
					SetPageProperties (_startRowIndex, _maximumRows, true);
				}
			}
		}

		public string QueryStringField {
			get {
				string ret = ViewState ["QueryStringField"] as string;
				if (ret == null)
					return String.Empty;

				return ret;
			}
			
			set { ViewState ["QueryStringField"] = value; }
		}

		[BrowsableAttribute(false)]
		public int StartRowIndex {
			get { return _startRowIndex; }
		}

		[BrowsableAttribute(false)]
		protected virtual HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Span; }
		}

		[BrowsableAttribute(false)]
		public int TotalRowCount {
			get { return _totalRowCount; }
		}

		string IAttributeAccessor.GetAttribute (string key)
		{
			return Attributes [key];
		}

		void IAttributeAccessor.SetAttribute (string key, string value)
		{
			Attributes [key] = value;
		}

		void ICompositeControlDesignerAccessor.RecreateChildControls ()
		{
			RecreateChildControls ();
		}
	}
}
#endif
