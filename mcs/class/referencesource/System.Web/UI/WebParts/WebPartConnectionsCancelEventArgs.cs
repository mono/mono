//------------------------------------------------------------------------------
// <copyright file="WebPartConnectionsCancelEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    public class WebPartConnectionsCancelEventArgs : CancelEventArgs {
        private WebPart _provider;
        private ProviderConnectionPoint _providerConnectionPoint;
        private WebPart _consumer;
        private ConsumerConnectionPoint _consumerConnectionPoint;
        private WebPartConnection _connection;

        public WebPartConnectionsCancelEventArgs(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                                 WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint) {
            // Arguments may be null, when deleting a connection because a part is no longer on the page
            _provider = provider;
            _providerConnectionPoint = providerConnectionPoint;
            _consumer = consumer;
            _consumerConnectionPoint = consumerConnectionPoint;
        }

        public WebPartConnectionsCancelEventArgs(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                           WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint,
                                           WebPartConnection connection) : this(provider, providerConnectionPoint,
                                                                                consumer, consumerConnectionPoint) {
            _connection = connection;
        }

        public WebPartConnection Connection {
            get {
                return _connection;
            }
        }

        public WebPart Consumer {
            get {
                return _consumer;
            }
        }

        public ConsumerConnectionPoint ConsumerConnectionPoint {
            get {
                return _consumerConnectionPoint;
            }
        }

        public WebPart Provider {
            get {
                return _provider;
            }
        }

        public ProviderConnectionPoint ProviderConnectionPoint {
            get {
                return _providerConnectionPoint;
            }
        }
    }
}


