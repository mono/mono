//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapTime
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
	[Serializable]
	public sealed class SoapTime : ISoapXsd
	{
		static string[] _datetimeFormats = new string[]
		{
			"HH:mm:ss",
			"HH:mm:ss.f",
			"HH:mm:ss.ff",
			"HH:mm:ss.fff",
			"HH:mm:ss.ffff",
			"HH:mm:ss.fffff",
			"HH:mm:ss.ffffff",
			"HH:mm:ss.fffffff",
			"HH:mm:sszzz",
			"HH:mm:ss.fzzz",
			"HH:mm:ss.ffzzz",
			"HH:mm:ss.fffzzz",
			"HH:mm:ss.ffffzzz",
			"HH:mm:ss.fffffzzz",
			"HH:mm:ss.ffffffzzz",
			"HH:mm:ss.fffffffzzz",
			"HH:mm:ssZ",
			"HH:mm:ss.fZ",
			"HH:mm:ss.ffZ",
			"HH:mm:ss.fffZ",
			"HH:mm:ss.ffffZ",
			"HH:mm:ss.fffffZ",
			"HH:mm:ss.ffffffZ",
			"HH:mm:ss.fffffffZ"
		};
		
		DateTime _value;
		
		public SoapTime ()
		{
		}

		public SoapTime (DateTime value)
		{
			_value = value;
		}

		public DateTime Value {
			get { return _value; } 
			set { _value = value; }
		}
		
		public static string XsdType {
			get { return "time"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapTime Parse (string value)
		{
			return new SoapTime (DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None));
		}

		public override string ToString()
		{
			return _value.ToString("HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
		}
	}
}
