//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Runtime;

    static class BinaryExpressionHelper
    {
        public static void OnGetArguments<TLeft, TRight>(CodeActivityMetadata metadata, InArgument<TLeft> left, InArgument<TRight> right)
        {
            RuntimeArgument rightArgument = new RuntimeArgument("Right", typeof(TRight), ArgumentDirection.In, true);
            metadata.Bind(right, rightArgument);

            RuntimeArgument leftArgument = new RuntimeArgument("Left", typeof(TLeft), ArgumentDirection.In, true);
            metadata.Bind(left, leftArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    rightArgument,
                    leftArgument
                });
        }

        public static bool TryGenerateLinqDelegate<TLeft, TRight, TResult>(ExpressionType operatorType, out Func<TLeft, TRight, TResult> function, out ValidationError validationError)
        {
            function = null;
            validationError = null;

            ParameterExpression leftParameter = Expression.Parameter(typeof(TLeft), "left");
            ParameterExpression rightParameter = Expression.Parameter(typeof(TRight), "right");

            try
            {
                BinaryExpression binaryExpression = Expression.MakeBinary(operatorType, leftParameter, rightParameter);

                Expression expressionToCompile = OperatorPermissionHelper.InjectReflectionPermissionIfNecessary(binaryExpression.Method, binaryExpression);
                Expression<Func<TLeft, TRight, TResult>> lambdaExpression = Expression.Lambda<Func<TLeft, TRight, TResult>>(expressionToCompile, leftParameter, rightParameter);
                function = lambdaExpression.Compile();

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
