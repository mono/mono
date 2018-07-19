//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Linq.Expressions;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class EnvironmentLocationReference<T> : CodeActivity<Location<T>>, IExpressionContainer, ILocationReferenceExpression
    {
        LocationReference locationReference;

        // Ctors are internal because we rely on validation from creator or descendant
        internal EnvironmentLocationReference()
        {
            this.UseOldFastPath = true;
        }

        internal EnvironmentLocationReference(LocationReference locationReference)
            : this()
        {
            this.locationReference = locationReference;
        }

        public virtual LocationReference LocationReference
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

        protected sealed override Location<T> Execute(CodeActivityContext context)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                return context.GetLocation<T>(this.LocationReference);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
        }

        ActivityWithResult ILocationReferenceExpression.CreateNewInstance(LocationReference locationReference)
        {
            return new EnvironmentLocationReference<T>(locationReference);
        }
    }
}
