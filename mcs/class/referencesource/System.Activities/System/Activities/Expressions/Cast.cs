//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime;

    public sealed class Cast<TOperand, TResult> : CodeActivity<TResult>
    {
        //Lock is not needed for operationFunction here. The reason is that delegates for a given Cast<TLeft, TRight, TResult> are the same.
        //It's possible that 2 threads are assigning the operationFucntion at the same time. But it's okay because the compiled codes are the same.
        static Func<TOperand, TResult> checkedOperationFunction;
        static Func<TOperand, TResult> uncheckedOperationFunction;
        bool checkedOperation = true;

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TOperand> Operand
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
            UnaryExpressionHelper.OnGetArguments(metadata, this.Operand);

            if (this.checkedOperation)
            {
                EnsureOperationFunction(metadata, ref checkedOperationFunction, ExpressionType.ConvertChecked);
            }
            else
            {
                EnsureOperationFunction(metadata, ref uncheckedOperationFunction, ExpressionType.Convert);
            }
        }

        void EnsureOperationFunction(CodeActivityMetadata metadata,
            ref Func<TOperand, TResult> operationFunction,
            ExpressionType operatorType)
        {
            if (operationFunction == null)
            {
                ValidationError validationError;
                if (!UnaryExpressionHelper.TryGenerateLinqDelegate(
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
            TOperand operandValue = this.Operand.Get(context);
            
            //if user changed Checked flag between Open and Execution, 
            //a NRE may be thrown and that's by design
            if (this.checkedOperation)
            {
                return checkedOperationFunction(operandValue);
            }
            else
            {
                return uncheckedOperationFunction(operandValue);
            }
        }
    }
}
