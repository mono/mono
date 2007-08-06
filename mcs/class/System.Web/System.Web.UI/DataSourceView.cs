//
// System.Web.UI.DataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2004 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0
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

		public virtual void Delete (IDictionary keys, IDictionary values,
						DataSourceViewOperationCallback callBack)
		{
			if (callBack == null)
				throw new ArgumentNullException ("callBack");

			int rowAffected;
			try {
				rowAffected = ExecuteDelete (keys, values);
			}
			catch (Exception e) {
				if (!callBack (0, e))
					throw;
				return;
			}
			callBack (rowAffected, null);
		}

		protected virtual int ExecuteDelete(IDictionary keys, IDictionary values)
		{
			throw new NotSupportedException ();
		}

		protected virtual int ExecuteInsert (IDictionary keys)
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
					DataSourceViewOperationCallback callBack)
		{
			if (callBack == null)
				throw new ArgumentNullException("callBack");

			int rowAffected;
			try {
				rowAffected = ExecuteInsert (values);
			} catch (Exception e) {
				if (!callBack (0, e))
					throw;
				return;
			}

			callBack (rowAffected, null);
		}

		protected virtual void OnDataSourceViewChanged (EventArgs eventArgs)
		{
			if (eventsList != null) {
				EventHandler evtHandler = eventsList [EventDataSourceViewChanged] as EventHandler;
				if (evtHandler != null)
					evtHandler(this, eventArgs);
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

		public virtual void Select (DataSourceSelectArguments selectArgs,
						DataSourceViewSelectCallback callBack)
		{
			if (callBack == null)
				throw new ArgumentNullException("callBack");

			selectArgs.RaiseUnsupportedCapabilitiesError (this);
			
			IEnumerable selectList = ExecuteSelect (selectArgs);
			callBack (selectList);
		}

		public virtual void Update(IDictionary keys, IDictionary values,
			IDictionary oldValues, DataSourceViewOperationCallback callBack)
		{
			if (callBack == null)
				throw new ArgumentNullException ("callBack");

			int rowAffected;
			try {
				rowAffected = ExecuteUpdate (keys, values, oldValues);
			} catch (Exception e) {
				if (!callBack (0, e))
					throw;
				return;
			}

			callBack (rowAffected, null);
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
#endif

