using System;
using System.ComponentModel.Design;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.Design {
	[MonoTODO]
	public class HtmlControlDesigner : ComponentDesigner {
		
		[MonoTODO]
		public HtmlControlDesigner () { throw new NotImplementedException (); }
		
		[MonoTODO]
		protected override void Dispose (bool disposing) { throw new NotImplementedException (); }
		
		[MonoTODO]
		protected virtual void OnBehaviorAttached () { throw new NotImplementedException (); }
		
		[MonoTODO]
		protected virtual void OnBehaviorDetaching () { throw new NotImplementedException (); }
		
		[MonoTODO]
		protected virtual void OnBindingsCollectionChanged (string propName) { throw new NotImplementedException (); }
		
		[MonoTODO]
		public virtual void OnSetParent () { throw new NotImplementedException (); }
			
		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public override void PreFilterEvents (IDictionary events) { throw new NotImplementedException (); }
			
		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public override void PreFilterProperties (IDictionary properties) { throw new NotImplementedException (); }
			
		[MonoTODO]
		public IHtmlControlDesignerBehavior Behavior { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		
		[MonoTODO]
		public DataBindingCollection DataBindings { get { throw new NotImplementedException (); } }
		
		[MonoTODO]
		protected object DesignTimeElement { get { throw new NotImplementedException (); } }
		
		[MonoTODO]
		public virtual bool ShouldCodeSerialize { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
	}
}