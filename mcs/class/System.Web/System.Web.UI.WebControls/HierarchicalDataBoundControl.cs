//
// System.Web.UI.WebControls.HierarchicalDataBoundControl.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.Adapters;

namespace System.Web.UI.WebControls
{
	[DesignerAttribute ("System.Web.UI.Design.WebControls.HierarchicalDataBoundControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class HierarchicalDataBoundControl : BaseDataBoundControl
	{
		[IDReferencePropertyAttribute (typeof(HierarchicalDataSourceControl))]
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
		
		protected virtual HierarchicalDataSourceView GetData (string viewPath)
		{
			if (DataSource != null && !String.IsNullOrEmpty (DataSourceID))
				throw new HttpException ();	
			IHierarchicalDataSource ds = GetDataSource ();
			if (ds != null)
				return ds.GetHierarchicalView (viewPath);
			
			if (DataSource is IHierarchicalEnumerable)
				return new ReadOnlyDataSourceView ((IHierarchicalEnumerable) DataSource);
			
			return null;
		}
		
		protected virtual IHierarchicalDataSource GetDataSource ()
		{
			if (IsBoundUsingDataSourceID) {
				Control ctrl = FindDataSource ();

				if (ctrl == null)
					throw new HttpException (string.Format ("A control with ID '{0}' could not be found.", DataSourceID));
				if (!(ctrl is IHierarchicalDataSource))
					throw new HttpException (string.Format ("The control with ID '{0}' is not a control of type IHierarchicalDataSource.", DataSourceID));
				return (IHierarchicalDataSource) ctrl;
			}
			
			return DataSource as IHierarchicalDataSource;
		}

		bool IsDataBound {
			get { return ViewState.GetBool ("DataBound", false); }
			set { ViewState ["DataBound"] = value; }
		}

		protected void MarkAsDataBound ()
		{
			IsDataBound = true;
		}
		
		protected override void OnDataPropertyChanged ()
		{
			RequiresDataBinding = true;
		}
		
		protected virtual void OnDataSourceChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}

		protected internal override void OnLoad (EventArgs e)
		{
			if (!Initialized) {
				Initialize ();
				ConfirmInitState ();
			}
			
			base.OnLoad(e);
		}

		void Initialize ()
		{
			if (!Page.IsPostBack || (IsViewStateEnabled && !IsDataBound))
				RequiresDataBinding = true;

			IHierarchicalDataSource ds = GetDataSource ();
			if (ds != null && DataSourceID != "")
				ds.DataSourceChanged += new EventHandler (OnDataSourceChanged);
		}

		protected override void OnPagePreLoad (object sender, EventArgs e)
		{
			base.OnPagePreLoad (sender, e);
			
			Initialize ();
		}
		
		protected void InternalPerformDataBinding ()
		{
			HierarchicalDataBoundControlAdapter adapter 
				= Adapter as HierarchicalDataBoundControlAdapter;
			if (adapter != null)
				adapter.PerformDataBinding ();
			else
				PerformDataBinding ();
		}
		
		protected internal virtual void PerformDataBinding ()
		{
		}
		
		protected override void PerformSelect ()
		{
			OnDataBinding (EventArgs.Empty);
			InternalPerformDataBinding ();
			// The PerformDataBinding method has completed.
			RequiresDataBinding = false;
			MarkAsDataBound ();
			OnDataBound (EventArgs.Empty);
		}
		
		protected override void ValidateDataSource (object dataSource)
		{
			if (dataSource == null || dataSource is IHierarchicalDataSource || dataSource is IHierarchicalEnumerable)
				return;
			throw new InvalidOperationException ("Invalid data source");
		}
	}
}



