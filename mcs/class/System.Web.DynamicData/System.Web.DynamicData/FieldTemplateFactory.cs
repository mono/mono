//
// FieldTemplateFactory.cs
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
using System.Web.UI.WebControls;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class FieldTemplateFactory : IFieldTemplateFactory
	{
		const string DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH = "FieldTemplates/";
		
		string templateFolderVirtualPath;
		
		public MetaModel Model { get; private set; }

		public string TemplateFolderVirtualPath {
			get {
				if (templateFolderVirtualPath) {
					MetaModel m = Model;

					templateFolderVirtualPath = m != null ?
						m.DynamicDataFolderVirtualPath + DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH :
						DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH;
				}

				return templateFolderVirtualPath;
			}
			
			set {
				MetaModel m = Model;

				if (m != null) {
					if (value == null)
						templateFolderVirtualPath = m.DynamicDataFolderVirtualPath + DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH;
					else if (value.Length == 0)
						// "compatibility"
						throw new ArgumentNullException ("value");
					else
						templateFolderVirtualPath = m.DynamicDataFolderVirtualPath + value;
				} else {
					if (value == null)
						templateFolderVirtualPath = DEFAULT_TEMPLATE_FOLDER_VIRTUAL_PATH;
					else
						templateFolderVirtualPath = value;
				}

				templateFolderVirtualPath = VirtualPathUtility.AppendTrailingSlash (templateFolderVirtualPath);
			}
		}

		[MonoTODO]
		public virtual string BuildVirtualPath (string templateName, MetaColumn column, DataBoundControlMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IFieldTemplate CreateFieldTemplate (MetaColumn column, DataBoundControlMode mode, string uiHint)
		{
			// NO checks are made on parameters in .NET, but well "handle" the NREX
			// throws in the other methods
			DataBoundControlMode newMode = PreprocessMode (column, mode);
			string path = null;
			
			return BuildManager.CreateInstanceFromVirtualPath (path, typeof (IFieldTemplate));
		}

		[MonoTODO]
		public virtual string GetFieldTemplateVirtualPath (MetaColumn column, DataBoundControlMode mode, string uiHint)
		{
			// NO checks are made on parameters in .NET, but well "handle" the NREX
			// throws in the other methods
			DataBoundControlMode newMode = PreprocessMode (column, mode);
			throw new NotImplementedException ();
		}
		
		public virtual void Initialize (MetaModel model)
		{
			Model = model;
		}

		[MonoTODO]
		public virtual DataBoundControlMode PreprocessMode (MetaColumn column, DataBoundControlMode mode)
		{
			// In good tradition of .NET's DynamicData, let's not check the
			// parameters...
			if (column == null)
				throw new NullReferenceException ();
			
			throw new NotImplementedException ();
		}
	}
}
