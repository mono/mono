//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    
    using System;
    using System.Activities.Statements;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using System.ComponentModel;

    [ContentProperty("Body")]
    class NoPersistScope : NativeActivity
    {
        Variable<NoPersistHandle> noPersistHandle;

        public NoPersistScope()
        {
            this.noPersistHandle = new Variable<NoPersistHandle>();
        }

        [DefaultValue(null)]
        public Activity Body 
        {
            get;
            set;
        }
        
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.Body);
            metadata.AddImplementationVariable(this.noPersistHandle);
        }
        
        protected override void Execute(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                NoPersistHandle handle = this.noPersistHandle.Get(context);
                handle.Enter(context);
                context.ScheduleActivity(this.Body);
            }
        }
    }         
}
