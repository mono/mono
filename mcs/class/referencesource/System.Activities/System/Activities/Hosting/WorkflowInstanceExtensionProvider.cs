//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Hosting
{
    using System.Runtime;

    abstract class WorkflowInstanceExtensionProvider
    {
        protected WorkflowInstanceExtensionProvider()
        {
        }

        public Type Type
        {
            get;
            protected set;
        }

        protected bool GeneratedTypeMatchesDeclaredType
        {
            get;
            set;
        }

        public abstract object ProvideValue();

        public bool IsMatch<TTarget>(object value)
            where TTarget : class
        {
            Fx.Assert(value != null, "extension providers never return a null extension");
            if (value is TTarget)
            {
                if (this.GeneratedTypeMatchesDeclaredType)
                {
                    return true;
                }
                else
                {
                    return TypeHelper.AreReferenceTypesCompatible(this.Type, typeof(TTarget));
                }
            }
            else
            {
                return false;
            }
        }
    }

    class WorkflowInstanceExtensionProvider<T> : WorkflowInstanceExtensionProvider
        where T : class
    {
        Func<T> providerFunction;
        bool hasGeneratedValue;

        public WorkflowInstanceExtensionProvider(Func<T> providerFunction)
            : base()
        {
            this.providerFunction = providerFunction;
            base.Type = typeof(T);
        }

        public override object ProvideValue()
        {
            T value = this.providerFunction();
            if (!this.hasGeneratedValue)
            {
                base.GeneratedTypeMatchesDeclaredType = object.ReferenceEquals(value.GetType(), this.Type);
                this.hasGeneratedValue = true;
            }

            return value;
        }
    }
}
