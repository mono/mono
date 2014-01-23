using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class WebHttpBindingTest
	{
		[Test]
		public void DefaultPropertyValues ()
		{
			WebHttpBinding b = new WebHttpBinding ();
			Assert.AreEqual (EnvelopeVersion.None, b.EnvelopeVersion, "#1");
			Assert.AreEqual ("http", b.Scheme, "#1");
			Assert.AreEqual (Encoding.UTF8, b.WriteEncoding, "#2");
			Assert.AreEqual (0x10000, b.MaxBufferSize, "#3");
#if !MOBILE
			Assert.AreEqual (0x80000, b.MaxBufferPoolSize, "#4");
#endif
			Assert.AreEqual (0x10000, b.MaxReceivedMessageSize, "#5");
			Assert.IsFalse (((IBindingRuntimePreferences) b).ReceiveSynchronously, "#6");
		}

		[Test]
		public void CreateBindingElements ()
		{
			WebHttpBinding b = new WebHttpBinding ();
			BindingElementCollection bc = b.CreateBindingElements ();
			Assert.AreEqual (2, bc.Count, "#1");
			Assert.AreEqual (typeof (WebMessageEncodingBindingElement), bc [0].GetType (), "#2");
			Assert.AreEqual (typeof (HttpTransportBindingElement), bc [1].GetType (), "#3");
		}
	}
}
