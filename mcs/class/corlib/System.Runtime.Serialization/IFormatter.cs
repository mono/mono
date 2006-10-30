//
// System.Runtime.Serialization.IFormatter
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

using System.IO;

namespace System.Runtime.Serialization {

	/// <summary>
	/// Formatting for serialized objects</summary>
#if NET_2_0
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif
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
		void Serialize( 
			Stream serializationStream,
			object graph
		);
	}

}
