//
// XmlSerializerOperationBehavior.cs
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml.Serialization;

namespace System.ServiceModel.Description
{
	public class XmlSerializerOperationBehavior
		: IOperationBehavior
#if !NET_2_1
			, IWsdlExportExtension
#endif
	{
		XmlSerializerFormatAttribute format;
		OperationDescription operation;

		public XmlSerializerOperationBehavior (
			OperationDescription operation)
			: this (operation, null)
		{
		}

		public XmlSerializerOperationBehavior (
			OperationDescription operation,
			XmlSerializerFormatAttribute format)
		{
			if (format == null)
				format = new XmlSerializerFormatAttribute ();
			this.format = format;
			this.operation = operation;
		}

		[MonoTODO]
		public Collection<XmlMapping> GetXmlMappings ()
		{
			throw new NotImplementedException ();
		}

		public XmlSerializerFormatAttribute XmlSerializerFormatAttribute {
			get { return format; }
		}

		void IOperationBehavior.AddBindingParameters (
			OperationDescription description,
			BindingParameterCollection parameters)
		{
			throw new NotImplementedException ();
		}
		
		void IOperationBehavior.ApplyDispatchBehavior (
			OperationDescription description,
			DispatchOperation dispatch)
		{
			throw new NotImplementedException ();
		}

		void IOperationBehavior.ApplyClientBehavior (
			OperationDescription description,
			ClientOperation proxy)
		{
			throw new NotImplementedException ();
		}

		void IOperationBehavior.Validate (
			OperationDescription description)
		{
			throw new NotImplementedException ();
		}

#if !NET_2_1
		void IWsdlExportExtension.ExportContract (
			WsdlExporter exporter,
			WsdlContractConversionContext context)
		{
			throw new NotImplementedException ();
		}

		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter,
			WsdlEndpointConversionContext context)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
