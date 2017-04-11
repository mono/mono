//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;


    delegate IConnectionOrientedTransportFactorySettings TransportSettingsCallback(Uri via);
    delegate void ConnectionClosedCallback(InitialServerConnectionReader connectionReader);

    // Host for a connection that deals with structured close/abort and notifying the owner appropriately
    // used for cases where no one else (channel, etc) actually owns the reader
    abstract class InitialServerConnectionReader : IDisposable
    {
        int maxViaSize;
        int maxContentTypeSize;
        IConnection connection;
        Action connectionDequeuedCallback;
        ConnectionClosedCallback closedCallback;
        bool isClosed;

        protected InitialServerConnectionReader(IConnection connection, ConnectionClosedCallback closedCallback)
            : this(connection, closedCallback,
            ConnectionOrientedTransportDefaults.MaxViaSize, ConnectionOrientedTransportDefaults.MaxContentTypeSize)
        {
        }

        protected InitialServerConnectionReader(IConnection connection, ConnectionClosedCallback closedCallback, int maxViaSize, int maxContentTypeSize)
        {
            if (connection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("connection");
            }

            if (closedCallback == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("closedCallback");
            }

            this.connection = connection;
            this.closedCallback = closedCallback;
            this.maxContentTypeSize = maxContentTypeSize;
            this.maxViaSize = maxViaSize;
        }

        public IConnection Connection
        {
            get { return connection; }
        }

        public Action ConnectionDequeuedCallback
        {
            get
            {
                return this.connectionDequeuedCallback;
            }

            set
            {
                this.connectionDequeuedCallback = value;
            }
        }

        public Action GetConnectionDequeuedCallback()
        {
            Action dequeuedCallback = this.connectionDequeuedCallback;
            this.connectionDequeuedCallback = null;
            return dequeuedCallback;
        }

        protected bool IsClosed
        {
            get { return isClosed; }
        }

        protected int MaxContentTypeSize
        {
            get
            {
                return maxContentTypeSize;
            }
        }

        protected int MaxViaSize
        {
            get
            {
                return maxViaSize;
            }
        }

        object ThisLock
        {
            get { return this; }
        }

        // used by the listener to release the connection object so it can be closed at a later time
        public void ReleaseConnection()
        {
            isClosed = true;
            connection = null;
        }

        // for cached connections -- try to shut down gracefully if possible
        public void CloseFromPool(TimeSpan timeout)
        {
            try
            {
                Close(timeout);
            }
            catch (CommunicationException communicationException)
            {
                DiagnosticUtility.TraceHandledException(communicationException, TraceEventType.Information);
            }
            catch (TimeoutException timeoutException)
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(timeoutException.Message);
                }
                DiagnosticUtility.TraceHandledException(timeoutException, TraceEventType.Information);
            }
        }

        public void Dispose()
        {
            lock (ThisLock)
            {
                if (isClosed)
                {
                    return;
                }

                this.isClosed = true;
            }

            IConnection connection = this.connection;
            if (connection != null)
            {
                connection.Abort();
            }

            if (this.connectionDequeuedCallback != null)
            {
                this.connectionDequeuedCallback();
            }
        }

        protected void Abort()
        {
            Abort(null);
        }

        internal void Abort(Exception e)
        {
            lock (ThisLock)
            {
                if (isClosed)
                    return;

                isClosed = true;
            }

            try
            {
                if (e != null)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.ChannelConnectionDropped,
                            SR.GetString(SR.TraceCodeChannelConnectionDropped), this, e);
                    }
                }

                connection.Abort();
            }
            finally
            {
                if (closedCallback != null)
                {
                    closedCallback(this);
                }

                if (this.connectionDequeuedCallback != null)
                {
                    this.connectionDequeuedCallback();
                }
            }
        }

        protected void Close(TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (isClosed)
                    return;

                isClosed = true;
            }

            bool success = false;
            try
            {
                connection.Close(timeout, true);
                success = true;
            }
            finally
            {
                if (!success)
                {
                    connection.Abort();
                }

                if (closedCallback != null)
                {
                    closedCallback(this);
                }

                if (this.connectionDequeuedCallback != null)
                {
                    this.connectionDequeuedCallback();
                }
            }
        }

        internal static void SendFault(IConnection connection, string faultString, byte[] drainBuffer, TimeSpan sendTimeout, int maxRead)
        {

            if (TD.ConnectionReaderSendFaultIsEnabled())
            {
                TD.ConnectionReaderSendFault(faultString);
            }

            EncodedFault encodedFault = new EncodedFault(faultString);
            TimeoutHelper timeoutHelper = new TimeoutHelper(sendTimeout);
            try
            {
                connection.Write(encodedFault.EncodedBytes, 0, encodedFault.EncodedBytes.Length, true, timeoutHelper.RemainingTime());
                connection.Shutdown(timeoutHelper.RemainingTime());
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                connection.Abort();
                return;
            }
            catch (TimeoutException e)
            {
                if (TD.SendTimeoutIsEnabled())
                {
                    TD.SendTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                connection.Abort();
                return;
            }

            // make sure we read until EOF or a quota is hit
            int read = 0;
            int readTotal = 0;
            for (;;)
            {
                try
                {
                    read = connection.Read(drainBuffer, 0, drainBuffer.Length, timeoutHelper.RemainingTime());
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    connection.Abort();
                    return;
                }
                catch (TimeoutException e)
                {
                    if (TD.SendTimeoutIsEnabled())
                    {
                        TD.SendTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    connection.Abort();
                    return;
                }

                if (read == 0)
                    break;

                readTotal += read;
                if (readTotal > maxRead || timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                {
                    connection.Abort();
                    return;
                }
            }

            ConnectionUtilities.CloseNoThrow(connection, timeoutHelper.RemainingTime());
        }

        public static IAsyncResult BeginUpgradeConnection(IConnection connection, StreamUpgradeAcceptor upgradeAcceptor,
            IDefaultCommunicationTimeouts defaultTimeouts, AsyncCallback callback, object state)
        {
            return new UpgradeConnectionAsyncResult(connection, upgradeAcceptor, defaultTimeouts, callback, state);
        }

        public static IConnection EndUpgradeConnection(IAsyncResult result)
        {
            // get our upgraded connection
            return UpgradeConnectionAsyncResult.End(result);
        }

        public static IConnection UpgradeConnection(IConnection connection, StreamUpgradeAcceptor upgradeAcceptor, IDefaultCommunicationTimeouts defaultTimeouts)
        {
            ConnectionStream connectionStream = new ConnectionStream(connection, defaultTimeouts);
            Stream stream = upgradeAcceptor.AcceptUpgrade(connectionStream);
            if (upgradeAcceptor is StreamSecurityUpgradeAcceptor)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.StreamSecurityUpgradeAccepted, SR.GetString(SR.TraceCodeStreamSecurityUpgradeAccepted),
                        new StringTraceRecord("Type", upgradeAcceptor.GetType().ToString()), connection, null);
                }
            }

            return new StreamConnection(stream, connectionStream);
        }

        class UpgradeConnectionAsyncResult : AsyncResult
        {
            ConnectionStream connectionStream;
            static AsyncCallback onAcceptUpgrade = Fx.ThunkCallback(new AsyncCallback(OnAcceptUpgrade));
            IConnection connection;
            StreamUpgradeAcceptor upgradeAcceptor;

            public UpgradeConnectionAsyncResult(IConnection connection,
                StreamUpgradeAcceptor upgradeAcceptor, IDefaultCommunicationTimeouts defaultTimeouts,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.upgradeAcceptor = upgradeAcceptor;
                this.connectionStream = new ConnectionStream(connection, defaultTimeouts);
                bool completeSelf = false;

                IAsyncResult result = upgradeAcceptor.BeginAcceptUpgrade(connectionStream, onAcceptUpgrade, this);

                if (result.CompletedSynchronously)
                {
                    CompleteAcceptUpgrade(result);
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    base.Complete(true);
                }
            }

            public static IConnection End(IAsyncResult result)
            {
                UpgradeConnectionAsyncResult thisPtr = AsyncResult.End<UpgradeConnectionAsyncResult>(result);
                return thisPtr.connection;
            }

            void CompleteAcceptUpgrade(IAsyncResult result)
            {
                Stream stream;
                bool endSucceeded = false;
                try
                {
                    stream = this.upgradeAcceptor.EndAcceptUpgrade(result);
                    endSucceeded = true;
                }
                finally
                {
                    if (upgradeAcceptor is StreamSecurityUpgradeAcceptor)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation && endSucceeded)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.StreamSecurityUpgradeAccepted, SR.GetString(SR.TraceCodeStreamSecurityUpgradeAccepted),
                                new StringTraceRecord("Type", upgradeAcceptor.GetType().ToString()), this, null);
                        }
                    }
                }
                this.connection = new StreamConnection(stream, this.connectionStream);
            }

            static void OnAcceptUpgrade(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                UpgradeConnectionAsyncResult thisPtr = (UpgradeConnectionAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.CompleteAcceptUpgrade(result);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }
        }
    }
}
