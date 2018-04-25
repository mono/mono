//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WsdlNS = System.Web.Services.Description;

    class WsdlWrapper : IXmlSerializable
    {
        WsdlNS.ServiceDescription wsdl;

        public WsdlWrapper(WsdlNS.ServiceDescription wsdl)
        {
            this.wsdl = wsdl;
        }

        public void WriteXml(XmlWriter xmlWriter)
        {

            if (wsdl != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    wsdl.Write(ms);

                    XmlDictionaryReaderQuotas quota = new XmlDictionaryReaderQuotas();
                    quota.MaxDepth = 32;
                    quota.MaxStringContentLength = 8192;
                    quota.MaxArrayLength = 16384;
                    quota.MaxBytesPerRead = 4096;
                    quota.MaxNameTableCharCount = 16384;

                    ms.Seek(0, SeekOrigin.Begin);

                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(ms, null, quota, null);

                    if ((reader.MoveToContent() == XmlNodeType.Element) && (reader.Name == "wsdl:definitions"))
                    {

                        xmlWriter.WriteNode(reader, false);
                    }

                    reader.Close();
                }
            }

        }

        public void ReadXml(XmlReader xmlReader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }
        public XmlSchema GetSchema()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }
    }
    [DataContract(Name = "ComPlusServiceHost")]
    class ComPlusServiceHostSchema : TraceRecord
    {
        [DataMember(Name = "appid")]
        Guid appid;

        [DataMember(Name = "clsid")]
        Guid clsid;

        public ComPlusServiceHostSchema(Guid appid, Guid clsid)
        {
            this.appid = appid;
            this.clsid = clsid;
        }

        internal override string EventId { get { return BuildEventId("ComPlusServiceHost"); } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        public override string ToString()
        {
            return SR.GetString(SR.ComPlusServiceSchema, this.appid.ToString(), this.clsid.ToString());
        }
    }

    [DataContract(Name = "ComPlusServiceHostCreatedServiceContract")]
    class ComPlusServiceHostCreatedServiceContractSchema : ComPlusServiceHostSchema
    {
        // ContractDescription has [DataContract] attribute, but its serialization appears to fail.
        // So explicitly write the needed properties

        [DataMember(Name = "ContractQName")]
        XmlQualifiedName contractQname;

        [DataMember(Name = "Contract")]
        string contract;

        public ComPlusServiceHostCreatedServiceContractSchema(Guid appid, Guid clsid,
            XmlQualifiedName contractQname, string contract)
            : base(appid, clsid)
        {
            this.contractQname = contractQname;
            this.contract = contract;
        }

        internal override string EventId { get { return BuildEventId("ComPlusServiceHostCreatedServiceContract"); } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }
    }

    [DataContract(Name = "ComPlusServiceHostStartedServiceDetails")]
    class ComPlusServiceHostStartedServiceDetailsSchema : ComPlusServiceHostSchema
    {
        [DataMember(Name = "ServiceDescription")]
        WsdlWrapper wsdlWrapper;

        public ComPlusServiceHostStartedServiceDetailsSchema(Guid appid, Guid clsid, WsdlNS.ServiceDescription wsdl)
            : base(appid, clsid)
        {
            this.wsdlWrapper = new WsdlWrapper(wsdl);
        }

        internal override string EventId { get { return BuildEventId("ComPlusServiceHostStartedServiceDetails"); } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }
    }

    [DataContract(Name = "ComPlusServiceHostCreatedServiceEndpoint")]
    class ComPlusServiceHostCreatedServiceEndpointSchema : ComPlusServiceHostSchema
    {
        [DataMember(Name = "Contract")]
        string contract;

        [DataMember(Name = "Address")]
        Uri address;

        [DataMember(Name = "Binding")]
        string binding;


        public ComPlusServiceHostCreatedServiceEndpointSchema(Guid appid, Guid clsid, string contract,
            Uri address, string binding)
            : base(appid, clsid)
        {
            this.contract = contract;
            this.address = address;
            this.binding = binding;
        }

        internal override string EventId { get { return BuildEventId("ComPlusServiceHostCreatedServiceEndpoint"); } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }
    }

    [DataContract(Name = "ComPlusDllHostInitializer")]
    class ComPlusDllHostInitializerSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusDllHostInitializer" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        public override string ToString()
        {
            return SR.GetString(SR.ComPlusServiceSchemaDllHost, this.appid.ToString());
        }

        [DataMember(Name = "appid")]
        Guid appid;

        public ComPlusDllHostInitializerSchema(
            Guid appid
            )
        {
            this.appid = appid;
        }
    }

    [DataContract(Name = "ComPlusDllHostInitializerAddingHost")]
    class ComPlusDllHostInitializerAddingHostSchema : ComPlusDllHostInitializerSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusDllHostInitializerAddingHost" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "clsid")]
        Guid clsid;

        [DataMember(Name = "BehaviorConfiguration")]
        string behaviorConfiguration;

        [DataMember(Name = "ServiceType")]
        string serviceType;

        [DataMember(Name = "Address")]
        string address;

        [DataMember(Name = "BindingConfiguration")]
        string bindingConfiguration;

        [DataMember(Name = "BindingName")]
        string bindingName;

        [DataMember(Name = "BindingNamespace")]
        string bindingNamespace;

        [DataMember(Name = "BindingSectionName")]
        string bindingSectionName;

        [DataMember(Name = "ContractType")]
        string contractType;

        public ComPlusDllHostInitializerAddingHostSchema(
            Guid appid,
            Guid clsid,
            string behaviorConfiguration,
            string serviceType,
            string address,
            string bindingConfiguration,
            string bindingName,
            string bindingNamespace,
            string bindingSectionName,
            string contractType
            )
            : base(appid)
        {
            this.clsid = clsid;
            this.behaviorConfiguration = behaviorConfiguration;
            this.serviceType = serviceType;
            this.address = address;
            this.bindingConfiguration = bindingConfiguration;
            this.bindingName = bindingName;
            this.bindingNamespace = bindingNamespace;
            this.bindingSectionName = bindingSectionName;
            this.contractType = contractType;
        }
    }

    [DataContract(Name = "ComPlusTLBImport")]
    class ComPlusTLBImportSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusTLBImport" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        public override string ToString()
        {
            return SR.GetString(SR.ComPlusTLBImportSchema, this.iid.ToString(), this.typeLibraryID.ToString());
        }

        [DataMember(Name = "InterfaceID")]
        Guid iid;

        [DataMember(Name = "TypeLibraryID")]
        Guid typeLibraryID;

        public ComPlusTLBImportSchema(
            Guid iid,
            Guid typeLibraryID
            )
        {
            this.iid = iid;
            this.typeLibraryID = typeLibraryID;
        }
    }

    [DataContract(Name = "ComPlusTLBImportFromAssembly")]
    class ComPlusTLBImportFromAssemblySchema : ComPlusTLBImportSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusTLBImportFromAssembly" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "Assembly")]
        string assembly;

        public ComPlusTLBImportFromAssemblySchema(
            Guid iid,
            Guid typeLibraryID,
            string assembly
            )
            : base(iid, typeLibraryID)
        {
            this.assembly = assembly;
        }
    }

    [DataContract(Name = "ComPlusTLBImportConverterEvent")]
    class ComPlusTLBImportConverterEventSchema : ComPlusTLBImportSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusTLBImportConverterEvent" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "EventKind")]
        ImporterEventKind eventKind;

        [DataMember(Name = "EventCode")]
        int eventCode;

        [DataMember(Name = "EventMessage")]
        string eventMessage;

        public ComPlusTLBImportConverterEventSchema(
            Guid iid,
            Guid typeLibraryID,
            ImporterEventKind eventKind,
            int eventCode,
            string eventMessage
            )
            : base(iid, typeLibraryID)
        {
            this.eventKind = eventKind;
            this.eventCode = eventCode;
            this.eventMessage = eventMessage;
        }
    }

    [DataContract(Name = "ComPlusInstanceCreationRequest")]
    class ComPlusInstanceCreationRequestSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusInstanceCreationRequest" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        public override string ToString()
        {
            return SR.GetString(SR.ComPlusInstanceCreationRequestSchema,
                this.from.ToString(),
                this.appid.ToString(),
                this.clsid.ToString(),
                this.incomingTransactionID.ToString(),
                this.requestingIdentity);
        }

        [DataMember(Name = "From")]
        Uri from;

        [DataMember(Name = "appid")]
        Guid appid;

        [DataMember(Name = "clsid")]
        Guid clsid;

        [DataMember(Name = "IncomingTransactionID")]
        Guid incomingTransactionID;

        [DataMember(Name = "RequestingIdentity")]
        string requestingIdentity;


        public ComPlusInstanceCreationRequestSchema(
            Guid appid,
            Guid clsid,
            Uri from,
            Guid incomingTransactionID,
            string requestingIdentity
            )
        {
            this.from = from;
            this.appid = appid;
            this.clsid = clsid;
            this.incomingTransactionID = incomingTransactionID;
            this.requestingIdentity = requestingIdentity;
        }

    }

    [DataContract(Name = "ComPlusInstanceCreationSuccess")]
    class ComPlusInstanceCreationSuccessSchema : ComPlusInstanceCreationRequestSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusInstanceCreationSuccess" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "InstanceID")]
        int instanceID;


        public ComPlusInstanceCreationSuccessSchema(
            Guid appid,
            Guid clsid,
            Uri from,
            Guid incomingTransactionID,
            string requestingIdentity,
            int instanceID
            )
            : base(appid, clsid, from, incomingTransactionID, requestingIdentity)
        {
            this.instanceID = instanceID;
        }
    }

    [DataContract(Name = "ComPlusInstanceReleased")]
    class ComPlusInstanceReleasedSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusInstanceReleased" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "appid")]
        Guid appid;

        [DataMember(Name = "clsid")]
        Guid clsid;

        [DataMember(Name = "InstanceID")]
        int instanceID;


        public ComPlusInstanceReleasedSchema(
            Guid appid,
            Guid clsid,
            int instanceID
            )
        {
            this.appid = appid;
            this.clsid = clsid;
            this.instanceID = instanceID;
        }
    }

    [DataContract(Name = "ComPlusActivity")]
    class ComPlusActivitySchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusActivity" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "ActivityID")]
        Guid activityID;

        [DataMember(Name = "LogicalThreadID")]
        Guid logicalThreadID;

        [DataMember(Name = "ManagedThreadID")]
        int managedThreadID;

        [DataMember(Name = "UnmanagedThreadID")]
        int unmanagedThreadID;


        public ComPlusActivitySchema(
            Guid activityID,
            Guid logicalThreadID,
            int managedThreadID,
            int unmanagedThreadID
            )
        {
            this.activityID = activityID;
            this.logicalThreadID = logicalThreadID;
            this.managedThreadID = managedThreadID;
            this.unmanagedThreadID = unmanagedThreadID;
        }
    }

    [DataContract(Name = "ComPlusMethodCall")]
    class ComPlusMethodCallSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMethodCall" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        public override string ToString()
        {
            return SR.GetString(SR.ComPlusMethodCallSchema,
                this.from.ToString(),
                this.appid.ToString(),
                this.clsid.ToString(),
                this.iid.ToString(),
                this.action,
                this.instanceID.ToString(CultureInfo.CurrentCulture),
                this.managedThreadID.ToString(CultureInfo.CurrentCulture),
                this.unmanagedThreadID.ToString(CultureInfo.CurrentCulture),
                this.requestingIdentity);
        }

        [DataMember(Name = "From")]
        Uri from;

        [DataMember(Name = "appid")]
        Guid appid;

        [DataMember(Name = "clsid")]
        Guid clsid;

        [DataMember(Name = "iid")]
        Guid iid;

        [DataMember(Name = "Action")]
        string action;

        [DataMember(Name = "InstanceID")]
        int instanceID;

        [DataMember(Name = "ManagedThreadID")]
        int managedThreadID;

        [DataMember(Name = "UnmanagedThreadID")]
        int unmanagedThreadID;

        [DataMember(Name = "RequestingIdentity")]
        string requestingIdentity;

        public ComPlusMethodCallSchema(
            Uri from,
            Guid appid,
            Guid clsid,
            Guid iid,
            string action,
            int instanceID,
            int managedThreadID,
            int unmanagedThreadID,
            string requestingIdentity
            )
        {
            this.from = from;
            this.appid = appid;
            this.clsid = clsid;
            this.iid = iid;
            this.action = action;
            this.instanceID = instanceID;
            this.managedThreadID = managedThreadID;
            this.unmanagedThreadID = unmanagedThreadID;
            this.requestingIdentity = requestingIdentity;
        }
    }

    [DataContract(Name = "ComPlusMethodCallTxMismatch")]
    class ComPlusMethodCallTxMismatchSchema : ComPlusMethodCallSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMethodCallTxMismatch" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        [DataMember(Name = "IncomingTransactionID")]
        Guid incomingTransactionID;

        [DataMember(Name = "CurrentTransactionID")]
        Guid currentTransactionID;

        public ComPlusMethodCallTxMismatchSchema(
            Uri from,
            Guid appid,
            Guid clsid,
            Guid iid,
            string action,
            int instanceID,
            int managedThreadID,
            int unmanagedThreadID,
            string requestingIdentity,
            Guid incomingTransactionID,
            Guid currentTransactionID)
            : base(from, appid, clsid, iid, action, instanceID, managedThreadID, unmanagedThreadID, requestingIdentity)
        {
            this.incomingTransactionID = incomingTransactionID;
            this.currentTransactionID = currentTransactionID;
        }
    }

    [DataContract(Name = "ComPlusMethodCallNewTx")]
    class ComPlusMethodCallNewTxSchema : ComPlusMethodCallSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMethodCallNewTx" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        [DataMember(Name = "NewTransactionID")]
        Guid newTransactionID;

        public ComPlusMethodCallNewTxSchema(
            Uri from,
            Guid appid,
            Guid clsid,
            Guid iid,
            string action,
            int instanceID,
            int managedThreadID,
            int unmanagedThreadID,
            string requestingIdentity,
            Guid newTransactionID)
            : base(from, appid, clsid, iid, action, instanceID, managedThreadID, unmanagedThreadID, requestingIdentity)
        {
            this.newTransactionID = newTransactionID;
        }
    }

    [DataContract(Name = "ComPlusMethodCallContextTx")]
    class ComPlusMethodCallContextTxSchema : ComPlusMethodCallSchema
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMethodCallContextTx" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        [DataMember(Name = "ContextTransactionID")]
        Guid contextTransactionID;

        public ComPlusMethodCallContextTxSchema(
            Uri from,
            Guid appid,
            Guid clsid,
            Guid iid,
            string action,
            int instanceID,
            int managedThreadID,
            int unmanagedThreadID,
            string requestingIdentity,
            Guid contextTransactionID)
            : base(from, appid, clsid, iid, action, instanceID, managedThreadID, unmanagedThreadID, requestingIdentity)
        {
            this.contextTransactionID = contextTransactionID;
        }
    }

    [DataContract(Name = "ComPlusServiceMoniker")]
    class ComPlusServiceMonikerSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusServiceMoniker" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "Address")]
        string address;

        [DataMember(Name = "Contract")]
        string contract;

        [DataMember(Name = "ContractNamespace")]
        string contractNamespace;

        [DataMember(Name = "Wsdl")]
        WsdlWrapper wsdlWrapper;

        [DataMember(Name = "SpnIdentity")]
        string spnIdentity;

        [DataMember(Name = "UpnIdentity")]
        string upnIdentity;

        [DataMember(Name = "DnsIdentity")]
        string dnsIdentity;

        [DataMember(Name = "Binding")]
        string binding;

        [DataMember(Name = "BindingConfiguration")]
        string bindingConfiguration;

        [DataMember(Name = "BindingNamespace")]
        string bindingNamespace;

        [DataMember(Name = "mexSpnIdentity")]
        string mexSpnIdentity;
        [DataMember(Name = "mexUpnIdentity")]
        string mexUpnIdentity;
        [DataMember(Name = "mexDnsIdentity")]
        string mexDnsIdentity;
        [DataMember(Name = "mexAddress")]
        string mexAddress;

        [DataMember(Name = "mexBinding")]
        string mexBinding;

        [DataMember(Name = "mexBindingConfiguration")]
        string mexBindingConfiguration;


        public ComPlusServiceMonikerSchema(
            string address,
            string contract,
            string contractNamespace,
            WsdlNS.ServiceDescription wsdl,
            string spnIdentity,
            string upnIdentity,
            string dnsIdentity,
            string binding,
            string bindingConfiguration,
            string bindingNamespace,
            string mexAddress,
            string mexBinding,
            string mexBindingConfiguration,
            string mexSpnIdentity,
            string mexUpnIdentity,
            string mexDnsIdentity
            )
        {
            this.address = address;
            this.contract = contract;
            this.contractNamespace = contractNamespace;
            this.wsdlWrapper = new WsdlWrapper(wsdl);
            this.spnIdentity = spnIdentity;
            this.upnIdentity = spnIdentity;
            this.dnsIdentity = spnIdentity;
            this.binding = binding;
            this.bindingConfiguration = bindingConfiguration;
            this.bindingNamespace = bindingNamespace;
            this.mexSpnIdentity = mexSpnIdentity;
            this.mexUpnIdentity = mexUpnIdentity;
            this.mexDnsIdentity = mexDnsIdentity;
            this.mexAddress = mexAddress;
            this.mexBinding = mexBinding;
            this.mexBindingConfiguration = mexBindingConfiguration;
        }
    }

    [DataContract(Name = "ComPlusWsdlChannelBuilder")]
    class ComPlusWsdlChannelBuilderSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusWsdlChannelBuilder" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "BindingQName")]
        XmlQualifiedName bindingQname;

        [DataMember(Name = "ContractQName")]
        XmlQualifiedName contractQname;

        [DataMember(Name = "ServiceQName")]
        XmlQualifiedName serviceQname;

        [DataMember(Name = "ImportedContract")]
        string importedContract;

        [DataMember(Name = "ImportedBinding")]
        string importedBinding;

        [DataMember(Name = "XmlSchemaSet")]
        XmlSchemaWrapper schema;

        public ComPlusWsdlChannelBuilderSchema(
            XmlQualifiedName bindingQname,
            XmlQualifiedName contractQname,
            XmlQualifiedName serviceQname,
            string importedContract,
            string importedBinding,
            XmlSchema schema
            )
        {
            this.bindingQname = bindingQname;
            this.contractQname = contractQname;
            this.serviceQname = serviceQname;
            this.importedContract = importedContract;
            this.importedBinding = importedBinding;
            this.schema = new XmlSchemaWrapper(schema);
        }
        class XmlSchemaWrapper : IXmlSerializable
        {
            XmlSchema schema;
            public XmlSchemaWrapper(XmlSchema schema)
            {
                this.schema = schema;
            }

            public void WriteXml(XmlWriter xmlWriter)
            {
                StringWriter textWriter = new StringWriter(CultureInfo.InvariantCulture);
                XmlTextWriter writer = new XmlTextWriter(textWriter);
                schema.Write(writer);
                writer.Flush();

                UTF8Encoding utf8 = new UTF8Encoding();

                byte[] wsdlText = utf8.GetBytes(textWriter.ToString());

                XmlDictionaryReaderQuotas quota = new XmlDictionaryReaderQuotas();
                quota.MaxDepth = 32;
                quota.MaxStringContentLength = 8192;
                quota.MaxArrayLength = 16384;
                quota.MaxBytesPerRead = 4096;
                quota.MaxNameTableCharCount = 16384;

                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(wsdlText, 0, wsdlText.GetLength(0), null, quota, null);

                if ((reader.MoveToContent() == XmlNodeType.Element) && (reader.Name == "xs:schema"))
                {

                    xmlWriter.WriteNode(reader, false);
                }

                reader.Close();
            }

            public void ReadXml(XmlReader xmlReader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
            public XmlSchema GetSchema()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

    }

    [DataContract(Name = "ComPlusTypedChannelBuilder")]
    class ComPlusTypedChannelBuilderSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusTypedChannelBuilder" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "Contract")]
        string contract;

        [DataMember(Name = "Binding")]
        string binding;

        public ComPlusTypedChannelBuilderSchema(
            string contract,
            string binding
            )
        {
            this.contract = contract;
            this.binding = binding;
        }
    }

    [DataContract(Name = "ComPlusMexChannelBuilder")]
    class ComPlusMexChannelBuilderSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMexChannelBuilder" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "Contract")]
        string contract;

        [DataMember(Name = "contractNamespace")]
        string contractNamespace;

        [DataMember(Name = "bindingNamespace")]
        string bindingNamespace;

        [DataMember(Name = "Binding")]
        string binding;

        [DataMember(Name = "Address")]
        string address;

        public ComPlusMexChannelBuilderSchema(
            string contract,
            string contractNamespace,
            string binding,
            string bindingNamespace,
            string address

            )
        {
            this.contract = contract;
            this.binding = binding;
            this.contractNamespace = contractNamespace;
            this.bindingNamespace = bindingNamespace;
            this.address = address;
        }
    }


    [DataContract(Name = "ComPlusMexBuilderMetadataRetrievedEndpoint")]
    class ComPlusMexBuilderMetadataRetrievedEndpoint : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMexBuilderMetadataRetrievedEndpoint" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }



        [DataMember(Name = "Binding")]
        string binding;

        [DataMember(Name = "BindingNamespace")]
        string bindingNamespace;

        [DataMember(Name = "Address")]
        string address;
        [DataMember(Name = "Contract")]
        string contract;

        [DataMember(Name = "ContractNamespace")]
        string contractNamespace;

        public ComPlusMexBuilderMetadataRetrievedEndpoint(ServiceEndpoint endpoint)
        {
            this.binding = endpoint.Binding.Name;
            this.bindingNamespace = endpoint.Binding.Namespace;
            this.address = endpoint.Address.ToString();
            this.contract = endpoint.Contract.Name;
            this.contractNamespace = endpoint.Contract.Namespace;

        }
    }

    [DataContract(Name = "ComPlusMexBuilderMetadataRetrieved")]
    class ComPlusMexBuilderMetadataRetrievedSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusMexBuilderMetadataRetrieved" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }



        [DataMember(Name = "bindingNamespaces")]
        ComPlusMexBuilderMetadataRetrievedEndpoint[] endpoints;


        public ComPlusMexBuilderMetadataRetrievedSchema(
            ComPlusMexBuilderMetadataRetrievedEndpoint[] endpoints
            )
        {
            this.endpoints = endpoints;
        }
    }


    [DataContract(Name = "ComPlusChannelCreated")]
    class ComPlusChannelCreatedSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusChannelCreated" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "Address")]
        Uri address;

        [DataMember(Name = "Contract")]
        string contract;

        public ComPlusChannelCreatedSchema(
            Uri address,
            string contract
            )
        {
            this.address = address;
            this.contract = contract;
        }
    }

    [DataContract(Name = "ComPlusDispatchMethodSchema")]
    class ComPlusDispatchMethodSchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusDispatchMethod" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        [DataMember(Name = "Name")]
        string name;

        [DataMember(Name = "ParameterInfo")]
        List<System.ServiceModel.ComIntegration.DispatchProxy.ParamInfo> paramList;

        [DataMember(Name = "ReturnValueInfo")]
        System.ServiceModel.ComIntegration.DispatchProxy.ParamInfo returnValue;

        public ComPlusDispatchMethodSchema(
            string name,
            List<System.ServiceModel.ComIntegration.DispatchProxy.ParamInfo> paramList,
            System.ServiceModel.ComIntegration.DispatchProxy.ParamInfo returnValue
            )
        {
            this.name = name;
            this.paramList = paramList;
            this.returnValue = returnValue;
        }
    }

    [DataContract(Name = "ComPlusTxProxySchema")]
    class ComPlusTxProxySchema : TraceRecord
    {
        const string schemaId = TraceRecord.EventIdBase + "ComPlusTxProxyTx" + TraceRecord.NamespaceSuffix;
        internal override string EventId { get { return schemaId; } }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }
        [DataMember(Name = "appid")]
        Guid appid;

        [DataMember(Name = "clsid")]
        Guid clsid;

        [DataMember(Name = "TransactionID")]
        Guid transactionID;

        [DataMember(Name = "InstanceID")]
        int instanceID;

        public ComPlusTxProxySchema(Guid appid, Guid clsid,
            Guid transactionID, int instanceID)
        {
            this.appid = appid;
            this.clsid = clsid;
            this.transactionID = transactionID;
            this.instanceID = instanceID;
        }
    }
}
