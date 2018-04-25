//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class NetTcpContextBinding : NetTcpBinding
    {
        bool contextManagementEnabled = ContextBindingElement.DefaultContextManagementEnabled;
        ProtectionLevel contextProtectionLevel = ContextBindingElement.DefaultProtectionLevel;

        public NetTcpContextBinding()
            : base()
        {
        }

        public NetTcpContextBinding(SecurityMode securityMode)
            : base(securityMode)
        {
        }

        public NetTcpContextBinding(string configName)
            : base()
        {
            if (configName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configName");
            }

            this.ApplyConfiguration(configName);
        }

        public NetTcpContextBinding(SecurityMode securityMode, bool reliableSessionEnabled)
            : base(securityMode, reliableSessionEnabled)
        {
        }

        NetTcpContextBinding(NetTcpBinding netTcpBinding)
        {
            NetTcpContextBindingPropertyTransferHelper helper = new NetTcpContextBindingPropertyTransferHelper();
            helper.InitializeFrom(netTcpBinding);
            helper.SetBindingElementType(typeof(NetTcpContextBinding));
            helper.ApplyConfiguration(this);
        }

        [DefaultValue(null)]
        public Uri ClientCallbackAddress
        {
            get;
            set;
        }

        [DefaultValue(ContextBindingElement.DefaultContextManagementEnabled)]
        public bool ContextManagementEnabled
        {
            get
            {
                return this.contextManagementEnabled;
            }
            set
            {
                this.contextManagementEnabled = value;
            }
        }

        [DefaultValue(ContextBindingElement.DefaultProtectionLevel)]
        public ProtectionLevel ContextProtectionLevel
        {
            get
            {
                return this.contextProtectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.contextProtectionLevel = value;
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection result = base.CreateBindingElements();
            result.Insert(0, new ContextBindingElement(this.ContextProtectionLevel, ContextExchangeMechanism.ContextSoapHeader, this.ClientCallbackAddress, this.ContextManagementEnabled));
            return result;
        }

        internal static new bool TryCreate(BindingElementCollection bindingElements, out Binding binding)
        {
            if (bindingElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }

            binding = null;

            ContextBindingElement contextBindingElement = bindingElements.Find<ContextBindingElement>();
            if (contextBindingElement != null && contextBindingElement.ContextExchangeMechanism != ContextExchangeMechanism.HttpCookie)
            {
                BindingElementCollection bindingElementsWithoutContext = new BindingElementCollection(bindingElements);
                bindingElementsWithoutContext.Remove<ContextBindingElement>();
                Binding netTcpBinding;
                if (NetTcpBinding.TryCreate(bindingElementsWithoutContext, out netTcpBinding))
                {
                    NetTcpContextBinding contextBinding = new NetTcpContextBinding((NetTcpBinding)netTcpBinding);
                    contextBinding.ContextProtectionLevel = contextBindingElement.ProtectionLevel;
                    contextBinding.ContextManagementEnabled = contextBindingElement.ContextManagementEnabled;
                    binding = contextBinding;
                }
            }

            return binding != null;
        }

        void ApplyConfiguration(string configurationName)
        {
            NetTcpContextBindingCollectionElement section = NetTcpContextBindingCollectionElement.GetBindingCollectionElement();
            NetTcpContextBindingElement element = section.Bindings[configurationName];
            element.ApplyConfiguration(this);
        }

        class NetTcpContextBindingPropertyTransferHelper : NetTcpBindingElement
        {
            Type bindingElementType = typeof(NetTcpBinding);

            protected override Type BindingElementType
            {
                get
                {
                    return this.bindingElementType;
                }
            }

            public void SetBindingElementType(Type bindingElementType)
            {
                this.bindingElementType = bindingElementType;
            }
        }
    }
}
