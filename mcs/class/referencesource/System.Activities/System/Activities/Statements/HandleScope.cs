//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class HandleScope<THandle> : NativeActivity 
        where THandle : Handle
    {
        Variable<THandle> declaredHandle;

        public HandleScope()
        {
        }

        public InArgument<THandle> Handle
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
            RuntimeArgument handleArgument = new RuntimeArgument("Handle", typeof(THandle), ArgumentDirection.In);
            metadata.Bind(this.Handle, handleArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { handleArgument });

            if (this.Body != null)
            {
                metadata.SetChildrenCollection(new Collection<Activity> { this.Body });
            }

            Collection<Variable> implementationVariables = null;

            if ((this.Handle == null) || this.Handle.IsEmpty)
            {
                if (this.declaredHandle == null)
                {
                    this.declaredHandle = new Variable<THandle>();
                }
            }
            else
            {
                this.declaredHandle = null;
            }

            if (this.declaredHandle != null)
            {
                ActivityUtilities.Add(ref implementationVariables, this.declaredHandle);
            }

            metadata.SetImplementationVariablesCollection(implementationVariables);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // We should go through the motions even if there is no Body for debugging
            // purposes.  When testing handles people will probably use empty scopes
            // expecting everything except the Body execution to occur.

            Handle scopedHandle = null;

            if ((this.Handle == null) || this.Handle.IsEmpty)
            {
                Fx.Assert(this.declaredHandle != null, "We should have declared the variable if we didn't have the argument set.");
                scopedHandle = this.declaredHandle.Get(context);
            }
            else
            {
                scopedHandle = this.Handle.Get(context);
            }

            if (scopedHandle == null)
            {
                throw FxTrace.Exception.ArgumentNull("Handle");
            }

            context.Properties.Add(scopedHandle.ExecutionPropertyName, scopedHandle);

            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body);
            }
        }
    }
}
