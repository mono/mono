//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Activities.Expressions;
    using System.Threading;

    // Helper class for InvokeMethod.
    // Factory for MethodExecutor strategies. Conceptually, resolves to the correct MethodInfo based on target type,
    // method name, parameters, and async flags + availability of Begin/End paired methods of the correct static-ness.
    sealed class MethodResolver
    {

        static readonly BindingFlags staticBindingFlags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static;
        static readonly BindingFlags instanceBindingFlags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
        static readonly string staticString = "static";     // Used in error messages below. Technical term, not localizable.
        static readonly string instanceString = "instance"; // Used in error messages below. Technical term, not localizable.
        MethodInfo syncMethod;
        MethodInfo beginMethod;
        MethodInfo endMethod;

        public MethodResolver()
        {
        }

        public Collection<Type> GenericTypeArguments { get; set; }

        public string MethodName { get; set; }

        public Collection<Argument> Parameters { get; set; }

        public RuntimeArgument Result { get; set; }

        public InArgument TargetObject { get; set; }

        public Type TargetType { get; set; }

        public bool RunAsynchronously { get; set; }

        public Activity Parent { get; set; }

        // Sometimes we may know the result type even if it won't be used,
        // i.e. it comes from an InvokeMethod<T>. We will want to generate
        // errors if it doesn't match the method's return value. 
        internal Type ResultType { get; set; }

        static bool HaveParameterArray(ParameterInfo[] parameters)
        {
            if (parameters.Length > 0)
            {
                ParameterInfo last = parameters[parameters.Length - 1];
                return last.GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0;
            }
            else
            {
                return false;
            }
        }

        // The Arguments added by the activity are named according to the method resolved by the MethodResolver.
        public void RegisterParameters(IList<RuntimeArgument> arguments)
        {
            bool useAsyncPattern = this.RunAsynchronously && this.beginMethod != null && this.endMethod != null;

            if (this.syncMethod != null || useAsyncPattern)
            {
                ParameterInfo[] formalParameters;
                int formalParamCount;
                string paramArrayBaseName = "";
                bool haveParameterArray = false;

                if (useAsyncPattern)
                {
                    formalParameters = this.beginMethod.GetParameters();
                    formalParamCount = formalParameters.Length - 2;
                }
                else
                {
                    formalParameters = this.syncMethod.GetParameters();
                    haveParameterArray = HaveParameterArray(formalParameters);

                    if (haveParameterArray)
                    {
                        formalParamCount = formalParameters.Length - 1;
                        paramArrayBaseName = formalParameters[formalParamCount].Name;
                    }
                    else
                    {
                        formalParamCount = formalParameters.Length;
                    }
                }

                for (int i = 0; i < formalParamCount; i++)
                {
                    string name = formalParameters[i].Name;
                    //for some methods like int[,].Get(int,int), formal parameters have no names in reflection info
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "Parameter" + i;
                    }

                    RuntimeArgument argument = new RuntimeArgument(name, Parameters[i].ArgumentType, Parameters[i].Direction, true);
                    Argument.Bind(Parameters[i], argument);
                    arguments.Add(argument);

                    if (!useAsyncPattern && haveParameterArray)
                    {
                        // Attempt to uniquify parameter names
                        if (name.StartsWith(paramArrayBaseName, false, null))
                        {
                            int n;
                            if (int.TryParse(name.Substring(paramArrayBaseName.Length), NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out n))
                            {
                                paramArrayBaseName += "_";
                            }
                        }
                    }
                }

                if (!useAsyncPattern && haveParameterArray)
                {
                    // RuntimeArgument bindings need names. In the case of params arrays, synthesize names based on the name of the formal params parameter
                    // plus a counter.
                    int paramArrayCount = Parameters.Count - formalParamCount;

                    for (int i = 0; i < paramArrayCount; i++)
                    {
                        string name = paramArrayBaseName + i;
                        int index = formalParamCount + i;
                        RuntimeArgument argument = new RuntimeArgument(name, Parameters[index].ArgumentType, Parameters[index].Direction, true);
                        Argument.Bind(Parameters[index], argument);
                        arguments.Add(argument);
                    }
                }
            }
            else
            {
                // We're still at design-time: make up "fake" arguments based on the parameters
                for (int i = 0; i < Parameters.Count; i++)
                {
                    string name = "argument" + i;
                    RuntimeArgument argument = new RuntimeArgument(name, Parameters[i].ArgumentType, Parameters[i].Direction, true);
                    Argument.Bind(Parameters[i], argument);
                    arguments.Add(argument);
                }
            }
        }

        public void Trace()
        {
            bool useAsyncPattern = this.RunAsynchronously && this.beginMethod != null && this.endMethod != null;

            if (useAsyncPattern)
            {
                if (TD.InvokeMethodUseAsyncPatternIsEnabled())
                {
                    TD.InvokeMethodUseAsyncPattern(this.Parent.DisplayName, this.beginMethod.ToString(), this.endMethod.ToString());
                }
            }
            else
            {
                if (this.RunAsynchronously)
                {
                    if (TD.InvokeMethodDoesNotUseAsyncPatternIsEnabled())
                    {
                        TD.InvokeMethodDoesNotUseAsyncPattern(this.Parent.DisplayName);
                    }
                }
            }
        }

        // Set methodExecutor, returning an error string if there are any problems (ambiguous match, etc.).
        public void DetermineMethodInfo(CodeActivityMetadata metadata, MruCache<MethodInfo, Func<object, object[], object>> funcCache, ReaderWriterLockSlim locker, 
            ref MethodExecutor methodExecutor)
        {
            bool returnEarly = false;

            MethodExecutor oldMethodExecutor = methodExecutor;
            methodExecutor = null;
            if (string.IsNullOrEmpty(this.MethodName))
            {
                metadata.AddValidationError(SR.ActivityPropertyMustBeSet("MethodName", this.Parent.DisplayName));
                returnEarly = true;
            }

            Type targetType = this.TargetType;

            // If TargetType and the type of TargetObject are both set, it's an error.
            if (targetType != null && this.TargetObject != null && !this.TargetObject.IsEmpty)
            {
                metadata.AddValidationError(SR.TargetTypeAndTargetObjectAreMutuallyExclusive(this.Parent.GetType().Name, this.Parent.DisplayName));
                returnEarly = true;
            }

            // If TargetType was set, look for a static method. If TargetObject was set, look for an instance method. They can't both be set.
            BindingFlags bindingFlags = this.TargetType != null ? staticBindingFlags : instanceBindingFlags;
            string bindingType = bindingFlags == staticBindingFlags ? staticString : instanceString;

            if (targetType == null)
            {
                if (this.TargetObject != null && !this.TargetObject.IsEmpty)
                {
                    targetType = this.TargetObject.ArgumentType;
                }
                else
                {
                    metadata.AddValidationError(SR.OneOfTwoPropertiesMustBeSet("TargetObject", "TargetType", this.Parent.GetType().Name, this.Parent.DisplayName));
                    returnEarly = true;
                }
            }

            // We've had one or more constraint violations already
            if (returnEarly)
            {
                return;
            }

            // Convert OutArgs and InOutArgs to out/ref types before resolution
            Type[] parameterTypes =
                Parameters.Select(argument => argument.Direction == ArgumentDirection.In ? argument.ArgumentType : argument.ArgumentType.MakeByRefType())
                    .ToArray();

            Type[] genericTypeArguments = this.GenericTypeArguments.ToArray();

            InheritanceAndParamArrayAwareBinder methodBinder = new InheritanceAndParamArrayAwareBinder(targetType, genericTypeArguments, this.Parent);

            // It may be possible to know (and check) the resultType even if the result won't be assigned anywhere.     
            // Used 1.) for detecting async pattern, and 2.) to make sure we selected the correct MethodInfo.
            Type resultType = this.ResultType;

            if (this.RunAsynchronously)
            {
                int formalParamCount = parameterTypes.Length;
                Type[] beginMethodParameterTypes = new Type[formalParamCount + 2];
                for (int i = 0; i < formalParamCount; i++)
                {
                    beginMethodParameterTypes[i] = parameterTypes[i];
                }
                beginMethodParameterTypes[formalParamCount] = typeof(AsyncCallback);
                beginMethodParameterTypes[formalParamCount + 1] = typeof(object);

                Type[] endMethodParameterTypes = { typeof(IAsyncResult) };

                this.beginMethod = Resolve(targetType, "Begin" + this.MethodName, bindingFlags,
                    methodBinder, beginMethodParameterTypes, genericTypeArguments, true);
                if (this.beginMethod != null && !this.beginMethod.ReturnType.Equals(typeof(IAsyncResult)))
                {
                    this.beginMethod = null;
                }
                this.endMethod = Resolve(targetType, "End" + this.MethodName, bindingFlags,
                    methodBinder, endMethodParameterTypes, genericTypeArguments, true);
                if (this.endMethod != null && resultType != null && !TypeHelper.AreTypesCompatible(this.endMethod.ReturnType, resultType))
                {
                    metadata.AddValidationError(SR.ReturnTypeIncompatible(this.endMethod.ReturnType.Name, MethodName, targetType.Name, this.Parent.DisplayName, resultType.Name));
                    this.endMethod = null;
                    return;
                }

                if (this.beginMethod != null && this.endMethod != null && this.beginMethod.IsStatic == this.endMethod.IsStatic)
                {
                    if (!(oldMethodExecutor is AsyncPatternMethodExecutor) ||
                        !((AsyncPatternMethodExecutor)oldMethodExecutor).IsTheSame(this.beginMethod, this.endMethod))
                    {
                        methodExecutor = new AsyncPatternMethodExecutor(metadata, this.beginMethod, this.endMethod, this.Parent, 
                            this.TargetType, this.TargetObject, this.Parameters, this.Result, funcCache, locker);
                    }
                    else
                    {
                        methodExecutor = new AsyncPatternMethodExecutor((AsyncPatternMethodExecutor)oldMethodExecutor, 
                            this.TargetType, this.TargetObject, this.Parameters, this.Result);
                    }
                    return;
                }
            }

            MethodInfo result;
            try
            {
                result = Resolve(targetType, this.MethodName, bindingFlags,
                    methodBinder, parameterTypes, genericTypeArguments, false);
            }
            catch (AmbiguousMatchException)
            {
                metadata.AddValidationError(SR.DuplicateMethodFound(targetType.Name, bindingType, MethodName, this.Parent.DisplayName));
                return;
            }

            if (result == null)
            {
                metadata.AddValidationError(SR.PublicMethodWithMatchingParameterDoesNotExist(targetType.Name, bindingType, MethodName, this.Parent.DisplayName));
                return;
            }
            else if (resultType != null && !TypeHelper.AreTypesCompatible(result.ReturnType, resultType))
            {
                metadata.AddValidationError(
                    SR.ReturnTypeIncompatible(result.ReturnType.Name, MethodName,
                        targetType.Name, this.Parent.DisplayName, resultType.Name));
                return;
            }
            else
            {
                this.syncMethod = result;
                if (this.RunAsynchronously)
                {
                    if (!(oldMethodExecutor is AsyncWaitCallbackMethodExecutor) ||
                        !((AsyncWaitCallbackMethodExecutor)oldMethodExecutor).IsTheSame(this.syncMethod))
                    {
                        methodExecutor = new AsyncWaitCallbackMethodExecutor(metadata, this.syncMethod, this.Parent, 
                            this.TargetType, this.TargetObject, this.Parameters, this.Result, funcCache, locker);
                    }
                    else
                    {
                        methodExecutor = new AsyncWaitCallbackMethodExecutor((AsyncWaitCallbackMethodExecutor)oldMethodExecutor,
                            this.TargetType, this.TargetObject, this.Parameters, this.Result);
                    }

                }
                else if (!(oldMethodExecutor is SyncMethodExecutor) ||
                    !((SyncMethodExecutor)oldMethodExecutor).IsTheSame(this.syncMethod))
                {
                    methodExecutor = new SyncMethodExecutor(metadata, this.syncMethod, this.Parent, this.TargetType,
                        this.TargetObject, this.Parameters, this.Result, funcCache, locker);
                }
                else
                {
                    methodExecutor = new SyncMethodExecutor((SyncMethodExecutor)oldMethodExecutor, this.TargetType,
                        this.TargetObject, this.Parameters, this.Result);
                }

            }
        }

        // returns null MethodInfo on failure
        MethodInfo Resolve(Type targetType, string methodName, BindingFlags bindingFlags,
            InheritanceAndParamArrayAwareBinder methodBinder, Type[] parameterTypes, Type[] genericTypeArguments, bool suppressAmbiguityException)
        {
            MethodInfo method;
            try
            {
                methodBinder.SelectMethodCalled = false;
                method = targetType.GetMethod(methodName, bindingFlags,
                    methodBinder, CallingConventions.Any, parameterTypes, null);
            }
            catch (AmbiguousMatchException)
            {
                if (suppressAmbiguityException) // For Begin/End methods, ambiguity just means no match
                {
                    return null;
                }
                else // For a regular sync method, ambiguity is distinct from no match and gets an explicit error message
                {
                    throw;
                }
            }

            if (method != null && !methodBinder.SelectMethodCalled && genericTypeArguments.Length > 0)
            // methodBinder is only used when there's more than one possible match, so method might still be generic
            {
                method = Instantiate(method, genericTypeArguments); // if it fails because of e.g. constraints it will just become null
            }
            return method;
        }

        // returns null on failure instead of throwing an exception (okay because it's an internal method)
        static MethodInfo Instantiate(MethodInfo method, Type[] genericTypeArguments)
        {
            if (method.ContainsGenericParameters && method.GetGenericArguments().Length == genericTypeArguments.Length)
            {
                try
                {
                    // Must be a MethodInfo because we've already filtered out constructors                            
                    return ((MethodInfo)method).MakeGenericMethod(genericTypeArguments);
                }
                catch (ArgumentException)
                {
                    // Constraint violations will throw this exception--don't add to candidates
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        // Store information about a particular asynchronous method call so we can update out/ref parameters, know 
        // when/what to return, etc.
        class InvokeMethodInstanceData
        {
            public object TargetObject { get; set; }
            public object[] ActualParameters { get; set; }
            public object ReturnValue { get; set; }
            public bool ExceptionWasThrown { get; set; }
            public Exception Exception { get; set; }
        }

        class InheritanceAndParamArrayAwareBinder : Binder
        {
            Type[] genericTypeArguments;

            Type declaringType; // Methods declared directly on this type are preferred, followed by methods on its parents, etc.

            internal bool SelectMethodCalled; // If this binder is actually used in resolution, it gets to do things like instantiate methods.
            // Set this flag to false before calling Type.GetMethod. Check this flag after.

            Activity parentActivity; // Used for generating AmbiguousMatchException error message

            public InheritanceAndParamArrayAwareBinder(Type declaringType, Type[] genericTypeArguments, Activity parentActivity)
            {
                this.declaringType = declaringType;
                this.genericTypeArguments = genericTypeArguments;
                this.parentActivity = parentActivity;
            }

            public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override object ChangeType(object value, Type type, CultureInfo culture)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override void ReorderArgumentArray(ref object[] args, object state)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
            {
                MethodBase[] methodCandidates;
                this.SelectMethodCalled = true;

                if (this.genericTypeArguments.Length > 0)
                {
                    // Accept only generic methods which can be successfully instantiated w/ these parameters
                    Collection<MethodBase> methods = new Collection<MethodBase>();
                    foreach (MethodBase method in match)
                    {
                        // Must be a MethodInfo because we've already filtered out constructors                            
                        MethodInfo instantiatedMethod = Instantiate((MethodInfo)method, this.genericTypeArguments);
                        if (instantiatedMethod != null)
                        {
                            methods.Add(instantiatedMethod);
                        }
                    }
                    methodCandidates = methods.ToArray();
                }
                else
                {
                    // Accept only candidates which are already instantiated
                    methodCandidates = match.Where(m => m.ContainsGenericParameters == false).ToArray();
                }

                if (methodCandidates.Length == 0)
                {
                    return null;
                }

                // Methods declared on this.declaringType class get top priority as matches
                Type declaringType = this.declaringType;
                MethodBase result = null;
                do
                {
                    MethodBase[] methodsDeclaredHere = methodCandidates.Where(mb => mb.DeclaringType == declaringType).ToArray();
                    if (methodsDeclaredHere.Length > 0)
                    {
                        // Try to find a match
                        result = FindMatch(methodsDeclaredHere, bindingAttr, types, modifiers);
                    }
                    declaringType = declaringType.BaseType;
                }
                while (declaringType != null && result == null); // short-circuit as soon as we find a match

                return result; // returns null if no match found                
            }

            MethodBase FindMatch(MethodBase[] methodCandidates, BindingFlags bindingAttr, Type[] types, ParameterModifier[] modifiers)
            {
                // Try the default binder first. Never gives false positive, but will fail to detect methods w/ parameter array because
                // it will not expand the formal parameter list when checking against actual parameters.
                MethodBase result = Type.DefaultBinder.SelectMethod(bindingAttr, methodCandidates, types, modifiers);

                // Could be false negative, check for parameter array and if so condense it back to an array before re-checking.
                if (result == null)
                {
                    foreach (MethodBase method in methodCandidates)
                    {
                        MethodInfo methodInfo = method as MethodInfo;
                        ParameterInfo[] formalParams = methodInfo.GetParameters();
                        if (MethodResolver.HaveParameterArray(formalParams)) // Check if the last parameter of method is marked w/ "params" attribute
                        {
                            Type elementType = formalParams[formalParams.Length - 1].ParameterType.GetElementType();

                            bool allCompatible = true;
                            // There could be more actual parameters than formal parameters, because the formal parameter is a params T'[] for some T'.
                            // So, check that each actual parameter starting at position [formalParams.Length - 1] is compatible with T'.
                            for (int i = formalParams.Length - 1; i < types.Length - 1; i++)
                            {
                                if (!TypeHelper.AreTypesCompatible(types[i], elementType))
                                {
                                    allCompatible = false;
                                    break;
                                }
                            }

                            if (!allCompatible)
                            {
                                continue;
                            }

                            // Condense the actual parameter back to an array.
                            Type[] typeArray = new Type[formalParams.Length];
                            for (int i = 0; i < typeArray.Length - 1; i++)
                            {
                                typeArray[i] = types[i];
                            }
                            typeArray[typeArray.Length - 1] = elementType.MakeArrayType();

                            // Recheck the condensed array
                            MethodBase newFound = Type.DefaultBinder.SelectMethod(bindingAttr, new MethodBase[] { methodInfo }, typeArray, modifiers);
                            if (result != null && newFound != null)
                            {
                                string type = newFound.ReflectedType.Name;
                                string name = newFound.Name;
                                string bindingType = bindingAttr == staticBindingFlags ? staticString : instanceString;
                                throw FxTrace.Exception.AsError(new AmbiguousMatchException(SR.DuplicateMethodFound(type, bindingType, name, this.parentActivity.DisplayName)));
                            }
                            else
                            {
                                result = newFound;
                            }
                        }
                    }
                }
                return result;
            }

            public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }
        }

        // Executes method synchronously
        class SyncMethodExecutor : MethodExecutor
        {
            MethodInfo syncMethod;
            Func<object, object[], object> func;

            public SyncMethodExecutor(CodeActivityMetadata metadata, MethodInfo syncMethod, Activity invokingActivity,
                Type targetType, InArgument targetObject, Collection<Argument> parameters,
                RuntimeArgument returnObject,
                 MruCache<MethodInfo, Func<object, object[], object>> funcCache,
                ReaderWriterLockSlim locker)
                : base(invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                Fx.Assert(syncMethod != null, "Must provide syncMethod");
                this.syncMethod = syncMethod;
                this.func = MethodCallExpressionHelper.GetFunc(metadata, this.syncMethod, funcCache, locker);
            }

            public SyncMethodExecutor(SyncMethodExecutor copy, Type targetType, InArgument targetObject, Collection<Argument> parameters,
                RuntimeArgument returnObject)
                : base(copy.invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                this.syncMethod = copy.syncMethod;
                this.func = copy.func;
            }

            public bool IsTheSame(MethodInfo newMethod)
            {
                return !MethodCallExpressionHelper.NeedRetrieve(newMethod, this.syncMethod, this.func);
            }

            public override bool MethodIsStatic { get { return this.syncMethod.IsStatic; } }

            protected override IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state)
            {
                object[] actualParameters = EvaluateAndPackParameters(context, this.syncMethod, false);

                object result = this.InvokeAndUnwrapExceptions(this.func, target, actualParameters);

                SetOutArgumentAndReturnValue(context, result, actualParameters);

                return new CompletedAsyncResult(callback, state);
            }

            protected override void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }
        }

        // Executes method using paired Begin/End async pattern methods
        class AsyncPatternMethodExecutor : MethodExecutor
        {

            MethodInfo beginMethod;
            MethodInfo endMethod;
            Func<object, object[], object> beginFunc;
            Func<object, object[], object> endFunc;
            
            public AsyncPatternMethodExecutor(CodeActivityMetadata metadata, MethodInfo beginMethod, MethodInfo endMethod,
                Activity invokingActivity, Type targetType, InArgument targetObject,
                Collection<Argument> parameters, RuntimeArgument returnObject,
                 MruCache<MethodInfo, Func<object, object[], object>> funcCache,
                ReaderWriterLockSlim locker)
                : base(invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                Fx.Assert(beginMethod != null && endMethod != null, "Must provide beginMethod and endMethod");
                this.beginMethod = beginMethod;
                this.endMethod = endMethod;
                this.beginFunc = MethodCallExpressionHelper.GetFunc(metadata, beginMethod, funcCache, locker);
                this.endFunc = MethodCallExpressionHelper.GetFunc(metadata, endMethod, funcCache, locker);
            }

            public AsyncPatternMethodExecutor(AsyncPatternMethodExecutor copy, Type targetType, InArgument targetObject,
                Collection<Argument> parameters, RuntimeArgument returnObject)
                : base(copy.invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                this.beginMethod = copy.beginMethod;
                this.endMethod = copy.endMethod;
                this.beginFunc = copy.beginFunc;
                this.endFunc = copy.endFunc;
            }

            public override bool MethodIsStatic { get { return this.beginMethod.IsStatic; } }

            public bool IsTheSame(MethodInfo newBeginMethod, MethodInfo newEndMethod)
            {
                return !(MethodCallExpressionHelper.NeedRetrieve(newBeginMethod, this.beginMethod, this.beginFunc)
                        || MethodCallExpressionHelper.NeedRetrieve(newEndMethod, this.endMethod, this.endFunc));
            }

            protected override IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state)
            {
                InvokeMethodInstanceData instance = new InvokeMethodInstanceData
                {
                    TargetObject = target,
                    ActualParameters = EvaluateAndPackParameters(context, this.beginMethod, true),
                };

                int count = instance.ActualParameters.Length;

                instance.ActualParameters[count - 2] = callback;
                instance.ActualParameters[count - 1] = state;
                context.UserState = instance;

                return (IAsyncResult)this.InvokeAndUnwrapExceptions(this.beginFunc, target, instance.ActualParameters);
            }

            protected override void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result)
            {
                InvokeMethodInstanceData instance = (InvokeMethodInstanceData)context.UserState;
                instance.ReturnValue = InvokeAndUnwrapExceptions(this.endFunc, instance.TargetObject, new object[] { result });
                this.SetOutArgumentAndReturnValue(context, instance.ReturnValue, instance.ActualParameters);
            }
        }

        // Executes method asynchronously on WaitCallback thread.
        class AsyncWaitCallbackMethodExecutor : MethodExecutor
        {
            MethodInfo asyncMethod;
            Func<object, object[], object> asyncFunc;
            
            public AsyncWaitCallbackMethodExecutor(CodeActivityMetadata metadata, MethodInfo asyncMethod, Activity invokingActivity,
                Type targetType, InArgument targetObject, Collection<Argument> parameters,
                RuntimeArgument returnObject,
                 MruCache<MethodInfo, Func<object, object[], object>> funcCache,
                ReaderWriterLockSlim locker)
                : base(invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                Fx.Assert(asyncMethod != null, "Must provide asyncMethod");
                this.asyncMethod = asyncMethod;
                this.asyncFunc = MethodCallExpressionHelper.GetFunc(metadata, asyncMethod, funcCache, locker);
            }


            public AsyncWaitCallbackMethodExecutor(AsyncWaitCallbackMethodExecutor copy, Type targetType, InArgument targetObject, 
                Collection<Argument> parameters, RuntimeArgument returnObject) : 
                base(copy.invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                this.asyncMethod = copy.asyncMethod;
                this.asyncFunc = copy.asyncFunc;
            }

            public override bool MethodIsStatic { get { return this.asyncMethod.IsStatic; } }

            public bool IsTheSame(MethodInfo newMethodInfo)
            {
                return !MethodCallExpressionHelper.NeedRetrieve(newMethodInfo, this.asyncMethod, this.asyncFunc);
            }

            protected override IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state)
            {
                InvokeMethodInstanceData instance = new InvokeMethodInstanceData
                {
                    TargetObject = target,
                    ActualParameters = EvaluateAndPackParameters(context, this.asyncMethod, false),
                };
                return new ExecuteAsyncResult(instance, this, callback, state);
            }

            protected override void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result)
            {
                InvokeMethodInstanceData instance = ExecuteAsyncResult.End(result);
                if (instance.ExceptionWasThrown)
                {
                    throw FxTrace.Exception.AsError(instance.Exception);
                }
                else
                {
                    this.SetOutArgumentAndReturnValue(context, instance.ReturnValue, instance.ActualParameters);
                }
            }

            class ExecuteAsyncResult : AsyncResult
            {
                static Action<object> asyncExecute = new Action<object>(AsyncExecute);
                InvokeMethodInstanceData instance;
                AsyncWaitCallbackMethodExecutor executor;

                public ExecuteAsyncResult(InvokeMethodInstanceData instance, AsyncWaitCallbackMethodExecutor executor, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.instance = instance;
                    this.executor = executor;
                    ActionItem.Schedule(asyncExecute, this);
                }

                public static InvokeMethodInstanceData End(IAsyncResult result)
                {
                    ExecuteAsyncResult thisPtr = AsyncResult.End<ExecuteAsyncResult>(result);
                    return thisPtr.instance;
                }

                static void AsyncExecute(object state)
                {
                    ExecuteAsyncResult thisPtr = (ExecuteAsyncResult)state;
                    thisPtr.AsyncExecuteCore();
                }

                void AsyncExecuteCore()
                {
                    try
                    {
                        this.instance.ReturnValue = this.executor.InvokeAndUnwrapExceptions(this.executor.asyncFunc, this.instance.TargetObject, this.instance.ActualParameters);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        this.instance.Exception = e;
                        this.instance.ExceptionWasThrown = true;
                    }
                    base.Complete(false);
                }
            }
        }
    }
}
