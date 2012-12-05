// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
#if FEATURE_DYNAMIC
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace System.Json
{
    /// <summary>
    /// This class provides dynamic behavior support for the JsonValue types.
    /// </summary>
    internal class JsonValueDynamicMetaObject : DynamicMetaObject
    {
        private static readonly MethodInfo _getValueByIndexMethodInfo = typeof(JsonValue).GetMethod("GetValue", new Type[] { typeof(int) });
        private static readonly MethodInfo _getValueByKeyMethodInfo = typeof(JsonValue).GetMethod("GetValue", new Type[] { typeof(string) });
        private static readonly MethodInfo _setValueByIndexMethodInfo = typeof(JsonValue).GetMethod("SetValue", new Type[] { typeof(int), typeof(object) });
        private static readonly MethodInfo _setValueByKeyMethodInfo = typeof(JsonValue).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) });
        private static readonly MethodInfo _castValueMethodInfo = typeof(JsonValue).GetMethod("CastValue", new Type[] { typeof(JsonValue) });
        private static readonly MethodInfo _changeTypeMethodInfo = typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) });

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="parameter">The expression representing this <see cref="DynamicMetaObject"/> during the dynamic binding process.</param>
        /// <param name="value">The runtime value represented by the <see cref="DynamicMetaObject"/>.</param>
        internal JsonValueDynamicMetaObject(Expression parameter, JsonValue value)
            : base(parameter, BindingRestrictions.Empty, value)
        {
        }

        /// <summary>
        /// Gets the default binding restrictions for this type.
        /// </summary>
        private BindingRestrictions DefaultRestrictions
        {
            get { return BindingRestrictions.GetTypeRestriction(Expression, LimitType); }
        }

        /// <summary>
        /// Implements dynamic cast for JsonValue types.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="ConvertBinder"/> that represents the details of the dynamic operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            Expression expression = Expression;

            bool implicitCastSupported =
                binder.Type.IsAssignableFrom(LimitType) ||
                binder.Type == typeof(IEnumerable<KeyValuePair<string, JsonValue>>) ||
                binder.Type == typeof(IDynamicMetaObjectProvider) ||
                binder.Type == typeof(object);

            if (!implicitCastSupported)
            {
                if (JsonValue.IsSupportedExplicitCastType(binder.Type))
                {
                    Expression instance = Expression.Convert(Expression, LimitType);
                    expression = Expression.Call(_castValueMethodInfo.MakeGenericMethod(binder.Type), new Expression[] { instance });
                }
                else
                {
                    string exceptionMessage = RS.Format(Properties.Resources.CannotCastJsonValue, LimitType.FullName, binder.Type.FullName);
                    expression = Expression.Throw(Expression.Constant(new InvalidCastException(exceptionMessage)), typeof(object));
                }
            }

            expression = Expression.Convert(expression, binder.Type);

            return new DynamicMetaObject(expression, DefaultRestrictions);
        }

        /// <summary>
        /// Implements setter for dynamic indexer by index (JsonArray)
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetIndexBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the get index operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }

            Expression indexExpression;
            if (!JsonValueDynamicMetaObject.TryGetIndexExpression(indexes, out indexExpression))
            {
                return new DynamicMetaObject(indexExpression, DefaultRestrictions);
            }

            MethodInfo methodInfo = indexExpression.Type == typeof(string) ? _getValueByKeyMethodInfo : _getValueByIndexMethodInfo;
            Expression[] args = new Expression[] { indexExpression };

            return GetMethodMetaObject(methodInfo, args);
        }

        /// <summary>
        /// Implements getter for dynamic indexer by index (JsonArray).
        /// </summary>
        /// <param name="binder">An instance of the <see cref="SetIndexBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the set index operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set index operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Expression indexExpression;
            if (!JsonValueDynamicMetaObject.TryGetIndexExpression(indexes, out indexExpression))
            {
                return new DynamicMetaObject(indexExpression, DefaultRestrictions);
            }

            MethodInfo methodInfo = indexExpression.Type == typeof(string) ? _setValueByKeyMethodInfo : _setValueByIndexMethodInfo;
            Expression[] args = new Expression[] { indexExpression, Expression.Convert(value.Expression, typeof(object)) };

            return GetMethodMetaObject(methodInfo, args);
        }

        /// <summary>
        /// Implements getter for dynamic indexer by key (JsonObject).
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            PropertyInfo propInfo = LimitType.GetProperty(binder.Name, BindingFlags.Instance | BindingFlags.Public);

            if (propInfo != null)
            {
                return base.BindGetMember(binder);
            }

            Expression[] args = new Expression[] { Expression.Constant(binder.Name) };

            return GetMethodMetaObject(_getValueByKeyMethodInfo, args);
        }

        /// <summary>
        /// Implements setter for dynamic indexer by key (JsonObject).
        /// </summary>
        /// <param name="binder">An instance of the <see cref="SetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set member operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Expression[] args = new Expression[] { Expression.Constant(binder.Name), Expression.Convert(value.Expression, typeof(object)) };

            return GetMethodMetaObject(_setValueByKeyMethodInfo, args);
        }

        /// <summary>
        /// Performs the binding of the dynamic invoke member operation.
        /// Implemented to support extension methods defined in <see cref="JsonValueExtensions"/> type.
        /// </summary>
        /// <param name="binder">An instance of the InvokeMemberBinder that represents the details of the dynamic operation.</param>
        /// <param name="args">An array of DynamicMetaObject instances - arguments to the invoke member operation.</param>
        /// <returns>The new DynamicMetaObject representing the result of the binding.</returns>
        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            List<Type> argTypeList = new List<Type>();

            for (int idx = 0; idx < args.Length; idx++)
            {
                argTypeList.Add(args[idx].LimitType);
            }

            MethodInfo methodInfo = Value.GetType().GetMethod(binder.Name, argTypeList.ToArray());

            if (methodInfo == null)
            {
                argTypeList.Insert(0, typeof(JsonValue));

                Type[] argTypes = argTypeList.ToArray();

                methodInfo = JsonValueDynamicMetaObject.GetExtensionMethod(typeof(JsonValueExtensions), binder.Name, argTypes);

                if (methodInfo != null)
                {
                    Expression thisInstance = Expression.Convert(Expression, LimitType);
                    Expression[] argsExpression = new Expression[argTypes.Length];

                    argsExpression[0] = thisInstance;
                    for (int i = 0; i < args.Length; i++)
                    {
                        argsExpression[i + 1] = args[i].Expression;
                    }

                    Expression callExpression = Expression.Call(methodInfo, argsExpression);

                    if (methodInfo.ReturnType == typeof(void))
                    {
                        callExpression = Expression.Block(callExpression, Expression.Default(binder.ReturnType));
                    }
                    else
                    {
                        callExpression = Expression.Convert(Expression.Call(methodInfo, argsExpression), binder.ReturnType);
                    }

                    return new DynamicMetaObject(callExpression, DefaultRestrictions);
                }
            }

            return base.BindInvokeMember(binder, args);
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of string reprenseting the dynamic member names.</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            JsonValue jsonValue = Value as JsonValue;

            if (jsonValue != null)
            {
                List<string> names = new List<string>();

                foreach (KeyValuePair<string, JsonValue> pair in jsonValue)
                {
                    names.Add(pair.Key);
                }

                return names;
            }

            return base.GetDynamicMemberNames();
        }

        /// <summary>
        /// Gets a <see cref="MethodInfo"/> instance for the specified method name in the specified type.
        /// </summary>
        /// <param name="extensionProviderType">The extension provider type.</param>
        /// <param name="methodName">The name of the method to get the info for.</param>
        /// <param name="argTypes">The types of the method arguments.</param>
        /// <returns>A <see cref="MethodInfo"/>instance or null if the method cannot be resolved.</returns>
        private static MethodInfo GetExtensionMethod(Type extensionProviderType, string methodName, Type[] argTypes)
        {
            MethodInfo methodInfo = null;
            MethodInfo[] methods = extensionProviderType.GetMethods();

            foreach (MethodInfo info in methods)
            {
                if (info.Name == methodName)
                {
                    methodInfo = info;

                    if (!info.IsGenericMethodDefinition)
                    {
                        bool paramsMatch = true;
                        ParameterInfo[] args = methodInfo.GetParameters();

                        if (args.Length == argTypes.Length)
                        {
                            for (int idx = 0; idx < args.Length; idx++)
                            {
                                if (!args[idx].ParameterType.IsAssignableFrom(argTypes[idx]))
                                {
                                    paramsMatch = false;
                                    break;
                                }
                            }

                            if (paramsMatch)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return methodInfo;
        }

        /// <summary>
        /// Attempts to get an expression for an index parameter.
        /// </summary>
        /// <param name="indexes">The operation indexes parameter.</param>
        /// <param name="expression">A <see cref="Expression"/> to be initialized to the index expression if the operation is successful, otherwise an error expression.</param>
        /// <returns>true the operation is successful, false otherwise.</returns>
        private static bool TryGetIndexExpression(DynamicMetaObject[] indexes, out Expression expression)
        {
            if (indexes.Length == 1 && indexes[0] != null && indexes[0].Value != null)
            {
                DynamicMetaObject index = indexes[0];
                Type indexType = indexes[0].Value.GetType();

                switch (Type.GetTypeCode(indexType))
                {
                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        Expression argExp = Expression.Convert(index.Expression, typeof(object));
                        Expression typeExp = Expression.Constant(typeof(int));
                        expression = Expression.Convert(Expression.Call(_changeTypeMethodInfo, new Expression[] { argExp, typeExp }), typeof(int));
                        return true;

                    case TypeCode.Int32:
                    case TypeCode.String:
                        expression = index.Expression;
                        return true;
                }

                expression = Expression.Throw(Expression.Constant(new ArgumentException(RS.Format(Properties.Resources.InvalidIndexType, indexType))), typeof(object));
                return false;
            }

            expression = Expression.Throw(Expression.Constant(new ArgumentException(Properties.Resources.NonSingleNonNullIndexNotSupported)), typeof(object));
            return false;
        }

        /// <summary>
        /// Gets a <see cref="DynamicMetaObject"/> for a method call.
        /// </summary>
        /// <param name="methodInfo">Info for the method to be performed.</param>
        /// <param name="args">expression array representing the method arguments</param>
        /// <returns>A meta object for the method call.</returns>
        private DynamicMetaObject GetMethodMetaObject(MethodInfo methodInfo, Expression[] args)
        {
            Expression instance = Expression.Convert(Expression, LimitType);
            Expression methodCall = Expression.Call(instance, methodInfo, args);
            BindingRestrictions restrictions = DefaultRestrictions;

            DynamicMetaObject metaObj = new DynamicMetaObject(methodCall, restrictions);

            return metaObj;
        }
    }
}
#endif
