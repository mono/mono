//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.ExpressionParser;
    using System.Activities.XamlIntegration;
    using System.Linq.Expressions;
    using System.Windows.Markup;
    using System.ComponentModel;
    using System.Runtime;

    [DebuggerStepThrough]
    public sealed class VisualBasicValue<TResult> : CodeActivity<TResult>, IValueSerializableExpression, IExpressionContainer, ITextExpression
    {
        Expression<Func<ActivityContext, TResult>> expressionTree;
        Func<ActivityContext, TResult> compiledExpression;
        CompiledExpressionInvoker invoker; 

        public VisualBasicValue() 
            : base()
        {
            this.UseOldFastPath = true;
        }

        public VisualBasicValue(string expressionText)
            : this()
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
                return VisualBasicHelper.Language;
            }
        }

        public bool RequiresCompilation
        {
            get
            {
                return false;
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            if (!this.invoker.IsStaticallyCompiled)
            {
                if (this.expressionTree != null)
                {
                    if (this.compiledExpression == null)
                    {
                        this.compiledExpression = this.expressionTree.Compile();
                    }
                    return this.compiledExpression(context);
                }
                else
                {
                    return default(TResult);
                }
            }
            else
            {
                return (TResult)this.invoker.InvokeExpression(context);
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.expressionTree = null;
            this.invoker = new CompiledExpressionInvoker(this, false, metadata);
            if (this.invoker.IsStaticallyCompiled == true)
            {
                return;
            }

            // If ICER is not implemented that means we haven't been compiled

            CodeActivityPublicEnvironmentAccessor publicAccessor = CodeActivityPublicEnvironmentAccessor.Create(metadata);            
            try
            {
                this.expressionTree = VisualBasicHelper.Compile<TResult>(this.ExpressionText, publicAccessor, false);
            }
            catch (SourceExpressionException e)
            {
                metadata.AddValidationError(e.Message);
            }
        }

        public bool CanConvertToString(IValueSerializerContext context)
        {
            // we can always convert to a string 
            return true;
        }

        public string ConvertToString(IValueSerializerContext context)
        {
            // Return our bracket-escaped text
            return "[" + this.ExpressionText + "]";
        }

        public Expression GetExpressionTree()
        {            
            if (this.IsMetadataCached)
            {
                if (this.expressionTree == null)
                {
                    // it's safe to create this CodeActivityMetadata here,
                    // because we know we are using it only as lookup purpose.
                    CodeActivityMetadata metadata = new CodeActivityMetadata(this, this.GetParentEnvironment(), false);
                    CodeActivityPublicEnvironmentAccessor publicAccessor = CodeActivityPublicEnvironmentAccessor.CreateWithoutArgument(metadata);
                    try
                    {                                                
                        this.expressionTree = VisualBasicHelper.Compile<TResult>(this.ExpressionText, publicAccessor, false);
                    }
                    catch (SourceExpressionException e)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.VBExpressionTamperedSinceLastCompiled(e.Message))); 
                    }
                    finally
                    {
                        metadata.Dispose();
                    }                   
                }

                Fx.Assert(this.expressionTree.NodeType == ExpressionType.Lambda, "Lambda expression required");
                return ExpressionUtilities.RewriteNonCompiledExpressionTree((LambdaExpression)this.expressionTree);
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ActivityIsUncached)); 
            }
        }
    }
}
