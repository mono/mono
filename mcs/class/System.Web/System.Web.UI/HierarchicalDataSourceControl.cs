//
// System.Web.UI.HierarchicalDataSourceControl
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
//  (C) 2003 Ben Maurer
//  (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;

namespace System.Web.UI
{
	[NonVisualControlAttribute]
	[DesignerAttribute ("System.Web.UI.Design.HierarchicalDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ControlBuilderAttribute (typeof(DataSourceControlBuilder))]
	[BindableAttribute (false)]
	public abstract class HierarchicalDataSourceControl : Control, IHierarchicalDataSource
	{
		static object dataSourceChanged = new object ();

		protected HierarchicalDataSourceControl()
		{
		}
		
		protected abstract HierarchicalDataSourceView GetHierarchicalView (string viewPath);
		
		HierarchicalDataSourceView IHierarchicalDataSource.GetHierarchicalView (string viewPath)
		{
			return GetHierarchicalView (viewPath);
		}

		[Browsable (false)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException (); }
		}
		
		[Browsable (false)]
		[DefaultValue ("")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string SkinID {
			get { return string.Empty; }
			set { throw new NotSupportedException (); }
		}
		
		[Browsable (false)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Visible { 
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Control FindControl (string id)
		{
			if (id == ID) return this;
			else return null;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool HasControls ()
		{
			return false;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void Focus ()
		{
			throw new NotSupportedException ();
		}
		
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
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void RenderControl (HtmlTextWriter writer)
		{
			// nop
		}
	}
	

}
#endif

