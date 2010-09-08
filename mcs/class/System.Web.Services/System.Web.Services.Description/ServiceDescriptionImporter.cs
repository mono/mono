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
using System.Xml.Schema;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;

#if !MOBILE
using Microsoft.CSharp;
#endif

namespace System.Web.Services.Description {
	public class ServiceDescriptionImporter {

		#region Fields

		string protocolName;
		XmlSchemas schemas;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceDescriptionImportStyle style;
		
#if NET_2_0 && !MOBILE
		CodeGenerationOptions options;
		CodeDomProvider codeGenerator = new CSharpCodeProvider ();
		ImportContext context;
#endif

		ArrayList importInfo = new ArrayList ();
		

		#endregion // Fields

		#region Constructors
	
		public ServiceDescriptionImporter ()
		{
			protocolName = String.Empty;
			schemas = new XmlSchemas ();
			serviceDescriptions = new ServiceDescriptionCollection ();
			serviceDescriptions.SetImporter (this);
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
		
#if NET_2_0 && !MONOTOUCH
		[System.Runtime.InteropServices.ComVisible(false)]
		public CodeGenerationOptions CodeGenerationOptions {
			get { return options; }
			set { options = value; }
		}
		
		[System.Runtime.InteropServices.ComVisible(false)]
		public CodeDomProvider CodeGenerator {
			get { return codeGenerator; }
			set { codeGenerator = value; }
		}
		
		
		internal ImportContext Context {
			get { return context; }
			set { context = value; }
		}
#endif
	
		#endregion // Properties

		#region Methods

		public void AddServiceDescription (ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl)
		{
			if (appSettingUrlKey != null && appSettingUrlKey == string.Empty && style == ServiceDescriptionImportStyle.Server)
				throw new InvalidOperationException ("Cannot set appSettingUrlKey if Style is Server");

			OnServiceDescriptionAdded (serviceDescription, appSettingUrlKey, appSettingBaseUrl);
		}

		internal void OnServiceDescriptionAdded (ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl)
		{
			serviceDescriptions.Add (serviceDescription);
			ImportInfo info = new ImportInfo (serviceDescription, appSettingUrlKey, appSettingBaseUrl);
			importInfo.Add (info);
			
			if (serviceDescription.Types != null)
				schemas.Add (serviceDescription.Types.Schemas);
		}

#if !MONOTOUCH
		public ServiceDescriptionImportWarnings Import (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			foreach (ProtocolImporter importer in GetSupportedImporters ()) {
				if ((protocolName != null && protocolName.Length != 0) && String.Compare (importer.ProtocolName, protocolName, true) != 0)
					continue;
				if (importer.Import (this, codeNamespace, importInfo))
					return importer.Warnings;
			}
			throw new Exception ("None of the supported bindings was found");
		}
		
		ArrayList GetSupportedImporters ()
		{
			ArrayList list = new ArrayList ();
			list.Add (new SoapProtocolImporter ());
#if NET_2_0 
			list.Add (new Soap12ProtocolImporter ());
#endif
			list.Add (new HttpGetProtocolImporter ());
			list.Add (new HttpPostProtocolImporter ());
			return list;
		}
#endif
		
#if NET_2_0 && !MONOTOUCH

		[MonoTODO] // where to use Verbose and Extensions in options?
		public static StringCollection GenerateWebReferences (
			WebReferenceCollection webReferences, 
			CodeDomProvider codeGenerator, 
			CodeCompileUnit codeCompileUnit, 
			WebReferenceOptions options)
		{
			StringCollection allWarnings = new StringCollection ();
			ImportContext context = new ImportContext (new CodeIdentifiers(), true);
			
			foreach (WebReference reference in webReferences) 
			{
				ServiceDescriptionImporter importer = new ServiceDescriptionImporter ();
				if (codeGenerator != null)
					importer.CodeGenerator = codeGenerator;
				importer.CodeGenerationOptions = options.CodeGenerationOptions;
				importer.Context = context;
				importer.Style = options.Style;
				importer.ProtocolName = reference.ProtocolName;
				
				importer.AddReference (reference);
				
				reference.Warnings = importer.Import (reference.ProxyCode, codeCompileUnit);
				reference.SetValidationWarnings (context.Warnings);
				foreach (string s in context.Warnings)
					allWarnings.Add (s);

				context.Warnings.Clear ();
			}

			return allWarnings;
		}
		
		internal void AddReference (WebReference reference)
		{
			foreach (object doc in reference.Documents.Values)
			{
				if (doc is ServiceDescription) {
					ServiceDescription service = (ServiceDescription) doc;
					ImportInfo info = new ImportInfo (service, reference);
					importInfo.Add (info);
					serviceDescriptions.Add (service);
					
					if (service.Types != null)
						schemas.Add (service.Types.Schemas);
				}
				else if (doc is XmlSchema) {
					schemas.Add ((XmlSchema) doc);
				}
			}
		}
		
#endif

#endregion
	}

	internal class ImportInfo
	{
		string _appSettingUrlKey;
		string _appSettingBaseUrl;
		ServiceDescription _serviceDescription;
		
		public WebReference _reference;
		
		public ImportInfo (ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl)
		{
			_serviceDescription = serviceDescription;
			_appSettingUrlKey = appSettingUrlKey;
			_appSettingBaseUrl = appSettingBaseUrl;
		}
		
		public ImportInfo (ServiceDescription serviceDescription, WebReference reference)
		{
			_reference = reference;
			_serviceDescription = serviceDescription;
		}
		
		public WebReference Reference {
			get { return _reference; }
		}
		
		public ServiceDescription ServiceDescription {
			get { return _serviceDescription; }
		}
		
		public string AppSettingUrlKey {
			get {
				if (_reference != null) return _reference.AppSettingUrlKey;
				else return _appSettingUrlKey;
			}
			set {
				_appSettingUrlKey = value;
			}
		}
		
		public string AppSettingBaseUrl {
			get {
				if (_reference != null) return _reference.AppSettingBaseUrl;
				else return _appSettingBaseUrl;
			}
			set {
				_appSettingBaseUrl = value;
			}
		}
	}

}
