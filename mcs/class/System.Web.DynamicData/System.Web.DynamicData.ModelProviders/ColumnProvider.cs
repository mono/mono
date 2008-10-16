//
// ColumnProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.DynamicData.ModelProviders
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class ColumnProvider
	{
		protected ColumnProvider (TableProvider table)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			this.Table = table;
		}

		[MonoTODO]
		public virtual AssociationProvider Association { get; protected set; }

		[MonoTODO]
		public virtual Type ColumnType { get; protected set; }

		[MonoTODO]
		public virtual PropertyInfo EntityTypeProperty { get; protected set; }

		[MonoTODO]
		public virtual bool IsCustomProperty { get; protected set; }

		[MonoTODO]
		public virtual bool IsForeignKeyComponent { get; protected set; }

		[MonoTODO]
		public virtual bool IsGenerated { get; protected set; }

		[MonoTODO]
		public virtual bool IsPrimaryKey { get; protected set; }

		[MonoTODO]
		public virtual bool IsSortable { get; protected set; }

		[MonoTODO]
		public virtual int MaxLength { get; protected set; }

		[MonoTODO]
		public virtual string Name { get; protected set; }

		[MonoTODO]
		public virtual bool Nullable { get; protected set; }

		[MonoTODO]
		public TableProvider Table { get; private set; }

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
