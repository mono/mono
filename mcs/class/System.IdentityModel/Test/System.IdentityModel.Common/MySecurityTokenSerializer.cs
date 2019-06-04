//
// MySecurityTokenSerializer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Common
{
	public class MySecurityTokenSerializer : SecurityTokenSerializer
	{
		protected override bool CanReadKeyIdentifierClauseCore (XmlReader reader)
		{
			return false;
		}

		protected override bool CanReadKeyIdentifierCore (XmlReader reader)
		{
			return false;
		}

		protected override bool CanReadTokenCore (XmlReader reader)
		{
			return false;
		}

		protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore (XmlReader reader)
		{
			throw new NotSupportedException ();
		}

		protected override SecurityKeyIdentifier ReadKeyIdentifierCore (XmlReader reader)
		{
			throw new NotSupportedException ();
		}

		protected override SecurityToken ReadTokenCore (XmlReader reader, SecurityTokenResolver resolver)
		{
			throw new NotSupportedException ();
		}

		protected override bool CanWriteKeyIdentifierClauseCore (SecurityKeyIdentifierClause clause)
		{
			return false;
		}

		protected override bool CanWriteKeyIdentifierCore (SecurityKeyIdentifier ident)
		{
			return false;
		}
		protected override bool CanWriteTokenCore (SecurityToken token)
		{
			return false;
		}

		protected override void WriteKeyIdentifierClauseCore (XmlWriter writer, SecurityKeyIdentifierClause clause)
		{
			throw new NotSupportedException ();
		}

		protected override void WriteKeyIdentifierCore (XmlWriter writer, SecurityKeyIdentifier ident)
		{
			throw new NotSupportedException ();
		}
		protected override void WriteTokenCore (XmlWriter writer, SecurityToken token)
		{
			throw new NotSupportedException ();
		}
	}
}
#endif