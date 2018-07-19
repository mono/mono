//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class CodeActivityContext : ActivityContext
    {
        // This is called by the Pool.
        internal CodeActivityContext()
        {
        }

        // This is only used by base classes which do not take
        // part in pooling.
        internal CodeActivityContext(ActivityInstance instance, ActivityExecutor executor)
            : base(instance, executor)
        {
        }

        internal void Initialize(ActivityInstance instance, ActivityExecutor executor)
        {
            base.Reinitialize(instance, executor);
        }

        public THandle GetProperty<THandle>() where THandle : Handle
        {
            ThrowIfDisposed();
            if (this.CurrentInstance.PropertyManager != null)
            {
                return (THandle)this.CurrentInstance.PropertyManager.GetProperty(Handle.GetPropertyName(typeof(THandle)), this.Activity.MemberOf);
            }
            else
            {
                return null;
            }
        }
        
        public void Track(CustomTrackingRecord record)
        {
            ThrowIfDisposed();

            if (record == null)
            {
                throw FxTrace.Exception.ArgumentNull("record");
            }

            base.TrackCore(record);
        }
    }
}
