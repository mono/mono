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
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml.Schema
{
	internal class XsdWildcard
	{
		public XsdWildcard (XmlSchemaObject wildcard)
		{
			xsobj = wildcard;
		}

		private XmlSchemaObject xsobj;

		public XmlSchemaContentProcessing ResolvedProcessing;
		public string TargetNamespace;
		public bool SkipCompile;
		public bool HasValueAny;
		public bool HasValueLocal;
		public bool HasValueOther;
		public bool HasValueTargetNamespace;
		public StringCollection ResolvedNamespaces;

		void Reset ()
		{
			HasValueAny = false;
			HasValueLocal = false;
			HasValueOther = false;
			HasValueTargetNamespace = false;
			ResolvedNamespaces = new StringCollection ();
		}

		public void Compile (string nss,
			ValidationEventHandler h, XmlSchema schema)
		{
			if (SkipCompile)
				return; // used by XmlSchemaAny.AnyTypeContent.

			Reset ();
			int nscount = 0;
			string actualNamespace = nss == null ? "##any" : nss;
			string[] nslist = XmlSchemaUtil.SplitList(actualNamespace);
			for (int i = 0; i < nslist.Length; i++) {
				string ns = nslist [i];
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
				for (int i = 0; i < this.ResolvedNamespaces.Count; i++) {
					if (!other.ResolvedNamespaces.Contains (this.ResolvedNamespaces [i]))
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
				for (int i = 0; i < this.ResolvedNamespaces.Count; i++)
					if (other.ResolvedNamespaces.Contains (this.ResolvedNamespaces [i]))
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
			for (int i = 0; i < ResolvedNamespaces.Count; i++)
				if (ns == ResolvedNamespaces [i])
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
			ValidateWildcardSubset (other, h, schema, true);
		}

		internal bool ValidateWildcardSubset (XsdWildcard other,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			// 1.
			if (other.HasValueAny)
				return true;
			// 2.
			if (HasValueOther && other.HasValueOther) {
				// 2.1 and 2.2
				if (TargetNamespace == other.TargetNamespace ||
					other.TargetNamespace == null || other.TargetNamespace == "")
					return true;
			}
			// 3.1. (not)
			if (this.HasValueAny) {
				if (raiseError)
					xsobj.error (h, "Invalid wildcard subset was found.");
				return false;
			}
			// 3.2
			if (other.HasValueOther) {
				// 3.2.2
				if ( (this.HasValueTargetNamespace && other.TargetNamespace == this.TargetNamespace) ||
					(this.HasValueLocal && (other.TargetNamespace == null || other.TargetNamespace.Length == 0)) ) {
					if (raiseError)
						xsobj.error (h, "Invalid wildcard subset was found.");
					return false;
				} else {
					for (int i = 0; i < ResolvedNamespaces.Count; i++) {
						if (ResolvedNamespaces [i] == other.TargetNamespace) {
							if (raiseError)
								xsobj.error (h, "Invalid wildcard subset was found.");
							return false;
						}
					}
				}
			} else {
				// 3.2.1
				if ((this.HasValueLocal && !other.HasValueLocal) ||
					this.HasValueTargetNamespace && !other.HasValueTargetNamespace) {
					if (raiseError)
						xsobj.error (h, "Invalid wildcard subset was found.");
					return false;
				} else if (this.HasValueOther) {
					if (raiseError)
						xsobj.error (h, "Invalid wildcard subset was found.");
					return false;
				} else {
					for (int i = 0; i < this.ResolvedNamespaces.Count; i++)
						if (!other.ResolvedNamespaces.Contains (this.ResolvedNamespaces [i])) {
							if (raiseError)
								xsobj.error (h, "Invalid wildcard subset was found.");
							return false;
						}
				}
			}
			return true;
		}
	}
}
