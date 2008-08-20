//
// SecurityTokenSerializer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public abstract class SecurityTokenSerializer
	{
		protected SecurityTokenSerializer ()
		{
		}

		[MonoTODO]
		public bool CanReadKeyIdentifier (XmlReader reader)
		{
			return CanReadKeyIdentifierCore (reader);
		}

		[MonoTODO]
		public bool CanReadKeyIdentifierClause (XmlReader reader)
		{
			return CanReadKeyIdentifierClauseCore (reader);
		}

		[MonoTODO]
		public bool CanReadToken (XmlReader reader)
		{
			return CanReadTokenCore (reader);
		}

		[MonoTODO]
		public SecurityKeyIdentifier ReadKeyIdentifier (
			XmlReader reader)
		{
			return ReadKeyIdentifierCore (reader);
		}

		[MonoTODO]
		public SecurityKeyIdentifierClause ReadKeyIdentifierClause (
			XmlReader reader)
		{
			return ReadKeyIdentifierClauseCore (reader);
		}

		[MonoTODO]
		public SecurityToken ReadToken (
			XmlReader reader,
			SecurityTokenResolver tokenResolver)
		{
			return ReadTokenCore (reader, tokenResolver);
		}

		[MonoTODO]
		public bool CanWriteKeyIdentifier (
			SecurityKeyIdentifier keyIdentifier)
		{
			return CanWriteKeyIdentifierCore (keyIdentifier);
		}

		[MonoTODO]
		public bool CanWriteKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			return CanWriteKeyIdentifierClauseCore (keyIdentifierClause);
		}

		[MonoTODO]
		public bool CanWriteToken (SecurityToken token)
		{
			return CanWriteTokenCore (token);
		}

		[MonoTODO]
		public void WriteKeyIdentifier (
			XmlWriter writer,
			SecurityKeyIdentifier keyIdentifier)
		{
			WriteKeyIdentifierCore (writer, keyIdentifier);
		}

		[MonoTODO]
		public void WriteKeyIdentifierClause (
			XmlWriter writer,
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			WriteKeyIdentifierClauseCore (writer, keyIdentifierClause);
		}

		[MonoTODO]
		public void WriteToken (
			XmlWriter writer, SecurityToken token)
		{
			WriteTokenCore (writer, token);
		}

		protected abstract bool CanReadKeyIdentifierClauseCore (XmlReader reader);

		protected abstract bool CanReadKeyIdentifierCore (XmlReader reader);

		protected abstract bool CanReadTokenCore (XmlReader reader);

		protected abstract SecurityKeyIdentifier ReadKeyIdentifierCore (
			XmlReader reader);

		protected abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore (
			XmlReader reader);

		protected abstract SecurityToken ReadTokenCore (
			XmlReader reader,
			SecurityTokenResolver tokenResolver);

		protected abstract bool CanWriteKeyIdentifierCore (
			SecurityKeyIdentifier keyIdentifier);

		protected abstract bool CanWriteKeyIdentifierClauseCore (
			SecurityKeyIdentifierClause keyIdentifierClause);

		protected abstract bool CanWriteTokenCore (SecurityToken token);

		protected abstract void WriteKeyIdentifierCore (
			XmlWriter writer,
			SecurityKeyIdentifier keyIdentifier);

		protected abstract void WriteKeyIdentifierClauseCore (
			XmlWriter writer,
			SecurityKeyIdentifierClause keyIdentifierClause);

		protected abstract void WriteTokenCore (
			XmlWriter writer, SecurityToken token);
	}
}
