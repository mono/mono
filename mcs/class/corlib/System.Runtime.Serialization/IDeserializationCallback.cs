//
// System.Runtime.Serialization.IDeserializationCallback.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

namespace System.Runtime.Serialization {
	public interface IDeserializationCallback {
		void OnDeserialization(object sender);
	}
}