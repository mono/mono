//
// System.Web.UI.WebControls.DataBoundControl
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.Web.Util;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public abstract class DataBoundControl : BaseDataBoundControl
	{
		public event EventHandler DataBound;
		
		protected DataBoundControl ()
		{
		}
		
		public sealed override void DataBind ()
		{
			PerformDataBinding ();
			RequiresDataBinding = false;
			OnDataBound (EventArgs.Empty);
		}
		
		protected void EnsureDataBound ()
		{
			if (RequiresDataBinding && DataSourceID != "")
				DataBind ();
		}

		protected virtual object GetDataSourceObject()
		{
			if (DataSourceID != "")
				return (IDataSource) NamingContainer.FindControl (DataSourceID);
			
			return DataSource;
		}
		
		
		protected virtual IEnumerable GetResolvedDataSource ()
		{
			if (DataSource != null && DataSourceID != "")
				throw new HttpException ();
			
			IDataSource ds = GetDataSourceObject () as IDataSource;
			if (ds != null && DataSourceID != "")
				return ds.GetView (DataMember).Select ();
			else if (DataSource != null)
				return DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
			else
				return null; 
		}
		
		protected bool IsBoundToDataSourceControl()
		{
			return (GetDataSourceObject () is IDataSource) && DataSourceID != "";
		}
		
		protected virtual void OnDataBound(EventArgs e) {
			if (DataBound != null)
				DataBound (this, e);
		}
		
		protected virtual void OnDataPropertyChanged ()
		{
			this.RequiresDataBinding = true;
		}
		
		protected virtual void OnDataSourceChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}
		
		// should be `internal protected' (why, oh WHY did they do that !?!)
		protected override void OnInit (EventArgs e)
		{
			base.OnInit(e);
			inited = true;
			if (!Page.IsPostBack)
				RequiresDataBinding = true;
		}
		
		// should be `internal protected' (why, oh WHY did they do that !?!)
		protected override void OnLoad (EventArgs e)
		{
			IDataSource ds = GetDataSourceObject () as IDataSource;
			if (ds != null && DataSourceID != "")
				ds.DataSourceChanged += new EventHandler (OnDataSourceChanged);
			
			base.OnLoad(e);
		}
		
		// should be `internal protected' (why, oh WHY did they do that !?!)
		protected override void OnPreRender (EventArgs e)
		{
			EnsureDataBound ();
			base.OnPreRender (e);
		}
		
		protected virtual void PerformDataBinding ()
		{
			OnDataBinding(EventArgs.Empty);
		}

		
		protected virtual void ValidateDataSource (object dataSource)
		{
			if (dataSource is IListSource || dataSource is IEnumerable)
				return;
			throw new ArgumentException ();
		}


		public string DataMember
		{
			get {
				object o = ViewState["DataMember"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set {
				ViewState["DataMember"] = value;
			}
		}

		object dataSource;
		public virtual object DataSource
		{
			get {
				return dataSource;
			}
			set {
				ValidateDataSource (value);
				dataSource = value;
			}
		}
		
		public virtual string DataSourceID {
			get {
				object o = ViewState ["DataSourceID"];
				if (o != null)
					return (string)o;
				
				return String.Empty;
			}
			set {
				if (inited)
					RequiresDataBinding = true;
				
				ViewState ["DataSourceID"] = value;
			}
		}
		
		bool requiresDataBinding;
		protected bool RequiresDataBinding {
			get { return requiresDataBinding; }
			set { requiresDataBinding = value; }
		}
		
		protected bool inited;
	}
}
#endif

