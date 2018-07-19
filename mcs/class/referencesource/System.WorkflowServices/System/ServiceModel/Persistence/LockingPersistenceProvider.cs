//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Persistence
{
    using System;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public abstract class LockingPersistenceProvider : PersistenceProvider
    {
        protected LockingPersistenceProvider(Guid id)
            : base(id)
        {
        }

        public override IAsyncResult BeginCreate(object instance, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginCreate(instance, timeout, false, callback, state);
        }

        public abstract IAsyncResult BeginCreate(object instance, TimeSpan timeout, bool unlockInstance, AsyncCallback callback, object state);

        public override IAsyncResult BeginLoad(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginLoad(timeout, false, callback, state);
        }

        public abstract IAsyncResult BeginLoad(TimeSpan timeout, bool lockInstance, AsyncCallback callback, object state);

        public override IAsyncResult BeginLoadIfChanged(TimeSpan timeout, object instanceToken, AsyncCallback callback, object state)
        {
            return this.BeginLoadIfChanged(timeout, instanceToken, false, callback, state);
        }

        public virtual IAsyncResult BeginLoadIfChanged(TimeSpan timeout, object instanceToken, bool lockInstance, AsyncCallback callback, object state)
        {
            return this.BeginLoad(timeout, lockInstance, callback, state);
        }

        public abstract IAsyncResult BeginUnlock(TimeSpan timeout, AsyncCallback callback, object state);

        public override IAsyncResult BeginUpdate(object instance, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginUpdate(instance, timeout, false, callback, state);
        }
        public abstract IAsyncResult BeginUpdate(object instance, TimeSpan timeout, bool unlockInstance, AsyncCallback callback, object state);

        public override object Create(object instance, TimeSpan timeout)
        {
            return this.Create(instance, timeout, false);
        }

        public abstract object Create(object instance, TimeSpan timeout, bool unlockInstance);
        public abstract void EndUnlock(IAsyncResult result);

        public override object Load(TimeSpan timeout)
        {
            return Load(timeout, false);
        }

        public abstract object Load(TimeSpan timeout, bool lockInstance);

        public override bool LoadIfChanged(TimeSpan timeout, object instanceToken, out object instance)
        {
            return this.LoadIfChanged(timeout, instanceToken, false, out instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021")]
        public virtual bool LoadIfChanged(TimeSpan timeout, object instanceToken, bool lockInstance, out object instance)
        {
            instance = this.Load(timeout, lockInstance);
            return true;
        }
        public abstract void Unlock(TimeSpan timeout);

        public override object Update(object instance, TimeSpan timeout)
        {
            return this.Update(instance, timeout, false);
        }
        public abstract object Update(object instance, TimeSpan timeout, bool unlockInstance);
    }
}
