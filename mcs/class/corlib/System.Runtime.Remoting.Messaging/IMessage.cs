//
// System.Runtime.Remoting.Messaging.IMessage.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Runtime.Remoting.Messaging {

	public interface IMessage {

		IDictionary Properties {
			get;
		}
	}
}
