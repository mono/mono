//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonth
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
	public sealed class SoapMonth : ISoapXsd
	{
		static string[] _datetimeFormats = new string[]
		{
			"--MM--",
			"--MM--zzz"
		};
		
		DateTime _value;
		
		public SoapMonth ()
		{
		}
		
		public SoapMonth (DateTime value)
		{
			_value = value;
		}
		
		public DateTime Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "gMonth"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapMonth Parse (string value)
		{
			DateTime d = DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
			return new SoapMonth (d);
		}

		public override string ToString()
		{
			return _value.ToString("--MM--", CultureInfo.InvariantCulture);
		}
	}
}
