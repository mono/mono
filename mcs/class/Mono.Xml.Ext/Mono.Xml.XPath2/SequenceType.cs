//
// SequenceType.cs - represents XPath 2.0 item type
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Query;
using System.Xml.XPath;
using Mono.Xml;

namespace Mono.Xml.XPath2
{
	public class SequenceType
	{
		static SequenceType singleItem = new SequenceType (ItemType.AnyItem, Occurence.One);
		static SequenceType singleAnyAtomic = new SequenceType (ItemType.AnyAtomicType, Occurence.One);

		internal static SequenceType AnyType {
			get { return Create (InternalPool.XsAnyType, Occurence.ZeroOrMore); }
		}
		internal static SequenceType SingleItem {
			get { return singleItem; }
		}
		internal static SequenceType SingleAnyAtomic {
			get { return singleAnyAtomic; }
		}

		internal static SequenceType Node {
			get { return Create (XmlTypeCode.Node, Occurence.One); }
		}
		internal static SequenceType Document {
			get { return Create (XmlTypeCode.Document, Occurence.One); }
		}
		internal static SequenceType Element {
			get { return Create (XmlTypeCode.Element, Occurence.One); }
		}
		internal static SequenceType Attribute {
			get { return Create (XmlTypeCode.Attribute, Occurence.One); }
		}
		internal static SequenceType Namespace {
			get { return Create (XmlTypeCode.Namespace, Occurence.One); }
		}
		internal static SequenceType Text {
			get { return Create (XmlTypeCode.Text, Occurence.One); }
		}
		internal static SequenceType XmlPI {
			get { return Create (XmlTypeCode.ProcessingInstruction, Occurence.One); }
		}
		internal static SequenceType Comment {
			get { return Create (XmlTypeCode.Comment, Occurence.One); }
		}

		internal static SequenceType AtomicString {
			get { return Create (InternalPool.XsString, Occurence.One); }
		}
		internal static SequenceType Boolean {
			get { return Create (InternalPool.XsBoolean, Occurence.One); }
		}
		internal static SequenceType Decimal {
			get { return Create (InternalPool.XsDecimal, Occurence.One); }
		}
		internal static SequenceType Integer {
			get { return Create (InternalPool.XsInteger, Occurence.One); }
		}
		internal static SequenceType Int {
			get { return Create (InternalPool.XsInt, Occurence.One); }
		}
		internal static SequenceType Short {
			get { return Create (InternalPool.XsShort, Occurence.One); }
		}
		internal static SequenceType UnsignedInt {
			get { return Create (InternalPool.XsUnsignedInt, Occurence.One); }
		}
		internal static SequenceType UnsignedShort {
			get { return Create (InternalPool.XsUnsignedShort, Occurence.One); }
		}
		internal static SequenceType Double {
			get { return Create (InternalPool.XsDouble, Occurence.One); }
		}
		internal static SequenceType Single {
			get { return Create (InternalPool.XsFloat, Occurence.One); }
		}
		internal static SequenceType DateTime {
			get { return Create (InternalPool.XsDateTime, Occurence.One); }
		}
		internal static SequenceType QName {
			get { return Create (InternalPool.XsQName, Occurence.One); }
		}

		internal static SequenceType IntegerList {
			get { return Create (XmlTypeCode.Integer, Occurence.ZeroOrMore); }
		}


		static Hashtable standardTypes = new Hashtable ();

		internal static SequenceType Create (Type cliType)
		{
			// typed Array
			if (cliType.IsArray)
				return Create (InternalPool.XmlTypeCodeFromRuntimeType (cliType.GetElementType (), true), Occurence.ZeroOrMore);
//			if (cliType.GetInterface ("System.Collections.IEnumerable") != null)
//				return Create (XmlTypeCode.Item, Occurence.ZeroOrMore);
			if (cliType == typeof (XmlQualifiedName))
				return QName;
			if (cliType == typeof (XPathNavigator) || cliType.IsSubclassOf (typeof (XPathNavigator)))
				return Node;
			if (cliType == typeof (XPathAtomicValue))
				return SingleAnyAtomic;
			if (cliType == typeof (XPathItem))
				return SingleItem;
			// FIXME: handle Nullable type
			return Create (InternalPool.XmlTypeCodeFromRuntimeType (cliType, true), Occurence.One);
		}

		internal static SequenceType Create (XmlTypeCode typeCode, Occurence occurence)
		{
			switch (typeCode) {
			case XmlTypeCode.Item:
			case XmlTypeCode.AnyAtomicType:
			case XmlTypeCode.Node:
			case XmlTypeCode.Document:
			case XmlTypeCode.Element:
			case XmlTypeCode.Attribute:
			case XmlTypeCode.ProcessingInstruction:
			case XmlTypeCode.Comment:
			case XmlTypeCode.Namespace:
			case XmlTypeCode.Text:
				return new SequenceType (new ItemType (typeCode), occurence);
			default:
				return Create (XmlSchemaType.GetBuiltInSimpleType (typeCode), occurence);
			}
		}

		internal static SequenceType Create (XmlSchemaType schemaType, Occurence occurence)
		{
			switch (schemaType.QualifiedName.Namespace) {
			case XmlSchema.Namespace:
			case InternalPool.XdtNamespace:
				break;
			default:
				return new SequenceType (schemaType, occurence);
			}

			Hashtable cacheForType = standardTypes [schemaType] as Hashtable;
			if (cacheForType == null) {
				cacheForType = new Hashtable ();
				standardTypes [schemaType] = cacheForType;
			} else {
				SequenceType type = cacheForType [occurence] as SequenceType;
				if (type != null)
					return type;
			}
			SequenceType t = new SequenceType (schemaType, occurence);
			cacheForType [occurence] = t;
			return t;
		}

		[MonoTODO]
		internal static SequenceType ComputeCommonBase (SequenceType t1, SequenceType t2)
		{
			// FIXME: implement
			// throw new NotImplementedException ();
			return SequenceType.AnyType;
		}

		internal static bool IsNumeric (XmlTypeCode code)
		{
			switch (code) {
			case XmlTypeCode.Decimal:
			case XmlTypeCode.Float:
			case XmlTypeCode.Double:
			case XmlTypeCode.Integer:
			case XmlTypeCode.NonPositiveInteger:
			case XmlTypeCode.NegativeInteger:
			case XmlTypeCode.Long:
			case XmlTypeCode.Int:
			case XmlTypeCode.Short:
			case XmlTypeCode.Byte:
			case XmlTypeCode.NonNegativeInteger:
			case XmlTypeCode.UnsignedLong:
			case XmlTypeCode.UnsignedInt:
			case XmlTypeCode.UnsignedShort:
			case XmlTypeCode.UnsignedByte:
			case XmlTypeCode.PositiveInteger:
				return true;
			}
			return false;
		}

		// Instance members

		private SequenceType (XmlSchemaType schemaType, Occurence occurence)
		{
			this.schemaType = schemaType;
			this.itemType = ItemType.AnyItem;
			this.occurence = occurence;
		}

		internal SequenceType (ItemType itemType, Occurence occurence)
		{
			this.schemaType = InternalPool.XsAnyType;
			this.itemType = itemType;
			this.occurence = occurence;
		}

		XmlSchemaType schemaType;
		Occurence occurence;
		ItemType itemType;

		public XmlSchemaType SchemaType {
			get { return schemaType; }
		}

		public ItemType ItemType {
			get { return itemType; }
		}

		public Occurence Occurence {
			get { return occurence; }
		}

		internal bool Matches (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal bool CanConvertTo (SequenceType other)
		{
			// FIXME: implement precisely
			return this == other;
			// throw new NotImplementedException ();
		}

		internal bool CanConvert (XPathSequence iter)
		{
			bool occured = false;
			bool onlyOnce = (occurence == Occurence.One || occurence == Occurence.Optional);
			bool required = (occurence == Occurence.One || occurence == Occurence.OneOrMore);
			foreach (XPathItem item in iter) {
				if (occured && onlyOnce)
					return false;
				if (!CanConvert (item))
					return false;
			}
			return occured || !required;
		}

		public bool CanConvert (XPathItem item)
		{
			throw new NotImplementedException ();
		}

		public bool IsInstance (XPathItem item)
		{
			throw new NotImplementedException ();
		}

		public XPathItem Convert (XPathItem item)
		{
			throw new NotImplementedException ();
		}

		public object ToRuntimeType (XPathSequence seq)
		{
			// FIXME: handle ZeroOrMore|OneOrMore
			switch (occurence) {
			case Occurence.One:
			case Occurence.Optional:
				if (!seq.MoveNext ())
					return null;
				XPathItem item = seq.Current;
				// FIXME: should check and reject two or 
				// more items??
				return item.TypedValue;
			}
			ArrayList al = new ArrayList ();
			while (seq.MoveNext ())
				al.Add (seq.Current.TypedValue);
			return al.ToArray (InternalPool.RuntimeTypeFromXmlTypeCode (schemaType.TypeCode));
//			return seq;
		}
	}

	public enum Occurence
	{
		One,
		Optional,
		ZeroOrMore,
		OneOrMore,
	}

	#region NodeTest

	public enum XPathAxisType
	{
		Child,
		Descendant,
		Attribute,
		Self,
		DescendantOrSelf,
		FollowingSibling,
		Following,
		Parent,
		Ancestor,
		PrecedingSibling,
		Preceding,
		AncestorOrSelf,
		Namespace // only applicable under XPath 2.0, not XQuery 1.0
	}

	public class XPathAxis
	{
		// FIXME: add more parameters to distinguish them
		private XPathAxis (XPathAxisType axisType)
		{
			this.axisType = axisType;
			switch (axisType) {
			case XPathAxisType.Parent:
			case XPathAxisType.Ancestor:
			case XPathAxisType.AncestorOrSelf:
			case XPathAxisType.Preceding:
			case XPathAxisType.PrecedingSibling:
				this.reverse = true;
				break;
			}
		}

		bool reverse;
		XPathAxisType axisType;

		public bool ReverseAxis {
			get { return reverse; }
		}

		public XPathAxisType AxisType {
			get { return axisType; }
		}

		static XPathAxis child, descendant, attribute, self, 
			descendantOrSelf, followingSibling, following, 
			parent, ancestor, precedingSibling, preceding, 
			ancestorOrSelf;

		static XPathAxis ()
		{
			child = new XPathAxis (XPathAxisType.Child);
			descendant = new XPathAxis (XPathAxisType.Descendant);
			attribute = new XPathAxis (XPathAxisType.Attribute);
			self = new XPathAxis (XPathAxisType.Self);
			descendantOrSelf = new XPathAxis (XPathAxisType.DescendantOrSelf);
			followingSibling = new XPathAxis (XPathAxisType.FollowingSibling);
			following = new XPathAxis (XPathAxisType.Following);
			parent = new XPathAxis (XPathAxisType.Parent);
			ancestor = new XPathAxis (XPathAxisType.Ancestor);
			precedingSibling = new XPathAxis (XPathAxisType.PrecedingSibling);
			preceding = new XPathAxis (XPathAxisType.Preceding);
			ancestorOrSelf = new XPathAxis (XPathAxisType.AncestorOrSelf);
		}

		public static XPathAxis Child {
			get { return child; }
		}

		public static XPathAxis Descendant {
			get { return descendant; }
		}

		public static XPathAxis Attribute {
			get { return attribute; }
		}

		public static XPathAxis Self {
			get { return self; }
		}

		public static XPathAxis DescendantOrSelf {
			get { return descendantOrSelf; }
		}

		public static XPathAxis FollowingSibling {
			get { return followingSibling; }
		}

		public static XPathAxis Following {
			get { return following; }
		}

		public static XPathAxis Parent {
			get { return parent; }
		}

		public static XPathAxis Ancestor {
			get { return ancestor; }
		}

		public static XPathAxis PrecedingSibling {
			get { return precedingSibling; }
		}

		public static XPathAxis Preceding {
			get { return preceding; }
		}

		public static XPathAxis AncestorOrSelf {
			get { return ancestorOrSelf; }
		}
	}

	// ItemType
	public class ItemType
	{
		static ItemType anyItem = new ItemType (XmlTypeCode.Item);
		static ItemType anyAtomicType = new ItemType (XmlTypeCode.AnyAtomicType);

		public static ItemType AnyItem {
			get { return anyItem; }
		}

		public static ItemType AnyAtomicType {
			get { return anyAtomicType; }
		}

		XmlTypeCode typeCode;

		public ItemType (XmlTypeCode typeCode)
		{
			this.typeCode = typeCode;
		}

		public XmlTypeCode TypeCode {
			get { return typeCode; }
		}

		internal virtual void CheckReference (XQueryASTCompiler compiler)
		{
		}
	}

	// KindTest

	public class KindTest : ItemType
	{
		public KindTest (XmlTypeCode type)
			: base (type)
		{
		}

		internal virtual void Compile (XQueryASTCompiler compiler)
		{
		}

		public virtual bool Matches (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			if (nav == null)
				return false;
			// FIXME: is it true? ('untyped' means 'matches with any type' ?
			if (item.XmlType == null)
				return true;
			if (item.XmlType.TypeCode != TypeCode)
				return false;
			return true;
		}
	}

	public class DocumentTest : KindTest
	{
		ElementTest content;

		public DocumentTest (ElementTest content)
			: base (XmlTypeCode.Document)
		{
			this.content = content;
		}

		public ElementTest Content {
			get { return content; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			content.CheckReference (compiler);
		}

		internal override void Compile (XQueryASTCompiler compiler)
		{
		}

		public override bool Matches (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			if (nav == null)
				return false;

			if (item.XmlType.TypeCode != XmlTypeCode.Document)
				return false;

			if (Content == null)
				return true;

			nav = nav.Clone ();
			nav.MoveToFirstChild ();
			while (nav.NodeType != XPathNodeType.Element)
				if (!nav.MoveToNext ())
					return false;
			return Content.Matches (nav);
		}
	}

	public class ElementTest : KindTest
	{
		XmlQualifiedName name;
		XmlQualifiedName typeName;
		XmlSchemaType schemaType;
		bool nillable;

		public ElementTest (XmlQualifiedName name)
			: base (XmlTypeCode.Element)
		{
			this.name = name;
		}

		public ElementTest (XmlQualifiedName name, XmlQualifiedName type, bool nillable)
			: base (XmlTypeCode.Element)
		{
			this.name = name;
			this.typeName = type;
			this.nillable = nillable;
		}

		public XmlQualifiedName Name {
			get { return name; }
		}

		public XmlQualifiedName TypeName {
			get { return typeName; }
		}

		public XmlSchemaType SchemaType {
			get {
				return schemaType;
			}
		}

		public bool Nillable {
			get { return nillable; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			compiler.CheckSchemaTypeName (typeName);
		}

		internal override void Compile (XQueryASTCompiler compiler)
		{
			schemaType = compiler.ResolveSchemaType (TypeName);
			if (schemaType == null)
				throw new XmlQueryCompileException ("Specified schema type was not found.");
		}

		public override bool Matches (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			if (nav == null)
				return false;

			if (item.XmlType.TypeCode != XmlTypeCode.Element)
				return false;

			if (Name != XmlQualifiedName.Empty)
				if (nav.LocalName != Name.Name || nav.NamespaceURI != Name.Namespace)
					return false;

			// FIXME: it won't be XQueryConvert.CanConvert(), but other strict-matching evaluation
			if (SchemaType != null && !XQueryConvert.CanConvert (item, SchemaType))
				return false;
			// FIXME: check nillable

			return true;
		}
	}

	public class AttributeTest : KindTest
	{
		static AttributeTest anyAttribute;

		static AttributeTest ()
		{
			anyAttribute = new AttributeTest (XmlQualifiedName.Empty);
		}

		public static AttributeTest AnyAttribute {
			get { return anyAttribute; }
		}

		public AttributeTest (XmlQualifiedName name)
			: base (XmlTypeCode.Attribute)
		{
			this.name = name;
		}

		public AttributeTest (XmlQualifiedName name, XmlQualifiedName typeName)
			: base (XmlTypeCode.Attribute)
		{
			this.name = name;
			this.typeName = typeName;
		}

		XmlQualifiedName name;
		XmlQualifiedName typeName;
		XmlSchemaType schemaType;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public XmlQualifiedName TypeName {
			get { return typeName; }
		}

		public XmlSchemaType SchemaType {
			get { return schemaType; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			compiler.CheckSchemaTypeName (typeName);
		}

		internal override void Compile (XQueryASTCompiler compiler)
		{
			schemaType = compiler.ResolveSchemaType (TypeName);
			if (schemaType == null)
				throw new XmlQueryCompileException ("Specified schema type was not found.");
		}

		public override bool Matches (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			if (nav == null)
				return false;

			if (item.XmlType.TypeCode != XmlTypeCode.Attribute)
				return false;

			if (Name != XmlQualifiedName.Empty)
				if (nav.LocalName != Name.Name || nav.NamespaceURI != Name.Namespace)
					return false;

			// FIXME: it won't be XQueryConvert.CanConvert(), but other strict-matching evaluation
			if (SchemaType != null && !XQueryConvert.CanConvert (item, SchemaType))
				return false;

			return true;
		}
	}

	public class XmlPITest : KindTest
	{
		string name;

		public XmlPITest (string nameTest)
			: base (XmlTypeCode.ProcessingInstruction)
		{
			this.name = nameTest;
		}

		public string Name {
			get { return name; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
		}

		internal override void Compile (XQueryASTCompiler compiler)
		{
		}

		public override bool Matches (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			if (nav == null)
				return false;

			if (item.XmlType.TypeCode != XmlTypeCode.ProcessingInstruction)
				return false;
			if (Name != String.Empty && nav.LocalName != Name)
				return false;
			return true;
		}
	}

	#endregion
}

#endif
