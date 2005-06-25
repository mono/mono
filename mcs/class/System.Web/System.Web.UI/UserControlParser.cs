//
// System.Web.UI.UserControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	internal class UserControlParser : TemplateControlParser
	{
		internal UserControlParser (string virtualPath, string inputFile, HttpContext context)
			: this (virtualPath, inputFile, context, null)
		{
		}

		internal UserControlParser (string virtualPath, string inputFile, ArrayList deps, HttpContext context)
			: this (virtualPath, inputFile, context, null)
		{
			this.Dependencies = deps;
		}
		
		internal UserControlParser (string virtualPath, string inputFile, HttpContext context, string type)
		{
			if (type == null) type = PagesConfig.UserControlBaseType;
			Context = context;
			BaseVirtualDir = UrlUtils.GetDirectory (virtualPath);
			InputFile = inputFile;
			SetBaseType (type);
			AddApplicationAssembly ();
		}
		
		internal static Type GetCompiledType (string virtualPath, string inputFile, ArrayList deps, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (virtualPath, inputFile, deps, context);
			return ucp.CompileIntoType ();
		}

		public static Type GetCompiledType (string virtualPath, string inputFile, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (virtualPath, inputFile, context);
			return ucp.CompileIntoType ();
		}

		protected override Type CompileIntoType ()
		{
			AspGenerator generator = new AspGenerator (this);
			return generator.GetCompiledType ();
		}

		internal override void ProcessMainAttributes (Hashtable atts)
		{
			base.ProcessMainAttributes (atts);
		}
		
		internal override Type DefaultBaseType {
			get { return typeof (UserControl); }
		}

		internal override string DefaultBaseTypeName {
			get { return "System.Web.UI.UserControl"; }
		}

		internal override string DefaultDirectiveName {
			get { return "control"; }
		}
	}
}

