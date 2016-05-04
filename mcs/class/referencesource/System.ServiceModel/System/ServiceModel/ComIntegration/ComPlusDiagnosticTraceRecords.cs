//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using ServiceModelSR = System.ServiceModel.SR;
    using WsdlNS = System.Web.Services.Description;

    static class ComPlusServiceHostTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusServiceHostSchema record = new ComPlusServiceHostSchema(info.AppID, info.Clsid);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, ContractDescription contract)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                XmlQualifiedName contractQName = new XmlQualifiedName(contract.Name, contract.Namespace);
                ComPlusServiceHostCreatedServiceContractSchema record = new
                                ComPlusServiceHostCreatedServiceContractSchema(info.AppID, info.Clsid,
                                contractQName, contract.ContractType.ToString());
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, ServiceDescription service)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                WsdlExporter exporter = new WsdlExporter();
                string serviceNs = NamingHelper.DefaultNamespace;
                XmlQualifiedName serviceQName = new XmlQualifiedName("comPlusService", serviceNs);
                exporter.ExportEndpoints(service.Endpoints, serviceQName);
                WsdlNS.ServiceDescription wsdl = exporter.GeneratedWsdlDocuments[serviceNs];
                ComPlusServiceHostStartedServiceDetailsSchema record =
                    new ComPlusServiceHostStartedServiceDetailsSchema(info.AppID, info.Clsid, wsdl);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, ServiceEndpointCollection endpointCollection)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                foreach (ServiceEndpoint endpoint in endpointCollection)
                {
                    ComPlusServiceHostCreatedServiceEndpointSchema record =
                        new ComPlusServiceHostCreatedServiceEndpointSchema(info.AppID, info.Clsid, endpoint.Contract.Name,
                        endpoint.Address.Uri, endpoint.Binding.Name);
                    TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
                }
            }
        }

    }

    static class ComPlusDllHostInitializerTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Guid appid)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusDllHostInitializerSchema record =
                    new ComPlusDllHostInitializerSchema(appid);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, Guid appid, Guid clsid, ServiceElement service)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                foreach (ServiceEndpointElement endpointElement in service.Endpoints)
                {
                    ComPlusDllHostInitializerAddingHostSchema record =
                        new ComPlusDllHostInitializerAddingHostSchema(appid, clsid, service.BehaviorConfiguration, service.Name,
                                                endpointElement.Address.ToString(), endpointElement.BindingConfiguration, endpointElement.BindingName, endpointElement.BindingNamespace,
                                                endpointElement.Binding, endpointElement.Contract);
                    TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
                }
            }
        }
    }

    static class ComPlusTLBImportTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Guid iid, Guid typeLibraryID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTLBImportSchema record =
                    new ComPlusTLBImportSchema(iid, typeLibraryID);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, Guid iid, Guid typeLibraryID, string assembly)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTLBImportFromAssemblySchema record =
                    new ComPlusTLBImportFromAssemblySchema(iid, typeLibraryID, assembly);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, Guid iid, Guid typeLibraryID, ImporterEventKind eventKind, int eventCode, string eventMsg)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTLBImportConverterEventSchema record =
                    new ComPlusTLBImportConverterEventSchema(iid, typeLibraryID, eventKind, eventCode, eventMsg);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }

    static class ComPlusInstanceCreationTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Message message, Guid incomingTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                WindowsIdentity callerIdentity = MessageUtil.GetMessageIdentity(message);
                Uri from = null;
                if (message.Headers.From != null)
                    from = message.Headers.From.Uri;
                ComPlusInstanceCreationRequestSchema record =
                    new ComPlusInstanceCreationRequestSchema(info.AppID, info.Clsid,
                        from, incomingTransactionID, callerIdentity.Name);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record, null, null, message);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Message message, int instanceID, Guid incomingTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                WindowsIdentity callerIdentity = MessageUtil.GetMessageIdentity(message);
                Uri from = null;
                if (message.Headers.From != null)
                    from = message.Headers.From.Uri;
                ComPlusInstanceCreationSuccessSchema record =
                    new ComPlusInstanceCreationSuccessSchema(info.AppID, info.Clsid,
                        from, incomingTransactionID, callerIdentity.Name, instanceID);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record, null, null, message);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, InstanceContext instanceContext, int instanceID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusInstanceReleasedSchema record =
                new ComPlusInstanceReleasedSchema(info.AppID, info.Clsid, instanceID);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }

    }

    static class ComPlusActivityTrace
    {
        internal static readonly Guid IID_IComThreadingInfo = new Guid("000001ce-0000-0000-C000-000000000046");
        internal static readonly Guid IID_IObjectContextInfo = new Guid("75B52DDB-E8ED-11d1-93AD-00AA00BA3258");
        public static void Trace(TraceEventType type, int traceCode, string description)
        {

            if (DiagnosticUtility.ShouldTrace(type))
            {
                Guid guidLogicalThreadID = Guid.Empty;
                Guid guidActivityID = Guid.Empty;
                IComThreadingInfo comThreadingInfo;
                comThreadingInfo = (IComThreadingInfo)SafeNativeMethods.CoGetObjectContext(IID_IComThreadingInfo);
                if (comThreadingInfo != null)
                {
                    comThreadingInfo.GetCurrentLogicalThreadId(out guidLogicalThreadID);
                    IObjectContextInfo contextInfo = comThreadingInfo as IObjectContextInfo;
                    if (contextInfo != null)
                    {
                        contextInfo.GetActivityId(out guidActivityID);
                    }
                }
                ComPlusActivitySchema record =
                    new ComPlusActivitySchema(guidActivityID, guidLogicalThreadID,
                    System.Threading.Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId());
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }

    static class ComPlusMethodCallTrace
    {
        static readonly Guid IID_IComThreadingInfo = new Guid("000001ce-0000-0000-C000-000000000046");
        static readonly Guid IID_IObjectContextInfo = new Guid("75B52DDB-E8ED-11d1-93AD-00AA00BA3258");
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Uri from, string action, string callerIdentity,
                            Guid iid, int instanceID, bool traceContextTransaction)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {

                ComPlusMethodCallSchema record = null;
                Guid guidContextTrsansactionID = Guid.Empty;
                if (traceContextTransaction)
                {
                    IComThreadingInfo comThreadingInfo;
                    comThreadingInfo = (IComThreadingInfo)SafeNativeMethods.CoGetObjectContext(IID_IComThreadingInfo);
                    if (comThreadingInfo != null)
                    {
                        IObjectContextInfo contextInfo = comThreadingInfo as IObjectContextInfo;
                        if (contextInfo != null)
                        {
                            if (contextInfo.IsInTransaction())
                                contextInfo.GetTransactionId(out guidContextTrsansactionID);
                        }
                    }
                    if (guidContextTrsansactionID != Guid.Empty)
                    {
                        record = new ComPlusMethodCallContextTxSchema(from, info.AppID, info.Clsid, iid, action,
                        instanceID, System.Threading.Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity, guidContextTrsansactionID);
                    }
                }
                else
                {
                    record = new ComPlusMethodCallSchema(from, info.AppID, info.Clsid, iid, action,
                    instanceID, System.Threading.Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity);
                }
                if (record != null)
                    TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Uri from, string action, string callerIdentity,
                            Guid iid, int instanceID, Guid incomingTransactionID, Guid currentTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMethodCallTxMismatchSchema record = new ComPlusMethodCallTxMismatchSchema(from, info.AppID, info.Clsid, iid, action,
                    instanceID, System.Threading.Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity,
                    incomingTransactionID, currentTransactionID);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Uri from, string action, string callerIdentity,
                            Guid iid, int instanceID, Guid guidIncomingTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMethodCallNewTxSchema record = new ComPlusMethodCallNewTxSchema(from, info.AppID, info.Clsid, iid, action,
                    instanceID, System.Threading.Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity,
                    guidIncomingTransactionID);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }

    static class ComPlusServiceMonikerTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                string address = null;
                string contract = null;
                string binding = null;
                string bindingConfig = null;
                string spnIdentity = null;
                string upnIdentity = null;
                string dnsIdentity = null;
                string wsdlText = null;
                string mexAddress = null;
                string mexBinding = null;
                string mexBindingConfiguration = null;
                string mexSpnIdentity = null;
                string mexUpnIdentity = null;
                string mexDnsIdentity = null;
                string contractNamespace = null;
                string bindingNamespace = null;



                WsdlNS.ServiceDescription wsdl = null;
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Wsdl, out wsdlText);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out contract);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out address);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out binding);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingConfiguration, out bindingConfig);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out spnIdentity);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out upnIdentity);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out dnsIdentity);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexAddress, out mexAddress);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBinding, out mexBinding);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBindingConfiguration, out mexBindingConfiguration);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexSpnIdentity, out mexSpnIdentity);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexUpnIdentity, out mexUpnIdentity);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexDnsIdentity, out mexDnsIdentity);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.ContractNamespace, out contractNamespace);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingNamespace, out bindingNamespace);

                if (!String.IsNullOrEmpty(wsdlText))
                {
                    TextReader reader = new StringReader(wsdlText);
                    wsdl = WsdlNS.ServiceDescription.Read(reader);
                }
                ComPlusServiceMonikerSchema record = new ComPlusServiceMonikerSchema(address, contract, contractNamespace, wsdl, spnIdentity, upnIdentity,
                     dnsIdentity, binding, bindingConfig, bindingNamespace, mexAddress, mexBinding, mexBindingConfiguration, mexSpnIdentity, mexUpnIdentity, mexDnsIdentity);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }

    static class ComPlusWsdlChannelBuilderTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, XmlQualifiedName bindingQname, XmlQualifiedName contractQname,
                                    WsdlNS.ServiceDescription wsdl, ContractDescription contract, Binding binding, XmlSchemas schemas)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                string name = "Service";
                if (wsdl.Name != null)
                    name = wsdl.Name;
                Type contractType = contract.ContractType;

                XmlQualifiedName serviceName = new XmlQualifiedName(name, wsdl.TargetNamespace);
                foreach (XmlSchema schema in schemas)
                {
                    ComPlusWsdlChannelBuilderSchema record = new ComPlusWsdlChannelBuilderSchema(bindingQname, contractQname, serviceName,
                             (contractType != null) ? contractType.ToString() : null, (binding != null) ? (binding.GetType()).ToString() : null, schema);
                    TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
                }
            }
        }
    }

    static class ComPlusMexChannelBuilderMexCompleteTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceEndpointCollection serviceEndpointsRetrieved)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                int nIndex = 0;
                ComPlusMexBuilderMetadataRetrievedEndpoint[] endpoints = new ComPlusMexBuilderMetadataRetrievedEndpoint[serviceEndpointsRetrieved.Count];
                foreach (ServiceEndpoint endpoint in serviceEndpointsRetrieved)
                {
                    endpoints[nIndex++] = new ComPlusMexBuilderMetadataRetrievedEndpoint(endpoint);
                }
                ComPlusMexBuilderMetadataRetrievedSchema record = new ComPlusMexBuilderMetadataRetrievedSchema(endpoints);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }

        }
    }

    static class ComPlusMexChannelBuilderTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, ContractDescription contract, Binding binding, string address)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMexChannelBuilderSchema record = new ComPlusMexChannelBuilderSchema(contract.Name, contract.Namespace, binding.Name, binding.Namespace, address);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }
    static class ComPlusTypedChannelBuilderTrace
    {
        public static void Trace(TraceEventType type, int v, string description, Type contractType, Binding binding)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTypedChannelBuilderSchema record = new ComPlusTypedChannelBuilderSchema(contractType.ToString(),
                     (binding != null) ? (binding.GetType()).ToString() : null);
                TraceUtility.TraceEvent(type, v, ServiceModelSR.GetString(description), record);
            }
        }
    }
    static class ComPlusChannelCreatedTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Uri address, Type contractType)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusChannelCreatedSchema record = new ComPlusChannelCreatedSchema(address, (contractType != null) ? contractType.ToString() : null);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }
    static class ComPlusDispatchMethodTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description,
                    Dictionary<UInt32, System.ServiceModel.ComIntegration.DispatchProxy.MethodInfo> dispToOperationDescription)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {

                UInt32 dispIndex = 10;
                System.ServiceModel.ComIntegration.DispatchProxy.MethodInfo methodInfo = null;
                while (dispToOperationDescription.TryGetValue(dispIndex, out methodInfo))
                {
                    ComPlusDispatchMethodSchema record = new ComPlusDispatchMethodSchema(methodInfo.opDesc.Name, methodInfo.paramList, methodInfo.ReturnVal);
                    TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
                    dispIndex++;
                }
            }
        }
    }
    static class ComPlusTxProxyTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Guid appid, Guid clsid, Guid transactionID, int instanceID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTxProxySchema record = new ComPlusTxProxySchema(appid, clsid, transactionID, instanceID);
                TraceUtility.TraceEvent(type, traceCode, ServiceModelSR.GetString(description), record);
            }
        }
    }
}
