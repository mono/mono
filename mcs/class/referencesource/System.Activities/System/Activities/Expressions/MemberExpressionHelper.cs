//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Activities.Validation;

    static class MemberExpressionHelper
    {
        public static void AddOperandArgument<TOperand>(CodeActivityMetadata metadata, InArgument<TOperand> operand, bool isRequired)
        {
            RuntimeArgument operandArgument = new RuntimeArgument("Operand", typeof(TOperand), ArgumentDirection.In, isRequired);
            metadata.Bind(operand, operandArgument);
            metadata.AddArgument(operandArgument);
        }

        public static void AddOperandLocationArgument<TOperand>(CodeActivityMetadata metadata, InOutArgument<TOperand> operandLocation, bool isRequired)
        {
            RuntimeArgument operandLocationArgument = new RuntimeArgument("OperandLocation", typeof(TOperand), ArgumentDirection.InOut, isRequired);
            metadata.Bind(operandLocation, operandLocationArgument);
            metadata.AddArgument(operandLocationArgument);
        }

        public static bool TryGenerateLinqDelegate<TOperand, TResult>(string memberName, bool isField, bool isStatic, out Func<TOperand, TResult> operation, out ValidationError validationError)
        {
            operation = null;
            validationError = null;

            try
            {
                ParameterExpression operandParameter = Expression.Parameter(typeof(TOperand), "operand");
                MemberExpression memberExpression = null;
                if (isStatic)
                {
                    memberExpression = Expression.MakeMemberAccess(null, GetMemberInfo<TOperand>(memberName, isField));
                }
                else
                {
                    memberExpression = Expression.MakeMemberAccess(operandParameter, GetMemberInfo<TOperand>(memberName, isField));
                }
                Expression<Func<TOperand, TResult>> lambdaExpression = Expression.Lambda<Func<TOperand, TResult>>(memberExpression, operandParameter);
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

        static MemberInfo GetMemberInfo<TOperand>(string memberName, bool isField)
        {
            MemberInfo result = null;
            Type declaringType = typeof(TOperand);

            if (!isField)
            {
                result = declaringType.GetProperty(memberName);
            }
            else
            {
                result = declaringType.GetField(memberName);
            }
            if (result == null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR.MemberNotFound(memberName, typeof(TOperand).Name)));
            }
            return result;
        }


    }

}
