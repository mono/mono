//
// System.Web.UI.BaseTemplateParser
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) 2006-2010 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;

namespace System.Web.UI
{
	public abstract class BaseTemplateParser : TemplateParser
	{		
		protected BaseTemplateParser ()
		{
			
		}

		protected Type GetReferencedType (string virtualPath)
		{
			if (String.IsNullOrEmpty (virtualPath))
				throw new ArgumentNullException ("virtualPath");
			
			var pageParserFilter = PageParserFilter;
			if (pageParserFilter != null) {
				var cfg = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
				if (cfg == null)
					throw new HttpException ("Internal error. Missing configuration section.");

				string extension = VirtualPathUtility.GetExtension (virtualPath);
				Type btype = cfg.BuildProviders.GetProviderTypeForExtension (extension);
				VirtualReferenceType reftype;

				if (btype == null)
					reftype = VirtualReferenceType.Other;
				else if (btype == typeof (PageBuildProvider))
					reftype = VirtualReferenceType.Page;
				else if (btype == typeof (UserControlBuildProvider))
					reftype = VirtualReferenceType.UserControl;
				else if (btype == typeof (MasterPageBuildProvider))
					reftype = VirtualReferenceType.Master;
				else
					reftype = VirtualReferenceType.SourceFile;
				
				if (!pageParserFilter.AllowVirtualReference (virtualPath, reftype))
					throw new HttpException ("The parser does not permit a virtual reference to the UserControl.");
			}
			
			return BuildManager.GetCompiledType (virtualPath);
		}

		[MonoTODO ("We don't do anything here with the no-compile controls.")]
		protected internal Type GetUserControlType (string virtualPath)
		{
			// Documented as a wrapper for the call below, but what does it do?
			return GetReferencedType (virtualPath);
		}
	}
}
