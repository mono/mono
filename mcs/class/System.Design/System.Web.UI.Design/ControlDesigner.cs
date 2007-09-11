
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
using System;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.Design
{
	[MonoTODO]
	public class ControlDesigner : HtmlControlDesigner
	{
		public ControlDesigner () {  }
		
		[MonoTODO]
		protected string CreatePlaceHolderDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO]
		protected string CreatePlaceHolderDesignTimeHtml (string instruction) { throw new NotImplementedException (); }
		[MonoTODO]
		public virtual string GetDesignTimeHtml () { throw new NotImplementedException (); }
#if NET_2_0
		[MonoNotSupported ("")]
		public virtual string GetDesignTimeHtml (DesignerRegionCollection regions) { throw new NotImplementedException (); }
		
		[MonoNotSupported ("")]
		public virtual string GetEditableDesignerRegionContent (EditableDesignerRegion region) { throw new NotImplementedException (); }
		
		[MonoNotSupported ("")]
		public virtual void SetEditableDesignerRegionContent (EditableDesignerRegion region, string content) { throw new NotImplementedException (); }
#endif
		[MonoTODO]
		protected virtual string GetEmptyDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO]
		protected virtual string GetErrorDesignTimeHtml (Exception e) { throw new NotImplementedException (); }
		[MonoTODO]
		public virtual string GetPersistInnerHtml () { throw new NotImplementedException (); }
		[MonoTODO]
		public override void Initialize (IComponent component) { throw new NotImplementedException (); }
		[MonoTODO]
		public bool IsPropertyBound (string propName) { throw new NotImplementedException (); }
		[MonoTODO]
		protected override void OnBehaviorAttached () { throw new NotImplementedException (); }
		[MonoTODO]
		protected override void OnBindingsCollectionChanged (string propName) { throw new NotImplementedException (); }
		[MonoTODO]
		public virtual void OnComponentChanged (object sender, ComponentChangedEventArgs ce) { throw new NotImplementedException (); }
		[MonoTODO]
		protected virtual void OnControlResize () { throw new NotImplementedException (); }
		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties) { throw new NotImplementedException (); }
		[MonoTODO]
		public void RaiseResizeEvent () { throw new NotImplementedException (); }
		[MonoTODO]
		public virtual void UpdateDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO]
		public virtual bool AllowResize { get { throw new NotImplementedException (); } }
		[MonoTODO]
		protected object DesignTimeElementView { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public virtual bool DesignTimeHtmlRequiresLoadComplete { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public virtual string ID { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public bool IsDirty { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public bool ReadOnly { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }

#if NET_2_0
		[MonoNotSupported ("")]
		protected virtual bool UsePreviewControl {
			get {
				throw new NotImplementedException ();
			}
		}
#endif
	}
}
