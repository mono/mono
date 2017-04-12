//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.Security.Principal;
    using System.ServiceModel.Channels;

    public partial class TransactionFlowElement : BindingElementExtensionElement
    {
        public TransactionFlowElement() 
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            TransactionFlowBindingElement binding = (TransactionFlowBindingElement)bindingElement;
            binding.Transactions = true;
            binding.TransactionProtocol = this.TransactionProtocol;
            binding.AllowWildcardAction = this.AllowWildcardAction;
        }

        [ConfigurationProperty(ConfigurationStrings.TransactionProtocol, DefaultValue = TransactionFlowDefaults.TransactionProtocolString)]
        [TypeConverter(typeof(TransactionProtocolConverter))]
        public TransactionProtocol TransactionProtocol
        {
            get { return (TransactionProtocol)base[ConfigurationStrings.TransactionProtocol]; }
            set { base[ConfigurationStrings.TransactionProtocol] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransactionAllowWildcardAction, DefaultValue = false)]
        public bool AllowWildcardAction
        {
            get { return (bool)base[ConfigurationStrings.TransactionAllowWildcardAction]; }
            set { base[ConfigurationStrings.TransactionAllowWildcardAction] = value; }
        }


        public override Type BindingElementType
        {
            get { return typeof(TransactionFlowBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);            
            TransactionFlowElement source = (TransactionFlowElement)from;
#pragma warning suppress 56506 // Microsoft, base.CopyFrom() validates the argument            
            this.TransactionProtocol = source.TransactionProtocol;
        }

        override protected internal BindingElement CreateBindingElement()
        {
            return new TransactionFlowBindingElement(true, TransactionProtocol)
            {
                AllowWildcardAction = this.AllowWildcardAction
            };
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            TransactionFlowBindingElement binding = (TransactionFlowBindingElement)bindingElement;            
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionProtocol, binding.TransactionProtocol);
        }
    }
}



