//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Activities.ExpressionParser;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Windows.Markup;

    // consciously not XAML-friendly since Linq Expressions aren't create-set-use
    [Fx.Tag.XamlVisible(false)]
    [DebuggerStepThrough]
    public sealed class LambdaReference<T> : CodeActivity<Location<T>>, IExpressionContainer, IValueSerializableExpression
    {
        Expression<Func<ActivityContext, T>> locationExpression;
        Expression<Func<ActivityContext, T>> rewrittenTree;
        LocationFactory<T> locationFactory;

        public LambdaReference(Expression<Func<ActivityContext, T>> locationExpression)
        {
            if (locationExpression == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationExpression");
            }
            this.locationExpression = locationExpression;
            this.UseOldFastPath = true;
        }

        // this is called via reflection from Microsoft.CDF.Test.ExpressionUtilities.Activities.ActivityUtilities.ReplaceLambdaValuesInActivityTree
        internal Expression LambdaExpression
        {
            get
            {
                return this.locationExpression;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            CodeActivityPublicEnvironmentAccessor publicAccessor = CodeActivityPublicEnvironmentAccessor.Create(metadata);

            // We need to rewrite the tree.
            Expression newTree;
            if (ExpressionUtilities.TryRewriteLambdaExpression(this.locationExpression, out newTree, publicAccessor, true))
            {
                this.rewrittenTree = (Expression<Func<ActivityContext, T>>)newTree;
            }
            else
            {
                this.rewrittenTree = this.locationExpression;
            }

            // inspect the expressionTree to see if it is a valid location expression(L-value)
            string extraErrorMessage = null;
            if (!ExpressionUtilities.IsLocation(this.rewrittenTree, typeof(T), out extraErrorMessage))
            {
                string errorMessage = SR.InvalidLValueExpression;
                if (extraErrorMessage != null)
                {
                    errorMessage += ":" + extraErrorMessage;
                }
                metadata.AddValidationError(errorMessage);
            }
        }

        protected override Location<T> Execute(CodeActivityContext context)
        {
            if (this.locationFactory == null)
            {
                this.locationFactory = ExpressionUtilities.CreateLocationFactory<T>(this.rewrittenTree);
            }
            return this.locationFactory.CreateLocation(context);
        }

        public bool CanConvertToString(IValueSerializerContext context)
        {
            return true;
        }

        public string ConvertToString(IValueSerializerContext context)
        {
            // This workflow contains lambda expressions specified in code. 
            // These expressions are not XAML serializable. 
            // In order to make your workflow XAML-serializable, 
            // use either VisualBasicValue/Reference or ExpressionServices.Convert  
            // This will convert your lambda expressions into expression activities.
            throw FxTrace.Exception.AsError(new LambdaSerializationException());
        }
    }
}
