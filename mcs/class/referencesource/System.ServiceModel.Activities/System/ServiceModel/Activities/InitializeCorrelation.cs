//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Dispatcher;
    using SR2 = System.ServiceModel.Activities.SR;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Runtime.Collections;
    using System.Runtime;

    [ContentProperty("CorrelationData")]
    public sealed class InitializeCorrelation : NativeActivity
    {
        public InitializeCorrelation()
        {
            this.CorrelationData = new OrderedDictionary<string, InArgument<string>>();
        }

        [DefaultValue(null)]
        public InArgument<CorrelationHandle> Correlation
        {
            get;
            set;
        }

        public IDictionary<string, InArgument<string>> CorrelationData
        {
            get;
            private set;
        }

        protected override void Execute(NativeActivityContext context)
        {
            CorrelationHandle correlationHandle = (this.Correlation == null) ? null : this.Correlation.Get(context);

            if (correlationHandle == null)
            {
                //throw only if ambient correlation handle is also null
                correlationHandle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                if (correlationHandle == null)
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(SR2.NullCorrelationHandleInInitializeCorrelation(this.DisplayName)));
                }
            }
 
            CorrelationExtension extension = context.GetExtension<CorrelationExtension>();
            if (extension != null)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                foreach ( KeyValuePair<string, InArgument<string>> pair in this.CorrelationData )
                {
                    Fx.Assert(pair.Value != null, "pair.Value should be validated during cache metadata");
                    dictionary.Add(pair.Key, pair.Value.Get(context));
                }

                correlationHandle.InitializeBookmarkScope(context, extension.GenerateKey(dictionary));
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.InitializeCorrelationRequiresWorkflowServiceHost(this.DisplayName)));
            }
        }

    }
}
