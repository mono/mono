//
// System.Web.UI.DataSourceControl
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

