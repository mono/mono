//
// IXmlNamespaceResolver.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_1_2

using System;
using System.Collections.Specialized;
using System.Security.Policy;
using System.Xml.XPath;

namespace System.Xml
{
	public interface IXmlNamespaceResolver
	{
		XmlNameTable NameTable { get; } 

		StringDictionary GetNamespacesInScope (XmlNamespaceScope scope);

		string LookupNamespace (string prefix);  

		string LookupNamespace (string prefix, bool atomizedName);  

		string LookupPrefix (string ns);  

		string LookupPrefix (string ns, bool atomizedName);  
	}
}

#endif
