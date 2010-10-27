//
// System.Web.Compilation.TemplateBuildProvider
//
// Authors:
//   Chris Toshok (toshok@ximian.com)
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008-2010 Novell, Inc (http://novell.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace System.Web.Compilation
{
	internal class ThemeDirectoryBuildProvider : TemplateBuildProvider
	{
		public ThemeDirectoryBuildProvider ()
		{
		}

		protected override void OverrideAssemblyPrefix (TemplateParser parser, AssemblyBuilder assemblyBuilder)
		{
			if (parser == null || assemblyBuilder == null)
				return;

			string newPrefix = assemblyBuilder.OutputFilesPrefix + parser.ClassName + ".";
			assemblyBuilder.OutputFilesPrefix = newPrefix;
		}
		
		protected override BaseCompiler CreateCompiler (TemplateParser parser)
		{
			return new PageThemeCompiler (parser as PageThemeParser);
		}
		
		protected override TemplateParser CreateParser (VirtualPath virtualPath, string inputFile, TextReader reader, HttpContext context)
		{
			return CreateParser (virtualPath, inputFile, context);
		}

		protected override TemplateParser CreateParser (VirtualPath virtualPath, string inputFile, HttpContext context)
		{
			string vp = VirtualPathUtility.AppendTrailingSlash (virtualPath.Original);
			string physicalPath = virtualPath.PhysicalPath;
			if (!Directory.Exists (physicalPath))
				throw new HttpException (String.Concat ("Theme '", virtualPath.Original ,"' cannot be found in the application or global theme directories."));

			PageThemeParser ptp = new PageThemeParser (virtualPath, context);
			
			string[] css_files = Directory.GetFiles (physicalPath, "*.css");
			string[] css_urls = new string [css_files.Length];
			for (int i = 0; i < css_files.Length; i++) {
				css_urls [i] = VirtualPathUtility.Combine (vp, Path.GetFileName (css_files [i]));
				ptp.AddDependency (css_urls [i]);
			}
			Array.Sort (css_urls, StringComparer.OrdinalIgnoreCase);
			ptp.LinkedStyleSheets = css_urls;
			
			AspComponentFoundry shared_foundry = new AspComponentFoundry ();
			ptp.RootBuilder = new RootBuilder ();

			string [] skin_files = Directory.GetFiles (physicalPath, "*.skin");
			string skin_file_url;
			AspGenerator generator;
			
			foreach (string skin_file in skin_files) {
				skin_file_url = VirtualPathUtility.Combine (vp, Path.GetFileName (skin_file));
				PageThemeFileParser ptfp = new PageThemeFileParser (new VirtualPath (skin_file_url), skin_file, context);

				ptp.AddDependency (skin_file_url);
				generator = new AspGenerator (ptfp, shared_foundry);
				generator.Parse ();

				if (ptfp.RootBuilder.Children != null)
					foreach (object o in ptfp.RootBuilder.Children) {
						if (!(o is ControlBuilder))
							continue;
						ptp.RootBuilder.AppendSubBuilder ((ControlBuilder)o);
					}

				foreach (string ass in ptfp.Assemblies)
					if (!ptp.Assemblies.Contains (ass))
						ptp.AddAssemblyByFileName (ass);
			}

			return ptp;
		}

		protected override bool IsDirectoryBuilder {
			get { return true; }
		}
	}
}

