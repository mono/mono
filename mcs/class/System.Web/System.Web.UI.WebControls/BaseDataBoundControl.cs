//
// System.Web.UI.WebControls.BaseDataBoundControl.cs
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
	public abstract class BaseDataBoundControl: WebControl
	{
		public event EventHandler DataBound;
		
		object dataSource;
		string dataSourceId;
		bool initialized;
		bool requiresDataBinding = true;
		
		public virtual object DataSource {
			get {
				return dataSource;
			}
			set {
				ValidateDataSource (value);
				dataSource = value;
			}
		}
		
		public virtual string DataSourceID {
			get { return dataSourceId; }
			set { dataSourceId = value; }
		}
		
		protected bool Initialized {
			get { return initialized; }
		}
		
		[MonoTODO]
		protected bool IsBoundUsingDataSourceID {
			get { return dataSourceId != null; }
		}
		
		protected bool RequiresDataBinding {
			get { return requiresDataBinding; }
			set { requiresDataBinding = value; }
		}
		
		[MonoTODO]
		protected void ConfirmInitState ()
		{
		}
		
		public override void DataBind ()
		{
			PerformSelect ();
			RequiresDataBinding = false;
			OnDataBound (EventArgs.Empty);
		}
		
		protected void EnsureDataBound ()
		{
			if (RequiresDataBinding && DataSourceID != "")
				DataBind ();
		}
		
		protected virtual void OnDataBound (EventArgs e)
		{
			if (DataBound != null)
				DataBound (this, e);
		}
		
		[MonoTODO]
		protected virtual void OnDataPropertyChanged ()
		{
		}
		
		[MonoTODO]
		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			initialized = true;
			if (!Page.IsPostBack)
				RequiresDataBinding = true;
		}
		
		[MonoTODO]
		protected virtual void OnPagePreLoad (object sender, EventArgs e)
		{
		}
		
		protected override void OnPreRender (EventArgs e)
		{
			EnsureDataBound ();
			base.OnPreRender (e);
		}
		
		protected abstract void PerformSelect ();
		
		protected abstract void ValidateDataSource (object dataSource);
		
	}
}

#endif

