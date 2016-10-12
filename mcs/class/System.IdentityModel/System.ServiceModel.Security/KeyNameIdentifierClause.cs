//
// KeyNameIdentifierClause.cs
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
using System;
using System.Collections.Generic;
using System.Xml;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security
{
	public class KeyNameIdentifierClause : SecurityKeyIdentifierClause
	{
		public KeyNameIdentifierClause (string keyName)
			: base (null)
		{
			key_name = keyName;
		}

		string key_name;

		public string KeyName {
			get { return key_name; }
		}

		public override bool Matches (SecurityKeyIdentifierClause clause)
		{
			if (clause == null)
				throw new ArgumentNullException ("clause");
			KeyNameIdentifierClause knic =
				clause as KeyNameIdentifierClause;
			return knic != null && Matches (knic.KeyName);
		}

		public bool Matches (string keyName)
		{
			return key_name == keyName;
		}

		public override string ToString ()
		{
			return String.Concat ("KeyNameIdentifierClause(KeyName = '", KeyName, "')");
		}
	}
}
