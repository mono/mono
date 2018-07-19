//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Linq.Expressions;
    using System.Runtime;
   
    [Fx.Tag.XamlVisible(false)]
    sealed class LocationReferenceValue<T> : CodeActivity<T>, IExpressionContainer, ILocationReferenceWrapper, ILocationReferenceExpression
    {
        LocationReference locationReference;

        internal LocationReferenceValue(LocationReference locationReference)
        {
            this.UseOldFastPath = true;
            this.locationReference = locationReference;
        }

        LocationReference ILocationReferenceWrapper.LocationReference
        {
            get
            {
                return this.locationReference;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            // the creator of this activity is expected to have checked visibility of LocationReference.
            // we override the base CacheMetadata to avoid unnecessary reflection overhead.
        }

        protected override T Execute(CodeActivityContext context)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                return context.GetValue<T>(this.locationReference);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
        }

        ActivityWithResult ILocationReferenceExpression.CreateNewInstance(LocationReference locationReference)
        {
            return new LocationReferenceValue<T>(locationReference);
        }
    }
}
