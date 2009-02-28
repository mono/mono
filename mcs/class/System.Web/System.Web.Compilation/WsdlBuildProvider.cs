//
// System.Web.Compilation.WsdlBuildProvider
//
// Authors:
//	Marek Habersack <grendello@gmail.com>
//
// (C) 2006 Marek Habersack
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

#if NET_2_0 && WEBSERVICES_DEP

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.Services.Description;
using System.Xml.Serialization;
using System.Web.Util;

namespace System.Web.Compilation {

	[BuildProviderAppliesTo (BuildProviderAppliesTo.Web|BuildProviderAppliesTo.Code)]
	sealed class WsdlBuildProvider : BuildProvider
	{
		CompilerType _compilerType;
		
		public override CompilerType CodeCompilerType {
			get {
				if (_compilerType == null) {
					CompilationSection cs = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
					if (cs == null)
						throw new HttpException ("Unable to determine default compilation language.");
					
					_compilerType = BuildManager.GetDefaultCompilerTypeForLanguage (cs.DefaultLanguage, cs);
				}

				return _compilerType;
			}
		}
		
		public WsdlBuildProvider()
		{
		}

		string GetNamespace ()
		{
			var vp = new System.Web.VirtualPath (VirtualPath);
			string vpDirectory = vp.Directory;
			string path;
			
			if (StrUtils.StartsWith (vpDirectory, "/App_Code/"))
				path = vpDirectory.Substring (10);
			else if (StrUtils.StartsWith (vpDirectory, "/App_WebReferences/"))
				path = vpDirectory.Substring (19);
			else
				path = vpDirectory;

			path = path.Replace ("/", ".");
			if (path.EndsWith ("."))
				return path.Substring (0, path.Length - 1);

			return path;
		}
		
		public override void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace proxyCode = new CodeNamespace (GetNamespace ());
			unit.Namespaces.Add (proxyCode);			

			ServiceDescription description = ServiceDescription.Read (OpenReader ());
			ServiceDescriptionImporter importer = new ServiceDescriptionImporter ();
				
			importer.AddServiceDescription (description, null, null);
			importer.Style = ServiceDescriptionImportStyle.Client;
			importer.CodeGenerator = assemblyBuilder.CodeDomProvider;
			importer.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync;
			importer.Import (proxyCode, unit);
			assemblyBuilder.AddCodeCompileUnit (unit);
		}
	}
}
#endif

