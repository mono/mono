//
// System.Runtime.Remoting.Contexts.IConstructionReturnMessage..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Activation {

	public interface IConstructionReturnMessage :IMethodReturnMessage, IMethodMessage, IMessage {
	}
}

