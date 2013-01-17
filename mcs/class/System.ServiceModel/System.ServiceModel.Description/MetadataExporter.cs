//
// MetadataExporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

using WSBinding = System.Web.Services.Description.Binding;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public abstract class MetadataExporter
	{
		Collection<MetadataConversionError> errors = new Collection<MetadataConversionError> ();

		internal MetadataExporter ()
		{
		}

		public Collection<MetadataConversionError> Errors {
			get { return errors; }
		}

		public Dictionary<Object,Object> State {
			get { throw new NotImplementedException (); }
		}

		public abstract void ExportContract (ContractDescription contract);

		public abstract void ExportEndpoint (ServiceEndpoint endpoint);

		public abstract MetadataSet GetGeneratedMetadata ();

		protected internal PolicyConversionContext ExportPolicy (
			ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}

		internal MetadataConversionError AddError (string message, params object[] args)
		{
			var error = new MetadataConversionError (string.Format (message, args));
			Errors.Add (error);
			return error;
		}
		
		internal MetadataConversionError AddWarning (string message, params object[] args)
		{
			var error = new MetadataConversionError (string.Format (message, args), true);
			Errors.Add (error);
			return error;
		}
		
		internal class MetadataExportException : Exception
		{
			public MetadataConversionError Error {
				get;
				private set;
			}
			
			public MetadataExportException (MetadataConversionError error)
				: base (error.Message)
			{
				this.Error = error;
			}
			
			public MetadataExportException (MetadataConversionError error, Exception inner)
				: base (error.Message, inner)
			{
				this.Error = error;
			}
		}
	}
}
