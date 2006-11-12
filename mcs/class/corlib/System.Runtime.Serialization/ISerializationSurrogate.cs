//
// System.Runtime.Serialization.ISerializationSurrogate
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
	/// Interface for serialization surrogates</summary>
#if NET_2_0
        [System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif

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
