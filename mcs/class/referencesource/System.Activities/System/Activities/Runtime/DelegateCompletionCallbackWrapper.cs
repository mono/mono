//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    [DataContract]
    class DelegateCompletionCallbackWrapper : CompletionCallbackWrapper
    {
        static readonly Type callbackType = typeof(DelegateCompletionCallback);
        static readonly Type[] callbackParameterTypes = new Type[] { typeof(NativeActivityContext), typeof(ActivityInstance), typeof(IDictionary<string, object>) };

        Dictionary<string, object> results;

        public DelegateCompletionCallbackWrapper(DelegateCompletionCallback callback, ActivityInstance owningInstance)
            : base(callback, owningInstance)
        {
            this.NeedsToGatherOutputs = true;
        }

        [DataMember(EmitDefaultValue = false, Name = "results")]
        internal Dictionary<string, object> SerializedResults
        {
            get { return this.results; }
            set { this.results = value; }
        }

        protected override void GatherOutputs(ActivityInstance completedInstance)
        {
            if (completedInstance.Activity.HandlerOf != null)
            {
                IList<RuntimeDelegateArgument> runtimeArguments = completedInstance.Activity.HandlerOf.RuntimeDelegateArguments;
                LocationEnvironment environment = completedInstance.Environment;

                for (int i = 0; i < runtimeArguments.Count; i++)
                {
                    RuntimeDelegateArgument runtimeArgument = runtimeArguments[i];

                    if (runtimeArgument.BoundArgument != null)
                    {
                        if (ArgumentDirectionHelper.IsOut(runtimeArgument.Direction))
                        {
                            Location parameterLocation = environment.GetSpecificLocation(runtimeArgument.BoundArgument.Id);

                            if (parameterLocation != null)
                            {
                                if (this.results == null)
                                {
                                    this.results = new Dictionary<string, object>();
                                }

                                this.results.Add(runtimeArgument.Name, parameterLocation.Value);
                            }
                        }
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Because we are calling EnsureCallback",
            Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
        [SecuritySafeCritical]
        protected internal override void Invoke(NativeActivityContext context, ActivityInstance completedInstance)
        {
            EnsureCallback(callbackType, callbackParameterTypes);
            DelegateCompletionCallback completionCallback = (DelegateCompletionCallback)this.Callback;

            IDictionary<string, object> returnValue = this.results;

            if (returnValue == null)
            {
                returnValue = ActivityUtilities.EmptyParameters;
            }

            completionCallback(context, completedInstance, returnValue);
        }

    }
}
