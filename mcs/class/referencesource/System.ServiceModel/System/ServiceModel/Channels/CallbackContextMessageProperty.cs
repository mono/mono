//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Xml;
    using System.Runtime;

    [Serializable]
    public class CallbackContextMessageProperty : IMessageProperty
    {
        const string PropertyName = "CallbackContextMessageProperty";

        // these hold init data from ctors
        [NonSerializedAttribute]
        readonly EndpointAddress listenAddress;
        readonly IDictionary<string, string> context;

        // used to cache "assembled" EndpointAddress that contains context property
        [NonSerializedAttribute]
        EndpointAddress callbackAddress;

        // if this constructor is used, the listen address will have to be provided later by setting it in the ContextBindingElement
        // CallbackContextMessageProperty will flow on the wire only if listenaddress is set.
        public CallbackContextMessageProperty(IDictionary<string, string> context)
            : this((EndpointAddress)null, context)
        {
        }

        public CallbackContextMessageProperty(string listenAddress, IDictionary<string, string> context)
            : this(new Uri(listenAddress), context)
        {
        }

        public CallbackContextMessageProperty(Uri listenAddress, IDictionary<string, string> context)
            : this(new EndpointAddress(listenAddress), context)
        {
        }

        public CallbackContextMessageProperty(EndpointAddress listenAddress, IDictionary<string, string> context)
        {
            if (listenAddress != null && listenAddress.Headers.FindHeader(ContextMessageHeader.ContextHeaderName, ContextMessageHeader.ContextHeaderNamespace) != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ListenAddressAlreadyContainsContext));
            }
            this.listenAddress = listenAddress;
            this.context = context;
        }

        public CallbackContextMessageProperty(EndpointAddress callbackAddress)
        {
            if (callbackAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackAddress");
            }
            this.callbackAddress = callbackAddress;
        }

        public static string Name
        {
            get
            {
                return PropertyName;
            }
        }

        public EndpointAddress CallbackAddress
        {
            get
            {
                if (this.callbackAddress == null && this.listenAddress != null)
                {
                    this.callbackAddress = CreateCallbackAddress(this.listenAddress, this.context);
                }
                return this.callbackAddress;
            }
        }

        public IDictionary<string, string> Context
        {
            get
            {
                return this.context;
            }
        }

        public EndpointAddress CreateCallbackAddress(Uri listenAddress)
        {
            if (listenAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenAddress");
            }

            return CreateCallbackAddress(new EndpointAddress(listenAddress), this.context);
        }

        static EndpointAddress CreateCallbackAddress(EndpointAddress listenAddress, IDictionary<string, string> context)
        {
            if (listenAddress == null)
            {
                return null;
            }

            EndpointAddressBuilder builder = new EndpointAddressBuilder(listenAddress);
            if (context != null)
            {
                builder.Headers.Add(new ContextAddressHeader(context));
            }
            return builder.ToEndpointAddress();
        }

        public static bool TryGet(Message message, out CallbackContextMessageProperty contextMessageProperty)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return TryGet(message.Properties, out contextMessageProperty);
        }

        public static bool TryGet(MessageProperties properties, out CallbackContextMessageProperty contextMessageProperty)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            object value = null;
            if (properties.TryGetValue(PropertyName, out value))
            {
                contextMessageProperty = value as CallbackContextMessageProperty;
            }
            else
            {
                contextMessageProperty = null;
            }

            return contextMessageProperty != null;
        }

        public void AddOrReplaceInMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            this.AddOrReplaceInMessageProperties(message.Properties);
        }

        public void AddOrReplaceInMessageProperties(MessageProperties properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            properties[PropertyName] = this;
        }

        public IMessageProperty CreateCopy()
        {
            if (this.callbackAddress != null)
            {
                return new CallbackContextMessageProperty(this.callbackAddress);
            }
            else
            {
                return new CallbackContextMessageProperty(this.listenAddress, this.context);
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters,
            Justification = "The method needs to return two objects with one parsing")]
        public void GetListenAddressAndContext(out EndpointAddress listenAddress, out IDictionary<string, string> context)
        {
            // we expect the callback address to be already set when this is called
            if (this.CallbackAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackaddress");
            }
            EndpointAddressBuilder builder = new EndpointAddressBuilder(this.CallbackAddress);
            AddressHeader contextHeader = null;
            int contextHeaderIndex = -1;
            for (int i = 0; i < builder.Headers.Count; ++i)
            {
                if (builder.Headers[i].Name == ContextMessageHeader.ContextHeaderName && builder.Headers[i].Namespace == ContextMessageHeader.ContextHeaderNamespace)
                {
                    if (contextHeader != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.MultipleContextHeadersFoundInCallbackAddress)));
                    }
                    contextHeader = builder.Headers[i];
                    contextHeaderIndex = i;
                }
            }
            if (contextHeader != null)
            {
                builder.Headers.RemoveAt(contextHeaderIndex);
            }
            context = (contextHeader != null) ? ContextMessageHeader.ParseContextHeader(contextHeader.GetAddressHeaderReader()).Context : null;
            listenAddress = builder.ToEndpointAddress();
        }
    }
}
