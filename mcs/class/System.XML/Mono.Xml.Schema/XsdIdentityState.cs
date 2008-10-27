//
// Mono.Xml.Schema.XsdIdentityState.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
// These classses represents XML Schema's identity constraints validation state,
// created by xs:key, xs:keyref, xs:unique in xs:element.
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
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
using ValException = System.Xml.Schema.XmlSchemaValidationException;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
using ValException = System.Xml.Schema.XmlSchemaException;
#endif

namespace Mono.Xml.Schema
{
	internal class XsdKeyEntryField
	{
		XsdKeyEntry entry;
		XsdIdentityField field;

		public XsdKeyEntryField (XsdKeyEntry entry, XsdIdentityField field)
		{
			this.entry = entry;
			this.field = field;
		}

		public XsdIdentityField Field {
			get { return field; }
		}

		public bool FieldFound;
		public int FieldLineNumber;
		public int FieldLinePosition;
		public bool FieldHasLineInfo;
		public XsdAnySimpleType FieldType;

		public object Identity;
		public bool IsXsiNil;

		public int FieldFoundDepth;
		public XsdIdentityPath FieldFoundPath;

		public bool Consuming;
		public bool Consumed;

		// Return false if there is already the same key.
		public bool SetIdentityField (object identity, bool isXsiNil, XsdAnySimpleType type, int depth, IXmlLineInfo li)
		{
			FieldFoundDepth = depth;
			Identity = identity;
			IsXsiNil = isXsiNil;
			FieldFound |= isXsiNil;
			FieldType = type;
			Consuming = false;
			Consumed = true;
			if (li != null && li.HasLineInfo ()) {
				FieldHasLineInfo = true;
				FieldLineNumber = li.LineNumber;
				FieldLinePosition = li.LinePosition;
			}

			if (!(this.entry.OwnerSequence.SourceSchemaIdentity is XmlSchemaKeyref)) {
				for (int i = 0; i < entry.OwnerSequence.FinishedEntries.Count; i++) {
					XsdKeyEntry other = (XsdKeyEntry) entry.OwnerSequence.FinishedEntries [i];
					if (this.entry.CompareIdentity (other))
						return false;
				}
			}

			return true;
		}

		// if matchesAttr then check attributes; otherwise check elements.
		internal XsdIdentityPath Matches (bool matchesAttr, object sender, XmlNameTable nameTable, ArrayList qnameStack, string sourceUri, object schemaType, NSResolver nsResolver, IXmlLineInfo lineInfo, int depth, string attrName, string attrNS, object attrValue)
		{
			XsdIdentityPath matchedAttrPath = null;

			for (int i = 0; i < field.Paths.Length; i++) {
				XsdIdentityPath path = field.Paths [i];
				bool isAttribute = path.IsAttribute;
				if (matchesAttr != isAttribute)
					continue;
				XsdIdentityStep step;
				if (path.IsAttribute) {
					step = path.OrderedSteps [path.OrderedSteps.Length - 1];
					bool match = false;
					if (step.IsAnyName || step.NsName != null) {
						if (step.IsAnyName || attrNS == step.NsName)
							match = true;
					}
					else if (step.Name == attrName && step.Namespace == attrNS)
						match = true;
					if (!match)
						continue;
					// first -1 is to reduce attr path step, next -1 is to reduce Attribute's depth in XmlReader.
					if (entry.StartDepth + (path.OrderedSteps.Length - 1) != depth - 1)
						continue; // matched at different nest level
					matchedAttrPath = path;
				}
				if (FieldFound && (depth > this.FieldFoundDepth && this.FieldFoundPath == path))
					continue; // don't return; other fields might hit errorneously.

				// Only "." hits.
				if (path.OrderedSteps.Length == 0) {
					if (depth == entry.StartDepth)
						return path;
					else
						continue;
				}
				// It does not hit as yet (too shallow to hit).
				if (depth - entry.StartDepth < path.OrderedSteps.Length - 1)
					continue;

				int iter = path.OrderedSteps.Length;
				if (isAttribute)
					iter--;
				if (path.Descendants && depth < entry.StartDepth + iter)
					continue;
				else if (!path.Descendants && depth != entry.StartDepth + iter)
					continue;

				iter--;

				for (; iter >= 0; iter--) {
					step = path.OrderedSteps [iter];
					if (step.IsCurrent || step.IsAnyName)
						continue;
					XmlQualifiedName qname = (XmlQualifiedName) qnameStack [entry.StartDepth + iter + (isAttribute ? 0 : 1)];
					if (step.NsName != null && qname.Namespace == step.NsName)
						continue;
					if ((step.Name == "*" || step.Name == qname.Name) &&
						step.Namespace == qname.Namespace)
						continue;
					else
						break;
				}
				if (iter >= 0)	// i.e. did not match against the path.
					continue;

				if (!matchesAttr)
					return path;
			}
			if (matchedAttrPath != null) {
				this.FillAttributeFieldValue (sender, nameTable, sourceUri, schemaType, nsResolver, attrValue, lineInfo, depth);
				if (this.Identity != null)
					return matchedAttrPath;
			}
			return null;
		}

		private void FillAttributeFieldValue (object sender, XmlNameTable nameTable, string sourceUri, object schemaType, NSResolver nsResolver, object identity, IXmlLineInfo lineInfo, int depth)
		{
			if (this.FieldFound)
				throw new ValException (String.Format ("The key value was already found as '{0}'{1}.", Identity,
					(this.FieldHasLineInfo ?
						String.Format (CultureInfo.InvariantCulture, " at line {0}, position {1}", FieldLineNumber, FieldLinePosition) :
						"")),
					sender, sourceUri, entry.OwnerSequence.SourceSchemaIdentity, null);
			XmlSchemaDatatype dt = schemaType as XmlSchemaDatatype;
			XmlSchemaSimpleType st = schemaType as XmlSchemaSimpleType;
			if (dt == null && st != null)
				dt = st.Datatype;
			try {
				if (!this.SetIdentityField (identity, false, dt as XsdAnySimpleType, depth, lineInfo))
					throw new ValException ("Two or more identical field was found.",
						sender, sourceUri, entry.OwnerSequence.SourceSchemaIdentity, null);
				// HACK: This is not logical. Attributes will never be "cosuming",
				// so I used it as a temporary mark to sign it is validated *just now*.
				this.Consuming = true;
				this.FieldFound = true;
			} catch (Exception ex) {
				throw new ValException ("Failed to read typed value.", sender, sourceUri, entry.OwnerSequence.SourceSchemaIdentity, ex);
			}
		}
	}

	internal class XsdKeyEntryFieldCollection : CollectionBase
	{
		public XsdKeyEntryField this [int i] {
			get { return (XsdKeyEntryField) List [i]; }
			set { List [i] = value; }
		}

		public int Add (XsdKeyEntryField value)
		{
			return List.Add (value);
		}
	}

	// Created per field/key pair, created per selector-matched element.
	internal class XsdKeyEntry
	{
		public int StartDepth;

		public int SelectorLineNumber;
		public int SelectorLinePosition;
		public bool SelectorHasLineInfo;

		public XsdKeyEntryFieldCollection KeyFields;

		public bool KeyRefFound;

		public XsdKeyTable OwnerSequence;
		private bool keyFound = false;

		public XsdKeyEntry (
			XsdKeyTable keyseq, int depth, IXmlLineInfo li)
		{
			Init (keyseq, depth, li);
		}

		public bool KeyFound {
			get {
				if (keyFound)
					return true;
				for (int i = 0; i < KeyFields.Count; i++) {
					XsdKeyEntryField kf = KeyFields [i];
					if (kf.FieldFound) {
						keyFound = true;
						return true;
					}
				}
				return false;
			}
		}

		private void Init (XsdKeyTable keyseq, int depth, IXmlLineInfo li)
		{
			OwnerSequence = keyseq;
			KeyFields = new XsdKeyEntryFieldCollection ();
			for (int i = 0; i < keyseq.Selector.Fields.Length; i++)
				KeyFields.Add (new XsdKeyEntryField (this, keyseq.Selector.Fields [i]));
			StartDepth = depth;
			if (li != null) {
				if (li.HasLineInfo ()) {
					this.SelectorHasLineInfo = true;
					this.SelectorLineNumber = li.LineNumber;
					this.SelectorLinePosition = li.LinePosition;
				}
			}
		}

		public bool CompareIdentity (XsdKeyEntry other)
		{
			for (int i = 0; i < KeyFields.Count; i++) {
				XsdKeyEntryField f = this.KeyFields [i];
				XsdKeyEntryField of = other.KeyFields [i];
				if (f.IsXsiNil && !of.IsXsiNil || !f.IsXsiNil && of.IsXsiNil)
					return false;
				if (!XmlSchemaUtil.AreSchemaDatatypeEqual (
					of.FieldType, of.Identity, f.FieldType, f.Identity))
					return false;	// does not match
			}
			return true;	// matches
		}

		// In this method, attributes are ignored.
		// It might throw Exception.
		public void ProcessMatch (bool isAttribute, ArrayList qnameStack, object sender, XmlNameTable nameTable, string sourceUri, object schemaType, NSResolver nsResolver, IXmlLineInfo li, int depth, string attrName, string attrNS, object attrValue, bool isXsiNil, ArrayList currentKeyFieldConsumers)
		{
			for (int i = 0; i < KeyFields.Count; i++) {
				XsdKeyEntryField keyField = KeyFields [i];
				XsdIdentityPath path = keyField.Matches (isAttribute, sender, nameTable, qnameStack, sourceUri, schemaType, nsResolver, li, depth, attrName, attrNS, attrValue);
				if (path == null)
					continue;

				if (keyField.FieldFound) {
					// HACK: This is not logical by nature. Attributes never be cosuming,
					// so I used it as a temporary mark to sign it is *just* validated now.
					if (!keyField.Consuming)
						throw new ValException ("Two or more matching field was found.",
							sender, sourceUri, this.OwnerSequence.SourceSchemaIdentity, null);
					else
						keyField.Consuming = false;
				}
				if (keyField.Consumed) 
					continue;

				if (isXsiNil && !keyField.SetIdentityField (Guid.Empty, true, XsdAnySimpleType.Instance, depth, li))
					throw new ValException ("Two or more identical field was found.", sender, sourceUri, OwnerSequence.SourceSchemaIdentity, null);
				XmlSchemaComplexType ct = schemaType as XmlSchemaComplexType;
				if (ct != null && 
					(ct.ContentType == XmlSchemaContentType.Empty || ct.ContentType == XmlSchemaContentType.ElementOnly) && 
					schemaType != XmlSchemaComplexType.AnyType)
					throw new ValException ("Specified schema type is complex type, which is not allowed for identity constraints.", sender, sourceUri, OwnerSequence.SourceSchemaIdentity, null);
				keyField.FieldFound = true;
				keyField.FieldFoundPath = path;
				keyField.FieldFoundDepth = depth;
				keyField.Consuming = true;
				if (li != null && li.HasLineInfo ()) {
					keyField.FieldHasLineInfo = true;
					keyField.FieldLineNumber = li.LineNumber;
					keyField.FieldLinePosition = li.LinePosition;
				}
				currentKeyFieldConsumers.Add (keyField);
			}
		}
	}
}
