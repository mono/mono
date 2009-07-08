//
// DynamicControl.cs
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
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[ToolboxBitmap (typeof(DynamicControl), "DynamicControl.ico")]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DynamicControl : Control, IAttributeAccessor, IFieldTemplateHost, IFieldFormattingOptions
	{
		Dictionary <string, string> attributes;
		
		public DynamicControl ()
		{
		}

		[MonoTODO]
		public DynamicControl (DataBoundControlMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool ApplyFormatInEditMode { get; set; }

		[Browsable (false)]
		[MonoTODO]
		public MetaColumn Column { get; set; }

		[MonoTODO]
		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool ConvertEmptyStringToNull { get; set; }

		[MonoTODO]
		[Category ("Appearance")]
		[DefaultValue ("")]
		[CssClassProperty]
		public virtual string CssClass { get; set; }

		[MonoTODO]
		[Category ("Data")]
		[DefaultValue ("")]
		public string DataField { get; set; }

		[MonoTODO]
		[Category ("Data")]
		[DefaultValue ("")]
		public string DataFormatString { get; set; }

		[MonoTODO]
		[Browsable (false)]
		public Control FieldTemplate { get; private set; }

		[MonoTODO]
		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool HtmlEncode { get; set; }

		[MonoTODO]
		IFieldFormattingOptions IFieldTemplateHost.FormattingOptions {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataBoundControlMode Mode { get; set; }

		[MonoTODO]
		[Category ("Behavior")]
		[DefaultValue ("")]
		public string NullDisplayText { get; set; }

		[MonoTODO]
		[Browsable (false)]
		public virtual MetaTable Table { get; private set; }

		[MonoTODO]
		[Category ("Behavior")]
		[DefaultValue ("")]
		public virtual string UIHint { get; set; }

		[MonoTODO]
		[Themeable (false)]
		[Category ("Behavior")]
		[DefaultValue ("")]
		public virtual string ValidationGroup { get; set; }
		
		public string GetAttribute (string key)
		{
			if (attributes == null)
				return null;

			string ret;
			if (attributes.TryGetValue (key, out ret))
				return ret;

			return null;
		}

		[MonoTODO]
		protected override void OnInit (EventArgs e)
		{
			// It seems _all_ the properties are initialized _only_ here. Further user's
			// actions to set the Column property don't affect the other properties
			// which derive their values from the associated MetaColumn.
			base.OnInit (e);
		}

		protected override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);
			// Why override?
		}

		internal void InternalSetAttributes (Dictionary <string, string> attributes)
		{
			this.attributes = attributes;
		}
		
		public void SetAttribute (string key, string value)
		{
			if (attributes == null)
				attributes = new Dictionary <string, string> ();

			if (attributes.ContainsKey (key))
				attributes [key] = value;
			else
				attributes.Add (key, value);
		}
	}
}
