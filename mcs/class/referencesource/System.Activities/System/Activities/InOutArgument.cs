//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.XamlIntegration;
    using System.Activities.Runtime;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Windows.Markup;

    public abstract class InOutArgument : Argument
    {
        internal InOutArgument()
        {
            this.Direction = ArgumentDirection.InOut;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Subclass needed to enforce rules about which directions can be referenced.")]
        public static InOutArgument CreateReference(InOutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }

            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }

            return (InOutArgument)ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.InOut, referencedArgumentName);
        }
    }

    [ContentProperty("Expression")]
    [TypeConverter(typeof(InOutArgumentConverter))]
    [ValueSerializer(typeof(ArgumentValueSerializer))]
    public sealed class InOutArgument<T> : InOutArgument
    {
        public InOutArgument(Variable variable)
            : this()
        {
            if (variable != null)
            {
                this.Expression = new VariableReference<T> { Variable = variable };
            }
        }

        public InOutArgument(Variable<T> variable)
            : this()
        {
            if (variable != null)
            {
                this.Expression = new VariableReference<T> { Variable = variable };
            }
        }

        public InOutArgument(Expression<Func<ActivityContext, T>> expression)
            : this()
        {
            if (expression != null)
            {
                this.Expression = new LambdaReference<T>(expression);
            }
        }

        public InOutArgument(Activity<Location<T>> expression)
            : this()
        {
            this.Expression = expression;
        }

        public InOutArgument()
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

                Activity<Location<T>> typedActivity = value as Activity<Location<T>>;

                if (typedActivity != null)
                {
                    this.Expression = typedActivity;
                }
                else
                {
                    // We do not verify compatibility here. We will do that
                    // during CacheMetadata in Argument.Validate.
                    this.Expression = new ActivityWithResultWrapper<Location<T>>(value);
                }
            }
        }

        public static implicit operator InOutArgument<T>(Variable<T> variable)
        {
            return FromVariable(variable);
        }

        public static implicit operator InOutArgument<T>(Activity<Location<T>> expression)
        {
            return FromExpression(expression);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]    
        public static InOutArgument<T> FromVariable(Variable<T> variable)
        {
            return new InOutArgument<T>
            {
                Expression = new VariableReference<T> { Variable = variable }
            };
        }

        public static InOutArgument<T> FromExpression(Activity<Location<T>> expression)
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }

            return new InOutArgument<T>
            {
                Expression = expression
            };
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
            targetEnvironment.DeclareTemporaryLocation<Location<T>>(this.RuntimeArgument, activityInstance, false);
        }

        internal override bool TryPopulateValue(LocationEnvironment targetEnvironment, ActivityInstance targetActivityInstance, ActivityExecutor executor)
        {
            Fx.Assert(this.Expression != null, "This should only be called for non-empty bindings.");

            if (this.Expression.UseOldFastPath)
            {
                Location<T> argumentValue = executor.ExecuteInResolutionContext<Location<T>>(targetActivityInstance, this.Expression);
                targetEnvironment.Declare(this.RuntimeArgument, argumentValue.CreateReference(false), targetActivityInstance);
                return true;
            }
            else
            {
                targetEnvironment.DeclareTemporaryLocation<Location<T>>(this.RuntimeArgument, targetActivityInstance, false);
                return false;
            }
        }
    }
}
