#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xml.XLinq
{
	// [ExtensionAttribute]
	public static class XElementSequence
	{
		// [ExtensionAttribute]
		public static IEnumerable <XElement> Ancestors (IEnumerable <XElement> source)
		{
			throw new NotImplementedException ();
		}

		// [ExtensionAttribute]
		public static IEnumerable <XElement> Ancestors (IEnumerable <XElement> source, XName name)
		{
			throw new NotImplementedException ();
		}

		// [ExtensionAttribute]
		public static IEnumerable <XAttribute> Attributes (IEnumerable <XAttribute> source)
		{
			throw new NotImplementedException ();
		}

		// [ExtensionAttribute]
		public static IEnumerable <XAttribute> Attributes (IEnumerable <XAttribute> source, XName name)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
