//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    delegate void ConnectionAvailableCallback(IConnection connection, Action connectionDequeuedCallback);
    delegate void ErrorCallback(Exception exception);

    class ConnectionAcceptor : IDisposable
    {
        int maxAccepts;
        int maxPendingConnections;
        int connections;
        int pendingAccepts;
        IConnectionListener listener;
        AsyncCallback acceptCompletedCallback;
        Action<object> scheduleAcceptCallback;
        Action onConnectionDequeued;
        bool isDisposed;
        ConnectionAvailableCallback callback;
        ErrorCallback errorCallback;

        public ConnectionAcceptor(IConnectionListener listener, int maxAccepts, int maxPendingConnections,
            ConnectionAvailableCallback callback)
            : this(listener, maxAccepts, maxPendingConnections, callback, null)
        {
            // empty
        }

        public ConnectionAcceptor(IConnectionListener listener, int maxAccepts, int maxPendingConnections,
            ConnectionAvailableCallback callback, ErrorCallback errorCallback)
        {
            if (maxAccepts <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxAccepts", maxAccepts,
                    SR.GetString(SR.ValueMustBePositive)));
            }

            Fx.Assert(maxPendingConnections > 0, "maxPendingConnections must be positive");

            this.listener = listener;
            this.maxAccepts = maxAccepts;
            this.maxPendingConnections = maxPendingConnections;
            this.callback = callback;
            this.errorCallback = errorCallback;
            this.onConnectionDequeued = new Action(OnConnectionDequeued);
            this.acceptCompletedCallback = Fx.ThunkCallback(new AsyncCallback(AcceptCompletedCallback));
            this.scheduleAcceptCallback = new Action<object>(ScheduleAcceptCallback);
        }

        bool IsAcceptNecessary
        {
            get
            {
                return (pendingAccepts < maxAccepts)
                    && ((connections + pendingAccepts) < maxPendingConnections)
                    && !isDisposed;
            }
        }

        public int ConnectionCount
        {
            get { return connections; }
        }

        object ThisLock
        {
            get { return this; }
        }

        void AcceptIfNecessary(bool startAccepting)
        {
            if (IsAcceptNecessary)
            {
                lock (ThisLock)
                {
                    while (IsAcceptNecessary)
                    {
                        IAsyncResult result = null;
                        Exception unexpectedException = null;
                        try
                        {
                            result = listener.BeginAccept(acceptCompletedCallback, null);
                        }
                        catch (CommunicationException exception)
                        {
                            DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if (startAccepting)
                            {
                                // Since we're under a call to StartAccepting(), just throw the exception up the stack.
                                throw;
                            }
                            if ((errorCallback == null) && !ExceptionHandler.HandleTransportExceptionHelper(exception))
                            {
                                throw;
                            }
                            unexpectedException = exception;
                        }

                        if ((unexpectedException != null) && (errorCallback != null))
                        {
                            errorCallback(unexpectedException);
                        }

                        if (result != null)
                        {
                            // don't block our accept processing loop
                            if (result.CompletedSynchronously)
                            {
                                ActionItem.Schedule(scheduleAcceptCallback, result);
                            }

                            pendingAccepts++;
                        }
                    }
                }
            }
        }

        void AcceptCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            HandleCompletedAccept(result);
        }

        public void Dispose()
        {
            lock (ThisLock)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    listener.Dispose();
                }
            }
        }

        void HandleCompletedAccept(IAsyncResult result)
        {
            IConnection connection = null;

            lock (ThisLock)
            {
                bool success = false;
                Exception unexpectedException = null;
                try
                {
                    if (!isDisposed)
                    {
                        connection = listener.EndAccept(result);
                        if (connection != null)
                        {
                            if (connections + 1 >= maxPendingConnections)
                            {
                                if (TD.MaxPendingConnectionsExceededIsEnabled())
                                {
                                    TD.MaxPendingConnectionsExceeded(SR.GetString(SR.TraceCodeMaxPendingConnectionsReached));
                                }
                                if (DiagnosticUtility.ShouldTraceWarning)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Warning,
                                        TraceCode.MaxPendingConnectionsReached, SR.GetString(SR.TraceCodeMaxPendingConnectionsReached),
                                        new StringTraceRecord("MaxPendingConnections", maxPendingConnections.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                                        this,
                                        null);
                                }
                            }
                            else if (TD.PendingConnectionsRatioIsEnabled())
                            {
                                TD.PendingConnectionsRatio(connections + 1, maxPendingConnections);
                            }

                            // This is incremented after the Trace just in case the Trace throws.
                            connections++;
                        }
                    }
                    success = true;
                }
                catch (CommunicationException exception)
                {
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if ((errorCallback == null) && !ExceptionHandler.HandleTransportExceptionHelper(exception))
                    {
                        throw;
                    }
                    unexpectedException = exception;
                }
                finally
                {
                    if (!success)
                    {
                        connection = null;
                    }
                    pendingAccepts--;
                    if (pendingAccepts == 0 && TD.PendingAcceptsAtZeroIsEnabled())
                    {
                        TD.PendingAcceptsAtZero();
                    }
                }

                if ((unexpectedException != null) && (errorCallback != null))
                {
                    errorCallback(unexpectedException);
                }
            }

            AcceptIfNecessary(false);

            if (connection != null)
            {
                callback(connection, onConnectionDequeued);
            }
        }

        void OnConnectionDequeued()
        {
            lock (ThisLock)
            {
                connections--;
                if (TD.PendingConnectionsRatioIsEnabled())
                {
                    TD.PendingConnectionsRatio(connections, maxPendingConnections);
                }
            }
            AcceptIfNecessary(false);
        }

        void ScheduleAcceptCallback(object state)
        {
            HandleCompletedAccept((IAsyncResult)state);
        }

        public void StartAccepting()
        {
            listener.Listen();
            AcceptIfNecessary(true);
        }
    }
}
