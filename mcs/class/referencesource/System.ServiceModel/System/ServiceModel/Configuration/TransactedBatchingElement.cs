//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed partial class TransactedBatchingElement : BehaviorExtensionElement
    {
        [ConfigurationProperty(ConfigurationStrings.MaxBatchSize, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxBatchSize
        {
            get { return (int)base[ConfigurationStrings.MaxBatchSize]; }
            set { base[ConfigurationStrings.MaxBatchSize] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            TransactedBatchingElement source = from as TransactedBatchingElement;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.MaxBatchSize = source.MaxBatchSize;
        }

        protected internal override object CreateBehavior()
        {
            return new TransactedBatchingBehavior(this.MaxBatchSize);
        }

        public override Type BehaviorType
        {
            get { return typeof(TransactedBatchingBehavior); }
        }

    }
}



