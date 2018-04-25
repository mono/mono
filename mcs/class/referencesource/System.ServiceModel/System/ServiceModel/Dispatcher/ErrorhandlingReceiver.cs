//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class ErrorHandlingReceiver
    {
        ChannelDispatcher dispatcher;
        IChannelBinder binder;

        internal ErrorHandlingReceiver(IChannelBinder binder, ChannelDispatcher dispatcher)
        {
            this.binder = binder;
            this.dispatcher = dispatcher;
        }

        internal void Close()
        {
            try
            {
                this.binder.Channel.Close();
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
                if (this.binder.HasSession)
                {
                    this.binder.Abort();
                }
            }
        }

        internal bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            try
            {
                return this.binder.TryReceive(timeout, out requestContext);
            }
            catch (CommunicationObjectAbortedException)
            {
                requestContext = null;
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                requestContext = null;
                return true;
            }
            catch (CommunicationException e)
            {
                this.HandleError(e);
                requestContext = null;
                return false;
            }
            catch (TimeoutException e)
            {
                this.HandleError(e);
                requestContext = null;
                return false;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.HandleErrorOrAbort(e);
                requestContext = null;
                return false;
            }
        }

        internal IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.BeginTryReceive(timeout, callback, state);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (CommunicationException e)
            {
                this.HandleError(e);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (TimeoutException e)
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

        internal bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            ErrorHandlingCompletedAsyncResult handlerResult = result as ErrorHandlingCompletedAsyncResult;
            if (handlerResult != null)
            {
                requestContext = null;
                return ErrorHandlingCompletedAsyncResult.End(handlerResult);
            }
            else
            {
                try
                {
                    return this.binder.EndTryReceive(result, out requestContext);
                }
                catch (CommunicationObjectAbortedException)
                {
                    requestContext = null;
                    return true;
                }
                catch (CommunicationObjectFaultedException)
                {
                    requestContext = null;
                    return true;
                }
                catch (CommunicationException e)
                {
                    this.HandleError(e);
                    requestContext = null;
                    return false;
                }
                catch (TimeoutException e)
                {
                    this.HandleError(e);
                    requestContext = null;
                    return false;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.HandleErrorOrAbort(e);
                    requestContext = null;
                    return false;
                }
            }
        }

        internal void WaitForMessage()
        {
            try
            {
                this.binder.WaitForMessage(TimeSpan.MaxValue);
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

        internal IAsyncResult BeginWaitForMessage(AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.BeginWaitForMessage(TimeSpan.MaxValue, callback, state);
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

        internal void EndWaitForMessage(IAsyncResult result)
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
                    this.binder.EndWaitForMessage(result);
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
