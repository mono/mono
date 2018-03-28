//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    public abstract class DelegateOutArgument : DelegateArgument
    {
        internal DelegateOutArgument()
            : base()
        {
            this.Direction = ArgumentDirection.Out;
        }

    }

    public sealed class DelegateOutArgument<T> : DelegateOutArgument
    {
        public DelegateOutArgument()
            : base()
        {
        }

        public DelegateOutArgument(string name)
            : base()
        {
            this.Name = name;
        }

        protected override Type TypeCore
        {
            get
            {
                return typeof(T);
            }
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public new T Get(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            return context.GetValue<T>((LocationReference)this);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public new Location<T> GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            return context.GetLocation<T>(this);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            context.SetValue((LocationReference)this, value);
        }

        internal override Location CreateLocation()
        {
            return new Location<T>();
        }
    }
}
