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

namespace System.Xml.Serialization
{
	public abstract class XmlMapping
	{
		ObjectMap map;

		internal XmlMapping ()
		{
		}

		internal ObjectMap ObjectMap
		{
			get { return map; }
			set { map = value; }
		}
	}

	internal class ObjectMap
	{
	}
}
