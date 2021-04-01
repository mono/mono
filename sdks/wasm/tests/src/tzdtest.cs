using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class TZDTest {
	static TimeZoneInfo _timeZoneInstance;
	static string _timeZone;
	public static void Main (String [] args)
	{
		var local = TimeZoneInfo.Local;
		Console.WriteLine($"TimeZone: {local}");
		var dt = DateTime.Now;
		Console.WriteLine($"DateLocal: {dt}");
		var utc = DateTime.UtcNow;
		Console.WriteLine($"DateUTC: {utc}");

		Console.WriteLine($"GetUserTime: {GetUserTime()}");
		Console.WriteLine($"GetUtcUserTime: {GetUtcUserTime(dt)}");

		DateTime localDate = DateTime.Now;
		DateTime utcDate = DateTime.UtcNow;
		String[] cultureNames = { "en-US", "en-GB", "fr-FR",
								"de-DE", "ru-RU" } ;

		foreach (var cultureName in cultureNames) {
			var culture = new CultureInfo(cultureName);
			Console.WriteLine("{0}:", culture.NativeName);
			Console.WriteLine("   Local date and time: {0}, {1:G}",
							localDate.ToString(culture), localDate.Kind);
			Console.WriteLine("   UTC date and time: {0}, {1:G}\n",
							utcDate.ToString(culture), utcDate.Kind);
		}

		var tzd = TimeZoneInfo.FindSystemTimeZoneById("US/Eastern");
		Console.WriteLine($"TimeZone: {tzd}");

		Console.WriteLine($"TimeZone: {TimeZoneInfo.FindSystemTimeZoneById("Pacific/Honolulu")}");
		
		var tzs = TimeZoneInfo.GetSystemTimeZones();
		foreach(var tzi in tzs)
		{
			Console.WriteLine(tzi);
		}
	}

	/// <summary>
	/// Returns a UTC time in the user's specified timezone.
	/// </summary>
	/// <param name="utcTime">The utc time to convert</param>
	/// <param name="timeZoneName">Name of the timezone (Eastern Standard Time)</param>
	/// <returns>New local time</returns>
	public static DateTime GetUserTime(DateTime? utcTime = null)
	{
		if (utcTime == null)
			utcTime = DateTime.UtcNow;        
			
		return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, TimeZoneInstance);
	}

	/// <summary>
	/// Converts local server time to the user's timezone and
	/// returns the UTC date.
	/// 
	/// Use this to convert user captured date inputs and convert
	/// them to UTC.  
	/// 
	/// User input (their local time) comes in as local server time 
	/// -> convert to user's timezone from server time
	/// -> convert to UTC
	/// </summary>
	/// <param name="localServerTime"></param>
	/// <returns></returns>
	public static DateTime GetUtcUserTime(DateTime? localServerTime)
	{
		if (localServerTime == null)
			localServerTime = DateTime.Now;

		return TimeZoneInfo.ConvertTime(localServerTime.Value, TimeZoneInstance).ToUniversalTime();
	}	

	/// <summary>
	/// The users TimeZone using .NET TimeZoneNames
	/// </summary>
	public static string TimeZone
	{
		get { return _timeZone; }
		set
		{
			TimeZoneInstance = null;
			_timeZone = value;
		} 
	}
	public static TimeZoneInfo TimeZoneInstance
	{
		get
		{
			if (_timeZoneInstance == null)
			{
				try
				{
					_timeZoneInstance = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
				}
				catch
				{
					TimeZone = "Pacific/Honolulu";
					_timeZoneInstance = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
				}
			}
			return _timeZoneInstance;
		}
		private set { _timeZoneInstance = value; }
	}
}