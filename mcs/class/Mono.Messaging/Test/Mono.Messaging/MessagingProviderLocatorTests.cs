//
// Test.Mono.Messaging.MessagingProviderLocatorTests
//
// Authors:
//      Philip Garrett (philgarr@gmail.com)
//
// (C) 2015 Philip Garrett
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

using System;

using Mono.Messaging;
using NUnit.Framework;

namespace MonoTests.Mono.Messaging
{
	[TestFixture]
	public class MessagingProviderLocatorTests
	{
		[Test]
		public void TestDefaultProvider()
		{
			IMessagingProvider provider = MessagingProviderLocator.GetProvider ();
			string providerName = provider.GetType ().Name;
			Assert.That (providerName.Contains ("RabbitMQ"));
		}

		[Test]
		public void TestOverrideProvider()
		{
			try {
				MessagingProviderLocator.strategy = () => typeof(DummyMessagingProvider).AssemblyQualifiedName;
				MessagingProviderLocator.instance = new MessagingProviderLocator ();
				Assert.IsNotNull (MessagingProviderLocator.GetProvider () as DummyMessagingProvider);
			}
			finally {
				MessagingProviderLocator.strategy = MessagingProviderLocator.GetProviderClassName;
			}
		}

		[Test]
		public void TestOverrideSuitability()
		{
			MonoMessagingException caught = null;
			try {
				// System.Object is not a suitable IMessagingProvider
				MessagingProviderLocator.strategy = () => typeof(object).AssemblyQualifiedName;
				MessagingProviderLocator.instance = new MessagingProviderLocator ();
			}
			catch (MonoMessagingException e) {
				caught = e;
			}
			finally {
				MessagingProviderLocator.strategy = MessagingProviderLocator.GetProviderClassName;
			}
			Assert.IsNotNull (caught, "System.Object does not implement IMessagingProvider");
			Assert.IsTrue (caught.Message.Contains ("Object does not implement interface IMessagingProvider"),
				"should provide a helpful error");
		}
	}
}
