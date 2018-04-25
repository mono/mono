//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Runtime.Serialization;

    public abstract class ActivityWithResult : Activity
    {
        internal ActivityWithResult()
            : base()
        {
        }

        public Type ResultType
        {
            get
            {
                return this.InternalResultType;
            }
        }

        [IgnoreDataMember] // this member is repeated by all subclasses, which we control
        public OutArgument Result
        {
            get
            {
                return this.ResultCore;
            }
            set
            {
                this.ResultCore = value;
            }
        }

        internal abstract Type InternalResultType
        {
            get;
        }

        internal abstract OutArgument ResultCore
        {
            get;
            set;
        }

        internal RuntimeArgument ResultRuntimeArgument
        {
            get;
            set;
        }

        internal abstract object InternalExecuteInResolutionContextUntyped(CodeActivityContext resolutionContext);

        internal override bool IsActivityWithResult
        {
            get
            {
                return true;
            }
        }
    }
}
