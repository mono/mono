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
	public class Attribute {
	    public string Prefix;
	    public XmlQualifiedName QName;
	    public string Value;
	    
		public Attribute(string prefix, XmlQualifiedName qName, string value){
		    Prefix = prefix;
		    QName = qName;
		    Value = value;
		}
	}
}
