#if !MOBILE

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Activation
{
	class MyHostFactory : WebScriptServiceHostFactory
	{
		public ServiceHost DoCreateServiceHost (Type type, params Uri [] baseAddresses)
		{
			return CreateServiceHost (type, baseAddresses);
		}
	}

	[TestFixture]
	public class WebScriptServiceHostFactoryTest
	{
		[Test]
		public void CreateServiceHost ()
		{
			var f = new MyHostFactory ();
			var host = f.DoCreateServiceHost (typeof (TestService), new Uri [] {new Uri ("http://localhost:37564")});
			Assert.IsFalse (host is WebServiceHost, "#1");
			host.Open ();
			host.Close ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ResponseWrappedIsInvalid ()
		{
			var f = new MyHostFactory ();
			var host = f.DoCreateServiceHost (typeof (TestService2), new Uri [] {new Uri ("http://localhost:37564")});
			host.Open (); // should raise an error here.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MultipleContract ()
		{
			var f = new MyHostFactory ();
			var host = f.DoCreateServiceHost (typeof (TestServiceMultiple), new Uri [] {new Uri ("http://localhost:37564")});
			host.Open ();
		}

	}

	[ServiceContract]
	public interface ITestService
	{
		[OperationContract]
		string DoWork (string s1, string s2);
	}

	public class TestService : ITestService
	{
		public string DoWork (string s1, string s2)
		{
			return s1 + s2;
		}
	}

	[ServiceContract]
	public interface ITestService2
	{
		[OperationContract]
		[WebGet (BodyStyle = WebMessageBodyStyle.WrappedResponse)]
		string DoWork (string s1, string s2);
	}

	public class TestService2 : ITestService2
	{
		public string DoWork (string s1, string s2)
		{
			return s1 + s2;
		}
	}

	public class TestServiceMultiple : ITestService, ITestService2
	{
		public string DoWork (string s1, string s2)
		{
			return s1 + s2;
		}
	}
}

#endif