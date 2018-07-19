//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class Handle
    {
        ActivityInstance owner;

        // We check uninitialized because it should be false more often
        bool isUninitialized;

        protected Handle()
        {
            this.isUninitialized = true;
        }

        public ActivityInstance Owner
        {
            get
            {
                return this.owner;
            }
        }

        public string ExecutionPropertyName
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "owner")]
        internal ActivityInstance SerializedOwner
        {
            get { return this.owner; }
            set { this.owner = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "isUninitialized")]
        internal bool SerializedIsUninitialized
        {
            get { return this.isUninitialized; }
            set { this.isUninitialized = value; }
        }

        [DataMember(EmitDefaultValue = false)]
        internal bool CanBeRemovedWithExecutingChildren
        {
            get;
            set;
        }

        internal bool IsInitialized
        {
            get
            {
                return !this.isUninitialized;
            }
        }

        internal static string GetPropertyName(Type handleType)
        {
            Fx.Assert(TypeHelper.AreTypesCompatible(handleType, typeof(Handle)), "must pass in a Handle-based type here");
            return handleType.FullName;
        }

        internal void Initialize(HandleInitializationContext context)
        {
            this.owner = context.OwningActivityInstance;
            this.isUninitialized = false;

            OnInitialize(context);
        }

        internal void Reinitialize(ActivityInstance owner)
        {
            this.owner = owner;
        }

        internal void Uninitialize(HandleInitializationContext context)
        {
            OnUninitialize(context);
            this.isUninitialized = true;
        }

        protected virtual void OnInitialize(HandleInitializationContext context)
        {
        }

        protected virtual void OnUninitialize(HandleInitializationContext context)
        {
        }

        protected void ThrowIfUninitialized()
        {
            if (this.isUninitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.HandleNotInitialized));
            }
        }
    }
}


