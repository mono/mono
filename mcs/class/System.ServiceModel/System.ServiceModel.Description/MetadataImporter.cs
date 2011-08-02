//
// MetadataImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels;
using System.Web.Services.Description;
using System.Xml;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public abstract class MetadataImporter
	{
		KeyedByTypeCollection<IPolicyImportExtension> policy_extensions;
		Collection<MetadataConversionError> errors = new Collection<MetadataConversionError> ();

		internal MetadataImporter (IEnumerable<IPolicyImportExtension> policyImportExtensions)
		{
			if (policyImportExtensions != null) {
				policy_extensions = new KeyedByTypeCollection<IPolicyImportExtension> (policyImportExtensions);
				return;
			}
			
			//FIXME: Complete the list
			policy_extensions = new KeyedByTypeCollection<IPolicyImportExtension> ();
			policy_extensions.Add (new TransportBindingElementImporter ());
			policy_extensions.Add (new MessageEncodingBindingElementImporter ());
		}

		[MonoTODO ("Not in use yet")]
		public Collection<MetadataConversionError> Errors {
			get { return errors; }
		}

		public KeyedByTypeCollection<IPolicyImportExtension> PolicyImportExtensions {
			get { return policy_extensions; }
		}

		public Dictionary<Object,Object> State {
			get { throw new NotImplementedException (); }
		}

		public Dictionary<XmlQualifiedName,ContractDescription> KnownContracts {
			get { throw new NotImplementedException (); }
		}

		public abstract Collection<ContractDescription> ImportAllContracts ();

		public abstract ServiceEndpointCollection ImportAllEndpoints ();
	}
}
