// Web service test for WSDL document:
// http://live.capescience.com/wsdl/GlobalWeather.wsdl

using System;
using NUnit.Framework;
using GlobalWeatherTests.Soap;

namespace External.GlobalWeatherTests
{
	[TestFixture]
	public class GlobalWeatherTest: WebServiceTest
	{
		// CapeConnect / RPC
			
		[Test]
		public void TestStationInfo ()
		{
			StationInfo si = new StationInfo ();
			string[] countries = si.listCountries ();
			Assert.IsNotNull (countries);
			Assert.AreEqual (215, countries.Length);
			Assert.AreEqual ("afghanistan", countries[0]);
			Assert.AreEqual ("spain", countries[177]);
			Assert.AreEqual ("zimbabwe", countries[214]);
			
			Station[] stations = si.searchByCountry ("spain");
			Assert.IsNotNull (stations);
			foreach (Station sta in stations)
			{
				Assert.IsNotNull (sta);
				if (sta.icao == "LEBL")
					Assert.AreEqual ("Barcelona / Aeropuerto", sta.name);
			}
			
			Station[] st = si.searchByCode ("LEBL");
			Assert.IsNotNull (st);
			Assert.AreEqual (1, st.Length);
			Assert.AreEqual ("Barcelona / Aeropuerto", st[0].name);
		}
		
		[Test]
		public void TestGlobalWeather ()
		{
			GlobalWeather gw = new GlobalWeather ();
			WeatherReport wr = gw.getWeatherReport ("LEBL");
			
			Assert.IsNotNull (wr.station);
			Assert.AreEqual ("LEBL", wr.station.icao);
			Assert.AreEqual ("Barcelona / Aeropuerto", wr.station.name);
		}
	}
}
