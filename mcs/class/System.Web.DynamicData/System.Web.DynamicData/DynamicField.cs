//
// DynamicField.cs
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
	public class DynamicField : DataControlField, IAttributeAccessor, IFieldFormattingOptions
	{
		[MonoTODO]
		public DynamicField ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ApplyFormatInEditMode { get; set; }
		[MonoTODO]
		public bool ConvertEmptyStringToNull { get; set; }
		[MonoTODO]
		public virtual string DataField { get; set; }
		[MonoTODO]
		public string DataFormatString { get; set; }
		[MonoTODO]
		public override string HeaderText { get; set; }
		[MonoTODO]
		public bool HtmlEncode { get; set; }
		[MonoTODO]
		public string NullDisplayText { get; set; }
		[MonoTODO]
		public override string SortExpression { get; set; }
		[MonoTODO]
		public virtual string UIHint { get; set; }

		[MonoTODO]
		protected override void CopyProperties (DataControlField newField)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DataControlField CreateField ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void ExtractValuesFromCell (IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetAttribute (string key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void InitializeCell (DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetAttribute (string key, string value)
		{
			throw new NotImplementedException ();
		}
	}
}
