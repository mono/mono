//
// System.Runtime.Remoting.Activation.IConstructionCallMessage.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Runtime.Remoting.Activation {

	public interface IConstructionCallMessage {
		Type ActivationType {
			get;
		}

		string ActivationTypeName {
			get;
		}

		IActivator Activator {
			get;
			set;
		}

		object [] CallSiteActivationAttributes {
			get;
		}

		IList ContextProperties {
			get;
		}
	}
}
