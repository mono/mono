//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	// from Types.cs
	internal class TypeScope
	{
		static internal XmlQualifiedName ParseWsdlArrayType(string type, out string dims, XmlSchemaObject parent) {
			string ns;
			string name;

			int nsLen = type.LastIndexOf(':');

			if (nsLen <= 0) {
				ns = "";
			}
			else {
				ns = type.Substring(0, nsLen);
			}
			int nameLen = type.IndexOf('[', nsLen + 1);

			if (nameLen <= nsLen) {
				throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayTypeSyntax, type));
			}
			name = type.Substring(nsLen + 1, nameLen - nsLen - 1);
			dims = type.Substring(nameLen);

			// parent is not null only in the case when we used XmlSchema.Read(), 
			// in which case we need to fixup the wsdl:arayType attribute value
			while (parent != null) {
				if (parent.Namespaces != null) {
					string wsdlNs = (string)parent.Namespaces.Namespaces[ns];
					if (wsdlNs != null) {
						ns = wsdlNs;
						break;
					}
				}
				parent = parent.Parent;
			}
			return new XmlQualifiedName(name, ns);
		}
	}
}

