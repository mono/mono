//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel.Security;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;

    public sealed partial class DataContractSerializerElement : BehaviorExtensionElement
    {
        public DataContractSerializerElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.IgnoreExtensionDataObject, DefaultValue = DataContractSerializerDefaults.IgnoreExtensionDataObject)]
        public bool IgnoreExtensionDataObject
        {
            get { return (bool)base[ConfigurationStrings.IgnoreExtensionDataObject]; }
            set { base[ConfigurationStrings.IgnoreExtensionDataObject] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxItemsInObjectGraph, DefaultValue = DataContractSerializerDefaults.MaxItemsInObjectGraph)]
        [IntegerValidator(MinValue = 0)]
        public int MaxItemsInObjectGraph
        {
            get { return (int)base[ConfigurationStrings.MaxItemsInObjectGraph]; }
            set { base[ConfigurationStrings.MaxItemsInObjectGraph] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            DataContractSerializerElement source = (DataContractSerializerElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.IgnoreExtensionDataObject = source.IgnoreExtensionDataObject;
            this.MaxItemsInObjectGraph = source.MaxItemsInObjectGraph;
        }

        protected internal override object CreateBehavior()
        {
            return new DataContractSerializerServiceBehavior(this.IgnoreExtensionDataObject, this.MaxItemsInObjectGraph);
        }

        public override Type BehaviorType
        {
            get { return typeof(DataContractSerializerServiceBehavior); }
        }
    }
}



