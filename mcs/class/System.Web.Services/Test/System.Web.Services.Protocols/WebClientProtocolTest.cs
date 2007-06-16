// 
// System.Web.Services.Discovery.ContractReference.cs
//
// Author:
//   Konstantin Triger (kostat@mainsoft.com)
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

using NUnit.Framework;
using System;
using System.Net;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace MonoTests.System.Web.Services.Protocols
{
	[TestFixture]
	public class WebClientProtocolTest
	{
		[WebServiceBinding (Name = "PokerSoap", Namespace = "http://tempuri.org/")]
		public class Poker : SoapHttpClientProtocol
		{
		}

#if NET_2_0
		[Test]
		public void TestUseDefaultCredentials () {
			Poker client = new Poker ();
			Assert.IsFalse (client.UseDefaultCredentials, "#1");
			client.UseDefaultCredentials = true;
			Assert.IsTrue (client.Credentials == CredentialCache.DefaultCredentials, "#2");
			client.Credentials = new NetworkCredential ("a", "b");
			Assert.IsFalse (client.UseDefaultCredentials, "#3");
			client.UseDefaultCredentials = false;
			Assert.IsNull (client.Credentials, "#4");
			client.Credentials = CredentialCache.DefaultCredentials;
			Assert.IsTrue (client.UseDefaultCredentials, "#5");
		}
#endif
	}
}