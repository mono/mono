//
// System.Web.UI.WebControls.DataBoundControl
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
using System.Web.Util;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

	[DesignerAttribute ("System.Web.UI.Design.WebControls.HierarchicalDataBoundControlDesigner, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
	public abstract class DataBoundControl : BaseDataBoundControl
	{
		DataSourceSelectArguments selectArguments;
		DataSourceView currentView;

		protected DataBoundControl ()
		{
		}
		
		protected IDataSource GetDataSource ()
		{
			if (IsBoundUsingDataSourceID) {
				Control ctrl = NamingContainer.FindControl (DataSourceID);
				if (ctrl == null)
					throw new HttpException (string.Format ("A control with ID '{0}' could not be found.", DataSourceID));
				if (!(ctrl is IDataSource))
					throw new HttpException (string.Format ("The control with ID '{0}' is not a control of type IDataSource.", DataSourceID));
				return (IDataSource) ctrl;
			}
			
			if (DataSource == null) return null;
			
			IDataSource ds = DataSource as IDataSource;
			if (ds != null) return ds;
			
			IEnumerable ie = DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
			if (ie != null) return new CollectionDataSource (ie);
			
			throw new HttpException (string.Format ("Unexpected data source type: {0}", DataSource.GetType()));
		}
		
		protected DataSourceView GetData ()
		{
			if (currentView == null)
				UpdateViewData ();
			return currentView;
		}
		
		DataSourceView InternalGetData ()
		{
			if (DataSource != null && IsBoundUsingDataSourceID)
				throw new HttpException ("Control bound using both DataSourceID and DataSource properties.");
			
			IDataSource ds = GetDataSource ();
			if (ds != null)
				return ds.GetView (DataMember);
			else
				return null; 
		}
		
		protected override void OnDataPropertyChanged ()
		{
			base.OnDataPropertyChanged ();
			UpdateViewData ();
		}
		
		protected virtual void OnDataSourceViewChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}
		
		protected override void OnPagePreLoad (object sender, EventArgs e)
		{
			base.OnPagePreLoad (sender, e);
			UpdateViewData ();
		}
		
		void UpdateViewData ()
		{
			DataSourceView view = InternalGetData ();
			if (view == currentView) return;

			if (currentView != null)
				currentView.DataSourceViewChanged -= new EventHandler (OnDataSourceViewChanged);

			currentView = view;

			if (view != null)
				view.DataSourceViewChanged += new EventHandler (OnDataSourceViewChanged);
		}
		
		// should be `internal protected' (why, oh WHY did they do that !?!)
		protected override void OnLoad (EventArgs e)
		{
			if (IsBoundUsingDataSourceID && (!Page.IsPostBack || !EnableViewState))
				RequiresDataBinding = true;

			base.OnLoad(e);
		}
		
		protected virtual void PerformDataBinding (IEnumerable data)
		{
			OnDataBinding (EventArgs.Empty);
		}

		protected override void ValidateDataSource (object dataSource)
		{
			if (dataSource is IListSource || dataSource is IEnumerable || dataSource is IDataSource)
				return;
			throw new ArgumentException ("Invalid data source source type. The data source must be of type IListSource, IEnumerable or IDataSource.");
		}

		[ThemeableAttribute (false)]
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
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

	    [IDReferencePropertyAttribute (typeof(HierarchicalDataSourceControl))]
		public override string DataSourceID {
			get {
				object o = ViewState ["DataSourceID"];
				if (o != null)
					return (string)o;
				
				return String.Empty;
			}
			set {
				ViewState ["DataSourceID"] = value;
				base.DataSourceID = value;
			}
		}
		
		protected override void PerformSelect ()
		{
			DataSourceView view = GetData ();
			if (view != null)
				view.Select (SelectArguments, new DataSourceViewSelectCallback (OnSelect));
		}
		
		void OnSelect (IEnumerable data)
		{
			PerformDataBinding (data);
		}
		
		protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			return DataSourceSelectArguments.Empty;
		}
		
		protected DataSourceSelectArguments SelectArguments {
			get {
				if (selectArguments == null)
					selectArguments = CreateDataSourceSelectArguments ();
				return selectArguments;
			}
		}
	}
}
#endif

