//
// System.Runtime.Remoting.Contexts.IContextAttribute..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting.Activation;

namespace System.Runtime.Remoting.Contexts {

	public interface IContextAttribute {

		void GetPropertiesForNewContext (IConstructionCallMessage msg);

		bool IsContextOK (Context ctx, IConstructionCallMessage msg);
	}
}
