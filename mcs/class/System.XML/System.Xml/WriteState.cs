// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.WriteState
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

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

namespace System.Xml 
{


	/// <summary>
	/// </summary>
	public enum WriteState {

		/// <summary>
		/// A write method has not been called.
		/// </summary>
		Start = 0,

		/// <summary>
		/// The prolog is being written.
		/// </summary>
		Prolog = 1,

		/// <summary>
		/// An element start tag is being written.
		/// </summary>
		Element = 2,

		/// <summary>
		/// An attribute is being written.
		/// </summary>
		Attribute = 3,

		/// <summary>
		/// Element content is being written.
		/// </summary>
		Content = 4,

		/// <summary>
		/// The close method has been called.
		/// </summary>
		Closed = 5,

#if NET_2_0

		/// <summary>
		/// After an error has happened.
		/// </summary>
		Error = 6,
#endif

	} 
}
