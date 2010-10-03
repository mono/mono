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
	public class XamlType : IEquatable<XamlType>
	{
		public XamlType (Type underlyingType, XamlSchemaContext schemaContext)
			: this (underlyingType, schemaContext, null)
		{
		}

		static readonly Type [] predefined_types = {
				typeof (XData), typeof (Uri), typeof (TimeSpan), typeof (PropertyDefinition), typeof (MemberDefinition), typeof (Reference)
			};

		public XamlType (Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker)
			: this (schemaContext, invoker)
		{
			if (underlyingType == null)
				throw new ArgumentNullException ("underlyingType");
			type = underlyingType;
			underlying_type = type;

			XamlType xt;
			if (XamlLanguage.InitializingTypes) {
				Name = type.GetXamlName ();
				PreferredXamlNamespace = XamlLanguage.Xaml2006Namespace;
			} else if ((xt = XamlLanguage.AllTypes.FirstOrDefault (t => t.UnderlyingType == type)) != null) {
				Name = xt.Name;
				PreferredXamlNamespace = XamlLanguage.Xaml2006Namespace;
			} else {
				Name = type.GetXamlName ();
				PreferredXamlNamespace = String.Format ("clr-namespace:{0};assembly={1}", type.Namespace, type.Assembly.GetName ().Name);
			}
		}

		public XamlType (string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
			: this (schemaContext, null)
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
			explicit_ns = unknownTypeNamespace;
		}

		protected XamlType (string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
			: this (String.Empty, typeName, typeArguments, schemaContext)
		{
		}

		XamlType (XamlSchemaContext schemaContext, XamlTypeInvoker invoker)
		{
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			SchemaContext = schemaContext;
			this.invoker = invoker ?? new XamlTypeInvoker (this);
		}

		Type type, underlying_type;

		string explicit_ns;

		// populated properties
		XamlType base_type;
		XamlTypeInvoker invoker;

		internal EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler {
			get { return LookupSetMarkupExtensionHandler (); }
		}

		internal EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler {
			get { return LookupSetTypeConverterHandler (); }
		}

		public IList<XamlType> AllowedContentTypes {
			get { return LookupAllowedContentTypes (); }
		}

		public XamlType BaseType {
			get { return LookupBaseType (); }
		}

		public bool ConstructionRequiresArguments {
			get { return LookupConstructionRequiresArguments (); }
		}

		public XamlMember ContentProperty {
			get { return LookupContentProperty (); }
		}

		public IList<XamlType> ContentWrappers {
			get { return LookupContentWrappers (); }
		}

		public XamlValueConverter<XamlDeferringLoader> DeferringLoader {
			get { return LookupDeferringLoader (); }
		}

		public XamlTypeInvoker Invoker {
			get { return LookupInvoker (); }
		}

		public bool IsAmbient {
			get { return LookupIsAmbient (); }
		}

		public bool IsArray {
			get { return LookupCollectionKind () == XamlCollectionKind.Array; }
		}

		// it somehow treats array as not a collection...
		public bool IsCollection {
			get { return LookupCollectionKind () == XamlCollectionKind.Collection; }
		}

		public bool IsConstructible {
			get { return LookupIsConstructible (); }
		}

		public bool IsDictionary {
			get { return LookupCollectionKind () == XamlCollectionKind.Dictionary; }
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
			get { return XamlLanguage.IsValidXamlName (Name); }
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
				PreferredXamlNamespace == other.PreferredXamlNamespace && TypeArguments.ListEquals (other.TypeArguments);
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
			return String.IsNullOrEmpty (PreferredXamlNamespace) ? Name : String.Concat ("{", PreferredXamlNamespace, "}", Name);
		}

		public virtual bool CanAssignTo (XamlType xamlType)
		{
			throw new NotImplementedException ();
		}

		public XamlMember GetAliasedProperty (XamlDirective directive)
		{
			return LookupAliasedProperty (directive);
		}

		public ICollection<XamlMember> GetAllAttachableMembers ()
		{
			return new List<XamlMember> (LookupAllAttachableMembers ());
		}

		public ICollection<XamlMember> GetAllMembers ()
		{
			return new List<XamlMember> (LookupAllMembers ());
		}

		public XamlMember GetAttachableMember (string name)
		{
			return LookupAttachableMember (name);
		}

		public XamlMember GetMember (string name)
		{
			return LookupMember (name, false);
		}

		public IList<XamlType> GetPositionalParameters (int parameterCount)
		{
			return LookupPositionalParameters (parameterCount);
		}

		public virtual IList<string> GetXamlNamespaces ()
		{
			throw new NotImplementedException ();
			/* this does not work like documented!
			if (explicit_ns != null)
				return new string [] {explicit_ns};
			var l = SchemaContext.GetAllXamlNamespaces ();
			if (l != null)
				return new List<string> (l);
			return new string [] {String.Empty};
			*/
		}

		// lookups

		protected virtual XamlMember LookupAliasedProperty (XamlDirective directive)
		{
			if (directive == XamlLanguage.Key) {
				var a = this.GetCustomAttribute<DictionaryKeyPropertyAttribute> ();
				return a != null ? GetMember (a.Name) : null;
			}
			if (directive == XamlLanguage.Name) {
				var a = this.GetCustomAttribute<RuntimeNamePropertyAttribute> ();
				return a != null ? GetMember (a.Name) : null;
			}
			if (directive == XamlLanguage.Uid) {
				var a = this.GetCustomAttribute<UidPropertyAttribute> ();
				return a != null ? GetMember (a.Name) : null;
			}
			if (directive == XamlLanguage.Lang) {
				var a = this.GetCustomAttribute<XmlLangPropertyAttribute> ();
				return a != null ? GetMember (a.Name) : null;
			}
			return null;
		}

		protected virtual IEnumerable<XamlMember> LookupAllAttachableMembers ()
		{
			if (UnderlyingType == null)
				return BaseType != null ? BaseType.GetAllMembers () : null;
			return DoLookupAllAttachableMembers ();
		}

		IEnumerable<XamlMember> DoLookupAllAttachableMembers ()
		{
			yield break; // FIXME: what to return here?
		}

		protected virtual IEnumerable<XamlMember> LookupAllMembers ()
		{
			if (UnderlyingType == null)
				return BaseType != null ? BaseType.GetAllMembers () : null;
			if (all_members_cache == null)
				all_members_cache = new List<XamlMember> (DoLookupAllMembers ());
			return all_members_cache;
		}

		List<XamlMember> all_members_cache;

		IEnumerable<XamlMember> DoLookupAllMembers ()
		{
			foreach (var pi in UnderlyingType.GetProperties ())
				if (pi.CanRead && pi.CanWrite && pi.GetIndexParameters ().Length == 0)
					yield return new XamlMember (pi, SchemaContext);
		}

		protected virtual IList<XamlType> LookupAllowedContentTypes ()
		{
			// the actual implementation is very different from what is documented :(
			return null;

			/*
			var l = new List<XamlType> ();
			if (ContentWrappers != null)
				l.AddRange (ContentWrappers);
			if (ContentProperty != null)
				l.Add (ContentProperty.Type);
			if (ItemType != null)
				l.Add (ItemType);
			return l.Count > 0 ? l : null;
			*/
		}

		protected virtual XamlMember LookupAttachableMember (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual XamlType LookupBaseType ()
		{
			if (base_type == null) {
				if (UnderlyingType == null)
					// FIXME: probably something advanced is needed here.
					base_type = new XamlType (typeof (object), SchemaContext, Invoker);
				else
					base_type = type.BaseType == null || type.BaseType == typeof (object) ? null : new XamlType (type.BaseType, SchemaContext, Invoker);
			}
			return base_type;
		}

		// This implementation is not verified. (No place to use.)
		protected virtual XamlCollectionKind LookupCollectionKind ()
		{
			if (UnderlyingType == null)
				return BaseType != null ? BaseType.LookupCollectionKind () : XamlCollectionKind.None;
			if (type.IsArray)
				return XamlCollectionKind.Array;

			if (type.ImplementsAnyInterfacesOf (typeof (IDictionary), typeof (IDictionary<,>)))
				return XamlCollectionKind.Dictionary;

			if (type.ImplementsAnyInterfacesOf (typeof (ICollection), typeof (ICollection<>)))
				return XamlCollectionKind.Collection;

			return XamlCollectionKind.None;
		}

		protected virtual bool LookupConstructionRequiresArguments ()
		{
			if (UnderlyingType == null)
				return false;

			// not sure if it is required, but MemberDefinition return true while they are abstract and it makes no sense.
			if (UnderlyingType.IsAbstract)
				return true;

			// FIXME: probably some primitive types are treated as special.
			switch (Type.GetTypeCode (UnderlyingType)) {
			case TypeCode.String:
				return true;
			case TypeCode.Object:
				if (UnderlyingType == typeof (TimeSpan))
					return false;
				break;
			default:
				return false;
			}

			return UnderlyingType.GetConstructor (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null) == null;
		}

		protected virtual XamlMember LookupContentProperty ()
		{
			var a = this.GetCustomAttribute<ContentPropertyAttribute> ();
			return a != null && a.Name != null ? GetMember (a.Name) : null;
		}

		protected virtual IList<XamlType> LookupContentWrappers ()
		{
			if (CustomAttributeProvider == null)
				return null;

			var arr = CustomAttributeProvider.GetCustomAttributes (typeof (ContentWrapperAttribute), false);
			if (arr == null || arr.Length == 0)
				return null;
			var l = new XamlType [arr.Length];
			for (int i = 0; i < l.Length; i++) 
				l [i] = SchemaContext.GetXamlType (((ContentWrapperAttribute) arr [i]).ContentWrapper);
			return l;
		}

		internal ICustomAttributeProvider CustomAttributeProvider {
			get { return LookupCustomAttributeProvider (); }
		}

		protected virtual ICustomAttributeProvider LookupCustomAttributeProvider ()
		{
			return UnderlyingType;
		}
		protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader ()
		{
			throw new NotImplementedException ();
		}

		protected virtual XamlTypeInvoker LookupInvoker ()
		{
			return invoker;
		}

		protected virtual bool LookupIsAmbient ()
		{
			return this.GetCustomAttribute<AmbientAttribute> () != null;
		}

		// It is documented as if it were to reflect spec. section 5.2,
		// but the actual behavior shows it is *totally* wrong.
		// Here I have implemented this based on the nunit test results. sigh.
		protected virtual bool LookupIsConstructible ()
		{
			if (UnderlyingType == null)
				return true;
			if (IsMarkupExtension)
				return true;
			if (UnderlyingType.IsAbstract)
				return false;
			if (!IsNameValid)
				return false;
			return true;
		}

		protected virtual bool LookupIsMarkupExtension ()
		{
			return typeof (MarkupExtension).IsAssignableFrom (UnderlyingType);
		}

		protected virtual bool LookupIsNameScope ()
		{
			return typeof (INameScope).IsAssignableFrom (UnderlyingType);
		}

		protected virtual bool LookupIsNullable ()
		{
			return !type.IsValueType || type.ImplementsInterface (typeof (Nullable<>));
		}

		protected virtual bool LookupIsPublic ()
		{
			return underlying_type == null || underlying_type.IsPublic || underlying_type.IsNestedPublic;
		}

		protected virtual bool LookupIsUnknown ()
		{
			return UnderlyingType == null;
		}

		protected virtual bool LookupIsWhitespaceSignificantCollection ()
		{
			// probably for unknown types, it should preserve whitespaces.
			return IsUnknown || this.GetCustomAttribute<WhitespaceSignificantCollectionAttribute> () != null;
		}

		protected virtual bool LookupIsXData ()
		{
			// huh? XamlLanguage.XData.IsXData returns false(!)
			// return typeof (XData).IsAssignableFrom (UnderlyingType);
			return false;
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
			var a = this.GetCustomAttribute<MarkupExtensionReturnTypeAttribute> ();
			return a != null ? new XamlType (a.ReturnType, SchemaContext) : null;
		}

		protected virtual XamlMember LookupMember (string name, bool skipReadOnlyCheck)
		{
			if (UnderlyingType == null)
				return null;
			var pi = UnderlyingType.GetProperty (name);
			if (pi != null && (skipReadOnlyCheck || pi.CanWrite))
				return new XamlMember (pi, SchemaContext);
			var ei = UnderlyingType.GetEvent (name);
			if (ei != null)
				return new XamlMember (ei, SchemaContext);
			return null;
		}

		protected virtual IList<XamlType> LookupPositionalParameters (int parameterCount)
		{
			if (UnderlyingType == null/* || !IsMarkupExtension*/) // see nunit tests...
				return null;

			// check if there is applicable ConstructorArgumentAttribute.
			// If there is, then return its type.
			if (parameterCount == 1) {
				foreach (var xm in GetAllMembers ()) {
					var ca = xm.CustomAttributeProvider.GetCustomAttribute<ConstructorArgumentAttribute> (false);
					if (ca != null)
						return new XamlType [] {xm.Type};
				}
			}

			var methods = (from m in UnderlyingType.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) where m.GetParameters ().Length == parameterCount select m).ToArray ();
			if (methods.Length == 1)
				return (from p in methods [0].GetParameters () select SchemaContext.GetXamlType (p.ParameterType)).ToArray ();

			if (SchemaContext.SupportMarkupExtensionsWithDuplicateArity)
				throw new NotSupportedException ("The default LookupPositionalParameters implementation does not allow duplicate arity of markup extensions");
			return null;
		}

		BindingFlags flags_get_static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		protected virtual EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler ()
		{
			var a = this.GetCustomAttribute<XamlSetMarkupExtensionAttribute> ();
			if (a == null)
				return null;
			var mi = type.GetMethod (a.XamlSetMarkupExtensionHandler, flags_get_static);
			if (mi == null)
				throw new ArgumentException ("Binding to XamlSetMarkupExtensionHandler failed");
			return (EventHandler<XamlSetMarkupExtensionEventArgs>) Delegate.CreateDelegate (typeof (EventHandler<XamlSetMarkupExtensionEventArgs>), mi);
		}

		protected virtual EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler ()
		{
			var a = this.GetCustomAttribute<XamlSetTypeConverterAttribute> ();
			if (a == null)
				return null;
			var mi = type.GetMethod (a.XamlSetTypeConverterHandler, flags_get_static);
			if (mi == null)
				throw new ArgumentException ("Binding to XamlSetTypeConverterHandler failed");
			return (EventHandler<XamlSetTypeConverterEventArgs>) Delegate.CreateDelegate (typeof (EventHandler<XamlSetTypeConverterEventArgs>), mi);
		}

		protected virtual bool LookupTrimSurroundingWhitespace ()
		{
			return this.GetCustomAttribute<TrimSurroundingWhitespaceAttribute> () != null;
		}

		protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter ()
		{
			var t = UnderlyingType;
			if (t == null)
				return null;

			// equivalent to TypeExtension.
			// FIXME: not sure if it should be specially handled here.
			if (t == typeof (Type))
				t = typeof (TypeExtension);

			var a = CustomAttributeProvider.GetCustomAttribute<TypeConverterAttribute> (false);
			if (a != null)
				return SchemaContext.GetValueConverter<TypeConverter> (Type.GetType (a.ConverterTypeName), this);

			if (t == typeof (object))
				return SchemaContext.GetValueConverter<TypeConverter> (typeof (TypeConverter), this);

			// It's still not decent to check CollectionConverter.
			var tct = TypeDescriptor.GetConverter (t).GetType ();
			if (tct != typeof (TypeConverter) && tct != typeof (CollectionConverter) && tct != typeof (ReferenceConverter))
				return SchemaContext.GetValueConverter<TypeConverter> (tct, this);
			return null;
		}

		protected virtual Type LookupUnderlyingType ()
		{
			return underlying_type;
		}

		protected virtual bool LookupUsableDuringInitialization ()
		{
			var a = this.GetCustomAttribute<UsableDuringInitializationAttribute> ();
			return a != null && a.Usable;
		}

		static XamlValueConverter<ValueSerializer> string_value_serializer;

		protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer ()
		{
			return LookupValueSerializer (this, CustomAttributeProvider);
		}

		internal static XamlValueConverter<ValueSerializer> LookupValueSerializer (XamlType targetType, ICustomAttributeProvider provider)
		{
			if (provider == null)
				return null;

			var a = provider.GetCustomAttribute<ValueSerializerAttribute> (true);
			if (a != null)
				return new XamlValueConverter<ValueSerializer> (a.ValueSerializerType ?? Type.GetType (a.ValueSerializerTypeName), targetType);

			if (targetType.BaseType != null) {
				var ret = targetType.BaseType.LookupValueSerializer ();
				if (ret != null)
					return ret;
			}

			if (targetType.UnderlyingType == typeof (string)) {
				if (string_value_serializer == null)
					string_value_serializer = new XamlValueConverter<ValueSerializer> (typeof (StringValueSerializer), targetType);
				return string_value_serializer;
			}

			return null;
		}

		internal IEnumerable<XamlMember> GetConstructorArguments ()
		{
			return GetAllMembers ().Where (m => m.UnderlyingMember != null && m.CustomAttributeProvider.GetCustomAttribute<ConstructorArgumentAttribute> (false) != null);
		}
	}
}
