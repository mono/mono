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
using System.Reflection;
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlDirective : XamlMember
	{
		public XamlDirective (string xamlNamespace, string name)
			: base (name, null, false)
		{
			throw new NotImplementedException ();
		}

		public XamlDirective (IEnumerable<string> xamlNamespaces, string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
			: base (name, null, false)
		{
			AllowedLocation = allowedLocation;

			throw new NotImplementedException ();
		}

		public AllowedMemberLocations AllowedLocation { get; private set; }

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
		public override IList<string> GetXamlNamespaces ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed ICustomAttributeProvider LookupCustomAttributeProvider ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed IList<XamlMember> LookupDependsOn ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed XamlMemberInvoker LookupInvoker ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsAmbient ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsEvent ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsReadOnly ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsReadPublic ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsUnknown ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsWriteOnly ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed bool LookupIsWritePublic ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed XamlType LookupTargetType ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed XamlType LookupType ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed XamlValueConverter<TypeConverter> LookupTypeConverter ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed MethodInfo LookupUnderlyingGetter ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed MemberInfo LookupUnderlyingMember ()
		{
			throw new NotImplementedException ();
		}
		protected override sealed MethodInfo LookupUnderlyingSetter ()
		{
			throw new NotImplementedException ();
		}
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
