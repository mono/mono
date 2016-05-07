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
    public sealed class VisualBasicReference<TResult> : CodeActivity<Location<TResult>>, IValueSerializableExpression, IExpressionContainer, ITextExpression
    {
        Expression<Func<ActivityContext, TResult>> expressionTree;
        LocationFactory<TResult> locationFactory;
        CompiledExpressionInvoker invoker;

        public VisualBasicReference() 
            : base()
        {
            this.UseOldFastPath = true;
        }

        public VisualBasicReference(string expressionText)
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

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            if (!this.invoker.IsStaticallyCompiled)
            {
                if (this.expressionTree != null)
                {
                    if (this.locationFactory == null)
                    {
                        this.locationFactory = ExpressionUtilities.CreateLocationFactory<TResult>(this.expressionTree);
                    }

                    return this.locationFactory.CreateLocation(context);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return (Location<TResult>) this.invoker.InvokeExpression(context);
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.expressionTree = null;
            this.invoker = new CompiledExpressionInvoker(this, true, metadata);
            if (this.invoker.IsStaticallyCompiled)
            {
                return;
            }
            
            string validationError;

            // If ICER is not implemented that means we haven't been compiled
            CodeActivityPublicEnvironmentAccessor publicAccessor = CodeActivityPublicEnvironmentAccessor.Create(metadata);
            this.expressionTree = this.CompileLocationExpression(publicAccessor, out validationError);

            if (validationError != null)
            {
                metadata.AddValidationError(validationError);
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
                    string validationError;

                    // it's safe to create this CodeActivityMetadata here,
                    // because we know we are using it only as lookup purpose.
                    CodeActivityMetadata metadata = new CodeActivityMetadata(this, this.GetParentEnvironment(), false);
                    CodeActivityPublicEnvironmentAccessor publicAccessor = CodeActivityPublicEnvironmentAccessor.CreateWithoutArgument(metadata);
                    try
                    {
                        this.expressionTree = this.CompileLocationExpression(publicAccessor, out validationError);

                        if (validationError != null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.VBExpressionTamperedSinceLastCompiled(validationError)));
                        }            
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

        private Expression<Func<ActivityContext, TResult>> CompileLocationExpression(CodeActivityPublicEnvironmentAccessor publicAccessor, out string validationError)
        {            
            Expression<Func<ActivityContext, TResult>> expressionTreeToReturn = null;
            validationError = null;
            try
            {
                expressionTreeToReturn = VisualBasicHelper.Compile<TResult>(this.ExpressionText, publicAccessor, true);
                // inspect the expressionTree to see if it is a valid location expression(L-value)
                string extraErrorMessage = null;
                if (!publicAccessor.ActivityMetadata.HasViolations && (expressionTreeToReturn == null || !ExpressionUtilities.IsLocation(expressionTreeToReturn, typeof(TResult), out extraErrorMessage)))
                {
                    string errorMessage = SR.InvalidLValueExpression;

                    if (extraErrorMessage != null)
                    {
                        errorMessage += ":" + extraErrorMessage;
                    }
                    expressionTreeToReturn = null;
                    validationError = SR.CompilerErrorSpecificExpression(this.ExpressionText, errorMessage);
                }
            }
            catch (SourceExpressionException e)
            {
                validationError = e.Message;
            }

            return expressionTreeToReturn;
        }
    }
}
