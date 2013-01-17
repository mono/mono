//
// WsdlHelper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Description;
using System.Web.Services.Discovery;
using System.Runtime.Serialization;
using WebServices = System.Web.Services;
using System.CodeDom;

namespace MonoTests.System.Runtime.Serialization
{
	public static class WsdlHelper
	{
		/*
		 * This reads a normal .wsdl file from an embedded resource.
		 * 
		 * You can simply fetch them from your server using
		 * 'curl http://yourserver/YourService.svc?singleWsdl > YourService.wsdl',
		 * add the .wsdl file to Test/Resources/WSDL and add it to `TEST_RESOURCE_FILES'
		 * in the Makefile.
		 */

		public static MetadataSet GetMetadataSet (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			using (var stream = asm.GetManifestResourceStream (name)) {
				if (stream == null)
					throw new InvalidOperationException (string.Format (
						"Cannot find resource file '{0}'.", name));
				return GetMetadataSet (stream);
			}
		}
		
		public static MetadataSet GetMetadataSet (Stream stream)
		{
			var dr = new ContractReference ();
			var doc = (WebServices.Description.ServiceDescription) dr.ReadDocument (stream);
			
			var metadata = new MetadataSet ();
			metadata.MetadataSections.Add (
				new MetadataSection (MetadataSection.ServiceDescriptionDialect, "", doc));
			return metadata;
		}

		public static CodeCompileUnit Import (MetadataSet metadata, ImportOptions options)
		{
			var importer = new WsdlImporter (metadata);
			var xsdImporter = new XsdDataContractImporter ();
			xsdImporter.Options = options;
			importer.State.Add (typeof(XsdDataContractImporter), xsdImporter);
			
			var contracts = importer.ImportAllContracts ();
			
			CodeCompileUnit ccu = new CodeCompileUnit ();
			var generator = new ServiceContractGenerator (ccu);

			if (contracts.Count != 1)
				throw new InvalidOperationException (string.Format (
					"Metadata import failed: found {0} contracts.", contracts.Count));
			
			var contract = contracts.First ();
			generator.GenerateServiceContractType (contract);
			
			return ccu;
		}

		public static CodeNamespace Find (this CodeNamespaceCollection collection, string name)
		{
			foreach (CodeNamespace ns in collection) {
				if (ns.Name == name)
					return ns;
			}
			
			return null;
		}
		
		public static CodeNamespace FindNamespace (this CodeCompileUnit unit, string name)
		{
			foreach (CodeNamespace ns in unit.Namespaces) {
				if (ns.Name == name)
					return ns;
			}
			
			return null;
		}
		
		public static CodeTypeDeclaration FindType (this CodeNamespace ns, string name)
		{
			foreach (CodeTypeDeclaration type in ns.Types) {
				if (type.Name == name)
					return type;
			}
			
			return null;
		}
		
		public static CodeTypeDeclaration FindType (this CodeCompileUnit unit, string name)
		{
			foreach (CodeNamespace ns in unit.Namespaces) {
				foreach (CodeTypeDeclaration type in ns.Types) {
					if (type.Name == name)
						return type;
				}
			}
			
			return null;
		}
		
		public static CodeMemberMethod FindMethod (this CodeTypeDeclaration type, string name)
		{
			foreach (var member in type.Members) {
				var method = member as CodeMemberMethod;
				if (method == null)
					continue;
				if (method.Name == name)
					return method;
			}
			
			return null;
		}
		
		public static CodeMemberMethod FindMethod (this CodeCompileUnit unit, string typeName,
		                                           string methodName)
		{
			var type = unit.FindType (typeName);
			if (type == null)
				return null;
			return type.FindMethod (methodName);
		}

		public static CodeAttributeDeclaration FindAttribute (this CodeTypeDeclaration type, string name)
		{
			foreach (CodeAttributeDeclaration attr in type.CustomAttributes) {
				if (attr.Name == name)
					return attr;
			}

			return null;
		}

		public static CodeAttributeArgument FindArgument (this CodeAttributeDeclaration attr, string name)
		{
			foreach (CodeAttributeArgument arg in attr.Arguments) {
				if (arg.Name == name)
					return arg;
			}

			return null;
		}
	}
}

