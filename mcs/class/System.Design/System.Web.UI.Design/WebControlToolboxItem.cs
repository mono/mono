using System;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.Design {
	// HACK HACK: lets just let things compile
	[MonoTODO] public class WebControlToolboxItem /* : ToolboxItem */{
		[MonoTODO] public WebControlToolboxItem () { throw new NotImplementedException (); }
		[MonoTODO] public WebControlToolboxItem (Type type) { throw new NotImplementedException (); }
		//[MonoTODO] protected override IComponent [] CreateComponentsCore (IDesignerHost host) { throw new NotImplementedException (); }
		//[MonoTODO] protected override void Deserialize (SerializationInfo info, StreamingContext context) { throw new NotImplementedException (); }
		[MonoTODO] public object GetToolAttributeValue (IDesignerHost host, Type attributeType) { throw new NotImplementedException (); }
		[MonoTODO] public string GetToolHtml (IDesignerHost host) { throw new NotImplementedException (); }
		[MonoTODO] public Type GetToolType (IDesignerHost host) { throw new NotImplementedException (); }
		//[MonoTODO] public override void Initialize (Type type) { throw new NotImplementedException (); }
		//[MonoTODO] protected override void Serialize (SerializationInfo info, StreamingContext context) { throw new NotImplementedException (); }
	}

}