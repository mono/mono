//
// System.Net.MonoHttpDate
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Globalization;

namespace System.Net 
{
	/// <summary>
	/// See RFC 2068 3.3.1
	/// </summary>
	internal class MonoHttpDate
	{
		private static readonly string rfc1123_date = "r";
		private static readonly string rfc850_date = "dddd, dd-MMM-yy HH:mm:ss G\\MT";
		private static readonly string asctime_date = "ddd MMM d HH:mm:ss yyyy";
		private static readonly string [] formats = 
			new string [] {rfc1123_date, rfc850_date, asctime_date};
		
		internal static DateTime Parse (string dateStr)
		{			
			 return DateTime.ParseExact (dateStr, 
			                             formats, 
			                             CultureInfo.InvariantCulture, 
			                             DateTimeStyles.AllowWhiteSpaces);
		}
	}
}
