//
// System.Web.UI.HierarchicalDataSourceControl
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI {
	public abstract class HierarchicalDataSourceControl : Control, IHierarchicalDataSource {
		protected HierarchicalDataSourceControl()
		{
		}
		
		protected virtual HierarchicalDataSourceView GetHierarchicalView (string viewPath)
		{
			return null;
		}
		
		HierarchicalDataSourceView IHierarchicalDataSource.GetHierarchicalView (string viewPath)
		{
			return this.GetHierarchicalView (viewPath);
		}
		
		//public override bool EnablePersonalization { get; set; }
		//public override bool EnableTheming { get; set; }
		//public override string SkinID { get; set; }
		public override bool Visible { 
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		static object dataSourceChanged = new object ();
		event EventHandler System.Web.UI.IHierarchicalDataSource.DataSourceChanged {
			add { Events.AddHandler (dataSourceChanged, value); }
			remove { Events.RemoveHandler (dataSourceChanged, value); }
		}
		
		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			EventHandler eh = Events [dataSourceChanged] as EventHandler;
			if (eh != null)
				eh (this, e);
		}
	}
	

}
#endif

