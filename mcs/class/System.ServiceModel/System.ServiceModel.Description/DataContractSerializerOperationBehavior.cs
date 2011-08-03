//
// DataContractSerializerOperationBehavior.cs
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
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace System.ServiceModel.Description
{
	public class DataContractSerializerOperationBehavior : IOperationBehavior, IWsdlExportExtension
	{
		DataContractFormatAttribute format;
		OperationDescription operation;

		public DataContractSerializerOperationBehavior (OperationDescription operation)
		{
			format = new DataContractFormatAttribute ();
			this.operation = operation;
			MaxItemsInObjectGraph = int.MaxValue;
		}

		public DataContractSerializerOperationBehavior (
			OperationDescription operation,
			DataContractFormatAttribute dataContractFormatAttribute)
		{
			this.format = dataContractFormatAttribute;
			this.operation = operation;
			MaxItemsInObjectGraph = int.MaxValue;
		}

		public DataContractFormatAttribute DataContractFormatAttribute {
			get { return format; }
		}

#if NET_4_0
		public DataContractResolver DataContractResolver { get; set; }
#endif

		public bool IgnoreExtensionDataObject { get; set; }

		public int MaxItemsInObjectGraph { get; set; }

#if !NET_2_1
		public IDataContractSurrogate DataContractSurrogate { get; set; }
#endif

		public virtual XmlObjectSerializer CreateSerializer (Type type, string name, string ns, IList<Type> knownTypes)
		{
#if NET_2_1
			return new DataContractSerializer (type, name, ns, knownTypes);
#else
			return new DataContractSerializer (type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, false, DataContractSurrogate);
#endif
		}

		public virtual XmlObjectSerializer CreateSerializer (Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
		{
#if NET_2_1
			return new DataContractSerializer (type, name, ns, knownTypes);
#else
			return new DataContractSerializer (type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, false, DataContractSurrogate);
#endif
		}

		void IOperationBehavior.AddBindingParameters (
			OperationDescription description,
			BindingParameterCollection parameters)
		{
		}

		void IOperationBehavior.ApplyDispatchBehavior (
			OperationDescription description,
			DispatchOperation dispatch)
		{
			dispatch.Formatter = new DataContractMessagesFormatter (description, format);
		}

		void IOperationBehavior.ApplyClientBehavior (
			OperationDescription description,
			ClientOperation proxy)
		{
			proxy.Formatter = new DataContractMessagesFormatter (description, format);
		}

		void IOperationBehavior.Validate (
			OperationDescription description)
		{
		}
		
#if !NET_2_1
		//IWsdlExportExtension

		void IWsdlExportExtension.ExportContract (WsdlExporter exporter,
			WsdlContractConversionContext context)
		{
		}

		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter,
			WsdlEndpointConversionContext context)
		{
		}
#endif
	}
}
