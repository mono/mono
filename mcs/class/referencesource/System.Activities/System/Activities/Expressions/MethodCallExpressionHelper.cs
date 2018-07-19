//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Threading;

    static class MethodCallExpressionHelper
    {
        public const int FuncCacheCapacity = 500;

        static void PrepareForVariables(MethodBase methodInfo, ParameterExpression objectArray, Collection<ParameterExpression> variables,
            Collection<Expression> assignVariablesExpressions, Collection<Expression> assignVariablesBackExpressions)
        {
            if (methodInfo != null)
            {
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    Type parameterType = parameterInfos[i].ParameterType;
                    ParameterExpression variable = Expression.Parameter(parameterType.IsByRef ? parameterType.GetElementType() : parameterType, "arg" + i);
                    // If variable.Type is NOT a Nullable<T>, we include the call to Convert.ChangeType on the actual parameter.
                    if (variable.Type.IsValueType && Nullable.GetUnderlyingType(variable.Type) == null)
                    {
                        assignVariablesExpressions.Add(Expression.Assign(variable,
                             Expression.Convert(
                                Expression.Call(typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), Expression.ArrayIndex(objectArray, Expression.Constant(i)), Expression.Constant(variable.Type, typeof(Type))),
                                variable.Type)));
                    }
                    else
                    {
                        assignVariablesExpressions.Add(Expression.Assign(variable,
                                Expression.Convert(Expression.ArrayIndex(objectArray, Expression.Constant(i)), variable.Type)));
                    }
                    if (parameterType.IsByRef)
                    {
                        if (variable.Type.IsValueType)
                        {
                            assignVariablesBackExpressions.Add(Expression.Assign(Expression.ArrayAccess(objectArray, Expression.Constant(i)),
                                Expression.Convert(variable, typeof(object))));
                        }
                        else
                        {
                            assignVariablesBackExpressions.Add(Expression.Assign(Expression.ArrayAccess(objectArray, Expression.Constant(i)),
                                variable));
                        }
                    }
                    variables.Add(variable);
                }
            }
        }

        static MethodCallExpression PrepareForCallExpression(MethodInfo methodInfo, ParameterExpression targetInstance, Collection<ParameterExpression> variables)
        {
            MethodCallExpression callExpression;
            if (!methodInfo.IsStatic)
            {
                callExpression = Expression.Call(Expression.Convert(targetInstance, methodInfo.DeclaringType), methodInfo, variables);
            }
            else
            {
                callExpression = Expression.Call(methodInfo, variables);
            }
            return callExpression;
        }

        static MethodCallExpression PrepareForCallExpression(MethodInfo methodInfo, ParameterExpression targetInstance,
            Collection<ParameterExpression> variables, out ParameterExpression tempInstance, out Expression assignTempInstanceExpression)
        {
            MethodCallExpression callExpression;
            tempInstance = null;
            assignTempInstanceExpression = null;
            if (!methodInfo.IsStatic)
            {
                if (methodInfo.DeclaringType.IsValueType)
                {
                    tempInstance = Expression.Parameter(methodInfo.DeclaringType, "tempInstance");
                    assignTempInstanceExpression = Expression.Assign(tempInstance, Expression.Convert(targetInstance, methodInfo.DeclaringType));
                    callExpression = Expression.Call(tempInstance,
                        methodInfo, variables);
                }
                else
                {
                    callExpression = Expression.Call(Expression.Convert(targetInstance, methodInfo.DeclaringType), methodInfo, variables);
                }
            }
            else
            {
                callExpression = Expression.Call(methodInfo, variables);
            }
            return callExpression;
        }

        static Expression ComposeBlockExpression(Collection<ParameterExpression> variables, Collection<Expression> assignVariables, Expression callExpression,
            Collection<Expression> assignVariablesBack, Type returnType, bool isConstructor, bool valueTypeReference)
        {
            Collection<Expression> expressions = new Collection<Expression>();
            foreach (Expression expression in assignVariables)
            {
                expressions.Add(expression);
            }

            ParameterExpression result = Expression.Parameter(isConstructor ? returnType : typeof(object), "result");
            variables.Add(result);
            if (returnType != typeof(void))
            {
                Expression resultAssign = null;
                if (!isConstructor && returnType.IsValueType)
                {
                    resultAssign = Expression.Assign(result,
                        Expression.Convert(callExpression, typeof(object)));
                }
                else
                {
                    resultAssign = Expression.Assign(result, callExpression);
                }
                expressions.Add(resultAssign);
            }
            else
            {
                expressions.Add(callExpression);
            }
            foreach (Expression expression in assignVariablesBack)
            {
                expressions.Add(expression);
            }

            if (!valueTypeReference)
            {
                expressions.Add(result);
            }

            Expression block = Expression.Block(variables, expressions);
            return block;
        }

        static Expression ComposeLinqExpression(MethodInfo methodInfo, ParameterExpression targetInstance, ParameterExpression objectArray, Type returnType, bool valueTypeReference)
        {
            Collection<Expression> assignVariablesExpressions = new Collection<Expression>();
            Collection<Expression> assignVariablesBackExpressions = new Collection<Expression>();
            Collection<ParameterExpression> variables = new Collection<ParameterExpression>();

            PrepareForVariables(methodInfo, objectArray, variables, assignVariablesExpressions, assignVariablesBackExpressions);

            ParameterExpression tempInstance = null;
            Expression assignTempInstanceExpression = null;
            Expression expression;

            if (!methodInfo.IsStatic && methodInfo.DeclaringType.IsValueType && valueTypeReference)
            {
                expression = PrepareForCallExpression((MethodInfo)methodInfo, targetInstance, variables, out tempInstance, out assignTempInstanceExpression);
                variables.Add(tempInstance);
                assignVariablesExpressions.Add(assignTempInstanceExpression);
                assignVariablesBackExpressions.Add(Expression.Assign(targetInstance, Expression.Convert(tempInstance, typeof(object))));
                return ComposeBlockExpression(variables, assignVariablesExpressions, expression, assignVariablesBackExpressions, returnType, false, true);
            }
            else
            {
                expression = PrepareForCallExpression((MethodInfo)methodInfo, targetInstance, variables);
                return ComposeBlockExpression(variables, assignVariablesExpressions, expression, assignVariablesBackExpressions, returnType, false, false);
            }


        }

        static Expression ComposeLinqExpression<TResult>(ConstructorInfo constructorInfo, ParameterExpression objectArray)
        {
            Collection<Expression> assignVariablesExpressions = new Collection<Expression>();
            Collection<Expression> assignVariablesBackExpressions = new Collection<Expression>();
            Collection<ParameterExpression> variables = new Collection<ParameterExpression>();

            PrepareForVariables(constructorInfo, objectArray, variables, assignVariablesExpressions, assignVariablesBackExpressions);

            NewExpression newExpression;
            if (constructorInfo != null)
            {
                newExpression = Expression.New(constructorInfo, variables);
            }
            else
            {
                newExpression = Expression.New(typeof(TResult));
            }
            return ComposeBlockExpression(variables, assignVariablesExpressions, newExpression, assignVariablesBackExpressions, typeof(TResult), true, false);
        }

        static Func<object, object[], object> GetFunc(CodeActivityMetadata metadata, MethodInfo methodInfo, bool valueTypeReference = false)
        {
            try
            {
                ParameterExpression targetInstance = Expression.Parameter(typeof(object), "targetInstance");
                ParameterExpression objectArray = Expression.Parameter(typeof(object[]), "arguments");
                Expression block = ComposeLinqExpression(methodInfo, targetInstance, objectArray, methodInfo.ReturnType, valueTypeReference);
                Expression<Func<object, object[], object>> lambdaExpression = Expression.Lambda<Func<object, object[], object>>(block, targetInstance, objectArray);
                return lambdaExpression.Compile();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                metadata.AddValidationError(e.Message);
                return null;
            }

        }

        static Func<object[], TResult> GetFunc<TResult>(CodeActivityMetadata metadata, ConstructorInfo constructorInfo)
        {
            try
            {
                ParameterExpression objectArray = Expression.Parameter(typeof(object[]), "arguments");
                Expression block = ComposeLinqExpression<TResult>(constructorInfo, objectArray);
                Expression<Func<object[], TResult>> lambdaExpression = Expression.Lambda<Func<object[], TResult>>(block, objectArray);
                return lambdaExpression.Compile();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                metadata.AddValidationError(e.Message);
                return null;
            }

        }

        internal static bool NeedRetrieve(MethodBase newMethod, MethodBase oldMethod, Delegate func)
        {
            if (newMethod == null)
            {
                return false;
            }
            if ((newMethod == oldMethod) && (func != null))
            {
                return false;
            }
            return true;
        }

        internal static Func<object, object[], object> GetFunc(CodeActivityMetadata metadata, MethodInfo methodInfo, 
            MruCache<MethodInfo, Func<object, object[], object>> cache, ReaderWriterLockSlim locker, bool valueTypeReference = false)
        {
            Func<object, object[], object> func = null;
            locker.EnterWriteLock();
            try
            {
                cache.TryGetValue(methodInfo, out func);
            }
            finally
            {
                locker.ExitWriteLock();
            }
            if (func == null)
            {
                func = GetFunc(metadata, methodInfo, valueTypeReference);
                locker.EnterWriteLock();
                try
                {
                    //MruCache has on ContainsKey(), so we use TryGetValue()
                    Func<object, object[], object> result = null;
                    if (!cache.TryGetValue(methodInfo, out result))
                    {
                        cache.Add(methodInfo, func);
                    }
                    else
                    {
                        func = result;
                    }
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            return func;
        }

        internal static Func<object[], TResult> GetFunc<TResult>(CodeActivityMetadata metadata, ConstructorInfo constructorInfo, 
            MruCache<ConstructorInfo, Func<object[], TResult>> cache, ReaderWriterLockSlim locker)
        {
            Func<object[], TResult> func = null;
            if (constructorInfo != null)
            {
                locker.EnterWriteLock();
                try
                {
                    cache.TryGetValue(constructorInfo, out func);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            if (func == null)
            {
                func = GetFunc<TResult>(metadata, constructorInfo);
                if (constructorInfo != null)
                {
                    locker.EnterWriteLock();
                    try
                    {
                        //MruCache has on ContainsKey(), so we use TryGetValue()
                        Func<object[], TResult> result = null;
                        if (!cache.TryGetValue(constructorInfo, out result))
                        {
                            cache.Add(constructorInfo, func);
                        }
                        else
                        {
                            func = result;
                        }
                    }
                    finally
                    {
                        locker.ExitWriteLock();
                    }
                }
            }
            return func;
        }
    }
}
