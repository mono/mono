//
// System.Web.UI.WebControls.HierarchicalDataBoundControl.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Web.UI.WebControls
{
	public abstract class HierarchicalDataBoundControl : BaseDataBoundControl
	{
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
		
		protected HierarchicalDataSourceView GetData (string viewPath)
		{
			if (DataSource != null && DataSourceID != "")
				throw new HttpException ();
			
			IHierarchicalDataSource ds = GetDataSource ();
			if (ds != null)
				return ds.GetHierarchicalView (viewPath);
			else
				return null; 
		}
		
		protected IHierarchicalDataSource GetDataSource ()
		{
			if (DataSourceID != "")
				return NamingContainer.FindControl (DataSourceID) as IHierarchicalDataSource;
			
			return DataSource as IHierarchicalDataSource;
		}
		
		protected override void OnDataPropertyChanged ()
		{
			RequiresDataBinding = true;
		}
		
		protected virtual void OnDataSourceChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}

		protected override void OnLoad (EventArgs e)
		{
			IHierarchicalDataSource ds = GetDataSource ();
			if (ds != null && DataSourceID != "")
				ds.DataSourceChanged += new EventHandler (OnDataSourceChanged);
			
			base.OnLoad(e);
		}

		[MonoTODO]
		protected override void OnPagePreLoad (object sender, EventArgs e)
		{
			base.OnPagePreLoad (sender, e);
		}
		
		protected internal virtual void PerformDataBinding ()
		{
			OnDataBinding (EventArgs.Empty);
		}
		
		protected override void PerformSelect ()
		{
			PerformDataBinding ();
		}
		
		protected override void ValidateDataSource (object dataSource)
		{
			if (dataSource is IHierarchicalDataSource || dataSource is IHierarchicalEnumerable)
				return;
			throw new InvalidOperationException ("Invalid data source");
		}
	}
}
#endif

