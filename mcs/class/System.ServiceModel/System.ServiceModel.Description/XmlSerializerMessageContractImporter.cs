//
// XmlSerializerMessageContractImporter.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Text;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class XmlSerializerMessageContractImporter
		: IWsdlImportExtension
	{
		IWsdlImportExtension impl = new WsdlDataImportExtensionInternal () { ImportXmlType = true };

		public bool Enabled { get; set; }

		void IWsdlImportExtension.BeforeImport (
			ServiceDescriptionCollection wsdlDocuments,
			XmlSchemaSet xmlSchemas,
			ICollection<XmlElement> policy)
		{
			if (wsdlDocuments == null)
				throw new ArgumentNullException ("wsdlDocuments");
			if (xmlSchemas == null)
				throw new ArgumentNullException ("xmlSchemas");

			if (!Enabled)
				return;

			impl.BeforeImport (wsdlDocuments, xmlSchemas, policy);
		}

		void IWsdlImportExtension.ImportContract (WsdlImporter importer,
			WsdlContractConversionContext context)
		{
			if (importer == null)
				throw new ArgumentNullException ("importer");
			if (context == null)
				throw new ArgumentNullException ("context");

			if (!Enabled)
				return;

			impl.ImportContract (importer, context);
		}

		void IWsdlImportExtension.ImportEndpoint (WsdlImporter importer,
			WsdlEndpointConversionContext context)
		{
		}
	}
}
