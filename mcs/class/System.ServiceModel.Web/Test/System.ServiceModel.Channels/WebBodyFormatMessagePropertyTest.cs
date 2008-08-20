using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class WebBodyFormatMessagePropertyTest
	{
		[Test]
		public void Members ()
		{
			WebBodyFormatMessageProperty p = new WebBodyFormatMessageProperty (WebContentFormat.Json);
			Assert.AreEqual ("WebBodyFormatMessageProperty", WebBodyFormatMessageProperty.Name, "#1");
			Assert.AreEqual (WebContentFormat.Json, p.Format, "#2");
			Assert.AreEqual ("WebBodyFormatMessageProperty: WebContentFormat=Json", p.ToString (), "#3");
		}
	}
}
