// CS1922: A field or property `System.Type' cannot be initialized with a collection object initializer because type `System.Type' does not implement `System.Collections.IEnumerable' interface
// Line: 13

using System;
using System.Xml.Serialization;

namespace test
{
	public class Test
	{
		static void Main ()
		{
			XmlSerializer xs = new XmlSerializer (typeof (string), new Type () { typeof (bool) });
		}
	}
}

