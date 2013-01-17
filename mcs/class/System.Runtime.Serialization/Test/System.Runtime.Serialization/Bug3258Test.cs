using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class Bug3258Test
	{
		[Test]
		public void TestSerializeNullDateTimeOffsetNullable ()
		{
			 // Create the writer object.
			StringBuilder stringBuilder = new StringBuilder ();

			DateTimeOffset? dto = null;

			DataContractSerializer ser = new DataContractSerializer (typeof (DateTimeOffset?));

			using (var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (new StringWriter (stringBuilder))))
			{
				ser.WriteObject (xw, dto);
			}

			string actualXml   = stringBuilder.ToString ();
			string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><DateTimeOffset i:nil=\"true\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/System\" />";
			
			Assert.AreEqual (expectedXml, actualXml, "#1 Null DateTimeOffset? serialization error");

			using (var xr = XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (actualXml))))
			{
				DateTimeOffset? actualDto = (DateTimeOffset?)ser.ReadObject (xr, true);

				Assert.AreEqual (dto, actualDto, "#2 Null DateTimeOffset? deserialization error");
				Assert.IsNull (actualDto, "#3 Null DateTimeOffset? deserialization error");
			}
		}
		
		[Test]
		public void TestSerializeDateTimeOffsetNullable ()
		{
			 // Create the writer object.
			StringBuilder stringBuilder = new StringBuilder ();

			DateTimeOffset? dto = new DateTimeOffset (2012, 05, 04, 02, 34, 00, new TimeSpan (-2, 0, 0));;

			DataContractSerializer ser = new DataContractSerializer (typeof (DateTimeOffset?));

			using (var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (new StringWriter (stringBuilder))))
			{
				ser.WriteObject (xw, dto);
			}

			string actualXml   = stringBuilder.ToString ();
			string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><DateTimeOffset xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/System\"><DateTime>2012-05-04T04:34:00Z</DateTime><OffsetMinutes>-120</OffsetMinutes></DateTimeOffset>";
			
			Assert.AreEqual (expectedXml, actualXml, "#1 Nullable DateTimeOffset serialization error");

			using (var xr = XmlDictionaryReader.CreateDictionaryReader(XmlReader.Create (new StringReader (actualXml))))
			{
				DateTimeOffset? actualDto = (DateTimeOffset?)ser.ReadObject (xr, true);

				Assert.AreEqual (dto, actualDto, "#2 Nullable DateTimeOffset deserialization error");
			}
		}
	}
}
