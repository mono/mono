//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Threading;
    using System.Xml;

    class AnnouncementDispatcherAsyncResult : AsyncResult
    {
        readonly AnnouncementSendsAsyncResult[] innerResults;

        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int completions;

        AsyncCallback onAnnouncementSendsCompletedCallback;

        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int completesCounter;

        bool cancelled;

        [Fx.Tag.SynchronizationObject()]
        object thisLock;

        public AnnouncementDispatcherAsyncResult(
            Collection<AnnouncementEndpoint> endpoints,
            Collection<EndpointDiscoveryMetadata> metadatas,
            DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator,
            bool online,
            AsyncCallback callback,
            object state
            )
            : base(callback, state)
        {
            if (metadatas.Count == 0)
            {
                Complete(true);
                return;
            }
            bool success = false;
            this.cancelled = false;
            this.thisLock = new object();
            this.innerResults = new AnnouncementSendsAsyncResult[endpoints.Count];
            this.onAnnouncementSendsCompletedCallback = Fx.ThunkCallback(new AsyncCallback(OnAnnouncementSendsCompleted));
            Collection<UniqueId> messageIds = AllocateMessageIds(metadatas.Count);

            try
            {
                Random random = new Random();
                for (int i = 0; i < this.innerResults.Length; i++)
                {
                    AnnouncementClient announcementClient = new AnnouncementClient(endpoints[i]);
                    announcementClient.MessageSequenceGenerator = discoveryMessageSequenceGenerator;
                    this.innerResults[i] = 
                        new AnnouncementSendsAsyncResult(
                        announcementClient, 
                        metadatas, 
                        messageIds, 
                        online, 
                        endpoints[i].MaxAnnouncementDelay, 
                        random, 
                        this.onAnnouncementSendsCompletedCallback, 
                        this);
                }
                success = true;
            }
            finally
            {
                if (!success)
                {
                    this.Cancel();
                }
            }
        }

        public void Start(TimeSpan timeout, bool canCompleteSynchronously)
        {
            if (this.IsCompleted || this.cancelled)
            {
                return;
            }
            bool synchronousCompletion = canCompleteSynchronously;
            Exception error = null;
            bool complete = false;
            try
            {
                for (int i = 0; i < this.innerResults.Length; i++)
                {
                    this.innerResults[i].Start(timeout);
                    if (this.innerResults[i].CompletedSynchronously)
                    {
                        AnnouncementSendsAsyncResult.End(this.innerResults[i]);
                        complete = (Interlocked.Increment(ref this.completions) == innerResults.Length);
                    }
                    else
                    {
                        synchronousCompletion = false;

                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            if (error != null)
            {
                CallCompleteOnce(synchronousCompletion, error);
            }
            else if (complete)
            {
                CallCompleteOnce(synchronousCompletion, null);
            }
        }

        void OnAnnouncementSendsCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                Exception error = null;
                try
                {
                    AnnouncementSendsAsyncResult.End(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    error = e;
                }
                if (error != null)
                {
                    CallCompleteOnce(false, error);
                }
                else
                {
                    if (Interlocked.Increment(ref this.completions) == innerResults.Length)
                    {
                        CallCompleteOnce(false, null);
                    }
                }
            }
        }

        public void Cancel()
        {
            if (!this.cancelled)
            {
                bool doCancel = false;
                lock (this.thisLock)
                {
                    if (!this.cancelled)
                    {
                        doCancel = true;
                        this.cancelled = true;
                    }
                }
                if (doCancel)
                {
                    for (int i = 0; i < this.innerResults.Length; i++)
                    {
                        if (this.innerResults[i] != null)
                        {
                            this.innerResults[i].Cancel();
                        }
                    }
                    CompleteOnCancel();
                }
            }
        }

        void CompleteOnCancel()
        {
            if (Threading.Interlocked.Increment(ref this.completesCounter) == 1)
            {
                Complete(false, new OperationCanceledException());
            }
        }

        void CallCompleteOnce(bool completedSynchronously, Exception e)
        {
            if (Threading.Interlocked.Increment(ref this.completesCounter) == 1)
            {
                if (e != null)
                {
                    Cancel();
                }
                Complete(completedSynchronously, e);
            }
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<AnnouncementDispatcherAsyncResult>(result);
        }

        static Collection<UniqueId> AllocateMessageIds(int count)
        {
            Collection<UniqueId> messageIds = new Collection<UniqueId>();
            for (int i = 0; i < count; i++)
            {
                messageIds.Add(new UniqueId());
            }

            return messageIds;
        }
    }
}
