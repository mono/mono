//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class BasicHttpContextBinding : BasicHttpBinding
    {
        bool contextManagementEnabled = ContextBindingElement.DefaultContextManagementEnabled;

        public BasicHttpContextBinding()
            : base()
        {
            this.AllowCookies = true;
        }

        public BasicHttpContextBinding(BasicHttpSecurityMode securityMode)
            : base(securityMode)
        {
            this.AllowCookies = true;
        }

        public BasicHttpContextBinding(string configName)
            : base()
        {
            if (configName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configName");
            }

            BasicHttpContextBindingCollectionElement section = BasicHttpContextBindingCollectionElement.GetBindingCollectionElement();
            BasicHttpContextBindingElement element = section.Bindings[configName];
            element.ApplyConfiguration(this);
            if (element.ElementInformation.Properties["allowCookies"].ValueOrigin == PropertyValueOrigin.Default)
            {
                this.AllowCookies = true;
            }
            else if (!this.AllowCookies)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.BasicHttpContextBindingRequiresAllowCookie, this.Namespace, this.Name));
            }
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

        public override BindingElementCollection CreateBindingElements()
        {
            if (!this.AllowCookies)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BasicHttpContextBindingRequiresAllowCookie, this.Namespace, this.Name)));
            }

            BindingElementCollection result;
            try
            {
                // Passing AllowCookies=false to HttpTransportBinding means we don't want transport layer to manage
                // cookie containers. We are going to do this at the context channel level, because we need channel 
                // level isolation as opposed to channel factory level isolation. 

                this.AllowCookies = false;
                result = base.CreateBindingElements();
            }
            finally
            {
                this.AllowCookies = true;
            }
            result.Insert(0, new ContextBindingElement(ProtectionLevel.None, ContextExchangeMechanism.HttpCookie, null, this.ContextManagementEnabled));

            return result;
        }
    }
}
