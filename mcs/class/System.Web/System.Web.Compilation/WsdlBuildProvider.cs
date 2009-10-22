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
using System.Web.Services.Discovery;
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

		public override void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace proxyCode = new CodeNamespace ();
			unit.Namespaces.Add (proxyCode);	

			var description = ServiceDescription.Read (OpenReader ());
			var discCollection = new DiscoveryClientDocumentCollection () {
					{VirtualPath, description}
				};
			
			var webref = new WebReferenceCollection () {
					new WebReference (discCollection, proxyCode)
				};

			var options = new WebReferenceOptions ();
			options.Style = ServiceDescriptionImportStyle.Client;
			ServiceDescriptionImporter.GenerateWebReferences (webref, assemblyBuilder.CodeDomProvider, unit, options);

			assemblyBuilder.AddCodeCompileUnit (unit);
		}
	}
}
#endif

