using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

namespace System
{
	static class AssemblyRef
	{
		public const string MicrosoftJScript = "Microsoft.JScript";
		public const string SystemData = "System.Data";
	}

	static class ThisAssembly
	{
		public const string Version = "System.Xml";
	}
}

// Types within this namespace are taken from referencesource
namespace System.Xml.Serialization
{
	//------------------------------------------------------------------------------
	//     Copyright (c) Microsoft Corporation.  All rights reserved.
	//------------------------------------------------------------------------------

	// from Models.cs
	internal enum SpecifiedAccessor {
		None,
		ReadOnly,
		ReadWrite,
	}

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
	internal class Wsdl {
		private Wsdl() { }
		internal const string Namespace = "http://schemas.xmlsoap.org/wsdl/";
		internal const string ArrayType = "arrayType";
	}
}

namespace System.Security.Policy
{
	static class EvidenceExtensions
	{
		public static void AddHostEvidence (this Evidence evidence, Url url)
		{
			throw new NotImplementedException ();
		}
		public static void AddHostEvidence (this Evidence evidence, Zone zone)
		{
			throw new NotImplementedException ();
		}
		public static void AddHostEvidence (this Evidence evidence, Site site)
		{
			throw new NotImplementedException ();
		}
		public static void AddHostEvidence (this Evidence evidence, EvidenceBase e)
		{
			throw new NotImplementedException ();
		}
	}
}
