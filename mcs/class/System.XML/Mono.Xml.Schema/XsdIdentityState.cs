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

			if (!(this.entry.KeySequence.SourceSchemaIdentity is XmlSchemaKeyref)) {
				for (int i = 0; i < entry.KeySequence.FinishedEntries.Count; i++) {
					XsdKeyEntry other = (XsdKeyEntry) entry.KeySequence.FinishedEntries [i];
					if (this.entry.CompareIdentity (other))
						return false;
				}
			}

			return true;
		}

		// It might throw Exception including other than XmlSchemaException (ReadTypedValue).
		internal XsdIdentityPath FieldMatches (ArrayList qnameStack, XsdValidatingReader reader)
		{
			for (int i = 0; i < field.Paths.Length; i++) {
				XsdIdentityPath path = field.Paths [i];
				if (FieldFound && (reader.Depth > this.FieldFoundDepth && this.FieldFoundPath == path))
					continue;

				// Only "." hits.
				if (path.OrderedSteps.Length == 0) {
					if (reader.Depth == entry.StartDepth)
						return path;
					else
						continue;
				}
				// It does not hit as yet (too shallow to hit).
				if (reader.Depth - entry.StartDepth < path.OrderedSteps.Length - 1)
					continue;

				bool isAttributePath = false;
				int iter = path.OrderedSteps.Length;
				if (path.OrderedSteps [iter-1].IsAttribute) {
					isAttributePath = true;
					iter--;
				}
				if (path.Descendants && reader.Depth < entry.StartDepth + iter)
					continue;
				else if (!path.Descendants && reader.Depth != entry.StartDepth + iter)
					continue;

				iter--;

				XsdIdentityStep step;
				for (; iter >= 0; iter--) {
					step = path.OrderedSteps [iter];
					if (step.IsAnyName)
						continue;
					XmlQualifiedName qname = (XmlQualifiedName) qnameStack [entry.StartDepth + iter + 1];
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
				if (!isAttributePath)
					return path;
				step = path.OrderedSteps [path.OrderedSteps.Length - 1];
				if (step.IsAnyName || step.NsName != null) {
					try {
						while (reader.MoveToNextAttribute ()) {
							if (reader.NamespaceURI == XmlSchema.InstanceNamespace)
								continue;
							if (step.IsAnyName || reader.NamespaceURI == step.NsName) {
								this.FillAttributeFieldValue (reader);
							}
						}
					} finally {
						reader.MoveToElement ();
					}
					if (this.Identity != null)
						return path;
					else
						continue;
				}
				if (reader.MoveToAttribute (step.Name, step.Namespace)) {
					this.FillAttributeFieldValue (reader);
					reader.MoveToElement ();
					return path;
				}
				else
					continue;
			}
			return null;
		}

		private void FillAttributeFieldValue (XsdValidatingReader reader)
		{
			if (this.FieldFound)
				throw new XmlSchemaException ("The key value was was already found."
					+ (this.FieldHasLineInfo ?
						String.Format (CultureInfo.InvariantCulture, " At line {0}, position {1}.", FieldLineNumber, FieldLinePosition) :
						""),
					reader, reader.BaseURI, entry.KeySequence.SourceSchemaIdentity, null);
			XmlSchemaDatatype dt = reader.SchemaType as XmlSchemaDatatype;
			XmlSchemaSimpleType st = reader.SchemaType as XmlSchemaSimpleType;
			if (dt == null && st != null)
				dt = st.Datatype;
			object identity = XmlSchemaUtil.ReadTypedValue (reader, reader.SchemaType, reader.ParserContext.NamespaceManager, null);
			if (identity == null)
				identity = reader.Value;
			if (!this.SetIdentityField (identity, reader.Depth - 1 == reader.XsiNilDepth , dt as XsdAnySimpleType, reader.Depth, (IXmlLineInfo) reader))
				throw new XmlSchemaException ("Two or more identical field was found.", 
					reader, reader.BaseURI, entry.KeySequence.SourceSchemaIdentity, null);
			// HACK: This is not logical. Attributes never be cosuming,
			// so I used it as a temporary mark to sign it is *just* validated now.
			this.Consuming = true;
			this.FieldFound = true;
		}
	}

	internal class XsdKeyEntryFieldCollection : IList
	{
		ArrayList al = new ArrayList ();

		#region IList
		public bool IsReadOnly {
			get { return false; }
		}

		object IList.this [int i] {
			get { return al [i]; }
			set { al [i] = value; }
		}

		public XsdKeyEntryField this [int i] {
			get { return (XsdKeyEntryField) al [i]; }
			set { al [i] = value; }
		}

		public void RemoveAt (int i)
		{
			al.RemoveAt (i);
		}

		public void Insert (int i, object value)
		{
			al.Insert (i, value);
		}

		public void Remove (object value)
		{
			al.Remove (value);
		}

		public bool Contains (object value)
		{
			return al.Contains (value);
		}

		public void Clear ()
		{
			al.Clear ();
		}

		public int IndexOf (object value)
		{
			return al.IndexOf (value);
		}

		public int Add (object value)
		{
			return al.Add (value);
		}

		public bool IsFixedSize {
			get { return false; }
		}

		#endregion

		#region ICollection

		public bool IsSynchronized {
			get { return al.IsSynchronized; }
		}

		public int Count {
			get { return al.Count; }
		}

		public void CopyTo(Array array, int i)
		{
			al.CopyTo (array, i);
		}

		public object SyncRoot {
			get { return al.SyncRoot; }
		}

		#endregion

		#region IEnumerable

		public IEnumerator GetEnumerator ()
		{
			return ((IEnumerable) al).GetEnumerator ();
		}

		#endregion

	}

	// Created per field/key pair, created per selector-matched element.
	internal class XsdKeyEntry
	{
		public int StartDepth;

//		public bool ConsumptionTargetIsKey;

		public int SelectorLineNumber;
		public int SelectorLinePosition;
		public bool SelectorHasLineInfo;

		public XsdKeyEntryFieldCollection KeyFields;

		public bool KeyRefFound;
//		public int KeyRefSelectorLineNumber;
//		public int KeyRefSelectorLinePosition;
//		public bool KeyRefSelectorHasLineInfo;

		public XsdKeyTable KeySequence;
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
			KeySequence = keyseq;
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
				if (!XmlSchemaUtil.IsSchemaDatatypeEquals (
					of.FieldType, of.Identity, f.FieldType, f.Identity))
					return false;	// does not match
			}
			return true;	// matches
		}

		// In this method, attributes are ignored.
		// It might throw Exception including non-XmlSchemaException.
		public void FieldMatches (ArrayList qnameStack, XsdValidatingReader reader)
		{
			for (int i = 0; i < KeyFields.Count; i++) {
				XsdKeyEntryField keyField = KeyFields [i];
				XsdIdentityPath path = keyField.FieldMatches (qnameStack, reader);
				if (path != null) {
					if (keyField.FieldFound) {
						// HACK: This is not logical by nature. Attributes never be cosuming,
						// so I used it as a temporary mark to sign it is *just* validated now.
						if (!keyField.Consuming)
							throw new XmlSchemaException ("Two or more matching field was found.",
								reader, reader.BaseURI, this.KeySequence.SourceSchemaIdentity, null);
						else
							keyField.Consuming = false;
					}
					if (!keyField.Consumed) {
						if (reader.XsiNilDepth == reader.Depth && !keyField.SetIdentityField (Guid.Empty, true, XsdAnySimpleType.Instance, reader.Depth, (IXmlLineInfo) reader))
							throw new XmlSchemaException ("Two or more identical field was found.", reader, reader.BaseURI, KeySequence.SourceSchemaIdentity, null);
						else {
							XmlSchemaComplexType ct = reader.SchemaType as XmlSchemaComplexType;
							if (ct != null && 
								(ct.ContentType == XmlSchemaContentType.Empty || ct.ContentType == XmlSchemaContentType.ElementOnly) && 
								reader.SchemaType != XmlSchemaComplexType.AnyType)
								throw new XmlSchemaException ("Specified schema type is complex type, which is not allowed for identity constraints.", reader, reader.BaseURI, KeySequence.SourceSchemaIdentity, null);
							keyField.FieldFound = true;
							keyField.FieldFoundPath = path;
							keyField.FieldFoundDepth = reader.Depth;
							keyField.Consuming = true;
							IXmlLineInfo li = reader as IXmlLineInfo;
							if (li != null && li.HasLineInfo ()) {
								keyField.FieldHasLineInfo = true;
								keyField.FieldLineNumber = li.LineNumber;
								keyField.FieldLinePosition = li.LinePosition;
							}
							reader.CurrentKeyFieldConsumers.Add (keyField);
						}
					}
				}
			}
		}
	}
}
