//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Runtime.Serialization;

    public class FaultContractInfo
    {
        string action;
        Type detail;
        string elementName;
        string ns;
        IList<Type> knownTypes;
        DataContractSerializer serializer;

        public FaultContractInfo(string action, Type detail)
            : this(action, detail, null, null, null)
        {
        }
        internal FaultContractInfo(string action, Type detail, XmlName elementName, string ns, IList<Type> knownTypes)
        {
            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("action");
            }
            if (detail == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("detail");
            }

            this.action = action;
            this.detail = detail;
            if (elementName != null)
                this.elementName = elementName.EncodedName;
            this.ns = ns;
            this.knownTypes = knownTypes;
        }

        public string Action { get { return this.action; } }

        public Type Detail { get { return this.detail; } }

        internal string ElementName { get { return this.elementName; } }

        internal string ElementNamespace { get { return this.ns; } }

        internal IList<Type> KnownTypes { get { return this.knownTypes; } }

        internal DataContractSerializer Serializer
        {
            get
            {
                if (this.serializer == null)
                {
                    if (this.elementName == null)
                    {
                        this.serializer = DataContractSerializerDefaults.CreateSerializer(this.detail, this.knownTypes, int.MaxValue /* maxItemsInObjectGraph */);
                    }
                    else
                    {
                        this.serializer = DataContractSerializerDefaults.CreateSerializer(this.detail, this.knownTypes, this.elementName, this.ns == null ? string.Empty : this.ns, int.MaxValue /* maxItemsInObjectGraph */);
                    }
                }
                return this.serializer;
            }
        }
    }
}

