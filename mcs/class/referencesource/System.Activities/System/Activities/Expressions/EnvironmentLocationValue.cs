//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Linq.Expressions;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class EnvironmentLocationValue<T> : CodeActivity<T>, IExpressionContainer, ILocationReferenceExpression
    {
        LocationReference locationReference;

        // Ctors are internal because we rely on validation from creator or descendant
        internal EnvironmentLocationValue()
        {
            this.UseOldFastPath = true;
        }

        internal EnvironmentLocationValue(LocationReference locationReference)
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

        protected override T Execute(CodeActivityContext context)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                return context.GetValue<T>(this.LocationReference);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
        }

        ActivityWithResult ILocationReferenceExpression.CreateNewInstance(LocationReference locationReference)
        {
            return new EnvironmentLocationValue<T>(locationReference);
        }
    }
}
