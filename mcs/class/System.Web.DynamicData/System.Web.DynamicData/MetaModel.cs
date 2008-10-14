//
// MetaModel.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaModel
	{
		static MetaModel default_model;

		[MonoTODO]
		public static MetaModel Default {
			get { throw new NotImplementedException (); }
			internal set { default_model = value; }
		}

		[MonoTODO]
		public string DynamicDataFolderVirtualPath { get; set; }
		[MonoTODO]
		public IFieldTemplateFactory FieldTemplateFactory { get; set; }
		[MonoTODO]
		public ReadOnlyCollection<MetaTable> Tables { get; private set; }
		[MonoTODO]
		public List<MetaTable> VisibleTables { get; private set; }

		[MonoTODO]
		public string GetActionPath (string tableName, string action, object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MetaModel GetModel (Type contextType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MetaTable GetTable (string uniqueTableName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MetaTable GetTable (Type entityType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MetaTable GetTable (string tableName, Type contextType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterContext (Func<object> contextFactory)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterContext (Type contextType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterContext (DataModelProvider dataModelProvider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterContext (Func<object> contextFactory, ContextConfiguration configuration)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterContext (Type contextType, ContextConfiguration configuration)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterContext (DataModelProvider dataModelProvider, ContextConfiguration configuration)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ResetRegistrationException ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool TryGetTable (string uniqueTableName, out MetaTable table)
		{
			throw new NotImplementedException ();
		}
	}
}
