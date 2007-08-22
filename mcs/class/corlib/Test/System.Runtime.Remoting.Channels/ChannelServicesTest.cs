//
// ChannelServicesTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@xmian.com>
//
// Copyright (C) 2007 Novell, Inc.
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

using System;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting.Channels
{
	
	[TestFixture]
	public class ChannelServicesTest
	{
#if NET_2_0
		[Test]
		[ExpectedException (typeof (RemotingException))]
		public void ConstructorEnsureSecurity ()
		{
			IChannel ch = new NonSecureChannel ();
			ChannelServices.RegisterChannel (ch, true);
			// in case it happened to successfully register the channel...
			ChannelServices.UnregisterChannel (ch);
		}

		[Test]
		public void ConstructorEnsureSecurity2 ()
		{
			IChannel ch = new SecureChannel ();
			ChannelServices.RegisterChannel (ch, true);
			ChannelServices.UnregisterChannel (ch);
		}

		class NonSecureChannel : IChannel
		{
			public string Parse (string url, out string objectURI)
			{
				objectURI = "my:foo";
				return "foo";
			}

			public string ChannelName {
				get { return "my"; }
			}

			public int ChannelPriority {
				get { return 0; }
			}
		}

		class SecureChannel : NonSecureChannel, ISecurableChannel
		{
			public bool IsSecured {
				get { return false; }
				set { }
			}
		}
#endif
	}
}
