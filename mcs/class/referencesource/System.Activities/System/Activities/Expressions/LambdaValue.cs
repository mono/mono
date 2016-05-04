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
    public sealed class LambdaValue<TResult> : CodeActivity<TResult>, IExpressionContainer, IValueSerializableExpression
    {
        Func<ActivityContext, TResult> compiledLambdaValue;
        Expression<Func<ActivityContext, TResult>> lambdaValue;
        Expression<Func<ActivityContext, TResult>> rewrittenTree;

        public LambdaValue(Expression<Func<ActivityContext, TResult>> lambdaValue)
        {
            if (lambdaValue == null)
            {
                throw FxTrace.Exception.ArgumentNull("lambdaValue");
            }
            this.lambdaValue = lambdaValue;
            this.UseOldFastPath = true;
        }

        // this is called via reflection from Microsoft.CDF.Test.ExpressionUtilities.Activities.ActivityUtilities.ReplaceLambdaValuesInActivityTree
        internal Expression LambdaExpression
        {
            get
            {
                return this.lambdaValue;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            CodeActivityPublicEnvironmentAccessor publicAccessor = CodeActivityPublicEnvironmentAccessor.Create(metadata);

            // We need to rewrite the tree.
            Expression newTree;
            if (ExpressionUtilities.TryRewriteLambdaExpression(this.lambdaValue, out newTree, publicAccessor))
            {
                this.rewrittenTree = (Expression<Func<ActivityContext, TResult>>)newTree;
            }
            else
            {
                this.rewrittenTree = this.lambdaValue;
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            if (this.compiledLambdaValue == null)
            {
                this.compiledLambdaValue = this.rewrittenTree.Compile();
            }
            return this.compiledLambdaValue(context);
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
