//
// System.Web.UI.DataSourceControl
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
	public abstract class DataSourceControl : Control, IDataSource, System.ComponentModel.IListSource {


		protected DataSourceControl()
		{
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}
		
		protected virtual DataSourceView GetView (string viewName)
		{
			return null;
		}
		
		DataSourceView IDataSource.GetView (string viewName)
		{
			return GetView (viewName);
		}
		
		protected virtual ICollection GetViewNames ()
		{
			return null;
		}
		
		ICollection IDataSource.GetViewNames ()
		{
			return GetViewNames ();
		}

		IList System.ComponentModel.IListSource.GetList ()
		{
			return ListSourceHelper.GetList (this);
		}
		
		bool System.ComponentModel.IListSource.ContainsListCollection {
			get { return ListSourceHelper.ContainsListCollection (this); }
		}

		//public override bool EnablePersonalization { get; set; }
		//public override bool EnableTheming { get; set; }
		//public override string SkinID { get; set; }
		public override bool Visible { 
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		static object dataSourceChanged = new object ();
		event EventHandler System.Web.UI.IDataSource.DataSourceChanged {
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

