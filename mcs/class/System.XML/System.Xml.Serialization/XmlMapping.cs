//
// XmlMapping.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002 John Donagher
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
#if !MOONLIGHT
		SerializationSource source;
#endif
		
		internal string _elementName;
		internal string _namespace;
		
#if NET_2_0
		string key;
#endif		

		internal XmlMapping ()
		{
		}

		internal XmlMapping (string elementName, string ns)
		{
			_elementName = elementName;
			_namespace = ns;
		}

#if NET_2_0
		[MonoTODO]
		public string XsdElementName
		{
			get { return _elementName; }
		}

		public string ElementName
		{
			get { return _elementName; }
		}

		public string Namespace
		{
			get { return _namespace; }
		}
		
		public void SetKey (string key)
		{
			this.key = key;
		}
		
		internal string GetKey ()
		{
			return key;
		}
#endif

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
		
#if !MOONLIGHT
		internal SerializationSource Source
		{
			get { return source; }
			set { source = value; }
		}
#endif
	}

	internal class ObjectMap
	{
	}

	internal enum SerializationFormat { Encoded, Literal }
}
