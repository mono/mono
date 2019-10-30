using System;
using System.Collections;
using System.Collections.Generic;
using WebAssembly.ZoneInfo;

public class TZDTest {
	public static void Main (String [] args)
	{
		// var local = TimeZoneInfo.Local;
		// Console.WriteLine($"TimeZone: {local}");
		var tzd = TimeZoneInfo.FindSystemTimeZoneById("US/Eastern");
		Console.WriteLine($"TimeZone: {tzd}");
		var size = 0;
		var tz = TimeZoneInfo.GetSystemTimeZones();
		foreach(var ttt in tz)
		{
			Console.WriteLine(ttt);
		}
		//MonoWasmZoneInfo.mono_timezone_get_data("hhh", ref size);
		//var data = MonoWasmZoneInfo.GetEmbeddedResource("Europe/Luxembourg");
		//Console.WriteLine($"data: {data?.Length}");
		// var current = TimeZone.CurrentTimeZone;
		// var tzd = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
	}


}
