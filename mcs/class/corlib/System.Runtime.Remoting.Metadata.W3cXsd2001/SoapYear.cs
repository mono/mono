//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapYear
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
	public sealed class SoapYear : ISoapXsd
	{
		static string[] _datetimeFormats = new string[]
		{
			"yyyy",
			"'+'yyyy",
			"'-'yyyy",
			"yyyyzzz",
			"'+'yyyyzzz",
			"'-'yyyyzzz"
		};
		
		int _sign;
		DateTime _value;
		
		public SoapYear()
		{
		}

		public SoapYear (DateTime date)
		{
			_value = date;
		}

		public SoapYear (DateTime date, int sign)
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
			get { return "gYear"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapYear Parse (string value)
		{
			DateTime d = DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
			
			SoapYear res = new SoapYear (d);
			if (value.StartsWith ("-")) res.Sign = -1;
			else res.Sign = 0;
			return res;
		}

		public override string ToString()
		{
			if (_sign >= 0)
				return _value.ToString("yyyy", CultureInfo.InvariantCulture);
			else
				return _value.ToString("'-'yyyy", CultureInfo.InvariantCulture);
		}
	}
}
