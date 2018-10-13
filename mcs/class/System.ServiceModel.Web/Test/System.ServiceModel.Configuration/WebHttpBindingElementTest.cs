#if !MOBILE && !MONOMAC
using System;
using System.ServiceModel.Configuration;
using NUnit.Framework;
using System.ServiceModel;
using System.Text;
using System.Configuration;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class WebHttpBindingElementTest
	{
		class Poker : WebHttpBindingElement
		{
			public Type GetBindingElementType ()
			{
				return BindingElementType;
			}
		}

		[Test]
		public void BindingElementType ()
		{
			Poker poker = new Poker ();
			Assert.AreEqual (typeof (WebHttpBinding), poker.GetBindingElementType (), "BindingElementType");
		}
		
		[Test]
		public void ApplyConfiguration ()
		{
			WebHttpBinding b = CreateBindingFromConfig ();

			Assert.AreEqual (true, b.AllowCookies, "#1");
			Assert.AreEqual (true, b.BypassProxyOnLocal, "#2");
			Assert.AreEqual (HostNameComparisonMode.Exact, b.HostNameComparisonMode, "#3");
			Assert.AreEqual (262144, b.MaxBufferPoolSize, "#4");
			Assert.AreEqual (32768, b.MaxBufferSize, "#5");
			Assert.AreEqual (16384, b.MaxReceivedMessageSize, "#6");
			Assert.AreEqual ("proxy", b.ProxyAddress.ToString (), "#7");
			Assert.AreEqual (Encoding.Unicode, b.WriteEncoding, "#8");
			Assert.AreEqual (TransferMode.Streamed, b.TransferMode, "#9");
		}
		
		[Test]
		public void Security ()
		{
			WebHttpBinding b = CreateBindingFromConfig ();
			Assert.AreEqual (WebHttpSecurityMode.TransportCredentialOnly, b.Security.Mode, "#1");
			Assert.AreEqual (HttpClientCredentialType.Basic, b.Security.Transport.ClientCredentialType, "#2");
			
		}
		
		private WebHttpBinding CreateBindingFromConfig ()
		{
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/webHttpBinding")).GetSectionGroup ("system.serviceModel");
			WebHttpBindingCollectionElement collectionElement = (WebHttpBindingCollectionElement) config.Bindings ["webHttpBinding"];
			
			WebHttpBindingElement el = collectionElement.Bindings ["WebHttpBinding1_Service"];

			WebHttpBinding b = new WebHttpBinding ();
			el.ApplyConfiguration (b);

			return b;
		}
	}
}
#endif