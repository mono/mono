//
// System.Runtime.Remoting.Contexts.IDynamicMessageSink..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Contexts {

	public interface IDynamicMessageSink {

		void ProcessMessageFinish (IMessage reply_msg, bool client_site, bool async);

		void ProcessMessageStart  (IMessage req_msg, bool client_site, bool async);
	}
}
