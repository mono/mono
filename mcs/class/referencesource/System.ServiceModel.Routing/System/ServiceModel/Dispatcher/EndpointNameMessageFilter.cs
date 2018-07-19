//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel.Routing;

    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesMustHaveXamlCallableConstructors)]
    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesShouldHavePublicParameterlessConstructors)]
    public class EndpointNameMessageFilter : MessageFilter
    {
        const string EndpointNameKey = "System.ServiceModel.Routing.EndpointNameMessageFilter.Name";
        string endpointName;

        public EndpointNameMessageFilter(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("endpointName");
            }
            this.endpointName = endpointName;
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }
            return MatchInternal(message.Properties);
        }

        public override bool Match(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw FxTrace.Exception.ArgumentNull("buffer");
            }
            using (Message tempMessage = buffer.CreateMessage())
            {
                return MatchInternal(tempMessage.Properties);
            }
        }

        bool MatchInternal(MessageProperties messageProperties)
        {
            object value;
            if (messageProperties.TryGetValue(EndpointNameKey, out value))
            {
                string messageEndpoint = value.ToString();
                return string.Equals(messageEndpoint, this.endpointName, StringComparison.Ordinal);
            }
            return false;
        }

        internal static void Set(MessageProperties properties, string endpointName)
        {
            properties[EndpointNameKey] = endpointName;
        }

        internal static void Validate(ICollection<MessageFilter> messageFilters, HashSet<string> endpoints)
        {
            foreach (MessageFilter filter in messageFilters)
            {
                EndpointNameMessageFilter endpointFilter = filter as EndpointNameMessageFilter;
                if (endpointFilter != null)
                {
                    endpointFilter.Validate(endpoints);
                }
            }
        }

        void Validate(HashSet<string> endpoints)
        {
            if (!endpoints.Contains(this.endpointName))
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.EndpointNameNotFound(this.endpointName)));
            }
        }
    }
}
