//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    delegate void BinderExceptionHandler(IReliableChannelBinder sender, Exception exception);

    interface IReliableChannelBinder
    {
        bool CanSendAsynchronously { get; }
        IChannel Channel { get; }
        bool Connected { get; }
        TimeSpan DefaultSendTimeout { get; }
        bool HasSession { get; }
        EndpointAddress LocalAddress { get; }
        EndpointAddress RemoteAddress { get; }
        CommunicationState State { get; }

        event BinderExceptionHandler Faulted;
        event BinderExceptionHandler OnException;

        void Abort();

        void Close(TimeSpan timeout);
        void Close(TimeSpan timeout, MaskingMode maskingMode);
        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginClose(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        void EndClose(IAsyncResult result);

        void Open(TimeSpan timeout);
        IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        void EndOpen(IAsyncResult result);

        IAsyncResult BeginSend(Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);
        void Send(Message message, TimeSpan timeout);
        void Send(Message message, TimeSpan timeout, MaskingMode maskingMode);

        bool TryReceive(TimeSpan timeout, out RequestContext requestContext);
        bool TryReceive(TimeSpan timeout, out RequestContext requestContext, MaskingMode maskingMode);
        IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginTryReceive(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        bool EndTryReceive(IAsyncResult result, out RequestContext requestContext);

        ISession GetInnerSession();
        void HandleException(Exception e);
        bool IsHandleable(Exception e);
        void SetMaskingMode(RequestContext context, MaskingMode maskingMode);
        RequestContext WrapRequestContext(RequestContext context);
    }

    interface IClientReliableChannelBinder : IReliableChannelBinder
    {
        Uri Via { get; }
        event EventHandler ConnectionLost;

        bool EnsureChannelForRequest();

        IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginRequest(Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        Message EndRequest(IAsyncResult result);
        Message Request(Message message, TimeSpan timeout);
        Message Request(Message message, TimeSpan timeout, MaskingMode maskingMode);

    }

    interface IServerReliableChannelBinder : IReliableChannelBinder
    {
        bool AddressResponse(Message request, Message response);
        bool UseNewChannel(IChannel channel);

        IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForRequest(IAsyncResult result);
        bool WaitForRequest(TimeSpan timeout);
    }
}
