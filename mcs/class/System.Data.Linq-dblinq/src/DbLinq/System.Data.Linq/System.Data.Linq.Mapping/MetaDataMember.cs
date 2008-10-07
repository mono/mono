//
// MetaDataMember.cs
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
using System.Reflection;

namespace System.Data.Linq.Mapping
{
	public abstract class MetaDataMember
	{
		public abstract MetaAssociation Association { get; }
		public abstract AutoSync AutoSync { get; }
		public abstract bool CanBeNull { get; }
		public abstract string DbType { get; }
		public abstract MetaType DeclaringType { get; }
		public abstract MetaAccessor DeferredSourceAccessor { get; }
		public abstract MetaAccessor DeferredValueAccessor { get; }
		public abstract string Expression { get; }
		public abstract bool IsAssociation { get; }
		public abstract bool IsDbGenerated { get; }
		public abstract bool IsDeferred { get; }
		public abstract bool IsDiscriminator { get; }
		public abstract bool IsPersistent { get; }
		public abstract bool IsPrimaryKey { get; }
		public abstract bool IsVersion { get; }
		public abstract MethodInfo LoadMethod { get; }
		public abstract string MappedName { get; }
		public abstract MemberInfo Member { get; }
		public abstract MetaAccessor MemberAccessor { get; }
		public abstract string Name { get; }
		public abstract int Ordinal { get; }
		public abstract MetaAccessor StorageAccessor { get; }
		public abstract MemberInfo StorageMember { get; }
		public abstract Type Type { get; }
		public abstract UpdateCheck UpdateCheck { get; }

		public abstract bool IsDeclaredBy (MetaType type);
	}
}
