// Web service test for WSDL document:
// http://live.capescience.com/wsdl/GlobalWeather.wsdl

using System;
using NUnit.Framework;
using GlobalWeatherTests.Soap;

namespace GlobalWeatherTests
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
			AssertNotNull (countries);
			AssertEquals (215, countries.Length);
			AssertEquals ("afghanistan", countries[0]);
			AssertEquals ("spain", countries[177]);
			AssertEquals ("zimbabwe", countries[214]);
			
			Station[] stations = si.searchByCountry ("spain");
			AssertNotNull (stations);
			foreach (Station sta in stations)
			{
				AssertNotNull (sta);
				if (sta.icao == "LEBL")
					AssertEquals ("Barcelona / Aeropuerto", sta.name);
			}
			
			Station[] st = si.searchByCode ("LEBL");
			AssertNotNull (st);
			AssertEquals (1, st.Length);
			AssertEquals ("Barcelona / Aeropuerto", st[0].name);
		}
		
		[Test]
		public void TestGlobalWeather ()
		{
			GlobalWeather gw = new GlobalWeather ();
			WeatherReport wr = gw.getWeatherReport ("LEBL");
			
			AssertNotNull (wr.station);
			AssertEquals ("LEBL", wr.station.icao);
			AssertEquals ("Barcelona / Aeropuerto", wr.station.name);
		}
	}
}
