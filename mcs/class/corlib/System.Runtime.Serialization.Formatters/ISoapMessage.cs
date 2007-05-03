//
// System.Runtime.Serialization.Formatters.ISoapMessage
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

using System.Runtime.Remoting.Messaging;

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Runtime.Serialization.Formatters {

	/// <summary>
	/// Interface for making SOAP method calls</summary>
#if NET_2_0
	[ComVisible (true)]
#endif
	public interface ISoapMessage {

		/// <summary>
		/// Get or set the headers ("out-of-band" data) for the method call</summary>
		Header[] Headers {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method name</summary>
		string MethodName {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method parameter names</summary
		string[] ParamNames {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method parameter types</summary
		Type[] ParamTypes {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method parameter values</summary
		object[]  ParamValues {
			get;
			set;
		}

		/// <summary>
		/// Get or set the XML namespace for the location of the called object</summary
		string XmlNameSpace {
			get;
			set;
		}
	}
}
