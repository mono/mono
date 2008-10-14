//
// FieldTemplateUserControl.cs
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
	public class FieldTemplateUserControl : UserControl, IBindableControl, IFieldTemplate
	{
		[MonoTODO]
		public MetaChildrenColumn ChildrenColumn { get; private set; }
		[MonoTODO]
		protected string ChildrenPath { get; private set; }
		[MonoTODO]
		public MetaColumn Column { get; private set; }
		[MonoTODO]
		public virtual Control DataControl { get; private set; }
		[MonoTODO]
		public virtual object FieldValue { get; set; }
		[MonoTODO]
		public virtual string FieldValueEditString { get; private set; }
		[MonoTODO]
		public virtual string FieldValueString { get; private set; }
		[MonoTODO]
		public MetaForeignKeyColumn ForeignKeyColumn { get; private set; }
		[MonoTODO]
		protected string ForeignKeyPath { get; private set; }
		[MonoTODO]
		public IFieldFormattingOptions FormattingOptions { get; private set; }
		[MonoTODO]
		public IFieldTemplateHost Host { get; private set; }
		[MonoTODO]
		public System.ComponentModel.AttributeCollection MetadataAttributes { get; private set; }
		[MonoTODO]
		public DataBoundControlMode Mode { get; private set; }
		[MonoTODO]
		public virtual object Row { get; private set; }
		[MonoTODO]
		public MetaTable Table { get; private set; }

		[MonoTODO]
		protected string BuildChildrenPath (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected string BuildForeignKeyPath (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object ConvertEditedValue (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ExtractForeignKey (IDictionary dictionary, string selectedValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ExtractValues (IOrderedDictionary dictionary)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected FieldTemplateUserControl FindOtherFieldTemplate (string columnName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string FormatFieldValue (object fieldValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object GetColumnValue (MetaColumn column)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IBindableControl.ExtractValues (IOrderedDictionary dictionary)
		{
			ExtractValues (dictionary);
		}

		[MonoTODO]
		void IFieldTemplate.SetHost (IFieldTemplateHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void IgnoreModelValidationAttribute (Type attributeType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void PopulateListControl (ListControl listControl)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void SetUpValidator (BaseValidator validator)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void SetUpValidator (BaseValidator validator, MetaColumn column)
		{
			throw new NotImplementedException ();
		}
	}
}
