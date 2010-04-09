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
using System.Reflection;
using System.Xaml.Schema;
using System.Windows.Markup;

namespace System.Xaml
{
	public static class XamlLanguage
	{
		public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
		public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";

		static readonly XamlSchemaContext sctx = new XamlSchemaContext (null, null);

		static XamlType XT<T> ()
		{
			return new XamlType (typeof (T), sctx);
		}

		static XamlLanguage ()
		{
			// types

			Array = new XamlType (typeof (ArrayExtension), sctx);
			Boolean = new XamlType (typeof (bool), sctx);
			Byte = new XamlType (typeof (byte), sctx);
			Char = new XamlType (typeof (char), sctx);
			Decimal = new XamlType (typeof (decimal), sctx);
			Double = new XamlType (typeof (double), sctx);
			Int16 = new XamlType (typeof (short), sctx);
			Int32 = new XamlType (typeof (int), sctx);
			Int64 = new XamlType (typeof (long), sctx);
			Member = new XamlType (typeof (MemberDefinition), sctx);
			Null = new XamlType (typeof (NullExtension), sctx);
			Object = new XamlType (typeof (object), sctx);
			Property = new XamlType (typeof (PropertyDefinition), sctx);
			Reference = new XamlType (typeof (Reference), sctx);
			Single = new XamlType (typeof (float), sctx);
			Static = new XamlType (typeof (StaticExtension), sctx);
			String = new XamlType (typeof (string), sctx);
			TimeSpan = new XamlType (typeof (TimeSpan), sctx);
			Type = new XamlType (typeof (TypeExtension), sctx);
			Uri = new XamlType (typeof (Uri), sctx);
			XData = new XamlType (typeof (XData), sctx);

			AllTypes = new ReadOnlyCollection<XamlType> (new XamlType [] {Array, Boolean, Byte, Char, Decimal, Double, Int16, Int32, Int64, Member, Null, Object, Property, Reference, Single, Static, String, TimeSpan, Type, Uri, XData});

			// directives

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
		}

		public static IList<string> XamlNamespaces {
			get { throw new NotImplementedException (); }
		}

		public static IList<string> XmlNamespaces {
			get { throw new NotImplementedException (); }
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
	}
}
