using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class FaultContractAttributeTest
	{
		[Test]
		public void Defaults ()
		{
			var a = new FaultContractAttribute (typeof (MyDetail));
			Assert.AreEqual (typeof (MyDetail), a.DetailType, "#1");
			Assert.IsNull (a.Action, "#2");
			Assert.IsNull (a.Name, "#3");
			Assert.IsNull (a.Namespace, "#4");
		}

		class MyDetail
		{
		}
	}
}
