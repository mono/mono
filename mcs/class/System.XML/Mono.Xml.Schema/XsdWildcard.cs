//
// Mono.Xml.Schema.XsdWildcard.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
//
// This class represents common part of xs:any and xs:anyAttribute
//
//
using System;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml.Schema
{
	public class XsdWildcard
	{
		public XsdWildcard (XmlSchemaObject wildcard)
		{
			xsobj = wildcard;
		}

		private XmlSchemaObject xsobj;

		public XmlSchemaContentProcessing ResolvedProcessing;
		public string TargetNamespace;
		public bool HasValueAny;
		public bool HasValueLocal;
		public bool HasValueOther;
		public bool HasValueTargetNamespace;
		public StringCollection ResolvedNamespaces;

		public void Compile (string nss,
			ValidationEventHandler h, XmlSchema schema)
		{
			int nscount = 0;
			string actualNamespace = nss == null ? "##any" : nss;
			string[] nslist = XmlSchemaUtil.SplitList(actualNamespace);
			ResolvedNamespaces = new StringCollection ();
			foreach(string ns in nslist)
			{
				switch(ns) {
				case "##any": 
					if (HasValueAny)
						xsobj.error (h, "Multiple specification of ##any was found.");
					nscount |= 1;
					HasValueAny = true;
					break;
				case "##other":
					if (HasValueOther)
						xsobj.error (h, "Multiple specification of ##other was found.");
					nscount |= 2;
					HasValueOther = true;
					break;
				case "##targetNamespace":
					if (HasValueTargetNamespace)
						xsobj.error (h, "Multiple specification of ##targetNamespace was found.");
					nscount |= 4;
					HasValueTargetNamespace = true;
					break;
				case "##local":
					if (HasValueLocal)
						xsobj.error (h, "Multiple specification of ##local was found.");
					nscount |= 8;
					HasValueLocal = true;
					break;
				default:
					if(!XmlSchemaUtil.CheckAnyUri(ns))
						xsobj.error(h,"the namespace is not a valid anyURI");
					else if (ResolvedNamespaces.Contains (ns))
						xsobj.error (h, "Multiple specification of '" + ns + "' was found.");
					else {
						nscount |= 16;
						ResolvedNamespaces.Add (ns);
					}
					break;
				}
			}
			if((nscount&1) == 1 && nscount != 1)
				xsobj.error (h, "##any if present must be the only namespace attribute");
			if((nscount&2) == 2 && nscount != 2)
				xsobj.error (h, "##other if present must be the only namespace attribute");
		}

		// 3.8.6. Attribute Wildcard Intersection
		// Only try to examine if their intersection is expressible, and
		// returns true if the result is empty.
		public bool ExamineAttributeWildcardIntersection (XmlSchemaAny other,
			ValidationEventHandler h, XmlSchema schema)
		{
			// 1.
			if (this.HasValueAny == other.HasValueAny &&
				this.HasValueLocal == other.HasValueLocal &&
				this.HasValueOther == other.HasValueOther &&
				this.HasValueTargetNamespace == other.HasValueTargetNamespace &&
				this.ResolvedProcessing == other.ResolvedProcessContents) {
				bool notEqual = false;
				foreach (string ns in this.ResolvedNamespaces) {
					if (!other.ResolvedNamespaces.Contains (ns))
						notEqual = true;
				}
				if (!notEqual)
					return false;
			}
			// 2.
			if (this.HasValueAny)
				return !other.HasValueAny &&
					!other.HasValueLocal &&
					!other.HasValueOther &&
					!other.HasValueTargetNamespace &&
					other.ResolvedNamespaces.Count == 0;
			if (other.HasValueAny)
				return !this.HasValueAny &&
					!this.HasValueLocal &&
					!this.HasValueOther &&
					!this.HasValueTargetNamespace &&
					this.ResolvedNamespaces.Count == 0;
			// 5.
			if (this.HasValueOther && other.HasValueOther && this.TargetNamespace != other.TargetNamespace) {
//				xsobj.error (h, "The Wildcard intersection is not expressible.");
				return false;
			}
			// 3.
			if (this.HasValueOther) {
				if (other.HasValueLocal && this.TargetNamespace != String.Empty)
					return false;
				if (other.HasValueTargetNamespace && this.TargetNamespace != other.TargetNamespace)
					return false;
				return other.ValidateWildcardAllowsNamespaceName (this.TargetNamespace, h, schema, false);
			}
			if (other.HasValueOther) {
				if (this.HasValueLocal && other.TargetNamespace != String.Empty)
					return false;
				if (this.HasValueTargetNamespace && other.TargetNamespace != this.TargetNamespace)
					return false;
				return this.ValidateWildcardAllowsNamespaceName (other.TargetNamespace, h, schema, false);
			}
			// 4.
			if (this.ResolvedNamespaces.Count > 0) {
				foreach (string ns in this.ResolvedNamespaces)
					if (other.ResolvedNamespaces.Contains (ns))
						return false;
			}
			return true;
		}

		// 3.10.4 Wildcard Allows Namespace Name. (In fact it is almost copy...)
		public bool ValidateWildcardAllowsNamespaceName (string ns,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			if (HasValueAny)
				return true;
			if (HasValueOther && ns != TargetNamespace)
				return true;
			if (HasValueTargetNamespace && ns == TargetNamespace)
				return true;
			if (HasValueLocal && ns == "")
				return true;
			foreach (string rns in ResolvedNamespaces)
				if (ns == rns)
					return true;
			if (raiseError)
				xsobj.error (h, "This wildcard does not allow the namespace: " + ns);
			return false;
		}

		// 3.10.6 Wildcard Subset
		// Other = wider. this = restricted subset
		internal void ValidateWildcardSubset (XsdWildcard other,
			ValidationEventHandler h, XmlSchema schema)
		{
			// 1.
			if (other.HasValueAny)
				return;
			if (HasValueOther && other.HasValueOther) {
					// 2.1 and 2.2
					if (TargetNamespace == other.TargetNamespace ||
						other.TargetNamespace == null || other.TargetNamespace == "")
						return;
			}
			// 3.1.
			if (this.HasValueAny)
				xsobj.error (h, "Invalid wildcard subset was found.");
			// 3.2
			if (other.HasValueOther) {
				// 3.2.2
				if (other.TargetNamespace == null || other.TargetNamespace == String.Empty)
					return;
				else {
					foreach (string ns in ResolvedNamespaces)
						if (ns == other.TargetNamespace) {
							xsobj.error (h, "Invalid wildcard subset was found.");
							return;
						}
				}
			} else {
				// 3.2.1
				if (!other.HasValueLocal && HasValueLocal) {
					xsobj.error (h, "Invalid wildcard subset was found.");
					return;
				} else if (ResolvedNamespaces.Count == 0) {
					return;
				} else {
					foreach (string ns in ResolvedNamespaces)
						if (!other.ResolvedNamespaces.Contains (ns)) {
							xsobj.error (h, "Invalid wildcard subset was found.");
							return;
						}
				}
			}
		}
	}
}
