//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Activities.Statements;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections;

    public static class ExpressionServices
    {
        // Reflection is used to call generic function because type information are only known at runtime.
        static MethodInfo TryConvertBinaryExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertBinaryExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo TryConvertUnaryExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertUnaryExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo TryConvertMemberExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertMemberExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo TryConvertArgumentExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertArgumentExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo TryConvertReferenceMemberExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertReferenceMemberExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo TryConvertIndexerReferenceHandle = typeof(ExpressionServices).GetMethod("TryConvertIndexerReferenceWorker", BindingFlags.NonPublic | BindingFlags.Static);

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "The parameter is restricted correctly.")]
        public static Activity<TResult> Convert<TResult>(Expression<Func<ActivityContext, TResult>> expression)
        {
            Activity<TResult> result;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("expression", SR.ExpressionRequiredForConversion));
            }
            TryConvert<TResult>(expression.Body, true, out result);
            return result;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "The parameter is restricted correctly.")]
        public static bool TryConvert<TResult>(Expression<Func<ActivityContext, TResult>> expression, out Activity<TResult> result)
        {
            if (expression == null)
            {
                result = null;
                return false;
            }
            return TryConvert<TResult>(expression.Body, false, out result) == null;
        }

        static string TryConvert<TResult>(Expression body, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            UnaryExpression unaryExpressionBody = body as UnaryExpression;
            if (unaryExpressionBody != null)
            {
                Type operandType = unaryExpressionBody.Operand.Type;
                Type resultType = typeof(TResult);
                return TryConvertUnaryExpression<TResult>(unaryExpressionBody, operandType, throwOnError, out result);
            }
            BinaryExpression binaryExpressionBody = body as BinaryExpression;
            if (binaryExpressionBody != null)
            {
                Type leftType = binaryExpressionBody.Left.Type;
                Type rightType = binaryExpressionBody.Right.Type;
                if (binaryExpressionBody.NodeType == ExpressionType.ArrayIndex)
                {
                    return TryConvertArrayItemValue<TResult>(binaryExpressionBody, leftType, rightType, throwOnError, out result);
                }
                return TryConvertBinaryExpression<TResult>(binaryExpressionBody, leftType, rightType, throwOnError, out result);
            }
            MemberExpression memberExpressionBody = body as MemberExpression;
            if (memberExpressionBody != null)
            {
                Type memberType = memberExpressionBody.Expression == null ? memberExpressionBody.Member.DeclaringType : memberExpressionBody.Expression.Type;
                return TryConvertMemberExpression<TResult>(memberExpressionBody, memberType, throwOnError, out result);
            }
            MethodCallExpression methodCallExpressionBody = body as MethodCallExpression;
            if (methodCallExpressionBody != null)
            {
                MethodInfo calledMethod = methodCallExpressionBody.Method;
                Type declaringType = calledMethod.DeclaringType;
                ParameterInfo[] parameters = calledMethod.GetParameters();
                if (TypeHelper.AreTypesCompatible(declaringType, typeof(Variable)) && calledMethod.Name == "Get" && parameters.Length == 1 && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext)))
                {
                    return TryConvertVariableValue<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(Argument))
                     && calledMethod.Name == "Get" && parameters.Length == 1 && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext)))
                {
                    return TryConvertArgumentValue<TResult>(methodCallExpressionBody.Object as MemberExpression, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(DelegateArgument))
                    && calledMethod.Name == "Get" && parameters.Length == 1 && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext)))
                {
                    return TryConvertDelegateArgumentValue<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(ActivityContext)) && calledMethod.Name == "GetValue" && parameters.Length == 1 &&
                (TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(Argument)) || TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(RuntimeArgument))))
                {
                    MemberExpression memberExpression = methodCallExpressionBody.Arguments[0] as MemberExpression;
                    return TryConvertArgumentValue<TResult>(memberExpression, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(ActivityContext)) && calledMethod.Name == "GetValue" && parameters.Length == 1 &&
                    (TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(LocationReference))))
                {
                    return TryConvertLocationReference<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
                else
                {
                    return TryConvertMethodCallExpression<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
            }
            InvocationExpression invocationExpression = body as InvocationExpression;
            if (invocationExpression != null)
            {
                return TryConvertInvocationExpression<TResult>(invocationExpression, throwOnError, out result);
            }
            NewExpression newExpression = body as NewExpression;
            if (newExpression != null)
            {
                return TryConvertNewExpression<TResult>(newExpression, throwOnError, out result);
            }
            NewArrayExpression newArrayExpression = body as NewArrayExpression;
            if (newArrayExpression != null && newArrayExpression.NodeType != ExpressionType.NewArrayInit)
            {
                return TryConvertNewArrayExpression<TResult>(newArrayExpression, throwOnError, out result);
            }
            ConstantExpression constantExpressionBody = body as ConstantExpression;
            if (constantExpressionBody != null)
            {
                // This is to handle the leaf node as a literal value
                result = new Literal<TResult> { Value = (TResult)constantExpressionBody.Value };
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedExpressionType(body.NodeType)));
            }
            else
            {
                return SR.UnsupportedExpressionType(body.NodeType);
            }
        }        

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "The parameter is restricted correctly.")]
        public static Activity<Location<TResult>> ConvertReference<TResult>(Expression<Func<ActivityContext, TResult>> expression)
        {
            Activity<Location<TResult>> result;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("expression", SR.ExpressionRequiredForConversion));
            }

            TryConvertReference<TResult>(expression.Body, true, out result);
            return result;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "The parameter is restricted correctly.")]
        public static bool TryConvertReference<TResult>(Expression<Func<ActivityContext, TResult>> expression, out Activity<Location<TResult>> result)
        {
            if (expression == null)
            {
                result = null;
                return false;
            }
            return TryConvertReference<TResult>(expression.Body, false, out result) == null;
        }

        static string TryConvertReference<TResult>(Expression body, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            MemberExpression memberExpressionBody = body as MemberExpression;
            if (memberExpressionBody != null)
            {
                Type memberType = memberExpressionBody.Expression == null ? memberExpressionBody.Member.DeclaringType : memberExpressionBody.Expression.Type;
                return TryConvertReferenceMemberExpression<TResult>(memberExpressionBody, memberType, throwOnError, out result);
            }
            BinaryExpression binaryExpressionBody = body as BinaryExpression;
            if (binaryExpressionBody != null)
            {
                Type leftType = binaryExpressionBody.Left.Type;
                Type rightType = binaryExpressionBody.Right.Type;
                if (binaryExpressionBody.NodeType == ExpressionType.ArrayIndex)
                {
                    return TryConvertArrayItemReference<TResult>(binaryExpressionBody, leftType, rightType, throwOnError, out result);
                }
            }
            MethodCallExpression methodCallExpressionBody = body as MethodCallExpression;
            if (methodCallExpressionBody != null)
            {
                Type declaringType = methodCallExpressionBody.Method.DeclaringType;
                MethodInfo calledMethod = methodCallExpressionBody.Method;
                if (declaringType.IsArray && calledMethod.Name == "Get")
                {
                    return TryConvertMultiDimensionalArrayItemReference<TResult>(methodCallExpressionBody, throwOnError, out result);
                }

                if (calledMethod.IsSpecialName && calledMethod.Name == "get_Item")
                {
                    return TryConvertIndexerReference(methodCallExpressionBody, throwOnError, out result);
                }

                ParameterInfo[] parameters = calledMethod.GetParameters();
                if (TypeHelper.AreTypesCompatible(declaringType, typeof(Variable)) && calledMethod.Name == "Get" && parameters.Length == 1 && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext)))
                {
                    return TryConvertVariableReference<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(Argument))
                     && calledMethod.Name == "Get" && parameters.Length == 1 && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext)))
                {
                    return TryConvertArgumentReference<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(DelegateArgument))
                    && calledMethod.Name == "Get" && parameters.Length == 1 && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext)))
                {
                    return TryConvertDelegateArgumentReference<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
                else if (TypeHelper.AreTypesCompatible(declaringType, typeof(ActivityContext)) && calledMethod.Name == "GetValue" && parameters.Length == 1 &&
                    (TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(LocationReference))))
                {
                    return TryConvertReferenceLocationReference<TResult>(methodCallExpressionBody, throwOnError, out result);
                }
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedReferenceExpressionType(body.NodeType)));
            }
            else
            {
                return SR.UnsupportedReferenceExpressionType(body.NodeType);
            }
        }

        static string TryConvertIndexerReference<TResult>(MethodCallExpression methodCallExpressionBody, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            try
            {
                if (methodCallExpressionBody.Object == null)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR.InstanceMethodCallRequiresTargetObject));
                    }
                    else
                    {
                        return SR.InstanceMethodCallRequiresTargetObject;
                    }
                }
                MethodInfo specializedHandle = TryConvertIndexerReferenceHandle.MakeGenericMethod(methodCallExpressionBody.Object.Type, typeof(TResult));
                object[] parameters = new object[] { methodCallExpressionBody, throwOnError, null };
                string errorString = specializedHandle.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<Location<TResult>>;
                return errorString;
            }
            catch (TargetInvocationException e)
            {
                throw FxTrace.Exception.AsError(e.InnerException);
            }
        }

        static string TryConvertIndexerReferenceWorker<TOperand, TResult>(MethodCallExpression methodCallExpressionBody, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            Fx.Assert(methodCallExpressionBody.Object != null, "Indexer must have a target object");
            if (!typeof(TOperand).IsValueType)
            {
                Activity<TOperand> operand = null;
                string operandError = TryConvert<TOperand>(methodCallExpressionBody.Object, throwOnError, out operand);
                if (operandError != null)
                {
                    return operandError;
                }
                IndexerReference<TOperand, TResult> indexerReference = new IndexerReference<TOperand, TResult>
                {
                    Operand = new InArgument<TOperand>(operand) { EvaluationOrder = 0 },
                };
                string argumentError = TryConvertArguments(methodCallExpressionBody.Arguments, indexerReference.Indices, methodCallExpressionBody.GetType(), 1, null, throwOnError);
                if (argumentError != null)
                {
                    return argumentError;
                }
                result = indexerReference;

            }
            else
            {
                Activity<Location<TOperand>> operandReference = null;
                string operandError = TryConvertReference<TOperand>(methodCallExpressionBody.Object, throwOnError, out operandReference);
                if (operandError != null)
                {
                    return operandError;
                }
                ValueTypeIndexerReference<TOperand, TResult> indexerReference = new ValueTypeIndexerReference<TOperand, TResult>
                {
                    OperandLocation = new InOutArgument<TOperand>(operandReference) { EvaluationOrder = 0 },
                };
                string argumentError = TryConvertArguments(methodCallExpressionBody.Arguments, indexerReference.Indices, methodCallExpressionBody.GetType(), 1, null, throwOnError);
                if (argumentError != null)
                {
                    return argumentError;
                }
                result = indexerReference;
            }
            return null;
        }

        static string TryConvertMultiDimensionalArrayItemReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            Activity<Array> operand;
            if (methodCallExpression.Object == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.InstanceMethodCallRequiresTargetObject));
                }
                else
                {
                    return SR.InstanceMethodCallRequiresTargetObject;
                }
            }
            string errorString = TryConvert<Array>(methodCallExpression.Object, throwOnError, out operand);
            if (errorString != null)
            {
                return errorString;
            }

            MultidimensionalArrayItemReference<TResult> reference = new MultidimensionalArrayItemReference<TResult>
            {
                Array = new InArgument<Array>(operand) { EvaluationOrder = 0 },
            };

            Collection<InArgument<int>> arguments = reference.Indices;
            string argumentError = TryConvertArguments(methodCallExpression.Arguments, reference.Indices, methodCallExpression.GetType(), 1, null, throwOnError);
            if (argumentError != null)
            {
                return argumentError;
            }
            result = reference;
            return null;

        }

        static string TryConvertVariableReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            Variable variableObject = null;

            //
            // This is a fast path to handle a simple variable object.               
            //
            // Linq actually generate a temp class wrapping all the local variables.
            //
            // The real expression object look like
            // new TempClass() { A = a }.A.Get(env)
            // 
            // A is a field 

            if (methodCallExpression.Object is MemberExpression)
            {
                MemberExpression member = methodCallExpression.Object as MemberExpression;
                if (member.Expression is ConstantExpression)
                {
                    ConstantExpression memberExpression = member.Expression as ConstantExpression;
                    if (member.Member is FieldInfo)
                    {
                        FieldInfo field = member.Member as FieldInfo;
                        variableObject = field.GetValue(memberExpression.Value) as Variable;
                        Fx.Assert(variableObject != null, "Linq generated expression tree should be correct");
                        result = new VariableReference<TResult> { Variable = variableObject };
                        return null;
                    }
                }
            }

            //This is to handle the expression whose evaluation result is a variable object.
            //Limitation: The expression of variable object has to be evaludated in conversion time. It means after conversion, the variable object should not be changed any more.
            //For example, the following case is not legal:
            //
            //Program.static_X = new Variable<string> { Default = "Hello" };
            //Activity<Location<string>> weRef = ExpressionServices.ConvertReference<string>((env) => Program.static_X.Get(env));
            //Program.static_X = new Variable<string> { Default = "World" };
            //Sequence sequence = new Sequence
            //{
            //    Variables = { Program.static_X },
            //    Activities = 
            //      {
            //         new Assign<string>
            //         {
            //             To = new OutArgument<string>{Expression = weRef},
            //             Value = "haha",
            //         },
            //         new WriteLine
            //         {
            //             Text = Program.static_X,
            //         }
            //      }
            //};
            //WorkflowInvoker.Invoke(sequence);
            //
            // The reason is that "Program.static_X = new Variable<string> { Default = "World" }" happens after conversion.
            try
            {
                Expression<Func<Variable>> funcExpression = Expression.Lambda<Func<Variable>>(methodCallExpression.Object);
                Func<Variable> func = funcExpression.Compile();
                variableObject = func();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(e);
                }
                else
                {
                    return e.Message;
                }
            }
            Fx.Assert(variableObject is Variable<TResult>, "Linq generated expression tree should be correct");
            result = new VariableReference<TResult> { Variable = variableObject };
            return null;
        }

        static string TryConvertArrayItemReference<TResult>(BinaryExpression binaryExpression, Type leftType, Type rightType, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;

            //for ArrayIndex expression, Left type is always TResult[] and Right type is always int
            if (!leftType.IsArray)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.DoNotSupportArrayIndexerOnNonArrayType(leftType)));
                }
                else
                {
                    return SR.DoNotSupportArrayIndexerOnNonArrayType(leftType);
                }
            }
            //Because co-variance for LValue requires that TResult is compatible with actual type. However, we cannot write such a lambda expression. E,g:
            //Expression<Func<ActivityContext, DerivedClass> expr = env => a.Get(env). Here a.Get(env) returns BaseClass.  So we needn't co-viariance here.
            if (leftType.GetElementType() != typeof(TResult))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.DoNotSupportArrayIndexerReferenceWithDifferentArrayTypeAndResultType(leftType, typeof(TResult))));
                }
                else
                {
                    return SR.DoNotSupportArrayIndexerReferenceWithDifferentArrayTypeAndResultType(leftType, typeof(TResult));
                }
            }
            if (rightType != typeof(int))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType)));
                }
                else
                {
                    return SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType);
                }
            }

            Activity<TResult[]> array;
            string arrayError = TryConvert<TResult[]>(binaryExpression.Left, throwOnError, out array);
            if (arrayError != null)
            {
                return arrayError;
            }

            Activity<int> index;
            string indexError = TryConvert<int>(binaryExpression.Right, throwOnError, out index);
            if (indexError != null)
            {
                return indexError;
            }

            result = new ArrayItemReference<TResult>
            {
                Array = new InArgument<TResult[]>(array) { EvaluationOrder = 0 },
                Index = new InArgument<int>(index) { EvaluationOrder = 1 },
            };
            return null;
        }

        static string TryConvertVariableValue<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            Variable variableObject = null;
            
            //
            // This is a fast path to handle a simple variable object                
            //
            // Linq actually generate a temp class wrapping all the local variables.
            //
            // The real expression object look like
            // new TempClass() { A = a }.A.Get(env)
            // 
            // A is a field 

            if (methodCallExpression.Object is MemberExpression)
            {
                MemberExpression member = methodCallExpression.Object as MemberExpression;
                if (member.Expression is ConstantExpression)
                {
                    ConstantExpression memberExpression = member.Expression as ConstantExpression;
                    if (member.Member is FieldInfo)
                    {
                        FieldInfo field = member.Member as FieldInfo;
                        variableObject = field.GetValue(memberExpression.Value) as Variable;
                        result = new VariableValue<TResult> { Variable = variableObject };
                        return null;
                    }
                }
            }

            //This is to handle the expression whose evaluation result is a variable object.
            //Limitation: The expression of variable object has to be evaludated in conversion time. It means after conversion, the variable object should not be changed any more.
            //For example, the following case is not legal:
            //
            //  Program.static_X = new Variable<string> { Default = "Hello" };
            //  Activity<string> we = ExpressionServices.Convert((env) => Program.static_X.Get(env));
            //  Program.static_X = new Variable<string> { Default = "World" };
            //  Sequence sequence = new Sequence
            //  {
            //      Variables = { Program.static_X },
            //      Activities = 
            //      {
            //             new WriteLine
            //          {
            //                 Text = new InArgument<string>{Expression = we},
            //          }
            //      }
            //  };
            //  WorkflowInvoker.Invoke(sequence);
            //
            // The reason is that "Program.static_X = new Variable<string> { Default = "World" }" happens after conversion.

            try
            {
                Expression<Func<Variable>> funcExpression = Expression.Lambda<Func<Variable>>(methodCallExpression.Object);
                Func<Variable> func = funcExpression.Compile();
                variableObject = func();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(e);
                }
                else
                {
                    return e.Message;
                }
            }
            result = new VariableValue<TResult> { Variable = variableObject };
            return null;
        }

        static string TryConvertDelegateArgumentValue<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            DelegateArgument delegateArgument = null;

            //This is to handle the expression whose evaluation result is a DelegateArgument.
            //Limitation: The expression of variable object has to be evaluated in conversion time. It means after conversion, the DelegateArgument object should not be changed any more.
            //For example, the following case is not legal:
            //
            //  Program.static_X = new DelegateInArgument<string>();
            //  Activity<string> we = ExpressionServices.Convert((env) => Program.static_X.Get(env));
            //  Program.static_X = new DelegateInArgument<string>();
            //  ActivityAction<string> activityAction = new ActivityAction<string>
            //  {
            //      Argument = Program.static_X,
            //      Handler = new WriteLine
            //          {
            //                 Text = we,
            //          }
            //      }
            //  };
            //  WorkflowInvoker.Invoke( new InvokeAction<string>
            //                          {
            //                              Argument = "Hello",
            //                              Action = activityAction,
            //                          }
            //);
            //
            // The reason is that "Program.static_X" is changed after conversion.

            try
            {
                Expression<Func<DelegateArgument>> funcExpression = Expression.Lambda<Func<DelegateArgument>>(methodCallExpression.Object);
                Func<DelegateArgument> func = funcExpression.Compile();
                delegateArgument = func();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(e);
                }
                else
                {
                    return e.Message;
                }
            }
            result = new DelegateArgumentValue<TResult>(delegateArgument);
            return null;
        }

        static string TryConvertDelegateArgumentReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            DelegateArgument delegateArgument = null;

            //This is to handle the expression whose evaluation result is a DelegateArgument.
            //Limitation: The expression of variable object has to be evaluated in conversion time. It means after conversion, the DelegateArgument object should not be changed any more.
            //For example, the following case is not legal:
            //
            //  Program.static_X = new DelegateInArgument<string>();
            //  Activity<string> we = ExpressionServices.Convert((env) => Program.static_X.Get(env));
            //  Program.static_X = new DelegateInArgument<string>();
            //  ActivityAction<string> activityAction = new ActivityAction<string>
            //  {
            //      Argument = Program.static_X,
            //      Handler = new WriteLine
            //          {
            //                 Text = we,
            //          }
            //      }
            //  };
            //  WorkflowInvoker.Invoke( new InvokeAction<string>
            //                          {
            //                              Argument = "Hello",
            //                              Action = activityAction,
            //                          }
            //);
            //
            // The reason is that "Program.static_X" is changed after conversion.

            try
            {
                Expression<Func<DelegateArgument>> funcExpression = Expression.Lambda<Func<DelegateArgument>>(methodCallExpression.Object);
                Func<DelegateArgument> func = funcExpression.Compile();
                delegateArgument = func();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(e);
                }
                else
                {
                    return e.Message;
                }
            }
            result = new DelegateArgumentReference<TResult>(delegateArgument);
            return null;
        }

        static string TryConvertArgumentValue<TResult>(MemberExpression memberExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;

            if (memberExpression != null && TypeHelper.AreTypesCompatible(memberExpression.Type, typeof(RuntimeArgument)))
            {
                RuntimeArgument ra = null;
                try
                {
                    Expression<Func<RuntimeArgument>> expr = Expression.Lambda<Func<RuntimeArgument>>(memberExpression, null);
                    Func<RuntimeArgument> func = expr.Compile();
                    ra = func();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    return e.Message;
                }

                if (ra != null)
                {
                    result = new ArgumentValue<TResult>
                    {
                        ArgumentName = ra.Name,
                    };
                    return null;
                }
                else
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR.RuntimeArgumentNotCreated));
                    }
                    else
                    {
                        return SR.RuntimeArgumentNotCreated;
                    }
                }

            }
            else
            {
                //Assumption: Arguments must be properties of Activity object. Otherwise, it cannot be found by runtime via ArgumentValue.
                if (memberExpression != null && memberExpression.Member is PropertyInfo)
                {
                    PropertyInfo property = memberExpression.Member as PropertyInfo;
                    result = new ArgumentValue<TResult> { ArgumentName = property.Name };
                    return null;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.ArgumentMustbePropertyofWorkflowElement));
                }
                else
                {
                    return SR.ArgumentMustbePropertyofWorkflowElement;
                }
            }
        }

        static string TryConvertArgumentReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            //Assumption: Arguments must be properties of Activity object. Otherwise, it cannot be found by runtime via ArgumentReference.
            if (methodCallExpression.Object is MemberExpression)
            {
                MemberExpression member = methodCallExpression.Object as MemberExpression;
                if (member.Member is PropertyInfo)
                {
                    PropertyInfo property = member.Member as PropertyInfo;
                    result = new ArgumentReference<TResult> { ArgumentName = property.Name };
                    return null;
                }
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR.ArgumentMustbePropertyofWorkflowElement));
            }
            else
            {
                return SR.ArgumentMustbePropertyofWorkflowElement;
            }
        }

        static string TryConvertBinaryExpression<TResult>(BinaryExpression binaryExpressionBody, Type leftType, Type rightType, bool throwOnError, out Activity<TResult> result)
        {
            try
            {
                MethodInfo specializedHandle = TryConvertBinaryExpressionHandle.MakeGenericMethod(leftType, rightType, typeof(TResult));
                object[] parameters = new object[] { binaryExpressionBody, throwOnError, null };
                string errorString = specializedHandle.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<TResult>;
                return errorString;
            }
            catch (TargetInvocationException e)
            {
                throw FxTrace.Exception.AsError(e.InnerException);
            }
        }

        //this method handles single dimentional array. Multiple dimentional array accessor is method call expression
        static string TryConvertArrayItemValue<TResult>(BinaryExpression binaryExpression, Type leftType, Type rightType, bool throwOnError, out Activity<TResult> result)
        {
            result = null;

            //for ArrayIndex expression, Left type is always TResult[] and Right type is always int
            if (!leftType.IsArray)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.DoNotSupportArrayIndexerOnNonArrayType(leftType)));
                }
                else
                {
                    return SR.DoNotSupportArrayIndexerOnNonArrayType(leftType);
                }
            }
            if (!TypeHelper.AreTypesCompatible(leftType.GetElementType(), typeof(TResult)))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.DoNotSupportArrayIndexerValueWithIncompatibleArrayTypeAndResultType(leftType, typeof(TResult))));
                }
                else
                {
                    return SR.DoNotSupportArrayIndexerValueWithIncompatibleArrayTypeAndResultType(leftType, typeof(TResult));
                }
            }
            if (rightType != typeof(int))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType)));
                }
                else
                {
                    return SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType);
                }
            }

            Activity<TResult[]> array;
            string arrayError = TryConvert<TResult[]>(binaryExpression.Left, throwOnError, out array);
            if (arrayError != null)
            {
                return arrayError;
            }

            Activity<int> index;
            string indexError = TryConvert<int>(binaryExpression.Right, throwOnError, out index);
            if (indexError != null)
            {
                return indexError;
            }

            result = new ArrayItemValue<TResult>
            {
                Array = new InArgument<TResult[]>(array) { EvaluationOrder = 0 },
                Index = new InArgument<int>(index) { EvaluationOrder = 1 },
            };
            return null;
        }

        static string TryConvertBinaryExpressionWorker<TLeft, TRight, TResult>(BinaryExpression binaryExpressionBody, bool throwOnError, out Activity<TResult> result)
        {
            result = null;

            Activity<TLeft> left;
            string leftError = TryConvert<TLeft>(binaryExpressionBody.Left, throwOnError, out left);
            if (leftError != null)
            {
                return leftError;
            }
            Activity<TRight> right;
            string rightError = TryConvert<TRight>(binaryExpressionBody.Right, throwOnError, out right);
            if (rightError != null)
            {
                return rightError;
            }

            if (binaryExpressionBody.Method != null)
            {
                return TryConvertOverloadingBinaryOperator<TLeft, TRight, TResult>(binaryExpressionBody, left, right, throwOnError, out result);
            }

            InArgument<TLeft> leftArgument = new InArgument<TLeft>(left) { EvaluationOrder = 0 };
            InArgument<TRight> rightArgument = new InArgument<TRight>(right) { EvaluationOrder = 1 };

            switch (binaryExpressionBody.NodeType)
            {
                case ExpressionType.Add:
                    result = new Add<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument, Checked = false };
                    break;
                case ExpressionType.AddChecked:
                    result = new Add<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument, Checked = true };
                    break;
                case ExpressionType.Subtract:
                    result = new Subtract<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument, Checked = false };
                    break;
                case ExpressionType.SubtractChecked:
                    result = new Subtract<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument, Checked = true };
                    break;
                case ExpressionType.Multiply:
                    result = new Multiply<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument, Checked = false };
                    break;
                case ExpressionType.MultiplyChecked:
                    result = new Multiply<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument, Checked = true };
                    break;
                case ExpressionType.Divide:
                    result = new Divide<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.AndAlso:
                    Fx.Assert(typeof(TLeft) == typeof(bool), "AndAlso only accept bool.");
                    Fx.Assert(typeof(TRight) == typeof(bool), "AndAlso only accept bool.");
                    Fx.Assert(typeof(TResult) == typeof(bool), "AndAlso only accept bool.");
                    // Work around generic constraints
                    object leftObject1 = left;
                    object rightObject1 = right;
                    object resultObject1 = new AndAlso() { Left = (Activity<bool>)leftObject1, Right = (Activity<bool>)rightObject1 };
                    result = (Activity<TResult>)resultObject1;
                    break;
                case ExpressionType.OrElse:
                    Fx.Assert(typeof(TLeft) == typeof(bool), "OrElse only accept bool.");
                    Fx.Assert(typeof(TRight) == typeof(bool), "OrElse only accept bool.");
                    Fx.Assert(typeof(TResult) == typeof(bool), "OrElse only accept bool.");
                    // Work around generic constraints
                    object leftObject2 = left;
                    object rightObject2 = right;
                    object resultObject2 = new OrElse() { Left = (Activity<bool>)leftObject2, Right = (Activity<bool>)rightObject2 };
                    result = (Activity<TResult>)resultObject2;
                    break;
                case ExpressionType.Or:
                    result = new Or<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.And:
                    result = new And<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.LessThan:
                    result = new LessThan<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.LessThanOrEqual:
                    result = new LessThanOrEqual<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.GreaterThan:
                    result = new GreaterThan<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    result = new GreaterThanOrEqual<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.Equal:
                    result = new Equal<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                case ExpressionType.NotEqual:
                    result = new NotEqual<TLeft, TRight, TResult>() { Left = leftArgument, Right = rightArgument };
                    break;
                default:
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedExpressionType(binaryExpressionBody.NodeType)));
                    }
                    else
                    {
                        return SR.UnsupportedExpressionType(binaryExpressionBody.NodeType);
                    }
            }

            return null;
        }

        static string TryConvertUnaryExpression<TResult>(UnaryExpression unaryExpressionBody, Type operandType, bool throwOnError, out Activity<TResult> result)
        {
            try
            {
                MethodInfo specializedHandle = TryConvertUnaryExpressionHandle.MakeGenericMethod(operandType, typeof(TResult));
                object[] parameters = new object[] { unaryExpressionBody, throwOnError, null };
                string errorString = specializedHandle.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<TResult>;
                return errorString;
            }
            catch (TargetInvocationException e)
            {
                throw FxTrace.Exception.AsError(e.InnerException);
            }
        }

        static string TryConvertUnaryExpressionWorker<TOperand, TResult>(UnaryExpression unaryExpressionBody, bool throwOnError, out Activity<TResult> result)
        {
            result = null;

            Activity<TOperand> operand;
            string operandError = TryConvert<TOperand>(unaryExpressionBody.Operand, throwOnError, out operand);
            if (operandError != null)
            {
                return operandError;
            }

            if (unaryExpressionBody.Method != null)
            {
                return TryConvertOverloadingUnaryOperator<TOperand, TResult>(unaryExpressionBody, operand, throwOnError, out result);
            }

            switch (unaryExpressionBody.NodeType)
            {
                case ExpressionType.Not:
                    result = new Not<TOperand, TResult> { Operand = operand };
                    break;
                case ExpressionType.Convert:
                    result = new Cast<TOperand, TResult> { Operand = operand, Checked = false };
                    break;
                case ExpressionType.ConvertChecked:
                    result = new Cast<TOperand, TResult> { Operand = operand, Checked = true };
                    break;
                case ExpressionType.TypeAs:
                    result = new As<TOperand, TResult> { Operand = operand };
                    break;
                default:
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedExpressionType(unaryExpressionBody.NodeType)));
                    }
                    else
                    {
                        return SR.UnsupportedExpressionType(unaryExpressionBody.NodeType);
                    }
            }

            return null;
        }

        static string TryConvertMemberExpression<TResult>(MemberExpression memberExpressionBody, Type operandType, bool throwOnError, out Activity<TResult> result)
        {
            try
            {
                MethodInfo specializedHandle = TryConvertMemberExpressionHandle.MakeGenericMethod(operandType, typeof(TResult));
                object[] parameters = new object[] { memberExpressionBody, throwOnError, null };
                string errorString = specializedHandle.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<TResult>;
                return errorString;
            }
            catch (TargetInvocationException e)
            {
                throw FxTrace.Exception.AsError(e.InnerException);
            }
        }

        static string TryConvertMemberExpressionWorker<TOperand, TResult>(MemberExpression memberExpressionBody, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            Activity<TOperand> operand = null;
            if (memberExpressionBody.Expression != null)
            {
                // Static property might not have any expressions.
                string operandError = TryConvert<TOperand>(memberExpressionBody.Expression, throwOnError, out operand);
                if (operandError != null)
                {
                    return operandError;
                }
            }
            if (memberExpressionBody.Member is PropertyInfo)
            {
                if (operand == null)
                {
                    result = new PropertyValue<TOperand, TResult> { PropertyName = memberExpressionBody.Member.Name };
                }
                else
                {
                    result = new PropertyValue<TOperand, TResult> { Operand = operand, PropertyName = memberExpressionBody.Member.Name };
                }
                return null;
            }
            else if (memberExpressionBody.Member is FieldInfo)
            {
                if (operand == null)
                {
                    result = new FieldValue<TOperand, TResult> { FieldName = memberExpressionBody.Member.Name };
                }
                else
                {
                    result = new FieldValue<TOperand, TResult> { Operand = operand, FieldName = memberExpressionBody.Member.Name };
                }
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name)));
            }
            else
            {
                return SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name);
            }
        }

        static string TryConvertReferenceMemberExpression<TResult>(MemberExpression memberExpressionBody, Type operandType, bool throwOnError, out Activity<Location<TResult>> result)
        {
            try
            {
                MethodInfo specializedHandle = TryConvertReferenceMemberExpressionHandle.MakeGenericMethod(operandType, typeof(TResult));
                object[] parameters = new object[] { memberExpressionBody, throwOnError, null };
                string errorString = specializedHandle.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<Location<TResult>>;
                return errorString;
            }
            catch (TargetInvocationException e)
            {
                throw FxTrace.Exception.AsError(e.InnerException);
            }
        }

        static string TryConvertReferenceMemberExpressionWorker<TOperand, TResult>(MemberExpression memberExpressionBody, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            Activity<TOperand> operand = null;
            Activity<Location<TOperand>> operandReference = null;
            bool isValueType = typeof(TOperand).IsValueType;
            if (memberExpressionBody.Expression != null)
            {
                // Static property might not have any expressions.
                if (!isValueType)
                {
                    string operandError = TryConvert<TOperand>(memberExpressionBody.Expression, throwOnError, out operand);
                    if (operandError != null)
                    {
                        return operandError;
                    }
                }
                else
                {
                    string operandError = TryConvertReference<TOperand>(memberExpressionBody.Expression, throwOnError, out operandReference);
                    if (operandError != null)
                    {
                        return operandError;
                    }
                }
            }
            if (memberExpressionBody.Member is PropertyInfo)
            {
                if (!isValueType)
                {
                    if (operand == null)
                    {
                        result = new PropertyReference<TOperand, TResult> { PropertyName = memberExpressionBody.Member.Name };
                    }
                    else
                    {
                        result = new PropertyReference<TOperand, TResult> { Operand = operand, PropertyName = memberExpressionBody.Member.Name };
                    }
                }
                else
                {
                    if (operandReference == null)
                    {
                        result = new ValueTypePropertyReference<TOperand, TResult> { PropertyName = memberExpressionBody.Member.Name };
                    }
                    else
                    {
                        result = new ValueTypePropertyReference<TOperand, TResult> { OperandLocation = operandReference, PropertyName = memberExpressionBody.Member.Name };
                    }

                }
                return null;
            }
            if (memberExpressionBody.Member is FieldInfo)
            {
                if (!isValueType)
                {
                    if (operand == null)
                    {
                        result = new FieldReference<TOperand, TResult> { FieldName = memberExpressionBody.Member.Name };
                    }
                    else
                    {
                        result = new FieldReference<TOperand, TResult> { Operand = operand, FieldName = memberExpressionBody.Member.Name };
                    }
                }
                else
                {
                    if (operandReference == null)
                    {
                        result = new ValueTypeFieldReference<TOperand, TResult> { FieldName = memberExpressionBody.Member.Name };
                    }
                    else
                    {
                        result = new ValueTypeFieldReference<TOperand, TResult> { OperandLocation = operandReference, FieldName = memberExpressionBody.Member.Name };
                    }

                }
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name)));
            }
            else
            {
                return SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name);
            }
        }

        static string TryConvertOverloadingUnaryOperator<TOperand, TResult>(UnaryExpression unaryExpression, Activity<TOperand> operand, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if (!unaryExpression.Method.IsStatic)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.OverloadingMethodMustBeStatic));
                }
                else
                {
                    return SR.OverloadingMethodMustBeStatic;
                }
            }

            result = new InvokeMethod<TResult>
            {
                MethodName = unaryExpression.Method.Name,
                TargetType = unaryExpression.Method.DeclaringType,
                Parameters = { new InArgument<TOperand> { Expression = operand } },
            };
            return null;
        }

        static string TryConvertOverloadingBinaryOperator<TLeft, TRight, TResult>(BinaryExpression binaryExpression, Activity<TLeft> left, Activity<TRight> right, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if (!binaryExpression.Method.IsStatic)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.OverloadingMethodMustBeStatic));
                }
                else
                {
                    return SR.OverloadingMethodMustBeStatic;
                }
            }

            result = new InvokeMethod<TResult>
            {
                MethodName = binaryExpression.Method.Name,
                TargetType = binaryExpression.Method.DeclaringType,
                Parameters = { new InArgument<TLeft> { Expression = left, EvaluationOrder = 0 }, new InArgument<TRight> { Expression = right, EvaluationOrder = 1 } },
            };
            return null;
        }

        static string TryConvertMethodCallExpression<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            MethodInfo methodInfo = methodCallExpression.Method;

            if (methodInfo == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.MethodInfoRequired(methodCallExpression.GetType().Name)));
                }
                else
                {
                    return SR.MethodInfoRequired(methodCallExpression.GetType().Name);
                }
            };
            if (string.IsNullOrEmpty(methodInfo.Name) || methodInfo.DeclaringType == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.MethodNameRequired(methodInfo.GetType().Name)));
                }
                else
                {
                    return SR.MethodNameRequired(methodInfo.GetType().Name);
                }
            }
            InvokeMethod<TResult> invokeMethod = new InvokeMethod<TResult>
            {
                MethodName = methodInfo.Name,
            };

            ParameterInfo[] parameterInfoArray = methodInfo.GetParameters();
            if (methodCallExpression.Arguments.Count != parameterInfoArray.Length)//no optional argument call for LINQ expression
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.ArgumentNumberRequiresTheSameAsParameterNumber(methodCallExpression.GetType().Name)));
                }
                else
                {
                    return SR.ArgumentNumberRequiresTheSameAsParameterNumber(methodCallExpression.GetType().Name);
                }
            }

            string error = TryConvertArguments(methodCallExpression.Arguments, invokeMethod.Parameters, methodCallExpression.GetType(), 1, parameterInfoArray, throwOnError);
            if (error != null)
            {
                return error;
            }

            foreach (Type type in methodInfo.GetGenericArguments())
            {
                if (type == null)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR.InvalidGenericTypeInfo(methodCallExpression.GetType().Name)));
                    }
                    else
                    {
                        return SR.InvalidGenericTypeInfo(methodCallExpression.GetType().Name);
                    }
                }
                invokeMethod.GenericTypeArguments.Add(type);
            }
            if (methodInfo.IsStatic)
            {
                invokeMethod.TargetType = methodInfo.DeclaringType;
            }
            else
            {
                if (methodCallExpression.Object == null)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR.InstanceMethodCallRequiresTargetObject));
                    }
                    else
                    {
                        return SR.InstanceMethodCallRequiresTargetObject;
                    }
                }
                object[] parameters = new object[] { methodCallExpression.Object, false, throwOnError, null };
                error = TryConvertArgumentExpressionHandle.MakeGenericMethod(methodCallExpression.Object.Type).Invoke(null, parameters) as string;
                if (error != null)
                {
                    return error;
                }
                InArgument argument = (InArgument)parameters[3];
                argument.EvaluationOrder = 0;
                invokeMethod.TargetObject = argument;
            }
            result = invokeMethod;
            return null;
        }

        static string TryConvertInvocationExpression<TResult>(InvocationExpression invocationExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if (invocationExpression.Expression == null || invocationExpression.Expression.Type == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.InvalidExpressionProperty(invocationExpression.GetType().Name)));
                }
                else
                {
                    return SR.InvalidExpressionProperty(invocationExpression.GetType().Name);
                }
            }
            InvokeMethod<TResult> invokeMethod = new InvokeMethod<TResult>
            {
                MethodName = "Invoke",
            };
            object[] parameters = new object[] { invocationExpression.Expression, false, throwOnError, null };
            string error = TryConvertArgumentExpressionHandle.MakeGenericMethod(invocationExpression.Expression.Type).Invoke(null, parameters) as string;
            if (error != null)
            {
                return error;
            }
            InArgument argument = (InArgument)parameters[3];
            argument.EvaluationOrder = 0;
            invokeMethod.TargetObject = argument;

            //InvocationExpression can not have a by-ref parameter.
            error = TryConvertArguments(invocationExpression.Arguments, invokeMethod.Parameters, invocationExpression.GetType(), 1, null, throwOnError);

            if (error != null)
            {
                return error;
            }

            result = invokeMethod;
            return null;
        }

        static string TryConvertArgumentExpressionWorker<TArgument>(Expression expression, bool isByRef, bool throwOnError, out System.Activities.Argument result)
        {
            result = null;

            string error = null;

            if (isByRef)
            {
                Activity<Location<TArgument>> argument;
                error = TryConvertReference<TArgument>(expression, throwOnError, out argument);
                if (error == null)
                {
                    result = new InOutArgument<TArgument>
                   {
                       Expression = argument,
                   };
                }
            }
            else
            {
                Activity<TArgument> argument;
                error = TryConvert<TArgument>(expression, throwOnError, out argument);
                if (error == null)
                {
                    result = new InArgument<TArgument>
                    {
                        Expression = argument,
                    };
                }
            }
            return error;
        }

        static string TryConvertNewExpression<TResult>(NewExpression newExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            New<TResult> newActivity = new New<TResult>();
            ParameterInfo[] parameterInfoArray = null;
            if (newExpression.Constructor != null)
            {
                parameterInfoArray = newExpression.Constructor.GetParameters();
                if (newExpression.Arguments.Count != parameterInfoArray.Length)//no optional argument call for LINQ expression
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(SR.ArgumentNumberRequiresTheSameAsParameterNumber(newExpression.GetType().Name)));
                    }
                    else
                    {
                        return SR.ArgumentNumberRequiresTheSameAsParameterNumber(newExpression.GetType().Name);
                    }
                }
            }

            string error = TryConvertArguments(newExpression.Arguments, newActivity.Arguments, newExpression.GetType(), 0, parameterInfoArray, throwOnError);
            if (error != null)
            {
                return error;
            }
            result = newActivity;
            return null;
        }

        static string TryConvertNewArrayExpression<TResult>(NewArrayExpression newArrayExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            NewArray<TResult> newArrayActivity = new NewArray<TResult>();
            string error = TryConvertArguments(newArrayExpression.Expressions, newArrayActivity.Bounds, newArrayExpression.GetType(), 0, null, throwOnError);
            if (error != null)
            {
                return error;
            }
            result = newArrayActivity;
            return null;

        }

        static string TryConvertArguments(ReadOnlyCollection<Expression> source, IList target, Type expressionType, int baseEvaluationOrder, ParameterInfo[] parameterInfoArray, bool throwOnError)
        {
            object[] parameters;
            for (int i = 0; i < source.Count; i++)
            {
                bool isByRef = false;
                Expression expression = source[i];
                if (parameterInfoArray != null)
                {
                    ParameterInfo parameterInfo = parameterInfoArray[i];

                    if (parameterInfo == null || parameterInfo.ParameterType == null)
                    {
                        if (throwOnError)
                        {
                            throw FxTrace.Exception.AsError(new ValidationException(SR.InvalidParameterInfo(i, expressionType.Name)));
                        }
                        else
                        {
                            return SR.InvalidParameterInfo(i, expressionType.Name);
                        }
                    }
                    isByRef = parameterInfo.ParameterType.IsByRef;
                }
                parameters = new object[] { expression, isByRef, throwOnError, null };
                string error = TryConvertArgumentExpressionHandle.MakeGenericMethod(expression.Type).Invoke(null, parameters) as string;
                if (error != null)
                {
                    return error;
                }
                Argument argument = (Argument)parameters[3];
                argument.EvaluationOrder = i + baseEvaluationOrder;
                target.Add(argument);
            }
            return null;
        }

        static string TryConvertLocationReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;

            Expression expression = methodCallExpression.Arguments[0];
            if (expression.NodeType != ExpressionType.Constant)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(
                        SR.UnexpectedExpressionNodeType(ExpressionType.Constant.ToString(), expression.NodeType.ToString())));
                }
                else
                {
                    return SR.UnexpectedExpressionNodeType(ExpressionType.Constant.ToString(), expression.NodeType.ToString());
                }
            }

            object value = ((ConstantExpression)expression).Value;
            Type valueType = value.GetType();

            if (typeof(RuntimeArgument).IsAssignableFrom(valueType))
            {
                RuntimeArgument runtimeArgument = (RuntimeArgument)value;
                result = new ArgumentValue<TResult>
                {
                    ArgumentName = runtimeArgument.Name,
                };
            }
            else if (typeof(Variable).IsAssignableFrom(valueType))
            {
                Variable variable = (Variable)value;
                result = new VariableValue<TResult> { Variable = variable };
            }
            else if (typeof(DelegateArgument).IsAssignableFrom(valueType))
            {
                DelegateArgument delegateArgument = (DelegateArgument)value;
                result = new DelegateArgumentValue<TResult>
                {
                    DelegateArgument = delegateArgument
                };
            }

            if (result == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedLocationReferenceValue));
                }
                else
                {
                    return SR.UnsupportedLocationReferenceValue;
                }
            }

            return null;
        }

        static string TryConvertReferenceLocationReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;

            Expression expression = methodCallExpression.Arguments[0];
            if (expression.NodeType != ExpressionType.Constant)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(
                        SR.UnexpectedExpressionNodeType(ExpressionType.Constant.ToString(), expression.NodeType.ToString())));
                }
                else
                {
                    return SR.UnexpectedExpressionNodeType(ExpressionType.Constant.ToString(), expression.NodeType.ToString());
                }
            }

            object value = ((ConstantExpression)expression).Value;
            Type valueType = value.GetType();

            if (typeof(RuntimeArgument).IsAssignableFrom(valueType))
            {
                RuntimeArgument runtimeArgument = (RuntimeArgument)value;
                result = new ArgumentReference<TResult>
                {
                    ArgumentName = runtimeArgument.Name,
                };
            }
            else if (typeof(Variable).IsAssignableFrom(valueType))
            {
                Variable variable = (Variable)value;
                result = new VariableReference<TResult> { Variable = variable };
            }
            else if (typeof(DelegateArgument).IsAssignableFrom(valueType))
            {
                DelegateArgument delegateArgument = (DelegateArgument)value;
                result = new DelegateArgumentReference<TResult>
                {
                    DelegateArgument = delegateArgument
                };
            }
            
            if (result == null && throwOnError)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.UnsupportedLocationReferenceValue));
                }
                else
                {
                    return SR.UnsupportedLocationReferenceValue;
                }
            }

            return null;
        }
    }
}
