//
// System.Web.Compilation.PageBuildProvider
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2006-2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation {

	[BuildProviderAppliesTo (BuildProviderAppliesTo.Web)]
	sealed class PageBuildProvider : TemplateBuildProvider {
		
		public PageBuildProvider()
		{
		}

		protected override string MapPath (VirtualPath virtualPath)
		{
			// We need this hack to support out-of-application wsdl helpers
			if (virtualPath.IsFake)
				return virtualPath.PhysicalPath;

			return base.MapPath (virtualPath);
		}               

		protected override TextReader SpecialOpenReader (VirtualPath virtualPath, out string physicalPath)
		{
			// We need this hack to support out-of-application wsdl helpers
			if (virtualPath.IsFake) {
				physicalPath = virtualPath.PhysicalPath;
				return new StreamReader (physicalPath);
			} else
				physicalPath = null;
			
			return base.SpecialOpenReader (virtualPath, out physicalPath);
		}
		
		protected override BaseCompiler CreateCompiler (TemplateParser parser)
		{
			return new PageCompiler (parser as PageParser);
		}

		protected override TemplateParser CreateParser (VirtualPath virtualPath, string physicalPath, HttpContext context)
		{	
			return CreateParser (virtualPath, physicalPath, OpenReader (virtualPath.Original), context);
		}
		
		protected override TemplateParser CreateParser (VirtualPath virtualPath, string physicalPath, TextReader reader, HttpContext context)
		{
			return new PageParser (virtualPath, physicalPath, reader, context);
		}
	}
}
#endif

