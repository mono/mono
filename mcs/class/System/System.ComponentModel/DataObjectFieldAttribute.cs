//
// Copyright 2006 Novell, Inc (http://www.novell.com)
//
// Authors:
//	Miguel de Icaza <miguel@novell.com>
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
using System.Text;

namespace System.ComponentModel
{
	[AttributeUsageAttribute (AttributeTargets.Property)]
	public sealed class DataObjectFieldAttribute : Attribute
	{
		bool primary_key, is_identity, is_nullable;
		int length = -1;
		
		public DataObjectFieldAttribute (bool primaryKey)
		{
			primary_key = primaryKey;
		}

		public DataObjectFieldAttribute (bool primaryKey, bool isIdentity)
		{
			primary_key = primaryKey;
			is_identity = isIdentity;
		}

		public DataObjectFieldAttribute (bool primaryKey, bool isIdentity, bool isNullable)
		{
			primary_key = primaryKey;
			is_identity = isIdentity;
			is_nullable = isNullable;
		}

		public DataObjectFieldAttribute (bool primaryKey, bool isIdentity, bool isNullable, int length)
		{
			primary_key = primaryKey;
			is_identity = isIdentity;
			is_nullable = isNullable;
			this.length = length;
		}

		public bool IsIdentity {
			get { return is_identity; }
		}

		public bool IsNullable {
			get { return is_nullable; }
		}

		public int Length {
			get { return length; }
		}

		public bool PrimaryKey {
			get { return primary_key; }
		}

		public override bool Equals (object obj)
		{
			DataObjectFieldAttribute other = obj as DataObjectFieldAttribute;
			if (other == null)
				return false;

			return other.primary_key == primary_key &&
				other.is_identity == is_identity &&
				other.is_nullable == is_nullable &&
				other.length == length;
		}

		public override int GetHashCode ()
		{
			return ((primary_key ? 1 : 0) |
				(is_identity ? 2 : 0) |
				(is_nullable ? 4 : 0)) ^
				length;
				
		}
	}
}

