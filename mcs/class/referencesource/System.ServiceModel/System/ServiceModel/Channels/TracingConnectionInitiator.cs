//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Text;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.IO;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    class TracingConnectionInitiator : IConnectionInitiator
    {
        IConnectionInitiator connectionInitiator;
        ServiceModelActivity activity;
        Uri connectedUri;
        bool isClient;

        internal TracingConnectionInitiator(IConnectionInitiator connectionInitiator, bool isClient)
        {
            this.connectionInitiator = connectionInitiator;
            this.activity = ServiceModelActivity.CreateActivity(DiagnosticTraceBase.ActivityId);
            this.isClient = isClient;
        }

        public IConnection Connect(Uri uri, TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                IConnection retval = this.connectionInitiator.Connect(uri, timeout);
                if (!this.isClient)
                {
                    TracingConnection tracingConnection = new TracingConnection(retval, false);
                    tracingConnection.ActivityStart(uri);
                    retval = tracingConnection;
                }
                return retval;
            }
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                this.connectedUri = uri;
                return this.connectionInitiator.BeginConnect(uri, timeout, callback, state);
            }
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                TracingConnection connection = new TracingConnection(this.connectionInitiator.EndConnect(result), false);
                connection.ActivityStart(this.connectedUri);
                return connection;
            }
        }
    }
}
