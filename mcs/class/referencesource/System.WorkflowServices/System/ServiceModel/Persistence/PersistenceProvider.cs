//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Persistence
{
    using System;
    using System.ServiceModel.Channels;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public abstract class PersistenceProvider : CommunicationObject
    {
        internal static readonly TimeSpan DefaultOpenClosePersistenceTimout = TimeSpan.FromSeconds(15);
        Guid id;

        protected PersistenceProvider(Guid id)
        {
            this.id = id;
        }

        public Guid Id
        {
            get
            {
                return this.id;
            }
        }
        public abstract IAsyncResult BeginCreate(object instance, TimeSpan timeout, AsyncCallback callback, object state);

        public abstract IAsyncResult BeginDelete(object instance, TimeSpan timeout, AsyncCallback callback, object state);
        public abstract IAsyncResult BeginLoad(TimeSpan timeout, AsyncCallback callback, object state);

        public virtual IAsyncResult BeginLoadIfChanged(TimeSpan timeout, object instanceToken, AsyncCallback callback, object state)
        {
            return this.BeginLoad(timeout, callback, state);
        }
        public abstract IAsyncResult BeginUpdate(object instance, TimeSpan timeout, AsyncCallback callback, object state);

        public abstract object Create(object instance, TimeSpan timeout);

        public abstract void Delete(object instance, TimeSpan timeout);
        public abstract object EndCreate(IAsyncResult result);
        public abstract void EndDelete(IAsyncResult result);
        public abstract object EndLoad(IAsyncResult result);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021")]
        public virtual bool EndLoadIfChanged(IAsyncResult result, out object instance)
        {
            instance = this.EndLoad(result);
            return true;
        }
        public abstract object EndUpdate(IAsyncResult result);

        public abstract object Load(TimeSpan timeout);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021")]
        public virtual bool LoadIfChanged(TimeSpan timeout, object instanceToken, out object instance)
        {
            instance = this.Load(timeout);
            return true;
        }

        public abstract object Update(object instance, TimeSpan timeout);
    }
}
