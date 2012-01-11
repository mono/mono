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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlDirective : XamlMember
	{
		class DirectiveMemberInvoker : XamlMemberInvoker
		{
			public DirectiveMemberInvoker (XamlDirective directive)
				: base (directive)
			{
			}
		}

		public XamlDirective (string xamlNamespace, string name)
			: this (new string [] {xamlNamespace}, name, new XamlType (typeof (object), new XamlSchemaContext (new XamlSchemaContextSettings ())), null, AllowedMemberLocations.Any)
		{
			if (xamlNamespace == null)
				throw new ArgumentNullException ("xamlNamespace");
			is_unknown = true;
		}

		public XamlDirective (IEnumerable<string> xamlNamespaces, string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
			: base (true, xamlNamespaces != null ? xamlNamespaces.FirstOrDefault () : null, name)
		{
			if (xamlNamespaces == null)
				throw new ArgumentNullException ("xamlNamespaces");
			if (xamlType == null)
				throw new ArgumentNullException ("xamlType");

			type = xamlType;
			xaml_namespaces = new List<string> (xamlNamespaces);
			AllowedLocation = allowedLocation;
			type_converter = typeConverter;
			
			invoker = new DirectiveMemberInvoker (this);
		}

		public AllowedMemberLocations AllowedLocation { get; private set; }
		XamlValueConverter<TypeConverter> type_converter;
		XamlType type;
		XamlMemberInvoker invoker;
		bool is_unknown;
		IList<string> xaml_namespaces;

		// this is for XamlLanguage.UnknownContent
		internal bool InternalIsUnknown {
			set { is_unknown = value; }
		}

		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		public override IList<string> GetXamlNamespaces ()
		{
			return xaml_namespaces;
		}

		protected override sealed ICustomAttributeProvider LookupCustomAttributeProvider ()
		{
			return null; // as documented.
		}

		protected override sealed XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader ()
		{
			return null; // as documented.
		}

		protected override sealed IList<XamlMember> LookupDependsOn ()
		{
			return null; // as documented.
		}

		protected override sealed XamlMemberInvoker LookupInvoker ()
		{
			return invoker;
		}

		protected override sealed bool LookupIsAmbient ()
		{
			return false;
		}

		protected override sealed bool LookupIsEvent ()
		{
			return false;
		}

		protected override sealed bool LookupIsReadOnly ()
		{
			return false;
		}

		protected override sealed bool LookupIsReadPublic ()
		{
			return true;
		}

		protected override sealed bool LookupIsUnknown ()
		{
			return is_unknown;
		}

		protected override sealed bool LookupIsWriteOnly ()
		{
			return false;
		}

		protected override sealed bool LookupIsWritePublic ()
		{
			return true;
		}

		protected override sealed XamlType LookupTargetType ()
		{
			return null;
		}

		protected override sealed XamlType LookupType ()
		{
			return type;
		}

		protected override sealed XamlValueConverter<TypeConverter> LookupTypeConverter ()
		{
			if (type_converter == null)
				type_converter = base.LookupTypeConverter ();
			return type_converter;
		}

		protected override sealed MethodInfo LookupUnderlyingGetter ()
		{
			return null;
		}

		protected override sealed MemberInfo LookupUnderlyingMember ()
		{
			return null;
		}

		protected override sealed MethodInfo LookupUnderlyingSetter ()
		{
			return null;
		}

		public override string ToString ()
		{
			return String.IsNullOrEmpty (PreferredXamlNamespace) ? Name : String.Concat ("{", PreferredXamlNamespace, "}", Name);
		}
	}
}
