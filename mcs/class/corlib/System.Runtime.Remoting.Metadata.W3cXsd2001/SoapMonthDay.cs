//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonthDay
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
	public sealed class SoapMonthDay : ISoapXsd
	{
		static string[] _datetimeFormats = new string[]
		{
			"--MM-dd",
			"--MM-ddzzz"
		};
		
		DateTime _value;
		
		public SoapMonthDay ()
		{
		}
		
		public SoapMonthDay (DateTime value)
		{
			_value = value;
		}
		
		public DateTime Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "gMonthDay"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapMonthDay Parse (string value)
		{
			DateTime d = DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
			return new SoapMonthDay (d);
		}

		public override string ToString()
		{
			return _value.ToString("--MM-dd", CultureInfo.InvariantCulture);
		}
	}
}
