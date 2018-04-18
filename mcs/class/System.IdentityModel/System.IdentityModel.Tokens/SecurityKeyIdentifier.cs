//
// SecurityKeyIdentifier.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Text;
using System.Xml;
using System.IdentityModel.Policy;

namespace System.IdentityModel.Tokens
{
	public class SecurityKeyIdentifier
		: IEnumerable<SecurityKeyIdentifierClause>, IEnumerable
	{
		public SecurityKeyIdentifier ()
		{
			this.list = new List<SecurityKeyIdentifierClause> ();
		}

		public SecurityKeyIdentifier (params SecurityKeyIdentifierClause [] clauses)
		{
			this.list = new List<SecurityKeyIdentifierClause> (clauses);
		}

		List<SecurityKeyIdentifierClause> list;
		bool is_readonly;

		public bool CanCreateKey {
			get {
				foreach (SecurityKeyIdentifierClause kic in this)
					if (kic.CanCreateKey)
						return true;
				return false;
			}
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		public SecurityKeyIdentifierClause this [int index] {
			get { return list [index]; }
		}

		public void Add (SecurityKeyIdentifierClause clause)
		{
			if (is_readonly)
				throw new InvalidOperationException ("This SecurityKeyIdentifier is read-only.");
			list.Add (clause);
		}

		public SecurityKey CreateKey ()
		{
			foreach (SecurityKeyIdentifierClause kic in this)
				if (kic.CanCreateKey)
					return kic.CreateKey ();
			throw new ArgumentException ("This key identifier cannot create a key");
		}

		public TClause Find<TClause> ()
			where TClause : SecurityKeyIdentifierClause
		{
			TClause kic;
			if (!TryFind<TClause> (out kic))
				throw new ArgumentException (String.Format ("{0} was not found in this SecurityKeyIdentifier", typeof (TClause)));
			return kic;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerator<SecurityKeyIdentifierClause> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void MakeReadOnly ()
		{
			is_readonly = true;
		}

		public override string ToString ()
		{
			if (Count == 0)
				return "(no key identifier clause)";
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("Total keys: {0}, ", Count);
			foreach (SecurityKeyIdentifierClause kic in this)
				sb.Append ('{').Append (kic.ToString ()).Append ('}');
			return sb.ToString ();
		}

		public bool TryFind<TClause> (out TClause clause)
			where TClause : SecurityKeyIdentifierClause
		{
			clause = default (TClause);
			foreach (SecurityKeyIdentifierClause kic in this)
				if (typeof (TClause).IsAssignableFrom (kic.GetType ())) {
					clause = (TClause) kic;
					return true;
				}
			return false;
		}
	}
}
