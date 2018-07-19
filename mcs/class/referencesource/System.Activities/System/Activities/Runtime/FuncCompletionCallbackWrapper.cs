//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Security;

    [DataContract]
    class FuncCompletionCallbackWrapper<T> : CompletionCallbackWrapper
    {
        static readonly Type callbackType = typeof(CompletionCallback<T>);
        static readonly Type[] callbackParameterTypes = new Type[] { typeof(NativeActivityContext), typeof(ActivityInstance), typeof(T) };

        T resultValue;

        public FuncCompletionCallbackWrapper(CompletionCallback<T> callback, ActivityInstance owningInstance)
            : base(callback, owningInstance)
        {
            this.NeedsToGatherOutputs = true;
        }

        [DataMember(EmitDefaultValue = false, Name = "resultValue")]
        internal T SerializedResultValue
        {
            get { return this.resultValue; }
            set { this.resultValue = value; }
        }

        int GetResultId(ActivityWithResult activity)
        {
            if (activity.Result != null)
            {
                return activity.Result.Id;
            }
            else
            {
                for (int i = 0; i < activity.RuntimeArguments.Count; i++)
                {
                    RuntimeArgument argument = activity.RuntimeArguments[i];

                    if (argument.IsResult)
                    {
                        return argument.Id;
                    }
                }
            }

            return -1;
        }

        protected override void GatherOutputs(ActivityInstance completedInstance)
        {
            int resultId = -1;

            if (completedInstance.Activity.HandlerOf != null)
            {
                DelegateOutArgument resultArgument = completedInstance.Activity.HandlerOf.GetResultArgument();
                if (resultArgument != null)
                {
                    resultId = resultArgument.Id;
                }
                else
                {
                    ActivityWithResult activity = completedInstance.Activity as ActivityWithResult;
                    // for auto-generated results, we should bind the value from the Handler if available
                    if (activity != null && TypeHelper.AreTypesCompatible(activity.ResultType, typeof(T)))
                    {
                        resultId = GetResultId(activity);
                    }
                }
            }
            else
            {
                Fx.Assert(completedInstance.Activity is ActivityWithResult, "should only be using FuncCompletionCallbackWrapper with ActivityFunc and ActivityWithResult");
                resultId = GetResultId((ActivityWithResult)completedInstance.Activity);
            }

            if (resultId >= 0)
            {
                Location location = completedInstance.Environment.GetSpecificLocation(resultId);
                Location<T> typedLocation = location as Location<T>;

                if (typedLocation != null)
                {
                    this.resultValue = typedLocation.Value;
                }
                else if (location != null)
                {
                    this.resultValue = TypeHelper.Convert<T>(location.Value);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Because we are calling EnsureCallback",
            Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
        [SecuritySafeCritical]
        protected internal override void Invoke(NativeActivityContext context, ActivityInstance completedInstance)
        {
            // Call the EnsureCallback overload that also looks for SomeMethod<T> where T is the result type
            // and the signature matches.
            EnsureCallback(callbackType, callbackParameterTypes, callbackParameterTypes[2]);
            CompletionCallback<T> completionCallback = (CompletionCallback<T>)this.Callback;
            completionCallback(context, completedInstance, this.resultValue);
        }

        protected override void OnSerializingGenericCallback()
        {
            ValidateCallbackResolution(callbackType, callbackParameterTypes, callbackParameterTypes[2]);
        }
    }
}
