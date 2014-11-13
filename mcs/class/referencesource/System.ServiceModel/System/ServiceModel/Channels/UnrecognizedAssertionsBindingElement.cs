//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class UnrecognizedAssertionsBindingElement : BindingElement
    {
        XmlQualifiedName wsdlBinding;
        ICollection<XmlElement> bindingAsserions;
        IDictionary<OperationDescription, ICollection<XmlElement>> operationAssertions;
        IDictionary<MessageDescription, ICollection<XmlElement>> messageAssertions;

        internal protected UnrecognizedAssertionsBindingElement(XmlQualifiedName wsdlBinding, ICollection<XmlElement> bindingAsserions)
        {
            Fx.Assert(wsdlBinding != null, "");
            this.wsdlBinding = wsdlBinding;
            this.bindingAsserions = bindingAsserions;
        }

        internal XmlQualifiedName WsdlBinding
        {
            get { return this.wsdlBinding; }
        }

        internal ICollection<XmlElement> BindingAsserions
        {
            get
            {
                if (this.bindingAsserions == null)
                    this.bindingAsserions = new Collection<XmlElement>();
                return this.bindingAsserions;
            }
        }

        internal IDictionary<OperationDescription, ICollection<XmlElement>> OperationAssertions
        {
            get
            {
                if (this.operationAssertions == null)
                    this.operationAssertions = new Dictionary<OperationDescription, ICollection<XmlElement>>();
                return this.operationAssertions;
            }
        }

        internal IDictionary<MessageDescription, ICollection<XmlElement>> MessageAssertions
        {
            get
            {
                if (this.messageAssertions == null)
                    this.messageAssertions = new Dictionary<MessageDescription, ICollection<XmlElement>>();
                return this.messageAssertions;
            }
        }

        internal void Add(OperationDescription operation, ICollection<XmlElement> assertions)
        {
            ICollection<XmlElement> existent;
            if (!OperationAssertions.TryGetValue(operation, out existent))
            {
                OperationAssertions.Add(operation, assertions);
            }
            else
            {
                foreach (XmlElement assertion in assertions)
                    existent.Add(assertion);
            }
        }

        internal void Add(MessageDescription message, ICollection<XmlElement> assertions)
        {
            ICollection<XmlElement> existent;
            if (!MessageAssertions.TryGetValue(message, out existent))
            {
                MessageAssertions.Add(message, assertions);
            }
            else
            {
                foreach (XmlElement assertion in assertions)
                    existent.Add(assertion);
            }
        }

        protected UnrecognizedAssertionsBindingElement(UnrecognizedAssertionsBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.wsdlBinding = elementToBeCloned.wsdlBinding;
            this.bindingAsserions = elementToBeCloned.bindingAsserions;
            this.operationAssertions = elementToBeCloned.operationAssertions;
            this.messageAssertions = elementToBeCloned.messageAssertions;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.GetInnerProperty<T>();
        }

        public override BindingElement Clone()
        {
            //throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedBindingElementClone, typeof(UnrecognizedAssertionsBindingElement).Name)));
            // do not allow Cloning, return an empty BindingElement
            return new UnrecognizedAssertionsBindingElement(new XmlQualifiedName(wsdlBinding.Name, wsdlBinding.Namespace), null);
        }
    }
}

