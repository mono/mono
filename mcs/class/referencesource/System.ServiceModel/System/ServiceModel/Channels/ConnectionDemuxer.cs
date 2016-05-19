//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    sealed class ConnectionDemuxer : IDisposable
    {
        static AsyncCallback onSingletonPreambleComplete;
        ConnectionAcceptor acceptor;

        // we use this list to track readers that don't have a clear owner (so they don't get GC'ed)
        List<InitialServerConnectionReader> connectionReaders;

        bool isDisposed;
        ConnectionModeCallback onConnectionModeKnown;
        ConnectionModeCallback onCachedConnectionModeKnown;
        ConnectionClosedCallback onConnectionClosed;
        ServerSessionPreambleCallback onSessionPreambleKnown;
        ServerSingletonPreambleCallback onSingletonPreambleKnown;
        Action<object> reuseConnectionCallback;
        ServerSessionPreambleDemuxCallback serverSessionPreambleCallback;
        SingletonPreambleDemuxCallback singletonPreambleCallback;
        TransportSettingsCallback transportSettingsCallback;
        Action pooledConnectionDequeuedCallback;
        Action<Uri> viaDelegate;
        TimeSpan channelInitializationTimeout;
        TimeSpan idleTimeout;
        int maxPooledConnections;
        int pooledConnectionCount;

        public ConnectionDemuxer(IConnectionListener listener, int maxAccepts, int maxPendingConnections,
            TimeSpan channelInitializationTimeout, TimeSpan idleTimeout, int maxPooledConnections,
            TransportSettingsCallback transportSettingsCallback,
            SingletonPreambleDemuxCallback singletonPreambleCallback,
            ServerSessionPreambleDemuxCallback serverSessionPreambleCallback, ErrorCallback errorCallback)
        {
            this.connectionReaders = new List<InitialServerConnectionReader>();
            this.acceptor =
                new ConnectionAcceptor(listener, maxAccepts, maxPendingConnections, OnConnectionAvailable, errorCallback);
            this.channelInitializationTimeout = channelInitializationTimeout;
            this.idleTimeout = idleTimeout;
            this.maxPooledConnections = maxPooledConnections;
            this.onConnectionClosed = new ConnectionClosedCallback(OnConnectionClosed);
            this.transportSettingsCallback = transportSettingsCallback;
            this.singletonPreambleCallback = singletonPreambleCallback;
            this.serverSessionPreambleCallback = serverSessionPreambleCallback;
        }

        object ThisLock
        {
            get { return this; }
        }

        public void Dispose()
        {
            lock (ThisLock)
            {
                if (isDisposed)
                    return;

                isDisposed = true;
            }

            for (int i = 0; i < connectionReaders.Count; i++)
            {
                connectionReaders[i].Dispose();
            }
            connectionReaders.Clear();

            acceptor.Dispose();
        }

        ConnectionModeReader SetupModeReader(IConnection connection, bool isCached)
        {
            ConnectionModeReader modeReader;
            if (isCached)
            {
                if (onCachedConnectionModeKnown == null)
                {
                    onCachedConnectionModeKnown = new ConnectionModeCallback(OnCachedConnectionModeKnown);
                }

                modeReader = new ConnectionModeReader(connection, onCachedConnectionModeKnown, onConnectionClosed);
            }
            else
            {
                if (onConnectionModeKnown == null)
                {
                    onConnectionModeKnown = new ConnectionModeCallback(OnConnectionModeKnown);
                }

                modeReader = new ConnectionModeReader(connection, onConnectionModeKnown, onConnectionClosed);
            }

            lock (ThisLock)
            {
                if (isDisposed)
                {
                    modeReader.Dispose();
                    return null;
                }

                connectionReaders.Add(modeReader);
                return modeReader;
            }
        }

        public void ReuseConnection(IConnection connection, TimeSpan closeTimeout)
        {
            connection.ExceptionEventType = TraceEventType.Information;
            ConnectionModeReader modeReader = SetupModeReader(connection, true);

            if (modeReader != null)
            {
                if (reuseConnectionCallback == null)
                {
                    reuseConnectionCallback = new Action<object>(ReuseConnectionCallback);
                }

                ActionItem.Schedule(reuseConnectionCallback, new ReuseConnectionState(modeReader, closeTimeout));
            }
        }

        void ReuseConnectionCallback(object state)
        {
            ReuseConnectionState connectionState = (ReuseConnectionState)state;
            bool closeReader = false;
            lock (ThisLock)
            {
                if (this.pooledConnectionCount >= this.maxPooledConnections)
                {
                    closeReader = true;
                }
                else
                {
                    this.pooledConnectionCount++;
                }
            }

            if (closeReader)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.ServerMaxPooledConnectionsQuotaReached,
                        SR.GetString(SR.TraceCodeServerMaxPooledConnectionsQuotaReached, maxPooledConnections),
                        new StringTraceRecord("MaxOutboundConnectionsPerEndpoint", maxPooledConnections.ToString(CultureInfo.InvariantCulture)),
                        this, null);
                }

                if (TD.ServerMaxPooledConnectionsQuotaReachedIsEnabled())
                {
                    TD.ServerMaxPooledConnectionsQuotaReached();
                }

                connectionState.ModeReader.CloseFromPool(connectionState.CloseTimeout);
            }
            else
            {
                if (this.pooledConnectionDequeuedCallback == null)
                {
                    this.pooledConnectionDequeuedCallback = new Action(PooledConnectionDequeuedCallback);
                }
                connectionState.ModeReader.StartReading(this.idleTimeout, this.pooledConnectionDequeuedCallback);
            }
        }

        void PooledConnectionDequeuedCallback()
        {
            lock (ThisLock)
            {
                this.pooledConnectionCount--;
                Fx.Assert(this.pooledConnectionCount >= 0, "Connection Throttle should never be negative");
            }
        }

        void OnConnectionAvailable(IConnection connection, Action connectionDequeuedCallback)
        {
            ConnectionModeReader modeReader = SetupModeReader(connection, false);

            if (modeReader != null)
            {
                // StartReading() will never throw non-fatal exceptions; 
                // it propagates all exceptions into the onConnectionModeKnown callback, 
                // which is where we need our robust handling
                modeReader.StartReading(this.channelInitializationTimeout, connectionDequeuedCallback);
            }
            else
            {
                connectionDequeuedCallback();
            }
        }

        void OnCachedConnectionModeKnown(ConnectionModeReader modeReader)
        {
            OnConnectionModeKnownCore(modeReader, true);
        }

        void OnConnectionModeKnown(ConnectionModeReader modeReader)
        {
            OnConnectionModeKnownCore(modeReader, false);
        }

        void OnConnectionModeKnownCore(ConnectionModeReader modeReader, bool isCached)
        {
            lock (ThisLock)
            {
                if (isDisposed)
                    return;

                this.connectionReaders.Remove(modeReader);
            }

            bool closeReader = true;
            try
            {
                FramingMode framingMode;
                try
                {
                    framingMode = modeReader.GetConnectionMode();
                }
                catch (CommunicationException exception)
                {
                    TraceEventType eventType = modeReader.Connection.ExceptionEventType;
                    DiagnosticUtility.TraceHandledException(exception, eventType);
                    return;
                }
                catch (TimeoutException exception)
                {
                    if (!isCached)
                    {
                        exception = new TimeoutException(SR.GetString(SR.ChannelInitializationTimeout, this.channelInitializationTimeout), exception);
                        System.ServiceModel.Dispatcher.ErrorBehavior.ThrowAndCatch(exception);
                    }

                    if (TD.ChannelInitializationTimeoutIsEnabled())
                    {
                        TD.ChannelInitializationTimeout(SR.GetString(SR.ChannelInitializationTimeout, this.channelInitializationTimeout));
                    }

                    TraceEventType eventType = modeReader.Connection.ExceptionEventType;
                    DiagnosticUtility.TraceHandledException(exception, eventType);
                    return;
                }

                switch (framingMode)
                {
                    case FramingMode.Duplex:
                        OnDuplexConnection(modeReader.Connection, modeReader.ConnectionDequeuedCallback,
                            modeReader.StreamPosition, modeReader.BufferOffset, modeReader.BufferSize,
                            modeReader.GetRemainingTimeout());
                        break;

                    case FramingMode.Singleton:
                        OnSingletonConnection(modeReader.Connection, modeReader.ConnectionDequeuedCallback,
                            modeReader.StreamPosition, modeReader.BufferOffset, modeReader.BufferSize,
                            modeReader.GetRemainingTimeout());
                        break;

                    default:
                        {
                            Exception inner = new InvalidDataException(SR.GetString(
                                SR.FramingModeNotSupported, framingMode));
                            Exception exception = new ProtocolException(inner.Message, inner);
                            FramingEncodingString.AddFaultString(exception, FramingEncodingString.UnsupportedModeFault);
                            System.ServiceModel.Dispatcher.ErrorBehavior.ThrowAndCatch(exception);
                            return;
                        }
                }

                closeReader = false;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (!ExceptionHandler.HandleTransportExceptionHelper(e))
                {
                    throw;
                }

                // containment -- the reader is aborted, no need for additional containment
            }
            finally
            {
                if (closeReader)
                {
                    modeReader.Dispose();
                }
            }
        }

        void OnConnectionClosed(InitialServerConnectionReader connectionReader)
        {
            lock (ThisLock)
            {
                if (isDisposed)
                    return;

                connectionReaders.Remove(connectionReader);
            }
        }

        void OnSingletonConnection(IConnection connection, Action connectionDequeuedCallback,
            long streamPosition, int offset, int size, TimeSpan timeout)
        {
            if (onSingletonPreambleKnown == null)
            {
                onSingletonPreambleKnown = OnSingletonPreambleKnown;
            }
            ServerSingletonPreambleConnectionReader singletonPreambleReader =
                new ServerSingletonPreambleConnectionReader(connection, connectionDequeuedCallback, streamPosition, offset, size,
                transportSettingsCallback, onConnectionClosed, onSingletonPreambleKnown);

            lock (ThisLock)
            {
                if (isDisposed)
                {
                    singletonPreambleReader.Dispose();
                    return;
                }

                connectionReaders.Add(singletonPreambleReader);
            }
            singletonPreambleReader.StartReading(viaDelegate, timeout);
        }

        void OnSingletonPreambleKnown(ServerSingletonPreambleConnectionReader serverSingletonPreambleReader)
        {
            lock (ThisLock)
            {
                if (isDisposed)
                {
                    return;
                }

                connectionReaders.Remove(serverSingletonPreambleReader);
            }

            if (onSingletonPreambleComplete == null)
            {
                onSingletonPreambleComplete = Fx.ThunkCallback(new AsyncCallback(OnSingletonPreambleComplete));
            }

            ISingletonChannelListener singletonChannelListener = singletonPreambleCallback(serverSingletonPreambleReader);
            Fx.Assert(singletonChannelListener != null,
                "singletonPreambleCallback must return a listener or send a Fault/throw");

            // transfer ownership of the connection from the preamble reader to the message handler

            IAsyncResult result = BeginCompleteSingletonPreamble(serverSingletonPreambleReader, singletonChannelListener, onSingletonPreambleComplete, this);

            if (result.CompletedSynchronously)
            {
                EndCompleteSingletonPreamble(result);
            }
        }

        IAsyncResult BeginCompleteSingletonPreamble(
            ServerSingletonPreambleConnectionReader serverSingletonPreambleReader, 
            ISingletonChannelListener singletonChannelListener,
            AsyncCallback callback, object state)
        {
            return new CompleteSingletonPreambleAndDispatchRequestAsyncResult(serverSingletonPreambleReader, singletonChannelListener, this, callback, state);
        }

        void EndCompleteSingletonPreamble(IAsyncResult result)
        {
            CompleteSingletonPreambleAndDispatchRequestAsyncResult.End(result);
        }

        static void OnSingletonPreambleComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            ConnectionDemuxer thisPtr = (ConnectionDemuxer)result.AsyncState;

            try
            {
                thisPtr.EndCompleteSingletonPreamble(result);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                //should never actually hit this code - the async result will handle all exceptions, trace them, then abort the reader
                DiagnosticUtility.TraceHandledException(ex, TraceEventType.Warning);
            }
        }
      
        void OnSessionPreambleKnown(ServerSessionPreambleConnectionReader serverSessionPreambleReader)
        {
            lock (ThisLock)
            {
                if (isDisposed)
                {
                    return;
                }

                connectionReaders.Remove(serverSessionPreambleReader);
            }

            TraceOnSessionPreambleKnown(serverSessionPreambleReader);

            serverSessionPreambleCallback(serverSessionPreambleReader, this);
        }

        static void TraceOnSessionPreambleKnown(ServerSessionPreambleConnectionReader serverSessionPreambleReader)
        {
            if (TD.SessionPreambleUnderstoodIsEnabled())
            {
                TD.SessionPreambleUnderstood((serverSessionPreambleReader.Via != null) ? serverSessionPreambleReader.Via.ToString() : String.Empty);
            }
        }

        void OnDuplexConnection(IConnection connection, Action connectionDequeuedCallback,
            long streamPosition, int offset, int size, TimeSpan timeout)
        {
            if (onSessionPreambleKnown == null)
            {
                onSessionPreambleKnown = OnSessionPreambleKnown;
            }
            ServerSessionPreambleConnectionReader sessionPreambleReader = new ServerSessionPreambleConnectionReader(
                connection, connectionDequeuedCallback, streamPosition, offset, size,
                transportSettingsCallback, onConnectionClosed, onSessionPreambleKnown);
            lock (ThisLock)
            {
                if (isDisposed)
                {
                    sessionPreambleReader.Dispose();
                    return;
                }

                connectionReaders.Add(sessionPreambleReader);
            }

            sessionPreambleReader.StartReading(viaDelegate, timeout);
        }

        public void StartDemuxing()
        {
            StartDemuxing(null);
        }

        public void StartDemuxing(Action<Uri> viaDelegate)
        {
            this.viaDelegate = viaDelegate;
            acceptor.StartAccepting();
        }

        class CompleteSingletonPreambleAndDispatchRequestAsyncResult : AsyncResult
        {
            ServerSingletonPreambleConnectionReader serverSingletonPreambleReader;
            ISingletonChannelListener singletonChannelListener;
            ConnectionDemuxer demuxer;
            TimeoutHelper timeoutHelper;

            static AsyncCallback onPreambleComplete = Fx.ThunkCallback(new AsyncCallback(OnPreambleComplete));

            public CompleteSingletonPreambleAndDispatchRequestAsyncResult(
                ServerSingletonPreambleConnectionReader serverSingletonPreambleReader,
                ISingletonChannelListener singletonChannelListener,
                ConnectionDemuxer demuxer,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.serverSingletonPreambleReader = serverSingletonPreambleReader;
                this.singletonChannelListener = singletonChannelListener;
                this.demuxer = demuxer;

                //if this throws, the calling code paths will abort the connection, so we only need to 
                //call AbortConnection if BeginCompletePramble completes asynchronously.
                if (BeginCompletePreamble())
                {
                    Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CompleteSingletonPreambleAndDispatchRequestAsyncResult>(result);
            }

            bool BeginCompletePreamble()
            {
                this.timeoutHelper = new TimeoutHelper(this.singletonChannelListener.ReceiveTimeout);
                IAsyncResult result = this.serverSingletonPreambleReader.BeginCompletePreamble(this.timeoutHelper.RemainingTime(), onPreambleComplete, this);

                if (result.CompletedSynchronously)
                {
                    return HandlePreambleComplete(result);
                }

                return false;
            }

            static void OnPreambleComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CompleteSingletonPreambleAndDispatchRequestAsyncResult thisPtr = (CompleteSingletonPreambleAndDispatchRequestAsyncResult)result.AsyncState;
                bool completeSelf = false;

                try
                {
                    completeSelf = thisPtr.HandlePreambleComplete(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    //Don't complete this AsyncResult with this non-fatal exception.  The calling code can't really do anything with it,
                    //so just trace it (inside of AbortConnection), then ---- it.
                    completeSelf = true;
                    thisPtr.AbortConnection(ex);
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false);
                }
            }

            bool HandlePreambleComplete(IAsyncResult result)
            {
                IConnection upgradedConnection = this.serverSingletonPreambleReader.EndCompletePreamble(result);
                ServerSingletonConnectionReader singletonReader = new ServerSingletonConnectionReader(serverSingletonPreambleReader, upgradedConnection, this.demuxer);

                //singletonReader doesn't have async version of ReceiveRequest, so just call the [....] method for now.
                RequestContext requestContext = singletonReader.ReceiveRequest(this.timeoutHelper.RemainingTime());
                singletonChannelListener.ReceiveRequest(requestContext, serverSingletonPreambleReader.ConnectionDequeuedCallback, true);

                return true;
            }

            void AbortConnection(Exception exception)
            {
                //this will trace the exception and abort the connection
                this.serverSingletonPreambleReader.Abort(exception);
            }
        }

        class ReuseConnectionState
        {
            ConnectionModeReader modeReader;
            TimeSpan closeTimeout;

            public ReuseConnectionState(ConnectionModeReader modeReader, TimeSpan closeTimeout)
            {
                this.modeReader = modeReader;
                this.closeTimeout = closeTimeout;
            }

            public ConnectionModeReader ModeReader
            {
                get { return this.modeReader; }
            }

            public TimeSpan CloseTimeout
            {
                get { return this.closeTimeout; }
            }
        }
    }
}
