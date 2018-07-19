//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Runtime;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Diagnostics.CodeAnalysis;
    
    public abstract class OutArgument : Argument
    {
        internal OutArgument()
        {
            this.Direction = ArgumentDirection.Out;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Subclass needed to enforce rules about which directions can be referenced.")]
        public static OutArgument CreateReference(OutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }

            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }

            return (OutArgument)ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.Out, referencedArgumentName);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Subclass needed to enforce rules about which directions can be referenced.")]
        public static OutArgument CreateReference(InOutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }

            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }

            // Note that we explicitly pass Out since we want an OutArgument created
            return (OutArgument)ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.Out, referencedArgumentName);
        }
    }

    [ContentProperty("Expression")]
    [TypeConverter(typeof(OutArgumentConverter))]    
    [ValueSerializer(typeof(ArgumentValueSerializer))]
    public sealed class OutArgument<T> : OutArgument
    {
        public OutArgument(Variable variable)
            : this()
        {
            if (variable != null)
            {
                this.Expression = new VariableReference<T> { Variable = variable };
            }
        }

        public OutArgument(DelegateArgument delegateArgument)
            : this()
        {
            if (delegateArgument != null)
            {
                this.Expression = new DelegateArgumentReference<T> { DelegateArgument = delegateArgument };
            }
        }

        public OutArgument(Expression<Func<ActivityContext, T>> expression)
            : this()
        {
            if (expression != null)
            {
                this.Expression = new LambdaReference<T>(expression);
            }
        }

        public OutArgument(Activity<Location<T>> expression)
            : this()
        {
            this.Expression = expression;
        }

        public OutArgument()
            : base()
        {
            this.ArgumentType = typeof(T);
        }

        [DefaultValue(null)]
        public new Activity<Location<T>> Expression
        {
            get;
            set;
        }

        internal override ActivityWithResult ExpressionCore
        {
            get
            {
                return this.Expression;
            }
            set
            {
                if (value == null)
                {
                    this.Expression = null;
                    return;
                }

                if (value is Activity<Location<T>>)
                {
                    this.Expression = (Activity<Location<T>>)value;
                }
                else
                {
                    // We do not verify compatibility here. We will do that
                    // during CacheMetadata in Argument.Validate.
                    this.Expression = new ActivityWithResultWrapper<Location<T>>(value);
                }
            }
        }

        public static implicit operator OutArgument<T>(Variable variable)
        {
            return FromVariable(variable);
        }

        public static implicit operator OutArgument<T>(DelegateArgument delegateArgument)
        {
            return FromDelegateArgument(delegateArgument);
        }

        public static implicit operator OutArgument<T>(Activity<Location<T>> expression)
        {
            return FromExpression(expression);
        }

        public static OutArgument<T> FromVariable(Variable variable)
        {
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            return new OutArgument<T>(variable);
        }

        public static OutArgument<T> FromDelegateArgument(DelegateArgument delegateArgument)
        {
            if (delegateArgument == null)
            {
                throw FxTrace.Exception.ArgumentNull("delegateArgument");
            }
            return new OutArgument<T>(delegateArgument);
        }

        public static OutArgument<T> FromExpression(Activity<Location<T>> expression)
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }

            return new OutArgument<T>(expression);
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

            ThrowIfNotInTree();

            return context.GetLocation<T>(this.RuntimeArgument);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public new T Get(ActivityContext context)
        {
            return Get<T>(context);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            ThrowIfNotInTree();

            context.SetValue(this, value);
        }

        internal override Location CreateDefaultLocation()
        {
            return Argument.CreateLocation<T>();
        }

        internal override void Declare(LocationEnvironment targetEnvironment, ActivityInstance activityInstance)
        {
            targetEnvironment.DeclareTemporaryLocation<Location<T>>(this.RuntimeArgument, activityInstance, true);
        }

        internal override bool TryPopulateValue(LocationEnvironment targetEnvironment, ActivityInstance targetActivityInstance, ActivityExecutor executor)
        {
            Fx.Assert(this.Expression != null, "This should only be called for non-empty bindings.");

            if (this.Expression.UseOldFastPath)
            {
                Location<T> argumentValue = executor.ExecuteInResolutionContext<Location<T>>(targetActivityInstance, this.Expression);
                targetEnvironment.Declare(this.RuntimeArgument, argumentValue.CreateReference(true), targetActivityInstance);
                return true;
            }
            else
            {
                targetEnvironment.DeclareTemporaryLocation<Location<T>>(this.RuntimeArgument, targetActivityInstance, true);
                return false;
            }
        }
    }
}

