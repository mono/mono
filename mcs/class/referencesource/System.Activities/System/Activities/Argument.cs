//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    public abstract class Argument
    {
        public static readonly int UnspecifiedEvaluationOrder = -1;

        public const string ResultValue = "Result";
        
        ArgumentDirection direction;
        RuntimeArgument runtimeArgument;
        int evaluationOrder;

        internal Argument()
        {
            this.evaluationOrder = Argument.UnspecifiedEvaluationOrder;
        }

        public Type ArgumentType
        {
            get;
            internal set;
        }

        public ArgumentDirection Direction
        {
            get
            {
                return this.direction;
            }
            internal set
            {
                ArgumentDirectionHelper.Validate(value, "value");
                this.direction = value;
            }
        }

        [DefaultValue(-1)]
        public int EvaluationOrder
        {
            get
            {
                return this.evaluationOrder;
            }
            set
            {
                if (value < 0 && value != Argument.UnspecifiedEvaluationOrder)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("EvaluationOrder", value, SR.InvalidEvaluationOrderValue);
                }
                this.evaluationOrder = value;
            }
        }

        [IgnoreDataMember] // this member is repeated by all subclasses, which we control
        [DefaultValue(null)]
        public ActivityWithResult Expression
        {
            get
            {
                return this.ExpressionCore;
            }
            set
            {
                this.ExpressionCore = value;
            }
        }        

        internal abstract ActivityWithResult ExpressionCore
        {
            get;
            set;
        }

        internal RuntimeArgument RuntimeArgument
        {
            get
            {
                return this.runtimeArgument;
            }
            set
            {
                this.runtimeArgument = value;
            }
        }

        internal bool IsInTree
        {
            get
            {
                return (this.runtimeArgument != null && this.runtimeArgument.IsInTree);
            }
        }

        internal bool WasDesignTimeNull
        {
            get;
            set;
        }

        internal int Id
        {
            get
            {
                Fx.Assert(this.runtimeArgument != null, "We shouldn't call Id unless we have a runtime argument.");
                return this.runtimeArgument.Id;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return this.Expression == null;
            }
        }

        public static Argument CreateReference(Argument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }

            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }

            return ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, argumentToReference.Direction, referencedArgumentName);
        }

        // for ArgumentValueSerializer
        internal bool CanConvertToString(IValueSerializerContext context)
        {
            if (this.WasDesignTimeNull)
            {
                return true;
            }            
            else
            {
                if (this.EvaluationOrder == Argument.UnspecifiedEvaluationOrder)
                {
                    return ActivityWithResultValueSerializer.CanConvertToStringWrapper(this.Expression, context);
                }
                else
                {
                    return false;
                }
            }
        }

        internal string ConvertToString(IValueSerializerContext context)
        {
            if (this.WasDesignTimeNull)
            {
                // this argument instance was artificially created by the runtime
                // to Xaml, this should appear as {x:Null}
                return null;
            }

            return ActivityWithResultValueSerializer.ConvertToStringWrapper(this.Expression, context);
        }

        internal static void Bind(Argument binding, RuntimeArgument argument)
        {
            if (binding != null)
            {
                Fx.Assert(binding.Direction == argument.Direction, "The directions must match.");
                Fx.Assert(binding.ArgumentType == argument.Type, "The types must match.");

                binding.RuntimeArgument = argument;
            }

            argument.BoundArgument = binding;
        }

        internal static void TryBind(Argument binding, RuntimeArgument argument, Activity violationOwner)
        {
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }

            bool passedValidations = true;

            if (binding != null)
            {
                if (binding.Direction != argument.Direction)
                {
                    violationOwner.AddTempValidationError(new ValidationError(SR.ArgumentDirectionMismatch(argument.Name, argument.Direction, binding.Direction)));
                    passedValidations = false;
                }

                if (binding.ArgumentType != argument.Type)
                {
                    violationOwner.AddTempValidationError(new ValidationError(SR.ArgumentTypeMismatch(argument.Name, argument.Type, binding.ArgumentType)));
                    passedValidations = false;
                }
            }

            if (passedValidations)
            {
                Bind(binding, argument);
            }
        }

        public static Argument Create(Type type, ArgumentDirection direction)
        {
            return ActivityUtilities.CreateArgument(type, direction);
        }

        internal abstract Location CreateDefaultLocation();

        internal abstract void Declare(LocationEnvironment targetEnvironment, ActivityInstance activityInstance);

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public object Get(ActivityContext context)
        {
            return Get<object>(context);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public T Get<T>(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            ThrowIfNotInTree();

            return context.GetValue<T>(this.RuntimeArgument);
        }

        public void Set(ActivityContext context, object value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            ThrowIfNotInTree();

            context.SetValue(this.RuntimeArgument, value);
        }

        internal void Validate(Activity owner, ref IList<ValidationError> validationErrors)
        {
            if (this.Expression != null)
            {
                if (this.Expression.Result != null && !this.Expression.Result.IsEmpty)
                {
                    ValidationError validationError = new ValidationError(SR.ResultCannotBeSetOnArgumentExpressions, false, this.RuntimeArgument.Name, owner);
                    ActivityUtilities.Add(ref validationErrors, validationError);
                }

                ActivityWithResult actualExpression = this.Expression;

                if (actualExpression is IExpressionWrapper)
                {
                    actualExpression = ((IExpressionWrapper)actualExpression).InnerExpression;
                }

                switch (this.Direction)
                {
                    case ArgumentDirection.In:
                        if (actualExpression.ResultType != this.ArgumentType)
                        {
                            ActivityUtilities.Add(
                                ref validationErrors,
                                new ValidationError(SR.ArgumentValueExpressionTypeMismatch(this.ArgumentType, actualExpression.ResultType), false, this.RuntimeArgument.Name, owner));
                        }
                        break;
                    case ArgumentDirection.InOut:
                    case ArgumentDirection.Out:
                        Type locationType;
                        if (!ActivityUtilities.IsLocationGenericType(actualExpression.ResultType, out locationType) ||
                            locationType != this.ArgumentType)
                        {
                            Type expectedType = ActivityUtilities.CreateActivityWithResult(ActivityUtilities.CreateLocation(this.ArgumentType));
                            ActivityUtilities.Add(
                                ref validationErrors,
                                new ValidationError(SR.ArgumentLocationExpressionTypeMismatch(expectedType.FullName, actualExpression.GetType().FullName), false, this.RuntimeArgument.Name, owner));
                        }
                        break;
                }
            }
        }

        // optional "fast-path" for arguments that can be resolved synchronously
        internal abstract bool TryPopulateValue(LocationEnvironment targetEnvironment, ActivityInstance targetActivityInstance, ActivityExecutor executor);

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public Location GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            ThrowIfNotInTree();

            return this.runtimeArgument.GetLocation(context);
        }

        internal void ThrowIfNotInTree()
        {
            if (!this.IsInTree)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ArgumentNotInTree(this.ArgumentType)));
            }
        }

        internal static Location<T> CreateLocation<T>()
        {
            return new Location<T>();
        }

        internal interface IExpressionWrapper
        {
            ActivityWithResult InnerExpression { get; }
        }
    }
}
