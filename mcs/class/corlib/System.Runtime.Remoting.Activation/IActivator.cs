//
// System.Runtime.Remoting.Contexts.IActivator..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.Remoting.Activation {

	public interface IActivator {
		ActivatorLevel Level {
			get;
		}

		IActivator NextActivator {
			get; set;
		}

		IConstructionReturnMessage Activate (IConstructionCallMessage msg);
	}
}

