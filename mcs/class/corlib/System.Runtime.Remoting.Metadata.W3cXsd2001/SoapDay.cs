//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay
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
	public sealed class SoapDay : ISoapXsd
	{
		static string[] _datetimeFormats = new string[]
		{
			"---dd",
			"---ddzzz"
		};
		
		DateTime _value;
		
		public SoapDay ()
		{
		}
		
		public SoapDay (DateTime value)
		{
			_value = value;
		}
		
		public DateTime Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "gDay"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapDay Parse (string value)
		{
			DateTime d = DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
			return new SoapDay (d);
		}
		
		public override string ToString()
		{
			return _value.ToString("---dd", CultureInfo.InvariantCulture);
		}
	}
}
