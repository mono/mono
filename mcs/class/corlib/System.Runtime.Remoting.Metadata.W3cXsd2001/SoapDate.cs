//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate
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
	public sealed class SoapDate : ISoapXsd
	{
		static string[] _datetimeFormats = new string[]
		{
			"yyyy-MM-dd",
			"'+'yyyy-MM-dd",
			"'-'yyyy-MM-dd",
			"yyyy-MM-ddzzz",
			"'+'yyyy-MM-ddzzz",
			"'-'yyyy-MM-ddzzz"
		};
		
		int _sign;
		DateTime _value;
		
		public SoapDate ()
		{
		}

		public SoapDate (DateTime date)
		{
			_value = date;
		}

		public SoapDate (DateTime date, int sign)
		{
			_value = date;
			_sign = sign;
		}

		public int Sign {
			get { return _sign; } 
			set { _sign = value; }
		}
		
		public DateTime Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "date"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapDate Parse (string value)
		{
			DateTime d = DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
			
			SoapDate res = new SoapDate (d);
			if (value.StartsWith ("-")) res.Sign = -1;
			else res.Sign = 0;
			return res;
		}

		public override string ToString()
		{
			if (_sign >= 0)
				return _value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			else
				return _value.ToString("'-'yyyy-MM-dd", CultureInfo.InvariantCulture);
		}
	}
}
