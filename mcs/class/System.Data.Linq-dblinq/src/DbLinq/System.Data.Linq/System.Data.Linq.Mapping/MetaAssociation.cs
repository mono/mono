//
// MetaAssociation.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

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
using System.Collections.ObjectModel;

namespace System.Data.Linq.Mapping
{
	public abstract class MetaAssociation
	{
		public abstract bool DeleteOnNull { get; }
		public abstract string DeleteRule { get; }
		public abstract bool IsForeignKey { get; }
		public abstract bool IsMany { get; }
		public abstract bool IsNullable { get; }
		public abstract bool IsUnique { get; }
		public abstract ReadOnlyCollection<MetaDataMember> OtherKey { get; }
		public abstract bool OtherKeyIsPrimaryKey { get; }
		public abstract MetaDataMember OtherMember { get; }
		public abstract MetaType OtherType { get; }
		public abstract ReadOnlyCollection<MetaDataMember> ThisKey { get; }
		public abstract bool ThisKeyIsPrimaryKey { get; }
		public abstract MetaDataMember ThisMember { get; }
	}
}
