//
// System.Runtime.Remoting.Messaging.LogicalCallContext.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[MonoTODO]
	public class LogicalCallContext : ISerializable, ICloneable {

		internal LogicalCallContext () {}

		public bool HasInfo {
			get { return false; }
		}

		public void FreeNamedDataSlot (string name) {
		}

		public object GetData (string name) {
			return null;
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context) {
		}

		public void SetData (string name, object data) {
		}

		public object Clone () {
			return null;
		}
	}
}
