//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Linq.Expressions;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;

    public sealed class Subtract<TLeft, TRight, TResult> : CodeActivity<TResult>
    {
        //Lock is not needed for operationFunction here. The reason is that delegates for a given Subtract<TLeft, TRight, TResult> are the same.
        //It's possible that 2 threads are assigning the operationFucntion at the same time. But it's okay because the compiled codes are the same.
        static Func<TLeft, TRight, TResult> checkedOperationFunction;
        static Func<TLeft, TRight, TResult> uncheckedOperationFunction;
        bool checkedOperation = true;

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TLeft> Left
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TRight> Right
        {
            get;
            set;
        }

        [DefaultValue(true)]
        public bool Checked
        {
            get { return this.checkedOperation; }
            set { this.checkedOperation = value; }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            BinaryExpressionHelper.OnGetArguments(metadata, this.Left, this.Right);

            if (this.checkedOperation)
            {
                EnsureOperationFunction(metadata, ref checkedOperationFunction, ExpressionType.SubtractChecked);
            }
            else
            {
                EnsureOperationFunction(metadata, ref uncheckedOperationFunction, ExpressionType.Subtract);
            }
        }

        void EnsureOperationFunction(CodeActivityMetadata metadata,
            ref Func<TLeft, TRight, TResult> operationFunction,
            ExpressionType operatorType)
        {
            if (operationFunction == null)
            {
                ValidationError validationError;
                if (!BinaryExpressionHelper.TryGenerateLinqDelegate(
                            operatorType,
                            out operationFunction,
                            out validationError))
                {
                    metadata.AddValidationError(validationError);
                }
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TLeft leftValue = this.Left.Get(context);
            TRight rightValue = this.Right.Get(context);

            //if user changed Checked flag between Open and Execution, 
            //a NRE may be thrown and that's by design
            if (this.checkedOperation)
            {
                return checkedOperationFunction(leftValue, rightValue);
            }
            else
            {
                return uncheckedOperationFunction(leftValue, rightValue);
            }
        }
    }
}
