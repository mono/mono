//
// XmlMapping.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002 John Donagher
//

using System;
using System.Collections;

namespace System.Xml.Serialization
{
	public abstract class XmlMapping
	{
		ObjectMap map;
		ArrayList relatedMaps;
		SerializationFormat format;

		internal XmlMapping ()
		{
		}

		internal ObjectMap ObjectMap
		{
			get { return map; }
			set { map = value; }
		}

		internal ArrayList RelatedMaps
		{
			get { return relatedMaps; }
			set { relatedMaps = value; }
		}

		internal SerializationFormat Format
		{
			get { return format; }
			set { format = value; }
		}
	}

	internal class ObjectMap
	{
	}

	internal enum SerializationFormat { Encoded, Literal }
}
