//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------


namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Configuration;
    using System.Globalization;
    using System.Xml;
    
    public abstract class BindingElementExtensionElement : ServiceModelExtensionElement
    {
        public virtual void ApplyConfiguration(BindingElement bindingElement)
        {
            // Some items make sense just as tags and have no other configuration
            if (null == bindingElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElement");
            }
        }

        public abstract Type BindingElementType
        {
            get;
        }

        protected internal abstract BindingElement CreateBindingElement();

        protected internal virtual void InitializeFrom(BindingElement bindingElement)
        {
            if (bindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElement");
            }
            if (bindingElement.GetType() != this.BindingElementType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement",
                    SR.GetString(SR.ConfigInvalidTypeForBindingElement,
                    this.BindingElementType.ToString(),
                    bindingElement.GetType().ToString()));
            }
        }
    }
}
