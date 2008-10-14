//
// FilterRepeater.cs
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
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[ParseChildren (true)]
//	[ToolboxItem]
	public class FilterRepeater : Repeater, IWhereParametersProvider
	{
		[MonoTODO]
		[Themeable (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		public string ContextTypeName { get; set; }

		[MonoTODO]
		[Themeable (false)]
		[Category ("Behavior")]
		[DefaultValue ("DynamicFilter")]
		[IDReferenceProperty (typeof (FilterUserControlBase))]
		public string DynamicFilterContainerId { get; set; }

		[MonoTODO]
		public MetaTable Table { get; private set; }

		[MonoTODO]
		[Themeable (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		public string TableName { get; set; }

		[MonoTODO]
		public override bool Visible { get; set; }

		[MonoTODO]
		public override void DataBind ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual IEnumerable<MetaColumn> GetFilteredColumns ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IEnumerable<Parameter> GetWhereParameters (IDynamicDataSource dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnFilterItemCreated (RepeaterItem item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnInit (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnItemCreated (RepeaterItemEventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}
