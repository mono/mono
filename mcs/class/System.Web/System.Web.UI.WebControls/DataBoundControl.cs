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
	public abstract class DataBoundControl : BaseDataBoundControl
	{
		protected DataBoundControl ()
		{
		}
		
		protected IDataSource GetDataSource ()
		{
			if (DataSourceID != "")
				return NamingContainer.FindControl (DataSourceID) as IDataSource;
			
			return DataSource as IDataSource;
		}
		
		protected DataSourceView GetData ()
		{
			if (DataSource != null && DataSourceID != "")
				throw new HttpException ();
			
			IDataSource ds = GetDataSource ();
			if (ds != null && DataSourceID != "")
				return ds.GetView (DataMember);
			else
				return null; 
		}
		
		protected override void OnDataPropertyChanged ()
		{
			RequiresDataBinding = true;
		}
		
		protected virtual void OnDataSourceViewChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}
		
		// should be `internal protected' (why, oh WHY did they do that !?!)
		protected override void OnLoad (EventArgs e)
		{
			IDataSource ds = GetDataSource ();
			if (ds != null && DataSourceID != "")
				ds.DataSourceChanged += new EventHandler (OnDataSourceViewChanged);
			
			base.OnLoad(e);
		}
		
		protected virtual void PerformDataBinding (IEnumerable data)
		{
			OnDataBinding (EventArgs.Empty);
		}

		
		protected override void ValidateDataSource (object dataSource)
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

		public override string DataSourceID {
			get {
				object o = ViewState ["DataSourceID"];
				if (o != null)
					return (string)o;
				
				return String.Empty;
			}
			set {
				if (Initialized)
					RequiresDataBinding = true;
				
				ViewState ["DataSourceID"] = value;
			}
		}
		
		protected override void PerformSelect ()
		{
			IEnumerable data = null;
			DataSourceView view = GetData ();
			if (view != null)
				data = view.ExecuteSelect (SelectArguments);
			else if (DataSource != null)
				data = DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);

			PerformDataBinding (data);
		}
		
		[MonoTODO]
		protected DataSourceSelectArguments SelectArguments {
			get { return null; }
		}
	}
}
#endif

