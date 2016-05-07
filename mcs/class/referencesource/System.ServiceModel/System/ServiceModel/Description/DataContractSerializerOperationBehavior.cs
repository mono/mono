//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.IO;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Xml;

    public class DataContractSerializerOperationBehavior : IOperationBehavior, IWsdlExportExtension
    {
        readonly bool builtInOperationBehavior;

        OperationDescription operation;
        DataContractFormatAttribute dataContractFormatAttribute;
        internal bool ignoreExtensionDataObject = DataContractSerializerDefaults.IgnoreExtensionDataObject;
        bool ignoreExtensionDataObjectSetExplicit;
        internal int maxItemsInObjectGraph = DataContractSerializerDefaults.MaxItemsInObjectGraph;
        bool maxItemsInObjectGraphSetExplicit;
        IDataContractSurrogate dataContractSurrogate;
        DataContractResolver dataContractResolver;

        public DataContractFormatAttribute DataContractFormatAttribute
        {
            get { return this.dataContractFormatAttribute; }
        }

        public DataContractSerializerOperationBehavior(OperationDescription operation)
            : this(operation, null)
        {
        }

        public DataContractSerializerOperationBehavior(OperationDescription operation, DataContractFormatAttribute dataContractFormatAttribute)
        {
            this.dataContractFormatAttribute = dataContractFormatAttribute ?? new DataContractFormatAttribute();
            this.operation = operation;
        }

        internal DataContractSerializerOperationBehavior(OperationDescription operation,
            DataContractFormatAttribute dataContractFormatAttribute, bool builtInOperationBehavior)
            : this(operation, dataContractFormatAttribute)
        {
            this.builtInOperationBehavior = builtInOperationBehavior;
        }

        internal bool IsBuiltInOperationBehavior
        {
            get { return this.builtInOperationBehavior; }
        }

        public int MaxItemsInObjectGraph 
        {
            get { return maxItemsInObjectGraph; }
            set 
            { 
                maxItemsInObjectGraph = value;
                maxItemsInObjectGraphSetExplicit = true;
            }
        }

        internal bool MaxItemsInObjectGraphSetExplicit
        {
            get { return maxItemsInObjectGraphSetExplicit; }
            set { maxItemsInObjectGraphSetExplicit = value; }
        }

        public bool IgnoreExtensionDataObject 
        {
            get { return ignoreExtensionDataObject; }
            set 
            { 
                ignoreExtensionDataObject = value;
                ignoreExtensionDataObjectSetExplicit = true;
            }
        }

        internal bool IgnoreExtensionDataObjectSetExplicit
        {
            get { return ignoreExtensionDataObjectSetExplicit; }
            set { ignoreExtensionDataObjectSetExplicit = value; }
        }

        public IDataContractSurrogate DataContractSurrogate 
        {
            get { return dataContractSurrogate; }
            set { dataContractSurrogate = value; }
        }

        public DataContractResolver DataContractResolver
        {
            get { return dataContractResolver; }
            set { dataContractResolver = value; }
        }

        public virtual XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, false /*preserveObjectReferences*/, DataContractSurrogate, DataContractResolver);
        }

        public virtual XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, false /*preserveObjectReferences*/, DataContractSurrogate, DataContractResolver);
        }

        internal object GetFormatter(OperationDescription operation, out bool formatRequest, out bool formatReply, bool isProxy)
        {
            MessageDescription request = operation.Messages[0];
            MessageDescription response = null;
            if (operation.Messages.Count == 2)
                response = operation.Messages[1];

            formatRequest = (request != null) && !request.IsUntypedMessage;
            formatReply = (response != null) && !response.IsUntypedMessage;

            if (formatRequest || formatReply)
            {
                if (PrimitiveOperationFormatter.IsContractSupported(operation))
                    return new PrimitiveOperationFormatter(operation, dataContractFormatAttribute.Style == OperationFormatStyle.Rpc);
                else
                    return new DataContractSerializerOperationFormatter(operation, dataContractFormatAttribute, this);
            }

            return null;
        }


        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            if (dispatch == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");

            if (dispatch.Formatter != null)
                return;

            bool formatRequest;
            bool formatReply;
            dispatch.Formatter = (IDispatchMessageFormatter)GetFormatter(description, out formatRequest, out formatReply, false);
            dispatch.DeserializeRequest = formatRequest;
            dispatch.SerializeReply = formatReply;
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            if (proxy == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("proxy");

            if (proxy.Formatter != null)
                return;

            bool formatRequest;
            bool formatReply;
            proxy.Formatter = (IClientMessageFormatter)GetFormatter(description, out formatRequest, out formatReply, true);
            proxy.SerializeRequest = formatRequest;
            proxy.DeserializeReply = formatReply;
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            if (endpointContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");

            MessageContractExporter.ExportMessageBinding(exporter, endpointContext, typeof(DataContractSerializerMessageContractExporter), this.operation);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext contractContext)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            if (contractContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractContext");

            new DataContractSerializerMessageContractExporter(exporter, contractContext, this.operation, this).ExportMessageContract();
        }

    }
}
