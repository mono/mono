#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Xml.XLinq
{
	[ExtensionAttribute]
	public static class XElementSequence
	{
		[ExtensionAttribute]
		public static IEnumerable <XElement> Ancestors (IEnumerable <XElement> source)
		{
			foreach (XElement item in source)
				foreach (XElement elem in item.Ancestors ())
					yield return elem;
		}

		[ExtensionAttribute]
		public static IEnumerable <XElement> Ancestors (IEnumerable <XElement> source, XName name)
		{
			foreach (XElement item in source)
				foreach (XElement elem in item.Ancestors (name))
					yield return elem;
		}

		[ExtensionAttribute]
		public static IEnumerable <XAttribute> Attributes (IEnumerable <XElement> source)
		{
			foreach (XElement item in source)
				foreach (XAttribute attr in item.Attributes ())
					yield return attr;
		}

		[ExtensionAttribute]
		public static IEnumerable <XAttribute> Attributes (IEnumerable <XElement> source, XName name)
		{
			foreach (XElement item in source)
				foreach (XAttribute attr in item.Attributes (name))
					yield return attr;
		}
	}
}
#endif
