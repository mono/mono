//
// System.Net.Sockets.SocketPolicyAsyncResult
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

#if NET_2_1

using System.Security;

namespace System.Net.Sockets {

	[MonoTODO]
	[SecurityCritical]
	public class SocketPolicyAsyncResult {

		// more m_* fields
		protected SocketPolicyCheckCallback m_Callback;
		protected object m_UserToken;

		private IPEndPoint endpoint;
		private ProtocolType protocol;


		public SocketPolicyAsyncResult (ProtocolType protocol, IPEndPoint endpoint, SocketPolicyCheckCallback callback,
			object userToken)
		{
			this.protocol = protocol;
			this.endpoint = endpoint;
			// safe bet
			m_Callback = callback;
			m_UserToken = userToken;
		}

		public bool Aborted { get; set; }

		public bool Allowed { get; set; }

		public bool CompletedSynchronously { get; set; }

		public IPEndPoint EndPoint {
			get { return endpoint; }
		}

		public ProtocolType Protocol {
			get { return protocol; }
		}

		public object UserToken {
			get { return m_UserToken; }
		}


		public virtual void Abort ()
		{
		}

		public void Complete ()
		{
		}
	}
}

#endif
