//
// System.Web.UI.DataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2004-2010 Novell, Inc. (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.ComponentModel;

namespace System.Web.UI {
	public abstract class DataSourceView
	{
		//IDataSource dataSourceOwner;
		string viewName = String.Empty;

		protected DataSourceView (IDataSource owner, string viewName)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			//this.dataSourceOwner = owner;
			this.viewName = viewName;
			owner.DataSourceChanged += new EventHandler (OnDataSourceChanged);
		}

		void OnDataSourceChanged (object sender, EventArgs e) {
			OnDataSourceViewChanged (EventArgs.Empty);
		}

		public virtual void Delete (IDictionary keys, IDictionary oldValues,
						DataSourceViewOperationCallback callback)
		{
			if (callback == null)
				throw new ArgumentNullException ("callBack");

			int rowAffected;
			try {
				rowAffected = ExecuteDelete (keys, oldValues);
			}
			catch (Exception e) {
				if (!callback (0, e))
					throw;
				return;
			}
			callback (rowAffected, null);
		}

		protected virtual int ExecuteDelete(IDictionary keys, IDictionary oldValues)
		{
			throw new NotSupportedException ();
		}

		protected virtual int ExecuteInsert (IDictionary values)
		{
			throw new NotSupportedException();
		}

		protected internal abstract IEnumerable ExecuteSelect (
					DataSourceSelectArguments arguments);

		protected virtual int ExecuteUpdate (IDictionary keys, IDictionary values, 
								IDictionary oldValues )
		{
			throw new NotSupportedException ();
		}

		public virtual void Insert (IDictionary values, 
					DataSourceViewOperationCallback callback)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			int rowAffected;
			try {
				rowAffected = ExecuteInsert (values);
			} catch (Exception e) {
				if (!callback (0, e))
					throw;
				return;
			}

			callback (rowAffected, null);
		}

		protected virtual void OnDataSourceViewChanged (EventArgs e)
		{
			if (eventsList != null) {
				EventHandler evtHandler = eventsList [EventDataSourceViewChanged] as EventHandler;
				if (evtHandler != null)
					evtHandler(this, e);
			}
		}
		
		protected internal virtual void RaiseUnsupportedCapabilityError (
						DataSourceCapabilities capability)
		{
			if ((capability & DataSourceCapabilities.Sort) != 0)
				if (!CanSort)
					throw new NotSupportedException ("Sort Capabilites");

			if ((capability & DataSourceCapabilities.Page) != 0)
				if (!CanPage)
					throw new NotSupportedException("Page Capabilites");

			if ((capability & DataSourceCapabilities.RetrieveTotalRowCount) != 0)
				if (!CanRetrieveTotalRowCount)
					throw new NotSupportedException("RetrieveTotalRowCount Capabilites");

			return;
		}

		public virtual void Select (DataSourceSelectArguments arguments,
						DataSourceViewSelectCallback callback)
		{
			if (callback == null)
				throw new ArgumentNullException("callBack");

			arguments.RaiseUnsupportedCapabilitiesError (this);
			
			IEnumerable selectList = ExecuteSelect (arguments);
			callback (selectList);
		}

		public virtual void Update(IDictionary keys, IDictionary values,
			IDictionary oldValues, DataSourceViewOperationCallback callback)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			int rowAffected;
			try {
				rowAffected = ExecuteUpdate (keys, values, oldValues);
			} catch (Exception e) {
				if (!callback (0, e))
					throw;
				return;
			}

			callback (rowAffected, null);
		} 
		
		public virtual bool CanDelete { get { return false; } }
		public virtual bool CanInsert { get { return false; } }
		public virtual bool CanPage { get { return false; } }
		public virtual bool CanRetrieveTotalRowCount { get { return false; } }
		public virtual bool CanSort { get { return false; } }
		public virtual bool CanUpdate { get { return false; } }

		EventHandlerList eventsList;
		protected EventHandlerList Events {
			get {
				if (eventsList == null)
					eventsList = new EventHandlerList();

				return eventsList;
			}
		}
		
		internal bool HasEvents ()
		{
			return eventsList != null;
		}

		public string Name { 
			get { return viewName; } 
		}

		static readonly object EventDataSourceViewChanged = new object ();
				
		public event EventHandler DataSourceViewChanged
		{
			add { Events.AddHandler (EventDataSourceViewChanged, value); }
			remove { Events.RemoveHandler (EventDataSourceViewChanged, value); }
		}
		
	}
	
}

