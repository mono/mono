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

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI {

	[DesignerAttribute ("System.Web.UI.Design.DataSourceDesigner, " + Consts.AssemblySystem_Design,
			    "System.ComponentModel.Design.IDesigner")]
	[ControlBuilderAttribute (typeof (DataSourceControlBuilder))]
	[NonVisualControlAttribute]
	[BindableAttribute (false)]
	public abstract class DataSourceControl : Control, IDataSource, System.ComponentModel.IListSource {


		protected DataSourceControl()
		{
		}
		
		[MonoTODO ("Not implemented")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void ApplyStyleSheetSkin (Page page)
		{
			throw new NotImplementedException ();
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		[MonoTODO ("why override?")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Control FindControl (string id)
		{
			return base.FindControl (id);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void Focus ()
		{
			throw new NotSupportedException ();
		}

		protected abstract DataSourceView GetView (string viewName);
		
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
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool HasControls ()
		{
			return base.HasControls ();
		}

		protected virtual void RaiseDataSourceChangedEvent (EventArgs e)
		{
			EventHandler eh = Events [dataSourceChanged] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void RenderControl (HtmlTextWriter writer)
		{
			base.RenderControl (writer);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string ClientID {
			get { return base.ClientID; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ControlCollection Controls {
			get { return base.Controls; }
		}

		[DefaultValue (false)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		[DefaultValue ("")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string SkinID {
			get { return base.SkinID; }
			set { base.SkinID = value; }
		}

		bool System.ComponentModel.IListSource.ContainsListCollection {
			get { return ListSourceHelper.ContainsListCollection (this); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (false)]
		public override bool Visible { 
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		static object dataSourceChanged = new object ();
		event EventHandler System.Web.UI.IDataSource.DataSourceChanged {
			add { Events.AddHandler (dataSourceChanged, value); }
			remove { Events.RemoveHandler (dataSourceChanged, value); }
		}

	}
}

