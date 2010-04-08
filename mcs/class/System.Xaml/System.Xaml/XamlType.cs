//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml
{
	static class TypeExtension
	{
		public static bool ImplementsAnyInterfacesOf (this Type type, params Type [] definitions)
		{
			return definitions.Any (t => ImplementsInterface (type, t));
		}

		public static bool ImplementsInterface (this Type type, Type definition)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (definition == null)
				throw new ArgumentNullException ("definition");

			foreach (var iface in type.GetInterfaces ())
				if (iface == definition || (iface.IsGenericType && iface.GetGenericTypeDefinition () == definition))
					return true;
			return false;
		}
	}

	public class XamlType : IEquatable<XamlType>
	{
		public XamlType (Type underlyingType, XamlSchemaContext schemaContext)
			: this (underlyingType, schemaContext, null)
		{
		}

		public XamlType (Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker)
		{
			if (underlyingType == null)
				throw new ArgumentNullException ("underlyingType");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			type = underlyingType;
			underlying_type = type;
			SchemaContext = schemaContext;
			Invoker = invoker;

			Name = type.Name;
			// FIXME: remove this hack
			if (Type.GetTypeCode (type) == TypeCode.Object && type != typeof (object))
				PreferredXamlNamespace = String.Format ("clr-namespace:{0};assembly={1}", type.Namespace, type.Assembly.GetName ().Name);
			else
				PreferredXamlNamespace = XamlLanguage.Xaml2006Namespace;
		}

		public XamlType (string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
		{
			if (unknownTypeNamespace == null)
				throw new ArgumentNullException ("unknownTypeNamespace");
			if (unknownTypeName == null)
				throw new ArgumentNullException ("unknownTypeName");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");

			type = typeof (object);
			Name = unknownTypeName;
			PreferredXamlNamespace = unknownTypeNamespace;
			TypeArguments = typeArguments != null && typeArguments.Count == 0 ? null : typeArguments;
			SchemaContext = schemaContext;
		}

		protected XamlType (string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
			: this (String.Empty, typeName, typeArguments, schemaContext)
		{
		}

		Type type, underlying_type;

		// populated properties
		XamlType base_type;

		public IList<XamlType> AllowedContentTypes {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XamlType BaseType {
			get {
				if (base_type == null) {
					if (UnderlyingType == null)
						// FIXME: probably something advanced is needed here.
						base_type = new XamlType (typeof (object), SchemaContext, Invoker);
					else
						base_type = type.BaseType == null || type.BaseType == typeof (object) ? null : new XamlType (type.BaseType, SchemaContext, Invoker);
				}
				return base_type;
			}
		}

		public bool ConstructionRequiresArguments {
			get { throw new NotImplementedException (); }
		}
		public XamlMember ContentProperty {
			get { throw new NotImplementedException (); }
		}
		public IList<XamlType> ContentWrappers {
			get { throw new NotImplementedException (); }
		}
		public XamlValueConverter<XamlDeferringLoader> DeferringLoader {
			get { throw new NotImplementedException (); }
		}

		public XamlTypeInvoker Invoker { get; private set; }

		public bool IsAmbient {
			get { return LookupIsAmbient (); }
		}
		public bool IsArray {
			get { return type.IsArray; }
		}
		public bool IsCollection {
			// it somehow treats array as not a collection...
			get { return !type.IsArray && type.ImplementsAnyInterfacesOf (typeof (ICollection), typeof (ICollection<>)); }
		}

		public bool IsConstructible {
			get { return LookupIsConstructible (); }
		}

		public bool IsDictionary {
			get { return type.ImplementsAnyInterfacesOf (typeof (IDictionary), typeof (IDictionary<,>)); }
		}

		public bool IsGeneric {
			get { return type.IsGenericType; }
		}

		public bool IsMarkupExtension {
			get { return LookupIsMarkupExtension (); }
		}
		public bool IsNameScope {
			get { return LookupIsNameScope (); }
		}
		public bool IsNameValid {
			get { throw new NotImplementedException (); }
		}

		public bool IsNullable {
			get { return LookupIsNullable (); }
		}

		public bool IsPublic {
			get { return LookupIsPublic (); }
		}

		public bool IsUnknown {
			get { return LookupIsUnknown (); }
		}

		public bool IsUsableDuringInitialization {
			get { return LookupUsableDuringInitialization (); }
		}

		public bool IsWhitespaceSignificantCollection {
			get { return LookupIsWhitespaceSignificantCollection (); }
		}

		public bool IsXData {
			get { return LookupIsXData (); }
		}

		public XamlType ItemType {
			get { return LookupItemType (); }
		}

		public XamlType KeyType {
			get { return LookupKeyType (); }
		}

		public XamlType MarkupExtensionReturnType {
			get { return LookupMarkupExtensionReturnType (); }
		}

		public string Name { get; private set; }

		public string PreferredXamlNamespace { get; private set; }

		public XamlSchemaContext SchemaContext { get; private set; }

		public bool TrimSurroundingWhitespace {
			get { return LookupTrimSurroundingWhitespace (); }
		}

		public IList<XamlType> TypeArguments { get; private set; }

		public XamlValueConverter<TypeConverter> TypeConverter {
			get { return LookupTypeConverter (); }
		}

		public Type UnderlyingType {
			get { return LookupUnderlyingType (); }
		}

		public XamlValueConverter<ValueSerializer> ValueSerializer {
			get { return LookupValueSerializer (); }
		}

		public static bool operator == (XamlType left, XamlType right)
		{
			return IsNull (left) ? IsNull (right) : left.Equals (right);
		}

		static bool IsNull (XamlType a)
		{
			return Object.ReferenceEquals (a, null);
		}

		public static bool operator != (XamlType left, XamlType right)
		{
			return !(left == right);
		}
		
		public bool Equals (XamlType other)
		{
			return !IsNull (other) &&
				UnderlyingType == other.UnderlyingType &&
				Name == other.Name &&
				PreferredXamlNamespace == other.PreferredXamlNamespace &&
				CompareTypes (TypeArguments, other.TypeArguments);
		}

		static bool CompareTypes (IList<XamlType> a1, IList<XamlType> a2)
		{
			if (a1 == null)
				return a2 == null;
			if (a2 == null)
				return false;
			if (a1.Count != a2.Count)
				return false;
			for (int i = 0; i < a1.Count; i++)
				if (a1 [i] != a2 [i])
					return false;
			return true;
		}

		public override bool Equals (object obj)
		{
			var a = obj as XamlType;
			return Equals (a);
		}
		
		public override int GetHashCode ()
		{
			if (UnderlyingType != null)
				return UnderlyingType.GetHashCode ();
			int x = Name.GetHashCode () << 7 + PreferredXamlNamespace.GetHashCode ();
			if (TypeArguments != null)
				foreach (var t in TypeArguments)
					x = t.GetHashCode () + x << 5;
			return x;
		}

		public override string ToString ()
		{
			return UnderlyingType != null ? UnderlyingType.ToString () : Name;
		}

		public virtual bool CanAssignTo (XamlType xamlType)
		{
			throw new NotImplementedException ();
		}
		public XamlMember GetAliasedProperty (XamlDirective directive)
		{
			throw new NotImplementedException ();
		}
		public ICollection<XamlMember> GetAllAttachableMembers ()
		{
			throw new NotImplementedException ();
		}
		public ICollection<XamlMember> GetAllMembers ()
		{
			throw new NotImplementedException ();
		}
		public XamlMember GetAttachableMember (string name)
		{
			throw new NotImplementedException ();
		}
		public XamlMember GetMember (string name)
		{
			throw new NotImplementedException ();
		}
		public IList<XamlType> GetPositionalParameters (int parameterCount)
		{
			throw new NotImplementedException ();
		}
		public virtual IList<string> GetXamlNamespaces ()
		{
			throw new NotImplementedException ();
		}

		// lookups

		protected virtual XamlMember LookupAliasedProperty (XamlDirective directive)
		{
			throw new NotImplementedException ();
		}
		protected virtual IEnumerable<XamlMember> LookupAllAttachableMembers ()
		{
			throw new NotImplementedException ();
		}
		protected virtual IEnumerable<XamlMember> LookupAllMembers ()
		{
			throw new NotImplementedException ();
		}
		protected virtual IList<XamlType> LookupAllowedContentTypes ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlMember LookupAttachableMember (string name)
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlType LookupBaseType ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlCollectionKind LookupCollectionKind ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupConstructionRequiresArguments ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlMember LookupContentProperty ()
		{
			throw new NotImplementedException ();
		}
		protected virtual IList<XamlType> LookupContentWrappers ()
		{
			throw new NotImplementedException ();
		}
		protected virtual ICustomAttributeProvider LookupCustomAttributeProvider ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlTypeInvoker LookupInvoker ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsAmbient ()
		{
			throw new NotImplementedException ();
		}

		protected virtual bool LookupIsConstructible ()
		{
			// see spec. 5.2.
			if (IsArray) // x:Array
				return false;
			if (type == typeof (XamlType)) // x:XamlType
				return false;
			// FIXME: handle x:XamlEvent
			if (IsMarkupExtension)
				return false;
			// FIXME: handle x:Code
			if (IsXData)
				return false;
			return true;
		}

		protected virtual bool LookupIsMarkupExtension ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsNameScope ()
		{
			throw new NotImplementedException ();
		}

		protected virtual bool LookupIsNullable ()
		{
			return type.ImplementsInterface (typeof (Nullable<>));
		}

		protected virtual bool LookupIsPublic ()
		{
			return type.IsPublic;
		}

		protected virtual bool LookupIsUnknown ()
		{
			return UnderlyingType == null;
		}

		protected virtual bool LookupIsWhitespaceSignificantCollection ()
		{
			throw new NotImplementedException ();
		}

		protected virtual bool LookupIsXData ()
		{
			throw new NotImplementedException ();
		}

		protected virtual XamlType LookupItemType ()
		{
			if (IsArray)
				return new XamlType (type.GetElementType (), SchemaContext);
			if (!IsCollection)
				return null;
			if (!IsGeneric)
				return new XamlType (typeof (object), SchemaContext);
			return new XamlType (type.GetGenericArguments () [0], SchemaContext);
		}

		protected virtual XamlType LookupKeyType ()
		{
			if (!IsDictionary)
				return null;
			if (!IsGeneric)
				return new XamlType (typeof (object), SchemaContext);
			return new XamlType (type.GetGenericArguments () [0], SchemaContext);
		}

		protected virtual XamlType LookupMarkupExtensionReturnType ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlMember LookupMember (string name, bool skipReadOnlyCheck)
		{
			throw new NotImplementedException ();
		}
		protected virtual IList<XamlType> LookupPositionalParameters (int parameterCount)
		{
			throw new NotImplementedException ();
		}
		protected virtual EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler ()
		{
			throw new NotImplementedException ();
		}
		protected virtual EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupTrimSurroundingWhitespace ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter ()
		{
			throw new NotImplementedException ();
		}

		protected virtual Type LookupUnderlyingType ()
		{
			return underlying_type;
		}

		protected virtual bool LookupUsableDuringInitialization ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer ()
		{
			throw new NotImplementedException ();
		}
	}
}
