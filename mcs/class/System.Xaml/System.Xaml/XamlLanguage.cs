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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xaml.Schema;
using System.Windows.Markup;

[assembly:XmlnsDefinition (System.Xaml.XamlLanguage.Xaml2006Namespace, "System.Windows.Markup")] // FIXME: verify.

namespace System.Xaml
{
	public static class XamlLanguage
	{
		public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
		public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";
		internal const string Xmlns2000Namespace = "http://www.w3.org/2000/xmlns/";

		// FIXME: I'm not sure if these "special names" should be resolved like this. I couldn't find any rule so far.
		internal static readonly SpecialTypeNameList SpecialNames;

		internal class SpecialTypeNameList : List<SpecialTypeName>
		{
			internal SpecialTypeNameList ()
			{
				Add (new SpecialTypeName ("Array", XamlLanguage.Array));
				Add (new SpecialTypeName ("Member", XamlLanguage.Member));
				Add (new SpecialTypeName ("Null", XamlLanguage.Null));
				Add (new SpecialTypeName ("Property", XamlLanguage.Property));
				Add (new SpecialTypeName ("Static", XamlLanguage.Static));
				Add (new SpecialTypeName ("Type", XamlLanguage.Type));
			}

			public XamlType Find (string name, string ns)
			{
				if (ns != XamlLanguage.Xaml2006Namespace)
					return null;
				var stn = this.FirstOrDefault (s => s.Name == name);
				return stn != null ? stn.Type : null;
			}
		}

		internal class SpecialTypeName
		{
			public SpecialTypeName (string name, XamlType type)
			{
				Name = name;
				Type = type;
			}
			
			public string Name { get; private set; }
			public XamlType Type { get; private set; }
		}

		static readonly XamlSchemaContext sctx = new XamlSchemaContext (new Assembly [] {typeof (XamlType).Assembly});

		static XamlType XT<T> ()
		{
			return sctx.GetXamlType (typeof (T));
		}

		internal static readonly bool InitializingDirectives;
		internal static readonly bool InitializingTypes;

		static XamlLanguage ()
		{
			InitializingTypes = true;

			// types

			Array = XT<ArrayExtension> ();
			Boolean = XT<bool> ();
			Byte = XT<byte> ();
			Char = XT<char> ();
			Decimal = XT<decimal> ();
			Double = XT<double> ();
			Int16 = XT<short> ();
			Int32 = XT<int> ();
			Int64 = XT<long> ();
			Member = XT<MemberDefinition> ();
			Null = XT<NullExtension> ();
			Object = XT<object> ();
			Property = XT<PropertyDefinition> ();
			Reference = XT<Reference> ();
			Single = XT<float> ();
			Static = XT<StaticExtension> ();
			String = XT<string> ();
			TimeSpan = XT<TimeSpan> ();
			Type = XT<TypeExtension> ();
			Uri = XT<Uri> ();
			XData = XT<XData> ();

			InitializingTypes = false;

			AllTypes = new ReadOnlyCollection<XamlType> (new XamlType [] {Array, Boolean, Byte, Char, Decimal, Double, Int16, Int32, Int64, Member, Null, Object, Property, Reference, Single, Static, String, TimeSpan, Type, Uri, XData});

			// directives

			// Looks like predefined XamlDirectives have no ValueSerializer. 
			// To handle this situation, differentiate them from non-primitive XamlMembers.
			InitializingDirectives = true;

			var nss = new string [] {XamlLanguage.Xaml2006Namespace};
			var nssXml = new string [] {XamlLanguage.Xml1998Namespace};

			Arguments = new XamlDirective (nss, "Arguments", XT<List<object>> (), null, AllowedMemberLocations.Any);
			AsyncRecords = new XamlDirective (nss, "AsyncRecords", XT<string> (), null, AllowedMemberLocations.Attribute);
			Base = new XamlDirective (nssXml, "base", XT<string> (), null, AllowedMemberLocations.Attribute);
			Class = new XamlDirective (nss, "Class", XT<string> (), null, AllowedMemberLocations.Attribute);
			ClassAttributes = new XamlDirective (nss, "ClassAttributes", XT<List<Attribute>> (), null, AllowedMemberLocations.MemberElement);
			ClassModifier = new XamlDirective (nss, "ClassModifier", XT<string> (), null, AllowedMemberLocations.Attribute);
			Code = new XamlDirective (nss, "Code", XT<string> (), null, AllowedMemberLocations.Attribute);
			ConnectionId = new XamlDirective (nss, "ConnectionId", XT<string> (), null, AllowedMemberLocations.Any);
			FactoryMethod = new XamlDirective (nss, "FactoryMethod", XT<string> (), null, AllowedMemberLocations.Any);
			FieldModifier = new XamlDirective (nss, "FieldModifier", XT<string> (), null, AllowedMemberLocations.Attribute);
			Initialization = new XamlDirective (nss, "_Initialization", XT<object> (), null, AllowedMemberLocations.Any);
			Items = new XamlDirective (nss, "_Items", XT<List<object>> (), null, AllowedMemberLocations.Any);
			Key = new XamlDirective (nss, "Key", XT<object> (), null, AllowedMemberLocations.Any);
			Lang = new XamlDirective (nssXml, "lang", XT<string> (), null, AllowedMemberLocations.Attribute);
			Members = new XamlDirective (nss, "Members", XT<List<MemberDefinition>> (), null, AllowedMemberLocations.MemberElement);
			Name = new XamlDirective (nss, "Name", XT<string> (), null, AllowedMemberLocations.Attribute);
			PositionalParameters = new XamlDirective (nss, "_PositionalParameters", XT<List<object>> (), null, AllowedMemberLocations.Any);
			Space = new XamlDirective (nssXml, "space", XT<string> (), null, AllowedMemberLocations.Attribute);
			Subclass = new XamlDirective (nss, "Subclass", XT<string> (), null, AllowedMemberLocations.Attribute);
			SynchronousMode = new XamlDirective (nss, "SynchronousMode", XT<string> (), null, AllowedMemberLocations.Attribute);
			Shared = new XamlDirective (nss, "Shared", XT<string> (), null, AllowedMemberLocations.Attribute);
			TypeArguments = new XamlDirective (nss, "TypeArguments", XT<string> (), null, AllowedMemberLocations.Attribute);
			Uid = new XamlDirective (nss, "Uid", XT<string> (), null, AllowedMemberLocations.Attribute);
			UnknownContent = new XamlDirective (nss, "_UnknownContent", XT<object> (), null, AllowedMemberLocations.MemberElement) { InternalIsUnknown = true };

			AllDirectives = new ReadOnlyCollection<XamlDirective> (new XamlDirective [] {Arguments, AsyncRecords, Base, Class, ClassAttributes, ClassModifier, Code, ConnectionId, FactoryMethod, FieldModifier, Initialization, Items, Key, Lang, Members, Name, PositionalParameters, Space, Subclass, SynchronousMode, Shared, TypeArguments, Uid, UnknownContent});

			InitializingDirectives = false;

			SpecialNames = new SpecialTypeNameList ();
		}

		static readonly string [] xaml_nss = new string [] {Xaml2006Namespace};

		public static IList<string> XamlNamespaces {
			get { return xaml_nss; }
		}

		static readonly string [] xml_nss = new string [] {Xml1998Namespace};

		public static IList<string> XmlNamespaces {
			get { return xml_nss; }
		}

		public static ReadOnlyCollection<XamlDirective> AllDirectives { get; private set; }

		public static XamlDirective Arguments { get; private set; }
		public static XamlDirective AsyncRecords { get; private set; }
		public static XamlDirective Base { get; private set; }
		public static XamlDirective Class { get; private set; }
		public static XamlDirective ClassAttributes { get; private set; }
		public static XamlDirective ClassModifier { get; private set; }
		public static XamlDirective Code { get; private set; }
		public static XamlDirective ConnectionId { get; private set; }
		public static XamlDirective FactoryMethod { get; private set; }
		public static XamlDirective FieldModifier { get; private set; }
		public static XamlDirective Initialization { get; private set; }
		public static XamlDirective Items { get; private set; }
		public static XamlDirective Key { get; private set; }
		public static XamlDirective Lang { get; private set; }
		public static XamlDirective Members { get; private set; }
		public static XamlDirective Name { get; private set; }
		public static XamlDirective PositionalParameters { get; private set; }
		public static XamlDirective Subclass { get; private set; }
		public static XamlDirective SynchronousMode { get; private set; }
		public static XamlDirective Shared { get; private set; }
		public static XamlDirective Space { get; private set; }
		public static XamlDirective TypeArguments { get; private set; }
		public static XamlDirective Uid { get; private set; }
		public static XamlDirective UnknownContent { get; private set; }

		public static ReadOnlyCollection<XamlType> AllTypes { get; private set; }

		public static XamlType Array { get; private set; }
		public static XamlType Boolean { get; private set; }
		public static XamlType Byte { get; private set; }
		public static XamlType Char { get; private set; }
		public static XamlType Decimal { get; private set; }
		public static XamlType Double { get; private set; }
		public static XamlType Int16 { get; private set; }
		public static XamlType Int32 { get; private set; }
		public static XamlType Int64 { get; private set; }
		public static XamlType Member { get; private set; }
		public static XamlType Null { get; private set; }
		public static XamlType Object { get; private set; }
		public static XamlType Property { get; private set; }
		public static XamlType Reference { get; private set; }
		public static XamlType Single { get; private set; }
		public static XamlType Static { get; private set; }
		public static XamlType String { get; private set; }
		public static XamlType TimeSpan { get; private set; }
		public static XamlType Type { get; private set; }
		public static XamlType Uri { get; private set; }
		public static XamlType XData { get; private set; }

		internal static bool IsValidXamlName (string name)
		{
			if (string.IsNullOrEmpty (name))
				return false;
			if (!IsValidXamlName (name [0], true))
				return false;
			foreach (char c in name)
				if (!IsValidXamlName (c, false))
					return false;
			return true;
		}

		static bool IsValidXamlName (char c, bool first)
		{
			if (c == '_')
				return true;
			switch (char.GetUnicodeCategory (c)) {
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.LetterNumber:
				return true;
			case UnicodeCategory.NonSpacingMark:
			case UnicodeCategory.DecimalDigitNumber:
			case UnicodeCategory.SpacingCombiningMark:
			case UnicodeCategory.ModifierLetter:
				return !first;
			default:
				return false;
			}
		}

		static readonly int clr_ns_len = "clr-namespace:".Length;
		static readonly int clr_ass_len = "assembly=".Length;

		internal static Type ResolveXamlTypeName (string xmlNamespace, string xmlLocalName, IList<XamlTypeName> typeArguments, IXamlNamespaceResolver nsResolver)
		{
			string ns = xmlNamespace;
			string name = xmlLocalName;

			if (ns == XamlLanguage.Xaml2006Namespace) {
				var xt = SpecialNames.Find (name, ns);
				if (xt == null)
					xt = AllTypes.FirstOrDefault (t => t.Name == xmlLocalName);
				if (xt == null)
					throw new FormatException (string.Format ("There is no type '{0}' in XAML namespace", name));
				return xt.UnderlyingType;
			}
			else if (!ns.StartsWith ("clr-namespace:", StringComparison.Ordinal))
				return null;

			Type [] genArgs = null;
			if (typeArguments != null) {
				var xtns = typeArguments;
				genArgs = new Type [xtns.Count];
				for (int i = 0; i < genArgs.Length; i++) {
					var xtn = xtns [i];
					genArgs [i] = ResolveXamlTypeName (xtn.Namespace, xtn.Name, xtn.TypeArguments, nsResolver);
				}
			}

			// convert xml namespace to clr namespace and assembly
			string [] split = ns.Split (';');
			if (split.Length != 2 || split [0].Length <= clr_ns_len || split [1].Length <= clr_ass_len)
				throw new XamlParseException (string.Format ("Cannot resolve runtime namespace from XML namespace '{0}'", ns));
			string tns = split [0].Substring (clr_ns_len);
			string aname = split [1].Substring (clr_ass_len);

			string tfn = tns.Length > 0 ? tns + '.' + name : name;
			if (genArgs != null)
				tfn += "`" + genArgs.Length;
			string taqn = tfn + (aname.Length > 0 ? ", " + aname : string.Empty);
			var ret = System.Type.GetType (taqn);
			if (ret == null)
				throw new XamlParseException (string.Format ("Cannot resolve runtime type from XML namespace '{0}', local name '{1}' with {2} type arguments ({3})", ns, name, typeArguments.Count, taqn));
			return genArgs == null ? ret : ret.MakeGenericType (genArgs);
		}
	}
}
