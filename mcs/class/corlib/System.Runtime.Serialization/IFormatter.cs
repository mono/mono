//
// System.Runtime.Serialization.IFormatter
//
// Author:
//   David Dawkins (david@dawkins.st)
//
// (C) David Dawkins
//

using System.IO;

namespace System.Runtime.Serialization {

	/// <summary>
	/// Formatting for serialized objects</summary>
	public interface IFormatter {

		//
		// Properties
		//

		/// <summary>
		/// Get or set the SerializationBinder used
		/// for looking up types during deserialization</summary>
		SerializationBinder Binder 
		{
			get; 
			set; 
		}

		/// <summary>
		/// Get or set the StreamingContext used for serialization
		/// and deserialization</summary>
		StreamingContext Context 
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// Get or set the SurrogateSelector used by the current
		/// formatter</summary>
		ISurrogateSelector SurrogateSelector 
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// Deserialize data from the specified stream, rebuilding
		/// the object hierarchy</summary>
		object Deserialize(
			Stream serializationStream
		);		

		/// <summary>
		/// Serialize the specified object to the specified stream.
		/// Object may be the root of a graph of objects to be
		/// serialized</summary>
		object Serialize( 
			Stream serializationStream,
			object graph
		);
	}

}
