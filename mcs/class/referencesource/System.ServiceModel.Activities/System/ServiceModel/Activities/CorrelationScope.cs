//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;

    // The correlation scope has to derive from NativeActivity
    // so that we can access execution properties from AEC.
    // 
    public class CorrelationScope : NativeActivity
    {
        Variable<CorrelationHandle> declaredHandle; // 

        public CorrelationScope()
            : base()
        {
            this.declaredHandle = new Variable<CorrelationHandle>();
        }

        // Explicit correlation OM
        public InArgument<CorrelationHandle> CorrelatesWith
        {
            get;
            set;
        }

        public Activity Body
        {
            get;
            set;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.Body);
            metadata.SetImplementationVariablesCollection(
                new Collection<Variable>
                {
                    this.declaredHandle
                });

            RuntimeArgument correlatesWithArgument = new RuntimeArgument("CorrelatesWith", typeof(CorrelationHandle), ArgumentDirection.In);
            metadata.Bind(this.CorrelatesWith, correlatesWithArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { correlatesWithArgument });
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                CorrelationHandle ambientHandle = null;
                if (this.CorrelatesWith != null && this.CorrelatesWith.Expression != null)
                {
                    ambientHandle = this.CorrelatesWith.Get(context);
                }

                if (ambientHandle == null)
                {
                    ambientHandle = this.declaredHandle.Get(context);
                }

                context.Properties.Add(CorrelationHandle.StaticExecutionPropertyName, ambientHandle);

                context.ScheduleActivity(this.Body);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeCorrelatesWith()
        {
            return this.CorrelatesWith != null && this.CorrelatesWith.Expression != null;
        }
    }
}
