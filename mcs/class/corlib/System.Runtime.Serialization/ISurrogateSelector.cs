//
// System.Runtime.Serialization.ISurrogateSelector
//
// Author:
//   David Dawkins (david@dawkins.st)
//
// (C) David Dawkins
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Runtime.Serialization {

	/// <summary>
	/// Creation of serialization surrogate selectors</summary>
#if NET_2_0
        [System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif

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
