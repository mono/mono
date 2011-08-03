//
// System.Net.EndPoint.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

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

using System.Net.Sockets;

namespace System.Net {
	[Serializable]
	public abstract class EndPoint {

		// NB: These methods really do nothing but throw
		// NotImplementedException
		
		public virtual AddressFamily AddressFamily {
			get { throw NotImplemented (); }
		}
		
		public virtual EndPoint Create (SocketAddress socketAddress)
		{
			throw NotImplemented ();
		}

		public virtual SocketAddress Serialize ()
		{
			throw NotImplemented ();
		}

		protected EndPoint ()
		{
		}

		static Exception NotImplemented ()
		{
			// hide the "normal" NotImplementedException from corcompare-like tools
			return new NotImplementedException ();
		}
	}
}

