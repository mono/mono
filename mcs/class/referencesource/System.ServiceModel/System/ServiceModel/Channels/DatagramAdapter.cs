//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    class DatagramAdapter
    {
        internal delegate T Source<T>();

        internal static IOutputChannel GetOutputChannel(Source<IOutputSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts)
        {
            return new OutputDatagramAdapterChannel(channelSource, timeouts);
        }

        internal static IRequestChannel GetRequestChannel(Source<IRequestSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts)
        {
            return new RequestDatagramAdapterChannel(channelSource, timeouts);
        }

        internal static IChannelListener<IInputChannel> GetInputListener(IChannelListener<IInputSessionChannel> inner,
                                                                         ServiceThrottle throttle,
                                                                         IDefaultCommunicationTimeouts timeouts)
        {
            return new InputDatagramAdapterListener(inner, throttle, timeouts);
        }

        internal static IChannelListener<IReplyChannel> GetReplyListener(IChannelListener<IReplySessionChannel> inner,
                                                                         ServiceThrottle throttle,
                                                                         IDefaultCommunicationTimeouts timeouts)
        {
            return new ReplyDatagramAdapterListener(inner, throttle, timeouts);
        }

        abstract class DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>
            : DelegatingChannelListener<TChannel>,
            ISessionThrottleNotification
            where TChannel : class, IChannel
            where TSessionChannel : class, IChannel
            where ItemType : class
        {
            static AsyncCallback acceptCallbackDelegate = Fx.ThunkCallback(new AsyncCallback(AcceptCallbackStatic));
            static Action<object> channelPumpDelegate = new Action<object>(ChannelPump);

            Action channelPumpAfterExceptionDelegate;
            SessionChannelCollection channels;
            IChannelListener<TSessionChannel> listener;
            ServiceThrottle throttle;
            int usageCount;  // When this goes to zero we Abort all the session channels.
            bool acceptLoopDone;
            IWaiter waiter;

            protected DatagramAdapterListenerBase(IChannelListener<TSessionChannel> listener, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts)
                : base(timeouts, listener)
            {
                if (listener == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listener");
                }

                this.channels = new SessionChannelCollection(this.ThisLock);
                this.listener = listener;
                this.throttle = throttle;
                this.channelPumpAfterExceptionDelegate = new Action(this.ChannelPump);
            }

            internal SessionChannelCollection Channels
            {
                get { return this.channels; }
            }

            new internal object ThisLock
            {
                get { return base.ThisLock; }
            }

            protected abstract IAsyncResult CallBeginReceive(TSessionChannel channel, AsyncCallback callback, object state);
            protected abstract ItemType CallEndReceive(TSessionChannel channel, IAsyncResult result);
            protected abstract void Enqueue(ItemType item, Action callback);
            protected abstract void Enqueue(Exception exception, Action callback);

            static void AcceptCallbackStatic(IAsyncResult result)
            {
                ((DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>)result.AsyncState).AcceptCallback(result);
            }

            void AcceptCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously && this.FinishAccept(result))
                {
                    this.ChannelPump();
                }
            }

            void AcceptLoopDone()
            {
                lock (this.ThisLock)
                {
                    if (this.acceptLoopDone)
                    {
                        Fx.Assert("DatagramAdapter Accept loop is already done");
                    }

                    this.acceptLoopDone = true;

                    if (this.waiter != null)
                    {
                        this.waiter.Signal();
                    }
                }
            }

            static void ChannelPump(object state)
            {
                ((DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>)state).ChannelPump();
            }

            void ChannelPump()
            {
                while (this.listener.State == CommunicationState.Opened)
                {
                    IAsyncResult result = null;
                    Exception exception = null;

                    try
                    {
                        result = this.listener.BeginAcceptChannel(TimeSpan.MaxValue, acceptCallbackDelegate, this);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        exception = e;
                    }

                    if (exception != null)
                    {
                        this.Enqueue(exception, channelPumpAfterExceptionDelegate);
                        break;
                    }
                    else if (!result.CompletedSynchronously || !this.FinishAccept(result))
                    {
                        break;
                    }
                }
            }

            bool FinishAccept(IAsyncResult result)
            {
                TSessionChannel channel = null;
                Exception exception = null;
                try
                {
                    channel = this.listener.EndAcceptChannel(result);
                }
                catch (ObjectDisposedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    exception = e;
                }

                if (exception != null)
                {
                    this.Enqueue(exception, channelPumpAfterExceptionDelegate);
                }
                else if (channel == null)
                {
                    this.AcceptLoopDone();
                }
                else
                {
                    if (this.State == CommunicationState.Opened)
                    {
                        DatagramAdapterReceiver.Pump(this, channel);
                    }
                    else
                    {
                        try
                        {
                            channel.Close();
                        }
                        catch (CommunicationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                        catch (TimeoutException e)
                        {
                            if (TD.CloseTimeoutIsEnabled())
                            {
                                TD.CloseTimeout(e.Message);
                            }
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            exception = e;
                        }

                        if (exception != null)
                        {
                            this.Enqueue(exception, channelPumpAfterExceptionDelegate);
                        }
                    }
                }

                return (channel != null) && this.throttle.AcquireSession(this);
            }

            internal void DecrementUsageCount()
            {
                bool done;

                lock (this.ThisLock)
                {
                    this.usageCount--;
                    done = this.usageCount == 0;
                }

                if (done)
                {
                    this.channels.AbortChannels();
                }
            }

            internal void IncrementUsageCount()
            {
                lock (this.ThisLock)
                {
                    this.usageCount++;
                }
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                base.OnOpen(timeout);
                ActionItem.Schedule(channelPumpDelegate, this);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                base.OnEndOpen(result);
                ActionItem.Schedule(channelPumpDelegate, this);
            }

            public void ThrottleAcquired()
            {
                ActionItem.Schedule(DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.channelPumpDelegate, this);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnClose(timeoutHelper.RemainingTime());
                this.WaitForAcceptLoop(timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state,
                                              base.OnBeginClose, base.OnEndClose,
                                              this.BeginWaitForAcceptLoop, this.EndWaitForAcceptLoop);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            void WaitForAcceptLoop(TimeSpan timeout)
            {
                SyncWaiter waiter = null;

                lock (this.ThisLock)
                {
                    if (!this.acceptLoopDone)
                    {
                        waiter = new SyncWaiter(this);
                        this.waiter = waiter;
                    }
                }

                if (waiter != null)
                {
                    waiter.Wait(timeout);
                }
            }

            IAsyncResult BeginWaitForAcceptLoop(TimeSpan timeout, AsyncCallback callback, object state)
            {
                AsyncWaiter waiter = null;

                lock (this.ThisLock)
                {
                    if (!this.acceptLoopDone)
                    {
                        waiter = new AsyncWaiter(timeout, callback, state);
                        this.waiter = waiter;
                    }
                }

                if (waiter != null)
                {
                    return waiter;
                }
                else
                {
                    return new CompletedAsyncResult(callback, state);
                }
            }

            void EndWaitForAcceptLoop(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    AsyncWaiter.End(result);
                }
            }

            class DatagramAdapterReceiver
            {
                static AsyncCallback receiveCallbackDelegate = Fx.ThunkCallback(new AsyncCallback(ReceiveCallbackStatic));
                static Action<object> startNextReceiveDelegate = new Action<object>(StartNextReceive);
                static EventHandler faultedDelegate;

                DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> parent;
                TSessionChannel channel;
                Action itemDequeuedDelegate;
                ServiceModelActivity activity;

                DatagramAdapterReceiver(DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> parent,
                                        TSessionChannel channel)
                {
                    this.parent = parent;
                    this.channel = channel;

                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        activity = ServiceModelActivity.Current;
                    }

                    if (DatagramAdapterReceiver.faultedDelegate == null)
                    {
                        DatagramAdapterReceiver.faultedDelegate = new EventHandler(FaultedCallback);
                    }
                    this.channel.Faulted += DatagramAdapterReceiver.faultedDelegate;
                    this.channel.Closed += new EventHandler(this.ClosedCallback);
                    this.itemDequeuedDelegate = this.StartNextReceive;

                    this.parent.channels.Add(channel);

                    try
                    {
                        channel.Open();
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.OpenTimeoutIsEnabled())
                        {
                            TD.OpenTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.FailedToOpenIncomingChannel,
                                SR.GetString(SR.TraceCodeFailedToOpenIncomingChannel));
                        }
                        channel.Abort();
                        this.parent.Enqueue(e, null);
                    }
                }

                void ClosedCallback(object sender, EventArgs e)
                {
                    TSessionChannel channel = (TSessionChannel)sender;
                    this.parent.channels.Remove(channel);
                    this.parent.throttle.DeactivateChannel();
                }

                static void FaultedCallback(object sender, EventArgs e)
                {
                    ((IChannel)sender).Abort();
                }

                static void StartNextReceive(object state)
                {
                    ((DatagramAdapterReceiver)state).StartNextReceive();
                }

                void StartNextReceive()
                {
                    if (this.channel.State == CommunicationState.Opened)
                    {
                        using (ServiceModelActivity.BoundOperation(this.activity))
                        {
                            IAsyncResult result = null;
                            Exception exception = null;
                            try
                            {
                                result = this.parent.CallBeginReceive(this.channel, receiveCallbackDelegate, this);
                            }
                            catch (ObjectDisposedException e)
                            {
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            }
                            catch (CommunicationException e)
                            {
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            }
                            catch (Exception e)
                            {
                                if (Fx.IsFatal(e))
                                {
                                    throw;
                                }
                                exception = e;
                            }

                            if (exception != null)
                            {
                                this.parent.Enqueue(exception, this.itemDequeuedDelegate);
                            }
                            else if (result.CompletedSynchronously)
                            {
                                this.FinishReceive(result);
                            }
                        }
                    }
                }

                internal static void Pump(DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> listener,
                                          TSessionChannel channel)
                {
                    DatagramAdapterReceiver receiver = new DatagramAdapterReceiver(listener, channel);
                    ActionItem.Schedule(startNextReceiveDelegate, receiver);
                }

                static void ReceiveCallbackStatic(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ((DatagramAdapterReceiver)result.AsyncState).FinishReceive(result);
                    }
                }

                void FinishReceive(IAsyncResult result)
                {
                    ItemType item = null;
                    Exception exception = null;
                    try
                    {
                        item = this.parent.CallEndReceive(this.channel, result);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        exception = e;
                    }

                    if (exception != null)
                    {
                        this.parent.Enqueue(exception, this.itemDequeuedDelegate);
                    }
                    else if (item != null)
                    {
                        this.parent.Enqueue(item, this.itemDequeuedDelegate);
                    }
                    else
                    {
                        try
                        {
                            this.channel.Close();
                        }
                        catch (CommunicationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                        catch (TimeoutException e)
                        {
                            if (TD.CloseTimeoutIsEnabled())
                            {
                                TD.CloseTimeout(e.Message);
                            }
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            exception = e;
                        }

                        if (exception != null)
                        {
                            this.parent.Enqueue(exception, this.itemDequeuedDelegate);
                        }
                    }
                }
            }

            internal class SessionChannelCollection : SynchronizedCollection<TSessionChannel>
            {
                EventHandler onChannelClosed;
                EventHandler onChannelFaulted;

                internal SessionChannelCollection(object syncRoot)
                    : base(syncRoot)
                {
                    this.onChannelClosed = new EventHandler(OnChannelClosed);
                    this.onChannelFaulted = new EventHandler(OnChannelFaulted);
                }

                public void AbortChannels()
                {
                    lock (this.SyncRoot)
                    {
                        for (int i = this.Count - 1; i >= 0; i--)
                        {
                            this[i].Abort();
                        }
                    }
                }

                void AddingChannel(TSessionChannel channel)
                {
                    channel.Faulted += this.onChannelFaulted;
                    channel.Closed += this.onChannelClosed;
                }

                void RemovingChannel(TSessionChannel channel)
                {
                    channel.Faulted -= this.onChannelFaulted;
                    channel.Closed -= this.onChannelClosed;

                    channel.Abort();
                }

                void OnChannelClosed(object sender, EventArgs args)
                {
                    TSessionChannel channel = (TSessionChannel)sender;
                    this.Remove(channel);
                }

                void OnChannelFaulted(object sender, EventArgs args)
                {
                    TSessionChannel channel = (TSessionChannel)sender;
                    this.Remove(channel);
                }

                protected override void ClearItems()
                {
                    List<TSessionChannel> items = this.Items;

                    for (int i = 0; i < items.Count; i++)
                    {
                        this.RemovingChannel(items[i]);
                    }

                    base.ClearItems();
                }

                protected override void InsertItem(int index, TSessionChannel item)
                {
                    this.AddingChannel(item);
                    base.InsertItem(index, item);
                }

                protected override void RemoveItem(int index)
                {
                    TSessionChannel oldItem = this.Items[index];

                    base.RemoveItem(index);
                    this.RemovingChannel(oldItem);
                }

                protected override void SetItem(int index, TSessionChannel item)
                {
                    TSessionChannel oldItem = this.Items[index];

                    this.AddingChannel(item);
                    base.SetItem(index, item);
                    this.RemovingChannel(oldItem);
                }
            }

            internal interface IWaiter
            {
                void Signal();
            }

            internal class AsyncWaiter : AsyncResult, IWaiter
            {
                static Action<object> timerCallback = new Action<object>(AsyncWaiter.TimerCallback);

                bool timedOut;
                readonly IOThreadTimer timer;

                internal AsyncWaiter(TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    if (timeout != TimeSpan.MaxValue)
                    {
                        this.timer = new IOThreadTimer(timerCallback, this, false);
                        this.timer.Set(timeout);
                    }
                }

                internal static bool End(IAsyncResult result)
                {
                    AsyncResult.End<AsyncWaiter>(result);
                    return !((AsyncWaiter)result).timedOut;
                }

                public void Signal()
                {
                    if ((this.timer == null) || this.timer.Cancel())
                    {
                        this.Complete(false);
                    }
                }

                static void TimerCallback(object state)
                {
                    AsyncWaiter waiter = (AsyncWaiter)state;
                    waiter.timedOut = true;
                    waiter.Complete(false);
                }
            }

            internal class SyncWaiter : IWaiter
            {
                bool didSignal;
                object thisLock;
                ManualResetEvent wait;

                internal SyncWaiter(object thisLock)
                {
                    this.thisLock = thisLock;
                }

                object ThisLock
                {
                    get { return this.thisLock; }
                }

                public void Signal()
                {
                    lock (this.ThisLock)
                    {
                        this.didSignal = true;

                        if (this.wait != null)
                        {
                            this.wait.Set();
                        }
                    }
                }

                public bool Wait(TimeSpan timeout)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.didSignal)
                        {
                            this.wait = new ManualResetEvent(false);
                        }
                    }

                    if ((this.wait == null) || TimeoutHelper.WaitOne(this.wait, timeout))
                    {
                        if (this.wait != null)
                        {
                            this.wait.Close();
                            this.wait = null;
                        }
                        return true;
                    }
                    else
                    {
                        lock (this.ThisLock)
                        {
                            this.wait.Close();
                            this.wait = null;
                        }
                        return false;
                    }
                }
            }
        }

        class InputDatagramAdapterListener : DatagramAdapterListenerBase<IInputChannel, IInputSessionChannel, Message>
        {
            SingletonChannelAcceptor<IInputChannel, InputChannel, Message> acceptor;

            internal InputDatagramAdapterListener(IChannelListener<IInputSessionChannel> listener,
                                                  ServiceThrottle throttle,
                                                  IDefaultCommunicationTimeouts timeouts)
                : base(listener, throttle, timeouts)
            {
                this.acceptor = new InputDatagramAdapterAcceptor(this);
                this.Acceptor = this.acceptor;
            }

            protected override IAsyncResult CallBeginReceive(IInputSessionChannel channel,
                                                             AsyncCallback callback, object state)
            {
                return channel.BeginReceive(TimeSpan.MaxValue, callback, state);
            }

            protected override Message CallEndReceive(IInputSessionChannel channel, IAsyncResult result)
            {
                return channel.EndReceive(result);
            }

            protected override void Enqueue(Message message, Action callback)
            {
                this.acceptor.Enqueue(message, callback);
            }

            protected override void Enqueue(Exception exception, Action callback)
            {
                this.acceptor.Enqueue(exception, callback);
            }
        }

        class InputDatagramAdapterAcceptor : InputChannelAcceptor
        {
            internal InputDatagramAdapterListener listener;

            internal InputDatagramAdapterAcceptor(InputDatagramAdapterListener listener)
                : base(listener)
            {
                this.listener = listener;
            }

            protected override InputChannel OnCreateChannel()
            {
                return new InputDatagramAdapterChannel(this.listener);
            }
        }

        class InputDatagramAdapterChannel : InputChannel
        {
            InputDatagramAdapterListener listener;

            internal InputDatagramAdapterChannel(InputDatagramAdapterListener listener)
                : base(listener, null)
            {
                this.listener = listener;
            }

            public override T GetProperty<T>()
            {
                lock (this.listener.ThisLock)
                {
                    if (this.listener.Channels.Count > 0)
                    {
                        return this.listener.Channels[0].GetProperty<T>();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            protected override void OnOpening()
            {
                this.listener.IncrementUsageCount();
                base.OnOpening();
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.listener.DecrementUsageCount();
            }
        }

        class ReplyDatagramAdapterListener : DatagramAdapterListenerBase<IReplyChannel, IReplySessionChannel, RequestContext>
        {
            SingletonChannelAcceptor<IReplyChannel, ReplyChannel, RequestContext> acceptor;

            internal ReplyDatagramAdapterListener(IChannelListener<IReplySessionChannel> listener,
                                                  ServiceThrottle throttle,
                                                  IDefaultCommunicationTimeouts timeouts)
                : base(listener, throttle, timeouts)
            {
                this.acceptor = new ReplyDatagramAdapterAcceptor(this);
                this.Acceptor = this.acceptor;
            }

            protected override IAsyncResult CallBeginReceive(IReplySessionChannel channel,
                                                             AsyncCallback callback, object state)
            {
                return channel.BeginReceiveRequest(TimeSpan.MaxValue, callback, state);
            }

            protected override RequestContext CallEndReceive(IReplySessionChannel channel, IAsyncResult result)
            {
                return channel.EndReceiveRequest(result);
            }

            protected override void Enqueue(RequestContext request, Action callback)
            {
                this.acceptor.Enqueue(request, callback);
            }

            protected override void Enqueue(Exception exception, Action callback)
            {
                this.acceptor.Enqueue(exception, callback);
            }
        }

        class ReplyDatagramAdapterAcceptor : ReplyChannelAcceptor
        {
            internal ReplyDatagramAdapterListener listener;

            internal ReplyDatagramAdapterAcceptor(ReplyDatagramAdapterListener listener)
                : base(listener)
            {
                this.listener = listener;
            }

            protected override ReplyChannel OnCreateChannel()
            {
                return new ReplyDatagramAdapterChannel(this.listener);
            }
        }

        class ReplyDatagramAdapterChannel : ReplyChannel
        {
            ReplyDatagramAdapterListener listener;

            internal ReplyDatagramAdapterChannel(ReplyDatagramAdapterListener listener)
                : base(listener, null)
            {
                this.listener = listener;
            }

            public override T GetProperty<T>()
            {
                lock (this.listener.ThisLock)
                {
                    if (this.listener.Channels.Count > 0)
                    {
                        return this.listener.Channels[0].GetProperty<T>();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            protected override void OnOpening()
            {
                this.listener.IncrementUsageCount();
                base.OnOpening();
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.listener.DecrementUsageCount();
            }
        }

        abstract class DatagramAdapterChannelBase<TSessionChannel> : CommunicationObject, IChannel
            where TSessionChannel : class, IChannel
        {
            ChannelParameterCollection channelParameters;
            Source<TSessionChannel> channelSource;
            TSessionChannel channel;
            TimeSpan defaultCloseTimeout;
            TimeSpan defaultOpenTimeout;
            TimeSpan defaultSendTimeout;
            List<TSessionChannel> activeChannels;

            protected DatagramAdapterChannelBase(Source<TSessionChannel> channelSource,
                                                 IDefaultCommunicationTimeouts timeouts)
            {
                if (channelSource == null)
                {
                    Fx.Assert("DatagramAdapterChannelBase.ctor: (channelSource == null)");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelSource");
                }
                this.channelParameters = new ChannelParameterCollection(this);
                this.channelSource = channelSource;
                this.defaultCloseTimeout = timeouts.CloseTimeout;
                this.defaultOpenTimeout = timeouts.OpenTimeout;
                this.defaultSendTimeout = timeouts.SendTimeout;
                this.activeChannels = new List<TSessionChannel>();
            }

            protected ChannelParameterCollection ChannelParameters
            {
                get { return this.channelParameters; }
            }

            protected override TimeSpan DefaultCloseTimeout
            {
                get { return this.defaultCloseTimeout; }
            }

            protected override TimeSpan DefaultOpenTimeout
            {
                get { return this.defaultOpenTimeout; }
            }

            protected TimeSpan DefaultSendTimeout
            {
                get { return this.defaultSendTimeout; }
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected TSessionChannel TakeChannel()
            {
                TSessionChannel channel;

                lock (this.ThisLock)
                {
                    this.ThrowIfDisposedOrNotOpen();

                    if (this.channel == null)
                    {
                        channel = this.channelSource();
                    }
                    else
                    {
                        channel = this.channel;
                        this.channel = null;
                    }

                    this.activeChannels.Add(channel);
                }

                return channel;
            }

            protected bool ReturnChannel(TSessionChannel channel)
            {
                lock (this.ThisLock)
                {
                    if (this.channel == null)
                    {
                        this.activeChannels.Remove(channel);
                        this.channel = channel;
                        return true;
                    }
                }

                return false;
            }

            protected void RemoveChannel(TSessionChannel channel)
            {
                lock (this.ThisLock)
                {
                    this.activeChannels.Remove(channel);
                }
            }

            public T GetProperty<T>() where T : class
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    return (T)(object)this.channelParameters;
                }

                TSessionChannel inner = channelSource();
                inner.Abort();
                return inner.GetProperty<T>();
            }

            protected override void OnAbort()
            {
                TSessionChannel channel;
                TSessionChannel[] activeChannels;

                lock (this.ThisLock)
                {
                    channel = this.channel;
                    activeChannels = new TSessionChannel[this.activeChannels.Count];
                    this.activeChannels.CopyTo(activeChannels);
                }

                if (channel != null)
                    channel.Abort();

                foreach (TSessionChannel currentChannel in activeChannels)
                    currentChannel.Abort();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TSessionChannel channel;
                TSessionChannel[] activeChannels;

                lock (this.ThisLock)
                {
                    channel = this.channel;
                    activeChannels = new TSessionChannel[this.activeChannels.Count];
                    this.activeChannels.CopyTo(activeChannels);
                }

                TimeoutHelper helper = new TimeoutHelper(timeout);

                if (channel != null)
                    channel.Close(helper.RemainingTime());

                foreach (TSessionChannel currentChannel in activeChannels)
                    currentChannel.Close(helper.RemainingTime());
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TSessionChannel channel;
                TSessionChannel[] activeChannels;

                lock (this.ThisLock)
                {
                    channel = this.channel;
                    activeChannels = new TSessionChannel[this.activeChannels.Count];
                    this.activeChannels.CopyTo(activeChannels);
                }

                if (this.channel == null)
                    return new CloseCollectionAsyncResult(timeout, callback, state, activeChannels);
                else
                    return new ChainedCloseAsyncResult(timeout, callback, state, channel.BeginClose, channel.EndClose, activeChannels);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                if (result is CloseCollectionAsyncResult)
                    CloseCollectionAsyncResult.End(result);
                else
                    ChainedCloseAsyncResult.End(result);
            }
        }

        class OutputDatagramAdapterChannel : DatagramAdapterChannelBase<IOutputSessionChannel>, IOutputChannel
        {
            EndpointAddress remoteAddress;
            Uri via;

            internal OutputDatagramAdapterChannel(Source<IOutputSessionChannel> channelSource,
                                                   IDefaultCommunicationTimeouts timeouts)
                : base(channelSource, timeouts)
            {
                IOutputSessionChannel inner = channelSource();
                try
                {
                    if (inner == null)
                    {
                        Fx.Assert("OutputDatagramAdapterChannel.ctor: (inner == null)");
                    }
                    this.remoteAddress = inner.RemoteAddress;
                    this.via = inner.Via;
                    inner.Close();
                }
                finally
                {
                    inner.Abort();
                }
            }

            public EndpointAddress RemoteAddress
            {
                get { return this.remoteAddress; }
            }

            public Uri Via
            {
                get { return this.via; }
            }

            public void Send(Message message)
            {
                this.Send(message, this.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                IOutputSessionChannel channel = this.TakeChannel();
                bool throwing = true;

                try
                {
                    if (channel.State == CommunicationState.Created)
                    {
                        this.ChannelParameters.PropagateChannelParameters(channel);
                        channel.Open(helper.RemainingTime());
                    }

                    channel.Send(message, helper.RemainingTime());
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        channel.Abort();
                        this.RemoveChannel(channel);
                    }
                }

                if (this.ReturnChannel(channel))
                    return;

                try
                {
                    channel.Close(helper.RemainingTime());
                }
                finally
                {
                    this.RemoveChannel(channel);
                }
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendAsyncResult(this, message, timeout, callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                SendAsyncResult.End(result);
            }

            class SendAsyncResult : AsyncResult
            {
                OutputDatagramAdapterChannel adapter;
                Message message;
                TimeoutHelper timeoutHelper;
                bool hasCompletedAsynchronously = true;

                public SendAsyncResult(OutputDatagramAdapterChannel adapter, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.adapter = adapter;
                    this.message = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    IOutputSessionChannel channel = this.adapter.TakeChannel();

                    try
                    {
                        if (channel.State == CommunicationState.Created)
                        {
                            this.adapter.ChannelParameters.PropagateChannelParameters(channel);
                            channel.BeginOpen(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnOpenComplete)), channel);
                        }
                        else
                        {
                            channel.BeginSend(message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnSendComplete)), channel);
                        }
                    }
                    catch
                    {
                        channel.Abort();
                        this.adapter.RemoveChannel(channel);
                        throw;
                    }
                }

                void OnOpenComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IOutputSessionChannel channel = (IOutputSessionChannel)result.AsyncState;

                    try
                    {
                        channel.EndOpen(result);
                        channel.BeginSend(this.message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnSendComplete)), channel);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        channel.Abort();
                        this.adapter.RemoveChannel(channel);
                        this.Complete(this.hasCompletedAsynchronously, exception);
                    }
                }

                void OnSendComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IOutputSessionChannel channel = (IOutputSessionChannel)result.AsyncState;

                    try
                    {
                        channel.EndSend(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        channel.Abort();
                        this.adapter.RemoveChannel(channel);
                        this.Complete(this.hasCompletedAsynchronously, exception);
                        return;
                    }

                    if (this.adapter.ReturnChannel(channel))
                    {
                        this.Complete(this.hasCompletedAsynchronously);
                        return;
                    }

                    try
                    {
                        channel.BeginClose(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnCloseComplete)), channel);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        this.adapter.RemoveChannel(channel);
                        this.Complete(this.hasCompletedAsynchronously, exception);
                    }
                }

                void OnCloseComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IOutputSessionChannel channel = (IOutputSessionChannel)result.AsyncState;

                    Exception exception = null;

                    try
                    {
                        channel.EndClose(result);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        exception = e;
                    }

                    this.adapter.RemoveChannel(channel);
                    this.Complete(this.hasCompletedAsynchronously, exception);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SendAsyncResult>(result);
                }
            }
        }

        class RequestDatagramAdapterChannel : DatagramAdapterChannelBase<IRequestSessionChannel>, IRequestChannel
        {
            EndpointAddress remoteAddress;
            Uri via;

            internal RequestDatagramAdapterChannel(Source<IRequestSessionChannel> channelSource,
                                                   IDefaultCommunicationTimeouts timeouts)
                : base(channelSource, timeouts)
            {
                IRequestSessionChannel inner = channelSource();
                try
                {
                    if (inner == null)
                    {
                        Fx.Assert("RequestDatagramAdapterChannel.ctor: (inner == null)");
                    }
                    this.remoteAddress = inner.RemoteAddress;
                    this.via = inner.Via;
                    inner.Close();
                }
                finally
                {
                    inner.Abort();
                }
            }

            public EndpointAddress RemoteAddress
            {
                get { return this.remoteAddress; }
            }

            public Uri Via
            {
                get { return this.via; }
            }

            public Message Request(Message request)
            {
                return this.Request(request, this.DefaultSendTimeout);
            }

            public Message Request(Message request, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                IRequestSessionChannel channel = this.TakeChannel();
                bool throwing = true;
                Message reply = null;

                try
                {
                    if (channel.State == CommunicationState.Created)
                    {
                        this.ChannelParameters.PropagateChannelParameters(channel);
                        channel.Open(helper.RemainingTime());
                    }

                    reply = channel.Request(request, helper.RemainingTime());
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        channel.Abort();
                        this.RemoveChannel(channel);
                    }
                }

                if (this.ReturnChannel(channel))
                    return reply;

                try
                {
                    channel.Close(helper.RemainingTime());
                }
                finally
                {
                    this.RemoveChannel(channel);
                }

                return reply;
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new RequestAsyncResult(this, message, timeout, callback, state);
            }

            public Message EndRequest(IAsyncResult result)
            {
                return RequestAsyncResult.End(result);
            }

            class RequestAsyncResult : AsyncResult
            {
                RequestDatagramAdapterChannel adapter;
                Message message;
                Message reply = null;
                TimeoutHelper timeoutHelper;
                bool hasCompletedAsynchronously = true;

                public RequestAsyncResult(RequestDatagramAdapterChannel adapter, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.adapter = adapter;
                    this.message = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    IRequestSessionChannel channel = this.adapter.TakeChannel();

                    try
                    {
                        if (channel.State == CommunicationState.Created)
                        {
                            this.adapter.ChannelParameters.PropagateChannelParameters(channel);
                            channel.BeginOpen(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnOpenComplete)), channel);
                        }
                        else
                        {
                            channel.BeginRequest(message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnRequestComplete)), channel);
                        }
                    }
                    catch
                    {
                        channel.Abort();
                        this.adapter.RemoveChannel(channel);
                        throw;
                    }
                }

                void OnOpenComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IRequestSessionChannel channel = (IRequestSessionChannel)result.AsyncState;

                    try
                    {
                        channel.EndOpen(result);
                        channel.BeginRequest(this.message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnRequestComplete)), channel);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        channel.Abort();
                        this.adapter.RemoveChannel(channel);
                        this.Complete(this.hasCompletedAsynchronously, exception);
                    }
                }

                void OnRequestComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IRequestSessionChannel channel = (IRequestSessionChannel)result.AsyncState;

                    try
                    {
                        this.reply = channel.EndRequest(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        channel.Abort();
                        this.adapter.RemoveChannel(channel);
                        this.Complete(this.hasCompletedAsynchronously, exception);
                        return;
                    }

                    if (this.adapter.ReturnChannel(channel))
                    {
                        this.Complete(this.hasCompletedAsynchronously);
                        return;
                    }

                    try
                    {
                        channel.BeginClose(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(OnCloseComplete)), channel);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        this.adapter.RemoveChannel(channel);
                        this.Complete(this.hasCompletedAsynchronously, exception);
                    }
                }

                void OnCloseComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IRequestSessionChannel channel = (IRequestSessionChannel)result.AsyncState;

                    Exception exception = null;

                    try
                    {
                        channel.EndClose(result);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        exception = e;
                    }

                    this.adapter.RemoveChannel(channel);
                    this.Complete(this.hasCompletedAsynchronously, exception);
                }

                public static Message End(IAsyncResult result)
                {
                    RequestAsyncResult requestResult = AsyncResult.End<RequestAsyncResult>(result);
                    return requestResult.reply;
                }
            }
        }
    }
}
