//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDateTime
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Globalization;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	public sealed class SoapDateTime
	{
		static string[] _datetimeFormats = new string[] {
		  "yyyy-MM-ddTHH:mm:ss",
		  "yyyy-MM-ddTHH:mm:ss.f",
		  "yyyy-MM-ddTHH:mm:ss.ff",
		  "yyyy-MM-ddTHH:mm:ss.fff",
		  "yyyy-MM-ddTHH:mm:ss.ffff",
		  "yyyy-MM-ddTHH:mm:ss.fffff",
		  "yyyy-MM-ddTHH:mm:ss.ffffff",
		  "yyyy-MM-ddTHH:mm:ss.fffffff",
		  "yyyy-MM-ddTHH:mm:sszzz",
		  "yyyy-MM-ddTHH:mm:ss.fzzz",
		  "yyyy-MM-ddTHH:mm:ss.ffzzz",
		  "yyyy-MM-ddTHH:mm:ss.fffzzz",
		  "yyyy-MM-ddTHH:mm:ss.ffffzzz",
		  "yyyy-MM-ddTHH:mm:ss.fffffzzz",
		  "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
		  "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
		  "yyyy-MM-ddTHH:mm:ssZ",
		  "yyyy-MM-ddTHH:mm:ss.fZ",
		  "yyyy-MM-ddTHH:mm:ss.ffZ",
		  "yyyy-MM-ddTHH:mm:ss.fffZ",
		  "yyyy-MM-ddTHH:mm:ss.ffffZ",
		  "yyyy-MM-ddTHH:mm:ss.fffffZ",
		  "yyyy-MM-ddTHH:mm:ss.ffffffZ",
		  "yyyy-MM-ddTHH:mm:ss.fffffffZ"
		};

		public SoapDateTime()
		{
		}
		
		public static string XsdType {
			get { return "dateTime"; }
		}

		public static DateTime Parse (string value)
		{
			return DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
		}

		public static string ToString(DateTime value)
		{
			return value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
		}
	}
}
