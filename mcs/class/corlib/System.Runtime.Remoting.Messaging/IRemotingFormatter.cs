//
// System.Runtime.Remoting.Messaging.IRemotingFormatter..cs
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	public interface IRemotingFormatter : IFormatter {
		object Deserialize (Stream serializationStream, HeaderHandler handler);
		void Serialize (Stream serializationStream, object graph, Header [] handlers); 
	}
}
