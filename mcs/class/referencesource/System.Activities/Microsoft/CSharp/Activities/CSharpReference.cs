//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.CSharp.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Windows.Markup;

    [DebuggerStepThrough]
    [ContentProperty("ExpressionText")]
    public class CSharpReference<TResult> : CodeActivity<Location<TResult>>, ITextExpression
    {
        CompiledExpressionInvoker invoker;
                
        public CSharpReference()
        {
            this.UseOldFastPath = true;
        }

        public CSharpReference(string expressionText) :
            this()
        {
            this.ExpressionText = expressionText;
        }

        public string ExpressionText
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Language
        {
            get
            {
                return "C#";
            }
        }

        public bool RequiresCompilation
        {
            get
            {
                return true;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.invoker = new CompiledExpressionInvoker(this, true, metadata);
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            Location<TResult> value = (Location<TResult>)this.invoker.InvokeExpression(context);

            return value;
        }

        public Expression GetExpressionTree()
        {
            if (this.IsMetadataCached)
            {
                return this.invoker.GetExpressionTree();
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ActivityIsUncached)); 
            }
        }
    }
}

