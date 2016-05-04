//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public partial class BasicHttpContextBindingElement : BasicHttpBindingElement
    {
        const string ContextManagementEnabledName = ContextBindingElementExtensionElement.ContextManagementEnabledName;

        public BasicHttpContextBindingElement()
            : base()
        {
        }

        public BasicHttpContextBindingElement(string name)
            : base(name)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(BasicHttpContextBinding); }
        }

        [ConfigurationProperty(ContextManagementEnabledName, DefaultValue = ContextBindingElement.DefaultContextManagementEnabled)]
        public bool ContextManagementEnabled
        {
            get { return (bool)base[ContextManagementEnabledName]; }
            set { base[ContextManagementEnabledName] = value; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            BasicHttpContextBinding basicHttpContextBinding = (BasicHttpContextBinding)binding;
            SetPropertyValueIfNotDefaultValue(BasicHttpContextBindingElement.ContextManagementEnabledName, basicHttpContextBinding.ContextManagementEnabled);
        }

        internal override void InitializeAllowCookies(HttpBindingBase binding)
        {
            // do not emit allowCookies=true in generated config because BasicHttpContextBinding will always set AllowCookies to true anyway
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            if (this.ElementInformation.Properties["allowCookies"].ValueOrigin == PropertyValueOrigin.Default)
            {
                ((BasicHttpBinding) binding).AllowCookies = true;
            }
            else if (!this.AllowCookies)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.BasicHttpContextBindingRequiresAllowCookie, this.Name, ""));
            }

            ((BasicHttpContextBinding)binding).ContextManagementEnabled = this.ContextManagementEnabled;
        }
    }
}
