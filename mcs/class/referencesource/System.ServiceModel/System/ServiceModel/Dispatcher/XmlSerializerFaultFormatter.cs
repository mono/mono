//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;


namespace System.ServiceModel.Dispatcher
{
    class XmlSerializerFaultFormatter : FaultFormatter
    {
        SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos;

        internal XmlSerializerFaultFormatter(Type[] detailTypes,
            SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos)
            : base(detailTypes)
        {
            Initialize(xmlSerializerFaultContractInfos);
        }

        internal XmlSerializerFaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfoCollection,
            SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos)
            : base(faultContractInfoCollection)
        {
            Initialize(xmlSerializerFaultContractInfos);
        }

        void Initialize(SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos)
        {
            if (xmlSerializerFaultContractInfos == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlSerializerFaultContractInfos");
            }
            this.xmlSerializerFaultContractInfos = xmlSerializerFaultContractInfos;
        }

        protected override XmlObjectSerializer GetSerializer(Type detailType, string faultExceptionAction, out string action)
        {
            action = faultExceptionAction;
            
            XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo faultInfo = null;
            for (int i = 0; i < this.xmlSerializerFaultContractInfos.Count; i++)
            {
                if (this.xmlSerializerFaultContractInfos[i].FaultContractInfo.Detail == detailType)
                {
                    faultInfo = this.xmlSerializerFaultContractInfos[i];
                    break;
                }
            }
            if (faultInfo != null)
            {
                if (action == null)
                    action = faultInfo.FaultContractInfo.Action;

                return faultInfo.Serializer;
            }
            else
                return new XmlSerializerObjectSerializer(detailType);
        }

        protected override FaultException CreateFaultException(MessageFault messageFault, string action)
        {
            IList<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> faultInfos;
            if (action != null)
            {
                faultInfos = new List<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo>();
                for (int i = 0; i < this.xmlSerializerFaultContractInfos.Count; i++)
                {
                    if (this.xmlSerializerFaultContractInfos[i].FaultContractInfo.Action == action
                        || this.xmlSerializerFaultContractInfos[i].FaultContractInfo.Action == MessageHeaders.WildcardAction)
                    {
                        faultInfos.Add(this.xmlSerializerFaultContractInfos[i]);
                    }
                }
            }
            else
            {
                faultInfos = this.xmlSerializerFaultContractInfos;
            }

            Type detailType = null;
            object detailObj = null;
            for (int i = 0; i < faultInfos.Count; i++)
            {
                XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo faultInfo = faultInfos[i];
                XmlDictionaryReader detailReader = messageFault.GetReaderAtDetailContents();
                XmlObjectSerializer serializer = faultInfo.Serializer;

                if (serializer.IsStartObject(detailReader))
                {
                    detailType = faultInfo.FaultContractInfo.Detail;
                    try
                    {
                        detailObj = serializer.ReadObject(detailReader);
                        FaultException faultException = CreateFaultException(messageFault, action, 
                            detailObj, detailType, detailReader);
                        if (faultException != null)
                            return faultException;
                    }
                    catch (SerializationException)
                    {
                    }
                }
            }
            return new FaultException(messageFault, action);
        }
    }
}
