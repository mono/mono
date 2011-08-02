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
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	internal class UserControlParser : TemplateControlParser
	{
		string masterPage;
#if NET_4_0
		string providerName;
#endif
		internal UserControlParser (VirtualPath virtualPath, string inputFile, HttpContext context)
			: this (virtualPath, inputFile, context, null)
		{
		}

		internal UserControlParser (VirtualPath virtualPath, string inputFile, List <string> deps, HttpContext context)
			: this (virtualPath, inputFile, context, null)
		{
			this.Dependencies = deps;
		}

		internal UserControlParser (VirtualPath virtualPath, string inputFile, HttpContext context, string type)
		{
			VirtualPath = virtualPath;
			Context = context;
			BaseVirtualDir = virtualPath.DirectoryNoNormalize;
			InputFile = inputFile;
			SetBaseType (type);
			AddApplicationAssembly ();
			LoadConfigDefaults ();
		}

		internal UserControlParser (VirtualPath virtualPath, TextReader reader, HttpContext context)
			: this (virtualPath, null, reader, context)
		{
		}
		
		internal UserControlParser (VirtualPath virtualPath, string inputFile, TextReader reader, HttpContext context)
		{
			VirtualPath = virtualPath;
			Context = context;
			BaseVirtualDir = virtualPath.DirectoryNoNormalize;
			
			if (String.IsNullOrEmpty (inputFile))
				InputFile = virtualPath.PhysicalPath;
			else
				InputFile = inputFile;
			
			Reader = reader;
			SetBaseType (null);
			AddApplicationAssembly ();
			LoadConfigDefaults ();
		}

		internal UserControlParser (TextReader reader, int? uniqueSuffix, HttpContext context)
		{
			Context = context;

			string fpath = context.Request.FilePath;
			VirtualPath = new VirtualPath (fpath);
			BaseVirtualDir = VirtualPathUtility.GetDirectory (fpath, false);

			// We're probably being called by ParseControl - let's use the requested
			// control's path plus unique suffix as our input file, since that's the
			// context we're being invoked from.
			InputFile = VirtualPathUtility.GetFileName (fpath) + "#" + (uniqueSuffix != null ? ((int)uniqueSuffix).ToString ("x") : "0");
			Reader = reader;
			SetBaseType (null);
			AddApplicationAssembly ();
			LoadConfigDefaults ();
		}		

		internal static Type GetCompiledType (TextReader reader, int? inputHashCode, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (reader, inputHashCode, context);
			return ucp.CompileIntoType ();
		}
		
		internal static Type GetCompiledType (string virtualPath, string inputFile, List <string> deps, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (new VirtualPath (virtualPath), inputFile, deps, context);

			return ucp.CompileIntoType ();
		}

		public static Type GetCompiledType (string virtualPath, string inputFile, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (new VirtualPath (virtualPath), inputFile, context);

			return ucp.CompileIntoType ();
		}

		internal override Type CompileIntoType ()
		{
			AspGenerator generator = new AspGenerator (this);
			return generator.GetCompiledType ();
		}

		internal override void ProcessMainAttributes (IDictionary atts)
		{
			masterPage = GetString (atts, "MasterPageFile", null);
			if (masterPage != null)
				AddDependency (masterPage);

			base.ProcessMainAttributes (atts);
		}
#if NET_4_0
		internal override void ProcessOutputCacheAttributes (IDictionary atts)
		{
			providerName = GetString (atts, "ProviderName", null);
			base.ProcessOutputCacheAttributes (atts);
		}

		internal override Type DefaultBaseType {
			get {
				Type ret = PageParser.DefaultUserControlBaseType;
				if (ret == null)
					return base.DefaultBaseType;

				return ret;
			}
		}
#endif
		internal override string DefaultBaseTypeName {
			get { return PagesConfig.UserControlBaseType; }
		}

		internal override string DefaultDirectiveName {
			get { return "control"; }
		}

		internal string MasterPageFile {
			get { return masterPage; }
		}
#if NET_4_0
		internal string ProviderName {
			get { return providerName; }
		}
#endif
	}
}

