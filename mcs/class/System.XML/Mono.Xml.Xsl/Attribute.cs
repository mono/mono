//
// Attribute.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;
using System.Xml;

namespace Mono.Xml.Xsl {      
	/// <summary>
	/// XML attribute.
	/// </summary>
	public struct Attribute {
		public string Prefix;
		public string Namespace;
		public string LocalName;
		public string Value;
	    
		public Attribute (string prefix, string namespaceUri, string localName, string value)
		{
			this.Prefix = prefix;
			this.Namespace = namespaceUri;
			this.LocalName = localName;
			this.Value = value;
		}
	}
}
