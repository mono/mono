// Web service test for WSDL document:
// http://www.e-naxos.com/scripts/enwscp.dll/wsdl/IODCODESPOSTAUX

using System;
using NUnit.Framework;
using IODCODESPOSTAUXTests.Soap;

namespace External.IODCODESPOSTAUXTests
{
	[TestFixture]
	public class IODCODESPOSTAUXTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			// Delphi / RPC
			
			IODCODESPOSTAUXservice ser = new IODCODESPOSTAUXservice ();
			
//			ser.GetTownsForSoundex ("Pyrénées Orientales",",");
			string res = ser.GetDepartmentForCode ("66");
			Console.WriteLine ("á");
			Console.WriteLine (res);
			Console.WriteLine ("Pyrénées Orientales");
			Assert.AreEqual ("Pyrénées Orientales", res);
			
			res = ser.GetZipForTown ("Perpignan",",",true);
			Assert.AreEqual ("66000=Perpignan,66100=Perpignan", res);
		}
	}
}
