#if NET_4_0
using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class WebHttpEndpointTest
	{
		[Test]
		public void ReplaceBinding1 ()
		{
			var se = new WebHttpEndpoint (ContractDescription.GetContract (typeof (IMetadataExchange)), null);
			se.Binding = new NetTcpBinding (); // this does not throw exception yet.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReplaceBinding2 ()
		{
			var se = new WebHttpEndpoint (ContractDescription.GetContract (typeof (IMetadataExchange)), null);
			se.Binding = new NetTcpBinding ();
			se.WriteEncoding = Encoding.UTF8;
		}
	}
}
#endif
