//
// System.Runtime.Serialization.ISurrogateSelector
//
// Author:
//   David Dawkins (david@dawkins.st)
//
// (C) David Dawkins
//

namespace System.Runtime.Serialization {

	/// <summary>
	/// Creation of serialization surrogate selectors</summary>
	public interface ISurrogateSelector {

		/// <summary>
		/// Insert specified selector into available surrogates</summary>
		void ChainSelector( ISurrogateSelector selector );

		/// <summary>
		/// Return next surrogate in the surrogate chain</summary>
		ISurrogateSelector GetNextSelector();

		/// <summary>
		/// Fetch the surrogate according the specified type, starting
		/// the search from the surrogate selector for the specified
		/// StreamingContext</summary>
		/// <param name="type">Type of the object to be serialized</param>
		/// <param name="context">Context for the serialization/deserialization</para,>
		/// <param name="selector">Upon return, contains a reference to the selector where the returned surrogate was found</param>
		/// <returns>The surrogate for the specified type and context</returns>
		ISerializationSurrogate GetSurrogate(
			Type type,
			StreamingContext context,
			out ISurrogateSelector selector
		);

	}

}
