//
// System.Runtime.Serialization.ISerializationSurrogate
//
// Author:
//   David Dawkins (david@dawkins.st)
//
// (C) David Dawkins
//

namespace System.Runtime.Serialization {

	/// <summary>
	/// Interface for serialization surrogates</summary>
	public interface ISerializationSurrogate {

		/// <summary>
		/// Get the SerializationInfo necessary to serialize
		/// the specified object </summary>
		/// <param name="obj">Object to be serialized</param>
		/// <param name="info">SerializationInfo to be populated</param>
		/// <param name="context">Destination for serialization</param>
		void GetObjectData(
			object obj,
			SerializationInfo info,
			StreamingContext context
		);	

		/// <summary>
		/// Populate an object using the specified SerializationInfo </summary>
		/// <param name="obj">Object to be populated</param>
		/// <param name="info">Data used for populating object</param>
		/// <param name="context">Source for deserialization of object</param>
		/// <param name="selector>Starting point for searching for compatible surrogates</param>
		/// <returns>The deserialized object</returns>
		object SetObjectData(
			object obj,
			SerializationInfo info,
			StreamingContext context,
			ISurrogateSelector selector
		);
	}

}
