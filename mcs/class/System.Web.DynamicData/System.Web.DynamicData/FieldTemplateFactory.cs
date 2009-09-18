//
// FieldTemplateFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class FieldTemplateFactory : IFieldTemplateFactory
	{
		const string DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH = "FieldTemplates/";

		static readonly Dictionary <Type, Type> typeFallbacks = new Dictionary <Type, Type> () {
			{typeof (float), typeof (decimal)},
			{typeof (double), typeof (decimal)},
			{typeof (short), typeof (int)},
			{typeof (long), typeof (int)},
			{typeof (byte), typeof (int)},
			{typeof (char), typeof (string)},
			{typeof (int), typeof (string)},
			{typeof (decimal), typeof (string)},
			{typeof (Guid), typeof (string)},
			{typeof (DateTime), typeof (string)},
			{typeof (DateTimeOffset), typeof (string)},
			{typeof (TimeSpan), typeof (string)}
		};
		
		string templateFolderVirtualPath;
		string userTemplateVirtualPath;
		
		public MetaModel Model { get; private set; }

		public string TemplateFolderVirtualPath {
			get {
				if (templateFolderVirtualPath == null) {
					MetaModel m = Model;
					string virtualPath = userTemplateVirtualPath == null ? DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH : userTemplateVirtualPath;

					if (m != null)
						templateFolderVirtualPath = VirtualPathUtility.Combine (m.DynamicDataFolderVirtualPath, virtualPath);
					else
						templateFolderVirtualPath = virtualPath;

					templateFolderVirtualPath = VirtualPathUtility.AppendTrailingSlash (templateFolderVirtualPath);
				}

				return templateFolderVirtualPath;
			}
			
			set {
				userTemplateVirtualPath = value;
				templateFolderVirtualPath = null;
			}
		}

		public virtual string BuildVirtualPath (string templateName, MetaColumn column, DataBoundControlMode mode)
		{
			// Tests show the 'column' parameter is not used here
			
			if (String.IsNullOrEmpty (templateName))
				throw new ArgumentNullException ("templateName");

			string basePath = TemplateFolderVirtualPath;
			string suffix;

			switch (mode) {
				default:
				case DataBoundControlMode.ReadOnly:
					suffix = String.Empty;
					break;

				case DataBoundControlMode.Edit:
					suffix = "_Edit";
					break;

				case DataBoundControlMode.Insert:
					suffix = "_Insert";
					break;
			}
			
			return basePath + templateName + suffix + ".ascx";
		}

		public virtual IFieldTemplate CreateFieldTemplate (MetaColumn column, DataBoundControlMode mode, string uiHint)
		{
			// NO checks are made on parameters in .NET, but well "handle" the NREX
			// throws in the other methods
			string virtualPath = GetFieldTemplateVirtualPath (column, mode, uiHint);
			if (String.IsNullOrEmpty (virtualPath))
				return null;
			
			return BuildManager.CreateInstanceFromVirtualPath (virtualPath, typeof (IFieldTemplate)) as IFieldTemplate;
		}

		public virtual string GetFieldTemplateVirtualPath (MetaColumn column, DataBoundControlMode mode, string uiHint)
		{
			// NO checks are made on parameters in .NET, but well "handle" the NREX
			// throws in the other methods
			DataBoundControlMode newMode = PreprocessMode (column, mode);

			// The algorithm is as follows:
			//
			//  1. If column has a DataTypeAttribute on it, get the data type
			//     - if it's Custom data type, uiHint is used unconditionally
			//     - if it's not a custom type, ignore uiHint and choose template based
			//       on type
			//
			//  2. If #1 is false and uiHint is not empty, use uiHint if the template
			//     exists
			//
			//  3. If #2 is false, look up type according to the following algorithm:
			//
			//     1. lookup column type's full name
			//     2. if #1 fails, look up short type name
			//     3. if #2 fails, map type to special type name (Int -> Integer, String
			//        -> Text etc)
			//     4. if #3 fails, try to find a fallback type
			//     5. if #4 fails, check if it's a foreign key or child column
			//     6. if #5 fails, return null
			//
			//     From: http://msdn.microsoft.com/en-us/library/cc488523.aspx (augmented)
			//

			DataTypeAttribute attr = column.DataTypeAttribute;
			bool uiHintPresent = !String.IsNullOrEmpty (uiHint);
			string templatePath = null;
			int step = uiHintPresent ? 0 : 1;
			Type columnType = column.ColumnType;

			if (!uiHintPresent && attr == null) {
				if (column is MetaChildrenColumn)
					templatePath = GetExistingTemplateVirtualPath ("Children", column, newMode);
				else if (column is MetaForeignKeyColumn)
					templatePath = GetExistingTemplateVirtualPath ("ForeignKey", column, newMode);
			}
			
			while (step < 6 && templatePath == null) {
				switch (step) {
					case 0:
						templatePath = GetExistingTemplateVirtualPath (uiHint, column, newMode);
						break;

					case 1:
						if (attr != null)
							templatePath = GetTemplateForDataType (attr.DataType, attr.GetDataTypeName (), uiHint, column, newMode);
						break;
							
					case 2:
						templatePath = GetExistingTemplateVirtualPath (columnType.FullName, column, newMode);
						break;

					case 3:
						templatePath = GetExistingTemplateVirtualPath (columnType.Name, column, newMode);
						break;

					case 4:
						templatePath = ColumnTypeToSpecialName (columnType, column, newMode);
						break;

					case 5:
						columnType = GetFallbackType (columnType, column, newMode);
						if (columnType == null)
							step = 5;
						else
							step = uiHintPresent ? 0 : 1;
						break;
				}

				step++;
			}

			return templatePath;
		}

		Type GetFallbackType (Type columnType, MetaColumn column, DataBoundControlMode mode)
		{
			Type ret;
			if (typeFallbacks.TryGetValue (columnType, out ret))
				return ret;
			
			return null;
		}
		
		string ColumnTypeToSpecialName (Type columnType, MetaColumn column, DataBoundControlMode mode)
		{
			if (columnType == typeof (int))
				return GetExistingTemplateVirtualPath ("Integer", column, mode);

			if (columnType == typeof (string))
				return GetExistingTemplateVirtualPath ("Text", column, mode);
			
			return null;
		}
		
		string GetExistingTemplateVirtualPath (string baseName, MetaColumn column, DataBoundControlMode mode)
		{
			string templatePath = BuildVirtualPath (baseName, column, mode);
			if (String.IsNullOrEmpty (templatePath))
				return null;

			// TODO: cache positive hits (and watch for removal events on those)
			string physicalPath = HostingEnvironment.MapPath (templatePath);
			if (File.Exists (physicalPath))
				return templatePath;

			return null;
		}
		
		string GetTemplateForDataType (DataType dataType, string customDataType, string uiHint, MetaColumn column, DataBoundControlMode mode)
		{
			switch (dataType) {
				case DataType.Custom:
					return GetExistingTemplateVirtualPath (customDataType, column, mode);

				case DataType.DateTime:
					return GetExistingTemplateVirtualPath ("DateTime", column, mode);

				case DataType.MultilineText:
					return GetExistingTemplateVirtualPath ("MultilineText", column, mode);
					
				default:
					return GetExistingTemplateVirtualPath ("Text", column, mode);
			}
		}
		
		public virtual void Initialize (MetaModel model)
		{
			Model = model;
		}

		public virtual DataBoundControlMode PreprocessMode (MetaColumn column, DataBoundControlMode mode)
		{
			// In good tradition of .NET's DynamicData, let's not check the
			// parameters...
			if (column == null)
				throw new NullReferenceException ();

			if (column.IsGenerated)
				return DataBoundControlMode.ReadOnly;
			
			if (column.IsPrimaryKey) {
				if (mode == DataBoundControlMode.Edit)
					return DataBoundControlMode.ReadOnly;
			}
			
			return mode;	
		}
	}
}
