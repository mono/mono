//
// MetaType.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Data.Linq.Mapping
{
	public abstract class MetaType
	{
		public abstract ReadOnlyCollection<MetaAssociation> Associations { get; }
		public abstract bool CanInstantiate { get; }
		public abstract ReadOnlyCollection<MetaDataMember> DataMembers { get; }
		public abstract MetaDataMember DBGeneratedIdentityMember { get; }
		public abstract ReadOnlyCollection<MetaType> DerivedTypes { get; }
		public abstract MetaDataMember Discriminator { get; }
		public abstract bool HasAnyLoadMethod { get; }
		public abstract bool HasAnyValidateMethod { get; }
		public abstract bool HasInheritance { get; }
		public abstract bool HasInheritanceCode { get; }
		public abstract bool HasUpdateCheck { get; }
		public abstract ReadOnlyCollection<MetaDataMember> IdentityMembers { get; }
		public abstract MetaType InheritanceBase { get; }
		public abstract object InheritanceCode { get; }
		public abstract MetaType InheritanceDefault { get; }
		public abstract MetaType InheritanceRoot { get; }
		public abstract ReadOnlyCollection<MetaType> InheritanceTypes { get; }
		public abstract bool IsEntity { get; }
		public abstract bool IsInheritanceDefault { get; }
		public abstract MetaModel Model { get; }
		public abstract string Name { get; }
		public abstract MethodInfo OnLoadedMethod { get; }
		public abstract MethodInfo OnValidateMethod { get; }
		public abstract ReadOnlyCollection<MetaDataMember> PersistentDataMembers { get; }
		public abstract MetaTable Table { get; }
		public abstract Type Type { get; }
		public abstract MetaDataMember VersionMember { get; }

		public abstract MetaDataMember GetDataMember (MemberInfo member);
		public abstract MetaType GetInheritanceType (Type type);
		public abstract MetaType GetTypeForInheritanceCode (object code);
	}
}
