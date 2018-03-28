//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Runtime;

    static class UnaryExpressionHelper
    {
        public static void OnGetArguments<TOperand>(CodeActivityMetadata metadata, InArgument<TOperand> operand)
        {
            RuntimeArgument operandArgument = new RuntimeArgument("Operand", typeof(TOperand), ArgumentDirection.In, true);
            metadata.Bind(operand, operandArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    operandArgument
                });
        }

        public static bool TryGenerateLinqDelegate<TOperand, TResult>(ExpressionType operatorType, out Func<TOperand, TResult> operation, out ValidationError validationError)
        {
            operation = null;
            validationError = null;

            ParameterExpression operandParameter = Expression.Parameter(typeof(TOperand), "operand");
            try
            {
                UnaryExpression unaryExpression = Expression.MakeUnary(operatorType, operandParameter, typeof(TResult));
                Expression expressionToCompile = OperatorPermissionHelper.InjectReflectionPermissionIfNecessary(unaryExpression.Method, unaryExpression);
                Expression<Func<TOperand, TResult>> lambdaExpression = Expression.Lambda<Func<TOperand, TResult>>(expressionToCompile, operandParameter);
                operation = lambdaExpression.Compile();
                return true;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                validationError = new ValidationError(e.Message);
                return false;
            }
        }
    }

}
