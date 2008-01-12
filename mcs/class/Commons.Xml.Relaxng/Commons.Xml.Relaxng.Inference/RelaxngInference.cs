//
// RelaxngInference.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2005 Novell Inc.
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
using System.Xml;
using System.Xml.Schema;
using Commons.Xml.Relaxng;

using QName = System.Xml.XmlQualifiedName;


namespace Commons.Xml.Relaxng.Inference
{
	public class RelaxngInference
	{
		public enum InferenceOption {
			Restricted,
			Relaxed,
		}

		InferenceOption occurrence = InferenceOption.Restricted;
		InferenceOption typeInference = InferenceOption.Restricted;

		public RelaxngInference ()
		{
		}

		public InferenceOption Occurrence {
			get { return occurrence; }
			set { occurrence = value; }
		}

		public InferenceOption TypeInference {
			get { return typeInference; }
			set { typeInference = value; }
		}

		public RelaxngGrammar InferSchema (XmlReader xmlReader)
		{
			return InferSchema (xmlReader, new RelaxngGrammar ());
		}

		public RelaxngGrammar InferSchema (XmlReader xmlReader,
			RelaxngGrammar grammar)
		{
			return RngInference.Process (xmlReader, grammar,
				occurrence == InferenceOption.Relaxed,
				typeInference == InferenceOption.Relaxed);
		}
	}

	class RngInference
	{
		public static RelaxngGrammar Process (XmlReader xmlReader, 
			RelaxngGrammar grammar,
			bool laxOccurence,
			bool laxTypeInference)
		{
			RngInference impl = new RngInference (xmlReader,
				grammar, laxOccurence, laxTypeInference);
			impl.Run ();
			return impl.grammar;
		}

		public const string NamespaceXml =
			"http://www.w3.org/XML/1998/namespace";

		public const string NamespaceXmlns =
			"http://www.w3.org/2000/xmlns/";

		public const string NamespaceXmlSchemaDatatypes =
			"http://www.w3.org/2001/XMLSchema-datatypes";

		public const string XdtNamespace =
			"http://www.w3.org/2003/11/xpath-datatypes";

		public const string NamespaceXmlSchema =
			System.Xml.Schema.XmlSchema.Namespace;

		static readonly QName QNameString = new QName (
			"string", NamespaceXmlSchema);

		static readonly QName QNameBoolean = new QName (
			"boolean", NamespaceXmlSchema);

		static readonly QName QNameAnyType = new QName (
			"anyType", NamespaceXmlSchema);

		static readonly QName QNameByte = new QName (
			"byte", NamespaceXmlSchema);

		static readonly QName QNameUByte = new QName (
			"unsignedByte", NamespaceXmlSchema);

		static readonly QName QNameShort = new QName (
			"short", NamespaceXmlSchema);

		static readonly QName QNameUShort = new QName (
			"unsignedShort", NamespaceXmlSchema);

		static readonly QName QNameInt = new QName (
			"int", NamespaceXmlSchema);

		static readonly QName QNameUInt = new QName (
			"unsignedInt", NamespaceXmlSchema);

		static readonly QName QNameLong = new QName (
			"long", NamespaceXmlSchema);

		static readonly QName QNameULong = new QName (
			"unsignedLong", NamespaceXmlSchema);

		static readonly QName QNameDecimal = new QName (
			"decimal", NamespaceXmlSchema);

		static readonly QName QNameUDecimal = new QName (
			"unsignedDecimal", NamespaceXmlSchema);

		static readonly QName QNameDouble = new QName (
			"double", NamespaceXmlSchema);

		static readonly QName QNameFloat = new QName (
			"float", NamespaceXmlSchema);

		static readonly QName QNameDateTime = new QName (
			"dateTime", NamespaceXmlSchema);

		static readonly QName QNameDuration = new QName (
			"duration", NamespaceXmlSchema);

		XmlReader source;
		RelaxngGrammar grammar;
		bool laxOccurence;
		bool laxTypeInference;

		Hashtable elements = new Hashtable ();
		Hashtable attributes = new Hashtable ();
		XmlNamespaceManager nsmgr;

		private RngInference (XmlReader xmlReader, 
			RelaxngGrammar grammar, 
			bool laxOccurence, 
			bool laxTypeInference)
		{
			this.source = xmlReader;
			this.grammar = grammar;
			this.laxOccurence = laxOccurence;
			this.laxTypeInference = laxTypeInference;
			nsmgr = new XmlNamespaceManager (source.NameTable);

			foreach (RelaxngDefine def in grammar.Defines) {
				if (def.Patterns.Count != 1)
					continue;
				RelaxngElement e = def.Patterns [0] as RelaxngElement;
				RelaxngAttribute a = def.Patterns [0] as RelaxngAttribute;
				if (e == null && a == null)
					continue;
				RelaxngName rn = e != null ?
					e.NameClass as RelaxngName :
					a.NameClass as RelaxngName;
				if (rn == null)
					continue;
				QName qname = new QName (rn.LocalName,
					rn.Namespace);
				if (e != null)
					elements.Add (qname, def);
				else
					attributes.Add (qname, def);
			}
		}

		private void Run ()
		{
			// move to top-level element
			source.MoveToContent ();
			int depth = source.Depth;
			if (source.NodeType != XmlNodeType.Element)
				throw new ArgumentException ("Argument XmlReader content is expected to be an element.");

			QName qname = new QName (source.LocalName,
				source.NamespaceURI);
			RelaxngDefine el = GetGlobalElement (qname);
			if (el == null) {
				el = CreateGlobalElement (qname);
				InferElement (el, true);
			}
			else
				InferElement (el, false);
			RelaxngStart start = new RelaxngStart ();
			start.Combine = "choice";
			RelaxngRef topRef = new RelaxngRef ();
			topRef.Name = el.Name;
			start.Pattern = topRef;
			grammar.Starts.Add (start);
		}

		private void InferElement (RelaxngRef r, bool isNew)
		{
			RelaxngDefine body = GetDefine (r.Name);
			InferElement (body, isNew);
		}

		private void InferElement (RelaxngDefine el, bool isNew)
		{
			RelaxngElement ct = (RelaxngElement) el.Patterns [0];

			// Attributes
			if (source.MoveToFirstAttribute ()) {
				InferAttributes (ct, isNew);
				source.MoveToElement ();
			}

			// Content
			if (source.IsEmptyElement) {
				InferAsEmptyElement (ct, isNew);
				source.Read ();
				source.MoveToContent ();
			}
			else {
				InferContent (ct, isNew);
				source.ReadEndElement ();
			}
			if (GetElementContent (ct) == null)
				el.Patterns.Add (new RelaxngEmpty ());
		}

		#region Attribute Inference

		// get attribute definition table.
		private Hashtable CollectAttrTable (RelaxngInterleave attList)
		{
			Hashtable table = new Hashtable ();
			if (attList == null)
				return table;
			foreach (RelaxngPattern p in attList.Patterns) {
				RelaxngAttribute a = p as RelaxngAttribute;
				if (a == null)
					a = (RelaxngAttribute)
						((RelaxngOptional) p)
						.Patterns [0];
				RelaxngName rn = a.NameClass as RelaxngName;
				table.Add (new QName (
					rn.LocalName, rn.Namespace),
					a);
			}
			return table;
		}

		private void InferAttributes (RelaxngElement ct, bool isNew)
		{
			RelaxngInterleave attList = null;
			Hashtable table = null;

			do {
				if (source.NamespaceURI == NamespaceXmlns)
					continue;

				if (table == null) {
					attList = GetAttributes (ct);
					table = CollectAttrTable (attList);
				}
				QName attrName = new QName (
					source.LocalName, source.NamespaceURI);
				RelaxngPattern attr = table [attrName]
					as RelaxngPattern;
				if (attr == null) {
					if (attList == null) {
						attList = new RelaxngInterleave ();
						ct.Patterns.Insert (0, attList);
					}
					attList.Patterns.Add (
						InferNewAttribute (
						attrName, isNew));
				} else {
					table.Remove (attrName);
					if (attrName.Namespace.Length > 0) {
						RelaxngDefine ga = GetGlobalAttribute (attrName);
						InferMergedAttribute (
							ga.Patterns [0]);
					}
					else
						InferMergedAttribute (attr);
				}
			} while (source.MoveToNextAttribute ());

			// mark all attr definitions that did not appear
			// as optional.
			if (table != null) {
				foreach (RelaxngPattern attr in table.Values) {
					if (attr is RelaxngOptional)
						continue;
					attList.Patterns.Remove (attr);
					RelaxngOptional opt = new RelaxngOptional ();
					opt.Patterns.Add (attr);
					attList.Patterns.Add (opt);
				}
			}
		}

		// It returns RelaxngAttribute for local attribute, and
		// RelaxngRef for global attribute.
		private RelaxngPattern InferNewAttribute (
			QName attrName, bool isNewTypeDefinition)
		{
			RelaxngPattern p = null;
			bool mergedRequired = false;
			if (attrName.Namespace.Length > 0) {
				// global attribute; might be already defined.
				// (Actually RELAX NG has no concept of "global
				// attributes" but it is still useful to
				// represent attributes in global scope.
				RelaxngDefine attr = GetGlobalAttribute (
					attrName);
				if (attr == null) {
					attr = CreateGlobalAttribute (attrName);
					attr.Patterns.Add (CreateSimplePattern (
						InferSimpleType (source.Value)));
				} else {
					RelaxngAttribute a = attr.Patterns [0] as RelaxngAttribute;
					if (a != null)
						mergedRequired = true;
					else {
						RelaxngOptional opt =
							(RelaxngOptional) attr.Patterns [0];
						a = (RelaxngAttribute) opt.Patterns [0];
					}
					InferMergedAttribute (a);
				}
				RelaxngRef r = new RelaxngRef ();
				r.Name = attr.Name;
				p = r;
			} else {
				// local attribute
				RelaxngAttribute a = new RelaxngAttribute ();
				a.NameClass = new RelaxngName (
					attrName.Name, attrName.Namespace);
				a.Pattern = CreateSimplePattern (
					InferSimpleType (source.Value));
				p = a;
			}
			// optional
			if (laxOccurence ||
				(!isNewTypeDefinition && !mergedRequired)) {
				RelaxngOptional opt = new RelaxngOptional ();
				opt.Patterns.Add (p);
				p = opt;
			}

			return p;
		}

		// validate string value agains attr and 
		// if invalid, then relax the type.
		private void InferMergedAttribute (RelaxngPattern ap)
		{
			switch (ap.PatternType) {
			case RelaxngPatternType.Ref:
				string refName = ((RelaxngRef) ap).Name;
				RelaxngDefine def = GetDefine (refName);
				InferMergedAttribute (def.Patterns [0]);
				return;
			case RelaxngPatternType.Optional:
				InferMergedAttribute (
					((RelaxngOptional) ap).Patterns [0]);
				return;
			}

			RelaxngAttribute attr = (RelaxngAttribute) ap;

			RelaxngPattern p = attr.Pattern;
			if (p is RelaxngText)
				return; // We could do nothing anymore.
			if (p is RelaxngEmpty) {
				if (source.Value.Length == 0)
					return; // We can keep empty.
				// We still could infer a choice of empty and
				// data, but it's being too complicated. So
				// here we just set text.
				attr.Pattern = new RelaxngText ();
				return;
			}
			RelaxngData data = p as RelaxngData;
			if (data == null)
				throw Error (p, "This inference implementation only allows text, empty and data for an attribute.");
			attr.Pattern = CreateSimplePattern (
				InferMergedType (source.Value,
				new QName (data.Type, data.DatatypeLibrary)));
		}

		private QName InferMergedType (string value, QName typeName)
		{
#if NET_2_0
			// examine value against specified type and
			// if unacceptable, then return a relaxed type.

			XmlSchemaSimpleType st = XmlSchemaType.GetBuiltInSimpleType (
				typeName);
			if (st == null) // non-primitive type => see above.
				return QNameString;
			do {
				try {
					st.Datatype.ParseValue (value,
						source.NameTable,
						source as IXmlNamespaceResolver);
					return typeName;
				} catch {
					st = st.BaseXmlSchemaType as XmlSchemaSimpleType;
					typeName = st != null ? st.QualifiedName : QName.Empty;
				}
			} while (typeName != QName.Empty);
#endif
			return QNameString;
		}

		private RelaxngInterleave GetAttributes (RelaxngElement el)
		{
			return el.Patterns.Count > 0 ?
				el.Patterns [0] as RelaxngInterleave : null;
		}

		#endregion

		#region Element Type

		private RelaxngPattern GetElementContent (RelaxngElement el)
		{
			if (el.Patterns.Count == 0)
				return null;
			RelaxngPattern p = el.Patterns [0];
			if (p is RelaxngInterleave)
				return el.Patterns.Count == 2 ?
					el.Patterns [1] : null;
			else
				return p;
		}

		private void InferAsEmptyElement (RelaxngElement ct, bool isNew)
		{
			RelaxngPattern content = GetElementContent (ct);
			if (content == null) {
				ct.Patterns.Add (new RelaxngEmpty ());
				return;
			}

			RelaxngGroup g = content as RelaxngGroup;
			if (g == null)
				return;
			RelaxngOptional opt = new RelaxngOptional ();
			opt.Patterns.Add (g);
			ct.Patterns.Remove (content);
			ct.Patterns.Add (opt);
		}

		private void InferContent (RelaxngElement ct, bool isNew)
		{
			source.Read ();
			source.MoveToContent ();
			switch (source.NodeType) {
			case XmlNodeType.EndElement:
				InferAsEmptyElement (ct, isNew);
				break;
			case XmlNodeType.Element:
				InferComplexContent (ct, isNew);
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
				InferTextContent (ct, isNew);
				source.MoveToContent ();
				if (source.NodeType == XmlNodeType.Element)
					goto case XmlNodeType.Element;
				break;
			case XmlNodeType.Whitespace:
				InferContent (ct, isNew); // skip and retry
				break;
			}
		}

		private void InferComplexContent (RelaxngElement ct, bool isNew)
		{
			bool makeMixed = false;
			RelaxngPattern content = GetElementContent (ct);
			if (content != null) {
				switch (content.PatternType) {
				case RelaxngPatternType.Text:
				case RelaxngPatternType.Data:
					makeMixed = true;
					ct.Patterns.Remove (content);
					ct.Patterns.Add (new RelaxngGroup ());
					break;
				}
			}
			else
				ct.Patterns.Add (new RelaxngGroup ());
			InferComplexContentCore (ct, isNew);
			if (makeMixed)
				MarkAsMixed (ct);
		}

		private void InferComplexContentCore (RelaxngElement ct,
			bool isNew)
		{
			int position = 0;
			bool consumed = false;

			do {
				switch (source.NodeType) {
				case XmlNodeType.Element:
					RelaxngPattern p =
						GetElementContent (ct);
					RelaxngGroup g = null;
					if (p == null)
						g = new RelaxngGroup ();
					switch (p.PatternType) {
					case RelaxngPatternType.OneOrMore:
					case RelaxngPatternType.ZeroOrMore:
						ProcessLax ((RelaxngSingleContentPattern) p);
						break;
					case RelaxngPatternType.Optional:
						g = (RelaxngGroup)
							((RelaxngOptional) p)
							.Patterns [0];
						goto default;
					case RelaxngPatternType.Group:
						g = (RelaxngGroup) p;
						goto default;
					case RelaxngPatternType.Text:
					case RelaxngPatternType.Data:
						g = new RelaxngGroup ();
						g.Patterns.Add (new RelaxngMixed ());
						goto default;
					default:
						if (g == null)
							throw Error (p, "Unexpected pattern: " + p.PatternType);
						ProcessSequence (ct, g,
							ref position,
							ref consumed,
							isNew);
						break;
					}
					source.MoveToContent ();
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					MarkAsMixed (ct);
					source.ReadString ();
					source.MoveToContent ();
					break;
				case XmlNodeType.EndElement:
					return; // finished
				case XmlNodeType.None:
					throw new NotImplementedException ("Internal Error: Should not happen.");
				}
			} while (true);
		}

		private void InferTextContent (RelaxngElement ct, bool isNew)
		{
			string value = source.ReadString ();
			RelaxngPattern p = GetElementContent (ct);
			if (p == null) {
				ct.Patterns.Add (CreateSimplePattern (
					InferSimpleType (value)));
				return;
			}
			RelaxngPatternList pl = null;
			switch (p.PatternType) {
			case RelaxngPatternType.Text:
			case RelaxngPatternType.Data:
				return; // no way to narrow it to data.
			case RelaxngPatternType.Empty:
				ct.Patterns.Remove (p);
				ct.Patterns.Add (new RelaxngText ());
				return;
			case RelaxngPatternType.Group:
				pl = ((RelaxngBinaryContentPattern) p).Patterns;
				break;
			case RelaxngPatternType.Optional:
			case RelaxngPatternType.ZeroOrMore:
			case RelaxngPatternType.OneOrMore:
				pl = ((RelaxngSingleContentPattern) p).Patterns;
				break;
			default:
				throw Error (p, "Unexpected pattern");
			}
			if (pl.Count > 0 && pl [0] is RelaxngMixed)
				return;
			RelaxngMixed m = new RelaxngMixed ();
			while (pl.Count > 0) {
				RelaxngPattern child = pl [0];
				m.Patterns.Add (child);
				pl.Remove (child);
			}
			pl.Add (m);
		}

		// Change pattern as to allow text content.
		private void MarkAsMixed (RelaxngElement ct)
		{
			RelaxngPattern p = GetElementContent (ct);
			// empty
			if (p == null || p is RelaxngEmpty) {
				if (p != null)
					ct.Patterns.Remove (p);
				ct.Patterns.Add (new RelaxngText ());
				return;
			}
			// text
			switch (p.PatternType) {
			case RelaxngPatternType.Text:
			case RelaxngPatternType.Data:
			case RelaxngPatternType.Mixed:
				return;
			case RelaxngPatternType.Choice:
			case RelaxngPatternType.Group:
				RelaxngBinaryContentPattern b =
					(RelaxngBinaryContentPattern) p;
				if (b != null) {
					RelaxngMixed m = b.Patterns [0]
						as RelaxngMixed;
					if (m == null) {
						m = new RelaxngMixed ();
						while (b.Patterns.Count > 0) {
							RelaxngPattern child =
								b.Patterns [0];
							m.Patterns.Add (child);
							b.Patterns.Remove (child);
						}
						b.Patterns.Add (m);
					}
				}
				break;
			default:
				throw Error (p, "Not allowed pattern.");
			}
		}

		#endregion

		#region Particles

		private void ProcessLax (RelaxngSingleContentPattern scp)
		{
			RelaxngChoice c = (RelaxngChoice) scp.Patterns [0];
			foreach (RelaxngPattern p in c.Patterns) {
				RelaxngRef el = p as RelaxngRef;
				if (el == null) {
					RelaxngOneOrMore oom =
						(RelaxngOneOrMore) p;
					el = (RelaxngRef) oom.Patterns [0];
				}
				if (el == null)
					throw Error (c, String.Format ("Target pattern contains unacceptable child pattern {0}. Only ref is allowed here."));
				if (ElementMatches (el)) {
					InferElement (el, false);
					return;
				}
			}
			// append a new element particle to lax term.
			QName qname = new QName (
				source.LocalName, source.NamespaceURI);
			RelaxngDefine def = GetGlobalElement (qname);
			if (def == null) {
				def = CreateGlobalElement (qname); // used to be CreateElement().
				InferElement (def, true);
			}
			else
				InferElement (def, false);
			RelaxngRef nel = new RelaxngRef ();
			nel.Name = def.Name;
			c.Patterns.Add (nel);
		}

		private bool ElementMatches (RelaxngRef el)
		{
			RelaxngDefine def = elements [new QName (
				source.LocalName, source.NamespaceURI)]
				as RelaxngDefine;
			return def != null && def.Name == el.Name;
		}

		private void ProcessSequence (RelaxngElement ct, RelaxngGroup s,
			ref int position, ref bool consumed,
			bool isNew)
		{
			RelaxngMixed m = s.Patterns.Count > 0 ? s.Patterns [0] as RelaxngMixed : null;
			RelaxngPatternList pl = m != null ?
				m.Patterns : s.Patterns;
			for (int i = 0; i < position; i++) {
				RelaxngPattern p = pl [i];
				RelaxngRef iel = p as RelaxngRef;
				if (iel == null) {
					RelaxngOneOrMore oom =
						p as RelaxngOneOrMore;
					iel = (RelaxngRef) oom.Patterns [0];
				}
				if (ElementMatches (iel)) {
					// Sequence element type violation
					// might happen (might not, but we
					// cannot backtrack here). So switch
					// to sequence of choice* here.
					ProcessLax (ToSequenceOfChoice (ct, s));
					return;
				}
			}

			if (pl.Count <= position) {
				QName name = new QName (source.LocalName,
					source.NamespaceURI);
				RelaxngDefine nel = GetGlobalElement (name);
				if (nel != null)
					InferElement (nel, false);
				else {
					nel = CreateGlobalElement (name); // used to be CreateElement().
					InferElement (nel, true);
				}
				RelaxngRef re = new RelaxngRef ();
				re.Name = nel.Name;
				pl.Add (re);
				consumed = true;
				return;
			}
			RelaxngPattern c = pl [position];
			RelaxngRef el = c as RelaxngRef;
			if (el == null) {
				RelaxngOneOrMore oom = c as RelaxngOneOrMore;
				el = (RelaxngRef) oom.Patterns [0];
			}
			if (el == null)
				throw Error (s, String.Format ("Target complex type content sequence has an unacceptable type of particle {0}", s.Patterns [position]));
			bool matches = ElementMatches (el);
			if (matches) {
				if (consumed && c is RelaxngRef) {
					RelaxngOneOrMore oom = new RelaxngOneOrMore ();
					oom.Patterns.Add (el);
					pl [position] = oom;
				}
				InferElement (el, false);
				source.MoveToContent ();
				switch (source.NodeType) {
				case XmlNodeType.None:
					if (source.NodeType ==
						XmlNodeType.Element)
						goto case XmlNodeType.Element;
					else if (source.NodeType ==
						XmlNodeType.EndElement)
						goto case XmlNodeType.EndElement;
					break;
				case XmlNodeType.Element:
					ProcessSequence (ct, s, ref position,
						ref consumed, isNew);
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					MarkAsMixed (ct);
					source.ReadString ();
					goto case XmlNodeType.None;
				case XmlNodeType.Whitespace:
					source.ReadString ();
					goto case XmlNodeType.None;
				case XmlNodeType.EndElement:
					return;
				default:
					source.Read ();
					break;
				}
			}
			else {
				if (consumed) {
					position++;
					consumed = false;
					ProcessSequence (ct, s,
						ref position, ref consumed,
						isNew);
				}
				else
					ProcessLax (ToSequenceOfChoice (ct, s));
			}
		}

		// Note that it does not return the changed sequence.
		private RelaxngSingleContentPattern ToSequenceOfChoice (
			RelaxngElement ct, RelaxngGroup s)
		{
			RelaxngSingleContentPattern scp =
				laxOccurence ?
				(RelaxngSingleContentPattern)
				new RelaxngZeroOrMore () :
				new RelaxngOneOrMore ();
			RelaxngChoice c = new RelaxngChoice ();
			foreach (RelaxngPattern p in s.Patterns)
				c.Patterns.Add (p);
			scp.Patterns.Add (c);
			ct.Patterns.Clear ();
			ct.Patterns.Add (scp);
			return scp;
		}

		#endregion

		#region String Value

		private RelaxngPattern CreateSimplePattern (QName typeName)
		{
			if (typeName == QNameString)
				return new RelaxngText ();

			RelaxngData data = new RelaxngData ();
			data.Type = typeName.Name;
			data.DatatypeLibrary =
				typeName.Namespace == NamespaceXmlSchema ?
				NamespaceXmlSchemaDatatypes :
				typeName.Namespace;
			return data;
		}

		// primitive type inference.
		// When running lax type inference, it just returns xs:string.
		private QName InferSimpleType (string value)
		{
			if (laxTypeInference)
				return QNameString;

			switch (value) {
			case "true":
			case "false":
				return QNameBoolean;
			}
			try {
				long dec = XmlConvert.ToInt64 (value);
				if (byte.MinValue <= dec && dec <= byte.MaxValue)
					return QNameUByte;
				if (sbyte.MinValue <= dec && dec <= sbyte.MaxValue)
					return QNameByte;
				if (ushort.MinValue <= dec && dec <= ushort.MaxValue)
					return QNameUShort;
				if (short.MinValue <= dec && dec <= short.MaxValue)
					return QNameShort;
				if (uint.MinValue <= dec && dec <= uint.MaxValue)
					return QNameUInt;
				if (int.MinValue <= dec && dec <= int.MaxValue)
					return QNameInt;
				return QNameLong;
			} catch (Exception) {
			}
			try {
				XmlConvert.ToUInt64 (value);
				return QNameULong;
			} catch (Exception) {
			}
			try {
				XmlConvert.ToDecimal (value);
				return QNameDecimal;
			} catch (Exception) {
			}
			try {
				double dbl = XmlConvert.ToDouble (value);
				if (float.MinValue <= dbl &&
					dbl <= float.MaxValue)
					return QNameFloat;
				else
					return QNameDouble;
			} catch (Exception) {
			}
			try {
				// FIXME: also try DateTimeSerializationMode
				// and gYearMonth
				XmlConvert.ToDateTime (value);
				return QNameDateTime;
			} catch (Exception) {
			}
			try {
				XmlConvert.ToTimeSpan (value);
				return QNameDuration;
			} catch (Exception) {
			}

			// xs:string
			return QNameString;
		}

		#endregion

		#region Utilities

		private RelaxngDefine GetDefine (string name)
		{
			foreach (RelaxngDefine def in grammar.Defines) {
				if (def.Name == name)
					return def;
			}
			return null;
		}

		private RelaxngDefine GetGlobalElement (QName name)
		{
			return elements [name] as RelaxngDefine;
		}

		private RelaxngDefine GetGlobalAttribute (QName name)
		{
			return attributes [name] as RelaxngDefine;
		}

		private string CreateUniqueName (string baseName)
		{
			string name = baseName;
			bool retry;
			do {
				retry = false;
				foreach (RelaxngDefine d in grammar.Defines) {
					if (d.Name == name) {
						name += "_";
						retry = true;
						break;
					}
				}
			} while (retry);
			return name;
		}

		// Already relaxed.
		private RelaxngDefine CreateGlobalElement (QName name)
		{
			RelaxngDefine def = new RelaxngDefine ();
			def.Name = CreateUniqueName (name.Name);
			RelaxngElement el = new RelaxngElement ();
			el.NameClass = new RelaxngName (name.Name,
				name.Namespace);
			def.Patterns.Add (el);
			elements.Add (name, def);
			grammar.Defines.Add (def);
			return def;
		}

		private RelaxngDefine CreateGlobalAttribute (QName name)
		{
			RelaxngDefine def = new RelaxngDefine ();
			def.Name = CreateUniqueName (name.Name + "-attr");
			RelaxngAttribute attr = new RelaxngAttribute ();
			attr.NameClass = new RelaxngName (
				name.Name, name.Namespace);
			def.Patterns.Add (attr);
			attributes.Add (name, def);
			grammar.Defines.Add (def);
			return def;
		}

		// FIXME: should create another type of RelaxngException.
		private RelaxngException Error (
			RelaxngElementBase sourceObj,
			string message)
		{
			// This override is mainly for schema component error.
			return Error (sourceObj, false, message);
		}

		private RelaxngException Error (
			RelaxngElementBase sourceObj,
			bool useReader,
			string message)
		{
			string msg = String.Concat (
				message,
				sourceObj != null ?
					String.Format (". Related schema component is {0} ({1}) line {2}, column {3}",
						sourceObj.BaseUri,
						sourceObj.GetType ().Name,
						sourceObj.LineNumber,
						sourceObj.LinePosition) :
					String.Empty,
				useReader ?
					String.Format (". {0}", source.BaseURI) :
					String.Empty);

			IXmlLineInfo li = source as IXmlLineInfo;
			if (useReader && li != null && li.HasLineInfo ())
				msg += String.Format (" line {0} column {1}",
					li.LineNumber, li.LinePosition);

			return new RelaxngException (msg);
		}

		#endregion
	}
}
