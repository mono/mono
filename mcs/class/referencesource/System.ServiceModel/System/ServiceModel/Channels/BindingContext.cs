//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;

    public class BindingContext
    {
        CustomBinding binding;
        BindingParameterCollection bindingParameters;
        Uri listenUriBaseAddress;
        ListenUriMode listenUriMode;
        string listenUriRelativeAddress;
        BindingElementCollection remainingBindingElements;  // kept to ensure each BE builds itself once

        public BindingContext(CustomBinding binding, BindingParameterCollection parameters)
            : this(binding, parameters, null, string.Empty, ListenUriMode.Explicit)
        {
        }

        public BindingContext(CustomBinding binding, BindingParameterCollection parameters, Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (listenUriRelativeAddress == null)
            {
                listenUriRelativeAddress = string.Empty;
            }
            if (!ListenUriModeHelper.IsDefined(listenUriMode))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("listenUriMode"));
            }

            Initialize(binding, binding.Elements, parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode);
        }

        BindingContext(CustomBinding binding,
                       BindingElementCollection remainingBindingElements,
                       BindingParameterCollection parameters,
                       Uri listenUriBaseAddress,
                       string listenUriRelativeAddress,
                       ListenUriMode listenUriMode)
        {
            Initialize(binding, remainingBindingElements, parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode);
        }

        void Initialize(CustomBinding binding,
                        BindingElementCollection remainingBindingElements,
                        BindingParameterCollection parameters,
                        Uri listenUriBaseAddress,
                        string listenUriRelativeAddress,
                        ListenUriMode listenUriMode)
        {
            this.binding = binding;

            this.remainingBindingElements = new BindingElementCollection(remainingBindingElements);
            this.bindingParameters = new BindingParameterCollection(parameters);
            this.listenUriBaseAddress = listenUriBaseAddress;
            this.listenUriRelativeAddress = listenUriRelativeAddress;
            this.listenUriMode = listenUriMode;
        }

        public CustomBinding Binding
        {
            get { return this.binding; }
        }

        public BindingParameterCollection BindingParameters
        {
            get { return this.bindingParameters; }
        }

        public Uri ListenUriBaseAddress
        {
            get { return this.listenUriBaseAddress; }
            set { this.listenUriBaseAddress = value; }
        }

        public ListenUriMode ListenUriMode
        {
            get { return this.listenUriMode; }
            set { this.listenUriMode = value; }
        }

        public string ListenUriRelativeAddress
        {
            get { return this.listenUriRelativeAddress; }
            set { this.listenUriRelativeAddress = value; }
        }

        public BindingElementCollection RemainingBindingElements
        {
            get { return this.remainingBindingElements; }
        }

        public IChannelFactory<TChannel> BuildInnerChannelFactory<TChannel>()
        {
            return this.RemoveNextElement().BuildChannelFactory<TChannel>(this);
        }

        public IChannelListener<TChannel> BuildInnerChannelListener<TChannel>()
            where TChannel : class, IChannel
        {
            return this.RemoveNextElement().BuildChannelListener<TChannel>(this);
        }

        public bool CanBuildInnerChannelFactory<TChannel>()
        {
            BindingContext clone = this.Clone();
            return clone.RemoveNextElement().CanBuildChannelFactory<TChannel>(clone);
        }

        public bool CanBuildInnerChannelListener<TChannel>()
            where TChannel : class, IChannel
        {
            BindingContext clone = this.Clone();
            return clone.RemoveNextElement().CanBuildChannelListener<TChannel>(clone);
        }

        public T GetInnerProperty<T>()
            where T : class
        {
            if (this.remainingBindingElements.Count == 0)
            {
                return null;
            }
            else
            {
                BindingContext clone = this.Clone();
                return clone.RemoveNextElement().GetProperty<T>(clone);
            }
        }

        public BindingContext Clone()
        {
            return new BindingContext(this.binding, this.remainingBindingElements, this.bindingParameters,
                this.listenUriBaseAddress, this.listenUriRelativeAddress, this.listenUriMode);
        }

        BindingElement RemoveNextElement()
        {
            BindingElement element = this.remainingBindingElements.Remove<BindingElement>();
            if (element != null)
                return element;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                SR.NoChannelBuilderAvailable, this.binding.Name, this.binding.Namespace)));
        }

        internal void ValidateBindingElementsConsumed()
        {
            if (this.RemainingBindingElements.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (BindingElement bindingElement in this.RemainingBindingElements)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                        builder.Append(" ");
                    }
                    string typeString = bindingElement.GetType().ToString();
                    builder.Append(typeString.Substring(typeString.LastIndexOf('.') + 1));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NotAllBindingElementsBuilt, builder.ToString())));
            }
        }
    }
}
