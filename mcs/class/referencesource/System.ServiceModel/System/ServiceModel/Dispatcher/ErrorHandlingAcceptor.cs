//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    class ErrorHandlingAcceptor
    {
        readonly ChannelDispatcher dispatcher;
        readonly IListenerBinder binder;

        internal ErrorHandlingAcceptor(IListenerBinder binder, ChannelDispatcher dispatcher)
        {
            if (binder == null)
            {
                Fx.Assert("binder is null");
            }
            if (dispatcher == null)
            {
                Fx.Assert("dispatcher is null");
            }

            this.binder = binder;
            this.dispatcher = dispatcher;
        }

        internal void Close()
        {
            try
            {
                this.binder.Listener.Close();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleError(e);
            }
        }

        void HandleError(Exception e)
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.HandleError(e);
            }
        }

        void HandleErrorOrAbort(Exception e)
        {
            if ((this.dispatcher == null) || !this.dispatcher.HandleError(e))
            {
                // We only stop if the listener faults.  It is a bug
                // if the listener is in an invalid state and does not
                // fault.  So there are no cases today where this aborts.
            }
        }

        internal bool TryAccept(TimeSpan timeout, out IChannelBinder channelBinder)
        {
            try
            {
                channelBinder = this.binder.Accept(timeout);
                if (channelBinder != null)
                {
                    this.dispatcher.PendingChannels.Add(channelBinder.Channel);
                }
                return true;
            }
            catch (CommunicationObjectAbortedException)
            {
                channelBinder = null;
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                channelBinder = null;
                return true;
            }
            catch (TimeoutException)
            {
                channelBinder = null;
                return false;
            }
            catch (CommunicationException e)
            {
                this.HandleError(e);
                channelBinder = null;
                return false;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleErrorOrAbort(e);
                channelBinder = null;
                return false;
            }
        }

        internal IAsyncResult BeginTryAccept(TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.BeginAccept(timeout, callback, state);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (TimeoutException)
            {
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (CommunicationException e)
            {
                this.HandleError(e);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleErrorOrAbort(e);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
        }

        internal bool EndTryAccept(IAsyncResult result, out IChannelBinder channelBinder)
        {
            ErrorHandlingCompletedAsyncResult handlerResult = result as ErrorHandlingCompletedAsyncResult;
            if (handlerResult != null)
            {
                channelBinder = null;
                return ErrorHandlingCompletedAsyncResult.End(handlerResult);
            }
            else
            {
                try
                {
                    channelBinder = this.binder.EndAccept(result);
                    if (channelBinder != null)
                    {
                        this.dispatcher.PendingChannels.Add(channelBinder.Channel);
                    }
                    return true;
                }
                catch (CommunicationObjectAbortedException)
                {
                    channelBinder = null;
                    return true;
                }
                catch (CommunicationObjectFaultedException)
                {
                    channelBinder = null;
                    return true;
                }
                catch (TimeoutException)
                {
                    channelBinder = null;
                    return false;
                }
                catch (CommunicationException e)
                {
                    this.HandleError(e);
                    channelBinder = null;
                    return false;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.HandleErrorOrAbort(e);
                    channelBinder = null;
                    return false;
                }
            }
        }

        internal void WaitForChannel()
        {
            try
            {
                this.binder.Listener.WaitForChannel(TimeSpan.MaxValue);
            }
            catch (CommunicationObjectAbortedException) { }
            catch (CommunicationObjectFaultedException) { }
            catch (CommunicationException e)
            {
                this.HandleError(e);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleErrorOrAbort(e);
            }
        }

        internal IAsyncResult BeginWaitForChannel(AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.Listener.BeginWaitForChannel(TimeSpan.MaxValue, callback, state);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new WaitCompletedAsyncResult(callback, state);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new WaitCompletedAsyncResult(callback, state);
            }
            catch (CommunicationException e)
            {
                this.HandleError(e);
                return new WaitCompletedAsyncResult(callback, state);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleErrorOrAbort(e);
                return new WaitCompletedAsyncResult(callback, state);
            }
        }

        internal void EndWaitForChannel(IAsyncResult result)
        {
            WaitCompletedAsyncResult handlerResult = result as WaitCompletedAsyncResult;
            if (handlerResult != null)
            {
                WaitCompletedAsyncResult.End(handlerResult);
            }
            else
            {
                try
                {
                    this.binder.Listener.EndWaitForChannel(result);
                }
                catch (CommunicationObjectAbortedException) { }
                catch (CommunicationObjectFaultedException) { }
                catch (CommunicationException e)
                {
                    this.HandleError(e);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.HandleErrorOrAbort(e);
                }
            }
        }

        class ErrorHandlingCompletedAsyncResult : CompletedAsyncResult<bool>
        {
            internal ErrorHandlingCompletedAsyncResult(bool data, AsyncCallback callback, object state)
                : base(data, callback, state)
            {
            }
        }

        class WaitCompletedAsyncResult : CompletedAsyncResult
        {
            internal WaitCompletedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }
        }
    }
}
