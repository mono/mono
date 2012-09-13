//
// DynamicValidator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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

namespace System.Web.DynamicData
{
	[ToolboxBitmap (typeof(DynamicValidator), "DynamicValidator.ico")]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DynamicValidator : BaseValidator
	{
		IDynamicDataSource dynamicDataSource;

		IDynamicDataSource DynamicDataSource {
			get {
				if (dynamicDataSource == null)
					dynamicDataSource = this.FindDataSourceControl ();

				return dynamicDataSource;
			}
		}
		
		[Themeable (false)]
		[Browsable (false)]
		public MetaColumn Column { get; set; }

		[Themeable (false)]
		[Browsable (false)]
		public string ColumnName {
			get {
				// LAMESPEC: returns Column.Name if Column is not null, String.Empty
				// otherwise
				MetaColumn column = Column;
				return column != null ? column.Name : String.Empty;
			}
		}

		protected virtual Exception ValidationException { get; set; }		

		protected override bool ControlPropertiesValid ()
		{
			return base.ControlPropertiesValid () && DynamicDataSource != null;
		}

		[MonoTODO]
		protected override bool EvaluateIsValid ()
		{
			Exception ex = ValidationException;
			if (ex != null) {
				ErrorMessage = HttpUtility.HtmlEncode (ex.Message);
				return false;
			}

			string controlToValidate = ControlToValidate;
			if (String.IsNullOrEmpty (controlToValidate))
				return true;

			GetControlValidationValue (controlToValidate);

			return true;
		}

		void HandleException (object sender, DynamicValidatorEventArgs args)
		{
			if (args == null)
				return;
			
			ValidateException (args.Exception);
		}
		
		protected override void OnInit (EventArgs e)
		{
			IDynamicDataSource dds = DynamicDataSource;
			if (dds != null)
				dds.Exception += HandleException;
			
			base.OnInit (e);
		}

		[MonoTODO]
		protected virtual void ValidateException (Exception exception)
		{
			// http://forums.asp.net/p/1287649/2478409.aspx#2478409
			//
			// The above suggests that IDynamicValidatorException.InnerExceptions is
			// indexed on column name
			//
		}
	}
}
