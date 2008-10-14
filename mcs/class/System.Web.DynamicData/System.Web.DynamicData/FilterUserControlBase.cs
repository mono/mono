//
// FilterUserControlBase.cs
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
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class FilterUserControlBase : UserControl, IControlParameterTarget
	{
		[MonoTODO]
		public MetaColumn Column { get; private set; }

		[MonoTODO]
		public string ContextTypeName { get; set; }

		[MonoTODO]
		public string DataField { get; set; }

		[MonoTODO]
		MetaColumn IControlParameterTarget.FilteredColumn {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		MetaTable IControlParameterTarget.Table {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string InitialValue { get; private set; }

		[MonoTODO]
		public virtual DataKey SelectedDataKey { get; private set; }

		[MonoTODO]
		public virtual string SelectedValue { get; private set; }

		[MonoTODO]
		public string TableName { get; set; }

		[MonoTODO]
		string IControlParameterTarget.GetPropertyNameExpression (string columnName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PopulateListControl (ListControl listControl)
		{
			throw new NotImplementedException ();
		}
	}
}
