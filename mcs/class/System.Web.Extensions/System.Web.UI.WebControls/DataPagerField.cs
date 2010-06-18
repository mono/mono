//
// System.Web.UI.WebControls.DataPagerField
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class DataPagerField : IStateManager
	{
		static readonly object FieldChangedEvent = new object ();
		
		EventHandlerList events;
		StateBag _state = new StateBag ();
		DataPager _dataPager;

		bool _queryStringHandled;
		bool _isTrackingViewState;
		string _queryStringNavigateUrl;

		internal event EventHandler FieldChanged {
			add { AddEventHandler (FieldChangedEvent, value); }
			remove { RemoveEventHandler (FieldChangedEvent, value); }
		}
				
		protected DataPagerField ()
		{
		}

		protected internal DataPagerField CloneField ()
		{
			DataPagerField ret = CreateField ();
			CopyProperties (ret);

			return ret;
		}

		protected virtual void CopyProperties (DataPagerField newField)
		{
			// assuming we should copy only the public properties
			newField.Visible = Visible;
		}

		public abstract void CreateDataPagers (DataPagerFieldItem container, int startRowIndex, int maximumRows,
						       int totalRowCount, int fieldIndex);

		protected abstract DataPagerField CreateField ();

		protected string GetQueryStringNavigateUrl (int pageNumber)
		{
			if (_queryStringNavigateUrl == null && _dataPager != null) {
				HttpContext ctx = HttpContext.Current;
				HttpRequest req = ctx != null ? ctx.Request : null;
				string queryFieldName = _dataPager.QueryStringField;
				
				if (req != null) {
					StringBuilder sb = new StringBuilder (req.Path + "?");
					NameValueCollection coll = req.QueryString;
					
					foreach (string k in coll.AllKeys) {
						if (String.Compare (k, queryFieldName, StringComparison.OrdinalIgnoreCase) == 0)
							continue;
						sb.Append (HttpUtility.UrlEncode (k) + "=" + HttpUtility.UrlEncode (coll [k]) + "&");
					}

					sb.Append (queryFieldName + "=");
					_queryStringNavigateUrl = sb.ToString ();
				} else
					_queryStringNavigateUrl = String.Empty;
			}

			return _queryStringNavigateUrl + pageNumber.ToString (CultureInfo.InvariantCulture);
		}

		public abstract void HandleEvent (CommandEventArgs e);

		protected virtual void LoadViewState (Object savedState)
		{
			if (savedState == null)
				return;

			((IStateManager) ViewState).LoadViewState (savedState);
		}
		
		protected virtual void OnFieldChanged ()
		{
			InvokeEvent (FieldChangedEvent, EventArgs.Empty);
		}

		protected virtual object SaveViewState ()
		{
			return ((IStateManager) ViewState).SaveViewState ();
		}

		protected virtual void TrackViewState ()
		{
			_isTrackingViewState = true;
			((IStateManager)ViewState).TrackViewState ();
		}

		protected DataPager DataPager {
			get { return _dataPager; }
		}
		
		protected bool QueryStringHandled {
			get { return _queryStringHandled; }
			set { _queryStringHandled = value; }
		}

		protected string QueryStringValue {
			get {
				if (_dataPager == null)
					return String.Empty;
				
				HttpContext ctx = HttpContext.Current;
				HttpRequest req = ctx != null ? ctx.Request : null;

				if (req == null)
					return String.Empty;

				return req.QueryString [_dataPager.QueryStringField];
			}
		}

		protected StateBag ViewState {
			get { return _state; }
		}

		public bool Visible {
			get {
				object o = ViewState ["Visible"];
				if (o == null)
					return true;

				return (bool) o;
			}
			
			set {
				if (value != Visible) {
					ViewState ["Visible"] = value;
					OnFieldChanged ();
				}
			}
		}

		protected bool IsTrackingViewState {
			get { return _isTrackingViewState; }
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}

		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}

		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}

		void IStateManager.LoadViewState (object state)
		{
			LoadViewState (state);
		}

		internal void SetDataPager (DataPager pager)
		{
			_dataPager = pager;
		}

		internal bool GetQueryModeStartRowIndex (int totalRowCount, int maximumRows, ref int startRowIndex, ref bool setPagePropertiesNeeded)
		{
			bool queryMode = !String.IsNullOrEmpty (DataPager.QueryStringField);
			if (!queryMode || QueryStringHandled)
				return queryMode;

			QueryStringHandled = true;

			// We need to calculate the new start index since it is probably out
			// of date because the GET parameter with the page number hasn't
			// been processed yet
			int pageNumber;
			try {
				pageNumber = Int32.Parse (QueryStringValue);
			} catch {
				// ignore
				pageNumber = -1;
			}

			if (pageNumber >= 0) {
				pageNumber--; // we're zero-based since we're calculating
				// the offset/index
				if (pageNumber >= 0) {
					// zero-based calculation again
					int pageCount = (totalRowCount - 1) / maximumRows; 
					if (pageNumber <= pageCount) {
						startRowIndex = pageNumber * maximumRows;
						setPagePropertiesNeeded = true;
					}
				}
			}

			return true;
		}
		
		void AddEventHandler (object key, EventHandler handler)
		{
			if (events == null)
				events = new EventHandlerList ();
			events.AddHandler (key, handler);
		}

		void RemoveEventHandler (object key, EventHandler handler)
		{
			if (events == null)
				return;
			events.RemoveHandler (key, handler);
		}

		void InvokeEvent (object key, EventArgs args)
		{
			if (events == null)
				return;

			EventHandler eh = events [key] as EventHandler;
			if (eh == null)
				return;
			eh (this, args);
		}
	}
}
#endif
