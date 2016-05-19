//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime;
    using System.Linq.Expressions;

    // Inverted Template Method pattern. MethodExecutor is the base class for executing a method; created by MethodResolver.
    // Private concrete implementations are created by MethodResolver, but this is the "public" API used by InvokeMethod.
    abstract class MethodExecutor
    {
        // Used for creating tracing messages w/ DisplayName
        protected Activity invokingActivity;

        // We may still need to know targetType if we're autocreating targets during ExecuteMethod
        Type targetType;
        InArgument targetObject;
        Collection<Argument> parameters;
        RuntimeArgument returnObject;

        public MethodExecutor(Activity invokingActivity, Type targetType, InArgument targetObject,
            Collection<Argument> parameters, RuntimeArgument returnObject)
        {
            Fx.Assert(invokingActivity != null, "Must provide invokingActivity");
            Fx.Assert(targetType != null || (targetObject != null), "Must provide targetType or targetObject");
            Fx.Assert(parameters != null, "Must provide parameters");
            // returnObject is optional 

            this.invokingActivity = invokingActivity;
            this.targetType = targetType;
            this.targetObject = targetObject;
            this.parameters = parameters;
            this.returnObject = returnObject;
        }

        public abstract bool MethodIsStatic { get; }

        protected abstract IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state);
        protected abstract void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result);

        static bool HaveParameterArray(ParameterInfo[] parameters)
        {
            if (parameters.Length > 0)
            {
                ParameterInfo last = parameters[parameters.Length - 1];
                return last.GetCustomAttributes(typeof(ParamArrayAttribute), true).GetLength(0) > 0;
            }
            else
            {
                return false;
            }
        }

        protected object[] EvaluateAndPackParameters(CodeActivityContext context, MethodInfo method,
            bool usingAsyncPattern)
        {
            ParameterInfo[] formalParameters = method.GetParameters();
            int formalParamCount = formalParameters.Length;
            object[] actualParameters = new object[formalParamCount];

            if (usingAsyncPattern)
            {
                formalParamCount -= 2;
            }

            bool haveParameterArray = HaveParameterArray(formalParameters);
            for (int i = 0; i < formalParamCount; i++)
            {
                if (i == formalParamCount - 1 && !usingAsyncPattern && haveParameterArray)
                {
                    int paramArrayCount = this.parameters.Count - formalParamCount + 1;

                    // If params are given explicitly, that's okay.
                    if (paramArrayCount == 1 && TypeHelper.AreTypesCompatible(this.parameters[i].ArgumentType,
                        formalParameters[i].ParameterType))
                    {
                        actualParameters[i] = this.parameters[i].Get<object>(context);
                    }
                    else
                    {
                        // Otherwise, pack them into an array for the reflection call.
                        actualParameters[i] =
                            Activator.CreateInstance(formalParameters[i].ParameterType, paramArrayCount);
                        for (int j = 0; j < paramArrayCount; j++)
                        {
                            ((object[])actualParameters[i])[j] = this.parameters[i + j].Get<object>(context);
                        }
                    }
                    continue;
                }
                actualParameters[i] = parameters[i].Get<object>(context);
            }

            return actualParameters;
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.InstantiateArgumentExceptionsCorrectly, Justification = "TargetObject is a parameter to InvokeMethod, rather than this specific method.")]
        public IAsyncResult BeginExecuteMethod(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            object targetInstance = null;

            if (!this.MethodIsStatic)
            {
                targetInstance = this.targetObject.Get(context);
                if (targetInstance == null)
                {
                    throw FxTrace.Exception.ArgumentNull("TargetObject");
                }
            }

            return BeginMakeMethodCall(context, targetInstance, callback, state); // defer to concrete instance for [....]/async variations
        }

        public void EndExecuteMethod(AsyncCodeActivityContext context, IAsyncResult result)
        {
            EndMakeMethodCall(context, result); // defer to concrete instance for [....]/async variations
        }

        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "We need throw out all exceptions from method invocation.")]
        internal object InvokeAndUnwrapExceptions(Func<object, object[], object> func, object targetInstance, object[] actualParameters)
        {
            try
            {
                return func(targetInstance, actualParameters);
            }
            catch (Exception e)
            {
                if (TD.InvokedMethodThrewExceptionIsEnabled())
                {
                    TD.InvokedMethodThrewException(this.invokingActivity.DisplayName, e.ToString());
                }
                throw FxTrace.Exception.AsError(e);
            }
        }

        public void SetOutArgumentAndReturnValue(ActivityContext context, object state, object[] actualParameters)
        {
            for (int index = 0; index < parameters.Count; index++)
            {
                if (parameters[index].Direction != ArgumentDirection.In)
                {
                    parameters[index].Set(context, actualParameters[index]);
                }
            }

            if (this.returnObject != null)
            {
                this.returnObject.Set(context, state);
            }
        }

        public void Trace(Activity parent)
        {
            if (this.MethodIsStatic)
            {
                if (TD.InvokeMethodIsStaticIsEnabled())
                {
                    TD.InvokeMethodIsStatic(parent.DisplayName);
                }
            }
            else
            {
                if (TD.InvokeMethodIsNotStaticIsEnabled())
                {
                    TD.InvokeMethodIsNotStatic(parent.DisplayName);
                }
            }
        }


    }
}
