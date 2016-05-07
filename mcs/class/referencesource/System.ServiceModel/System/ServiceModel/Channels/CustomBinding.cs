//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel.Configuration;
    using System.Net.Security;
    using System.ServiceModel.Security;
    using System.Windows.Markup;

    [ContentProperty("Elements")]
    public class CustomBinding : Binding
    {
        BindingElementCollection bindingElements = new BindingElementCollection();

        public CustomBinding()
            : base()
        {
        }

        public CustomBinding(string configurationName)
        {
            ApplyConfiguration(configurationName);
        }

        public CustomBinding(params BindingElement[] bindingElementsInTopDownChannelStackOrder)
            : base()
        {
            if (bindingElementsInTopDownChannelStackOrder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }

            foreach (BindingElement element in bindingElementsInTopDownChannelStackOrder)
            {
                this.bindingElements.Add(element);
            }
        }

        public CustomBinding(string name, string ns, params BindingElement[] bindingElementsInTopDownChannelStackOrder)
            : base(name, ns)
        {
            if (bindingElementsInTopDownChannelStackOrder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }

            foreach (BindingElement element in bindingElementsInTopDownChannelStackOrder)
            {
                this.bindingElements.Add(element);
            }
        }

        public CustomBinding(IEnumerable<BindingElement> bindingElementsInTopDownChannelStackOrder)
        {
            if (bindingElementsInTopDownChannelStackOrder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }

            foreach (BindingElement element in bindingElementsInTopDownChannelStackOrder)
            {
                this.bindingElements.Add(element);
            }
        }

        internal CustomBinding(BindingElementCollection bindingElements)
            : base()
        {
            if (bindingElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }

            for (int i = 0; i < bindingElements.Count; i++)
            {
                this.bindingElements.Add(bindingElements[i]);
            }
        }

        public CustomBinding(Binding binding)
            : this(binding, SafeCreateBindingElements(binding))
        {
        }

        static BindingElementCollection SafeCreateBindingElements(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            return binding.CreateBindingElements();
        }

        internal CustomBinding(Binding binding, BindingElementCollection elements)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }

            this.Name = binding.Name;
            this.Namespace = binding.Namespace;
            this.CloseTimeout = binding.CloseTimeout;
            this.OpenTimeout = binding.OpenTimeout;
            this.ReceiveTimeout = binding.ReceiveTimeout;
            this.SendTimeout = binding.SendTimeout;

            for (int i = 0; i < elements.Count; i++)
            {
                bindingElements.Add(elements[i]);
            }
        }

        public BindingElementCollection Elements
        {
            get
            {
                return bindingElements;
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            return this.bindingElements.Clone();
        }

        void ApplyConfiguration(string configurationName)
        {
            CustomBindingCollectionElement section = CustomBindingCollectionElement.GetBindingCollectionElement();
            CustomBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.CustomBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        public override string Scheme
        {
            get
            {
                TransportBindingElement transport = bindingElements.Find<TransportBindingElement>();
                if (transport == null)
                {
                    return String.Empty;
                }

                return transport.Scheme;
            }
        }
    }
}

