using System;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.Design {
	[MonoTODO] public class ControlDesigner : HtmlControlDesigner
	{
		[MonoTODO] public ControlDesigner () { throw new NotImplementedException (); }
		[MonoTODO] protected string CreatePlaceHolderDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO] protected string CreatePlaceHolderDesignTimeHtml (string instruction) { throw new NotImplementedException (); }
		[MonoTODO] public virtual string GetDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO] protected virtual string GetEmptyDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO] protected virtual string GetErrorDesignTimeHtml (Exception e) { throw new NotImplementedException (); }
		[MonoTODO] public virtual string GetPersistInnerHtml () { throw new NotImplementedException (); }
		[MonoTODO] public override void Initialize (IComponent component) { throw new NotImplementedException (); }
		[MonoTODO] public bool IsPropertyBound (string propName) { throw new NotImplementedException (); }
		[MonoTODO] protected override void OnBehaviorAttached () { throw new NotImplementedException (); }
		[MonoTODO] protected override void OnBindingsCollectionChanged (string propName) { throw new NotImplementedException (); }
		[MonoTODO] public virtual void OnComponentChanged (object sender, ComponentChangedEventArgs ce) { throw new NotImplementedException (); }
		[MonoTODO] protected virtual void OnControlResize () { throw new NotImplementedException (); }
		// LAMESPEC: we should have protected
		[MonoTODO] public override void PreFilterProperties (IDictionary properties) { throw new NotImplementedException (); }
		[MonoTODO] public void RaiseResizeEvent () { throw new NotImplementedException (); }
		[MonoTODO] public virtual void UpdateDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO] public virtual bool AllowResize { get { throw new NotImplementedException (); } }
		[MonoTODO] protected object DesignTimeElementView { get { throw new NotImplementedException (); } }
		[MonoTODO] public virtual bool DesignTimeHtmlRequiresLoadComplete { get { throw new NotImplementedException (); } }
		[MonoTODO] public virtual string ID { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO] public bool IsDirty { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO] public bool ReadOnly { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
	}
}