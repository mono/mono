// 
// System.Web.Services.Description.ServiceDescriptionImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Services.Description;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;

namespace System.Web.Services.Description {
	public class ServiceDescriptionImporter {

		#region Fields

		string protocolName;
		XmlSchemas schemas;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceDescriptionImportStyle style;

		ArrayList importInfo = new ArrayList ();
		

		#endregion // Fields

		#region Constructors
	
		public ServiceDescriptionImporter ()
		{
			protocolName = String.Empty;
			schemas = new XmlSchemas ();
			serviceDescriptions = new ServiceDescriptionCollection ();
			style = ServiceDescriptionImportStyle.Client;
		}
		
		#endregion // Constructors

		#region Properties

		public string ProtocolName {
			get { return protocolName; }
			set { protocolName = value; }
		}

		public XmlSchemas Schemas {
			get { return schemas; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}

		public ServiceDescriptionImportStyle Style {
			get { return style; }
			set { style = value; }
		}
		
#if NET_2_0
		[MonoTODO]
		[System.Runtime.InteropServices.ComVisible(false)]
		public CodeGenerationOptions CodeGenerationOptions {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		[System.Runtime.InteropServices.ComVisible(false)]
		public ICodeGenerator CodeGenerator {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif
	
		#endregion // Properties

		#region Methods

		public void AddServiceDescription (ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl)
		{
			if (appSettingUrlKey != null && appSettingUrlKey == string.Empty && style == ServiceDescriptionImportStyle.Server)
				throw new InvalidOperationException ("Cannot set appSettingUrlKey if Style is Server");

			ImportInfo info = new ImportInfo ();
			info.ServiceDescription = serviceDescription;
			info.AppSettingUrlKey = appSettingUrlKey;
			info.AppSettingBaseUrl = appSettingBaseUrl;
			importInfo.Add (info);
			serviceDescriptions.Add (serviceDescription);
			
			if (serviceDescription.Types != null)
				schemas.Add (serviceDescription.Types.Schemas);
		}

		public ServiceDescriptionImportWarnings Import (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			ProtocolImporter importer = GetImporter ();
			
			if (!importer.Import (this, codeNamespace, codeCompileUnit, importInfo))
				throw new Exception ("None of the supported bindings was found");
				
			return importer.Warnings;
		}
		
		ProtocolImporter GetImporter ()
		{
			ArrayList importers = GetSupportedImporters ();
			if (protocolName == null || protocolName == "") protocolName = "Soap";
			foreach (ProtocolImporter importer in importers) {
				if (importer.ProtocolName == protocolName)
					return importer;
			}
			
			throw new Exception ("Protocol " + protocolName + " not supported");
		}
		
		ArrayList GetSupportedImporters ()
		{
			ArrayList list = new ArrayList ();
			list.Add (new SoapProtocolImporter ());
			list.Add (new HttpGetProtocolImporter ());
			list.Add (new HttpPostProtocolImporter ());
			return list;
		}
		
#if NET_2_0
		[MonoTODO]
		public static StringCollection GenerateWebReferences (
			WebReferenceCollection webReferences, 
			CodeGenerationOptions options, 
			ServiceDescriptionImportStyle style, 
			ICodeGenerator codeGenerator)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static StringCollection GenerateWebReferences (
			WebReferenceCollection webReferences, 
			CodeGenerationOptions options, 
			ServiceDescriptionImportStyle style, 
			ICodeGenerator codeGenerator, 
			CodeCompileUnit codeCompileUnit, 
			bool verbose)
		{
			throw new NotImplementedException ();
		}
#endif

#endregion
	}

	internal class ImportInfo
	{
		public ServiceDescription ServiceDescription;
		public string AppSettingUrlKey;
		public string AppSettingBaseUrl;
	}

}
