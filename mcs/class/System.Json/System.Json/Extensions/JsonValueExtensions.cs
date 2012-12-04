// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
#if FEATURE_DYNAMIC
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Json;
using System.Linq.Expressions;

namespace System.Runtime.Serialization.Json
{
    /// <summary>
    /// This class extends the functionality of the <see cref="JsonValue"/> type. 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class JsonValueExtensions
    {
        /// <summary>
        /// Creates a <see cref="System.Json.JsonValue"/> object based on an arbitrary CLR object.
        /// </summary>
        /// <param name="value">The object to be converted to <see cref="System.Json.JsonValue"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> which represents the given object.</returns>
        /// <remarks>The conversion is done through the <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>;
        /// the object is first serialized into JSON using the serializer, then parsed into a <see cref="System.Json.JsonValue"/>
        /// object.</remarks>
        public static JsonValue CreateFrom(object value)
        {
            JsonValue jsonValue = null;

            if (value != null)
            {
                jsonValue = value as JsonValue;

                if (jsonValue == null)
                {
                    jsonValue = JsonValueExtensions.CreatePrimitive(value);

                    if (jsonValue == null)
                    {
                        jsonValue = JsonValueExtensions.CreateFromDynamic(value);

                        if (jsonValue == null)
                        {
                            jsonValue = JsonValueExtensions.CreateFromComplex(value);
                        }
                    }
                }
            }

            return jsonValue;
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into the type T.
        /// </summary>
        /// <typeparam name="T">The type to which the conversion is being performed.</typeparam>
        /// <param name="jsonValue">The <see cref="JsonValue"/> instance this method extension is to be applied to.</param>
        /// <param name="valueOfT">An instance of T initialized with this instance, or the default
        /// value of T, if the conversion cannot be performed.</param>
        /// <returns>true if this <see cref="System.Json.JsonValue"/> instance can be read as type T; otherwise, false.</returns>
        public static bool TryReadAsType<T>(this JsonValue jsonValue, out T valueOfT)
        {
            if (jsonValue == null)
            {
                throw new ArgumentNullException("jsonValue");
            }

            object value;
            if (JsonValueExtensions.TryReadAsType(jsonValue, typeof(T), out value))
            {
                valueOfT = (T)value;
                return true;
            }

            valueOfT = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into the type T.
        /// </summary>
        /// <typeparam name="T">The type to which the conversion is being performed.</typeparam>
        /// <param name="jsonValue">The <see cref="JsonValue"/> instance this method extension is to be applied to.</param>
        /// <returns>An instance of T initialized with the <see cref="System.Json.JsonValue"/> value
        /// specified if the conversion.</returns>
        /// <exception cref="System.NotSupportedException">If this <see cref="System.Json.JsonValue"/> value cannot be
        /// converted into the type T.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic parameter is used to specify the output type")]
        public static T ReadAsType<T>(this JsonValue jsonValue)
        {
            if (jsonValue == null)
            {
                throw new ArgumentNullException("jsonValue");
            }

            return (T)JsonValueExtensions.ReadAsType(jsonValue, typeof(T));
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into the type T, returning a fallback value
        /// if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The type to which the conversion is being performed.</typeparam>
        /// <param name="jsonValue">The <see cref="JsonValue"/> instance this method extension is to be applied to.</param>
        /// <param name="fallback">A fallback value to be retuned in case the conversion cannot be performed.</param>
        /// <returns>An instance of T initialized with the <see cref="System.Json.JsonValue"/> value
        /// specified if the conversion succeeds or the specified fallback value if it fails.</returns>
        public static T ReadAsType<T>(this JsonValue jsonValue, T fallback)
        {
            if (jsonValue == null)
            {
                throw new ArgumentNullException("jsonValue");
            }

            T outVal;
            if (JsonValueExtensions.TryReadAsType<T>(jsonValue, out outVal))
            {
                return outVal;
            }

            return fallback;
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into an instance of the specified type.
        /// </summary>
        /// <param name="jsonValue">The <see cref="JsonValue"/> instance this method extension is to be applied to.</param>
        /// <param name="type">The type to which the conversion is being performed.</param>
        /// <returns>An object instance initialized with the <see cref="System.Json.JsonValue"/> value
        /// specified if the conversion.</returns>
        /// <exception cref="System.NotSupportedException">If this <see cref="System.Json.JsonValue"/> value cannot be
        /// converted into the type T.</exception>
        public static object ReadAsType(this JsonValue jsonValue, Type type)
        {
            if (jsonValue == null)
            {
                throw new ArgumentNullException("jsonValue");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            object result;
            if (JsonValueExtensions.TryReadAsType(jsonValue, type, out result))
            {
                return result;
            }

            throw new NotSupportedException(RS.Format(System.Json.Properties.Resources.CannotReadAsType, jsonValue.GetType().FullName, type.FullName));
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into an instance of the specified type.
        /// </summary>
        /// <param name="jsonValue">The <see cref="JsonValue"/> instance this method extension is to be applied to.</param>
        /// <param name="type">The type to which the conversion is being performed.</param>
        /// <param name="value">An object to be initialized with this instance or null if the conversion cannot be performed.</param>
        /// <returns>true if this <see cref="System.Json.JsonValue"/> instance can be read as the specified type; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate",
            Justification = "This is the non-generic version of the method.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception translates to fail.")]
        public static bool TryReadAsType(this JsonValue jsonValue, Type type, out object value)
        {
            if (jsonValue == null)
            {
                throw new ArgumentNullException("jsonValue");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type == typeof(JsonValue) || type == typeof(object))
            {
                value = jsonValue;
                return true;
            }

            if (type == typeof(object[]) || type == typeof(Dictionary<string, object>))
            {
                if (!JsonValueExtensions.CanConvertToClrCollection(jsonValue, type))
                {
                    value = null;
                    return false;
                }
            }

            if (jsonValue.TryReadAs(type, out value))
            {
                return true;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    jsonValue.Save(ms);
                    ms.Position = 0;
                    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(type);
                    value = dcjs.ReadObject(ms);
                }

                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonValue"/> instance can be converted to the specified collection <see cref="Type"/>.
        /// </summary>
        /// <param name="jsonValue">The instance to be converted.</param>
        /// <param name="collectionType">The collection type to convert the instance to.</param>
        /// <returns>true if the instance can be converted, false otherwise</returns>
        private static bool CanConvertToClrCollection(JsonValue jsonValue, Type collectionType)
        {
            if (jsonValue != null)
            {
                return (jsonValue.JsonType == JsonType.Object && collectionType == typeof(Dictionary<string, object>)) ||
                       (jsonValue.JsonType == JsonType.Array && collectionType == typeof(object[]));
            }

            return false;
        }

        private static JsonValue CreatePrimitive(object value)
        {
            JsonPrimitive jsonPrimitive;

            if (JsonPrimitive.TryCreate(value, out jsonPrimitive))
            {
                return jsonPrimitive;
            }

            return null;
        }

        private static JsonValue CreateFromComplex(object value)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(value.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                dcjs.WriteObject(ms, value);
                ms.Position = 0;
                return JsonValue.Load(ms);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "value is not the same")]
        private static JsonValue CreateFromDynamic(object value)
        {
            JsonObject parent = null;
            DynamicObject dynObj = value as DynamicObject;

            if (dynObj != null)
            {
                parent = new JsonObject();
                Stack<CreateFromTypeStackInfo> infoStack = new Stack<CreateFromTypeStackInfo>();
                IEnumerator<string> keys = null;

                do
                {
                    if (keys == null)
                    {
                        keys = dynObj.GetDynamicMemberNames().GetEnumerator();
                    }

                    while (keys.MoveNext())
                    {
                        JsonValue child = null;
                        string key = keys.Current;
                        SimpleGetMemberBinder binder = new SimpleGetMemberBinder(key);

                        if (dynObj.TryGetMember(binder, out value))
                        {
                            DynamicObject childDynObj = value as DynamicObject;

                            if (childDynObj != null)
                            {
                                child = new JsonObject();
                                parent.Add(key, child);

                                infoStack.Push(new CreateFromTypeStackInfo(parent, dynObj, keys));

                                parent = child as JsonObject;
                                dynObj = childDynObj;
                                keys = null;

                                break;
                            }
                            else
                            {
                                if (value != null)
                                {
                                    child = value as JsonValue;

                                    if (child == null)
                                    {
                                        child = JsonValueExtensions.CreatePrimitive(value);

                                        if (child == null)
                                        {
                                            child = JsonValueExtensions.CreateFromComplex(value);
                                        }
                                    }
                                }

                                parent.Add(key, child);
                            }
                        }
                    }

                    if (infoStack.Count > 0 && keys != null)
                    {
                        CreateFromTypeStackInfo info = infoStack.Pop();

                        parent = info.JsonObject;
                        dynObj = info.DynamicObject;
                        keys = info.Keys;
                    }
                }
                while (infoStack.Count > 0);
            }

            return parent;
        }

        private class CreateFromTypeStackInfo
        {
            public CreateFromTypeStackInfo(JsonObject jsonObject, DynamicObject dynamicObject, IEnumerator<string> keyEnumerator)
            {
                JsonObject = jsonObject;
                DynamicObject = dynamicObject;
                Keys = keyEnumerator;
            }

            /// <summary>
            /// Gets of sets
            /// </summary>
            public JsonObject JsonObject { get; set; }

            /// <summary>
            /// Gets of sets
            /// </summary>
            public DynamicObject DynamicObject { get; set; }

            /// <summary>
            /// Gets of sets
            /// </summary>
            public IEnumerator<string> Keys { get; set; }
        }

        private class SimpleGetMemberBinder : GetMemberBinder
        {
            public SimpleGetMemberBinder(string name)
                : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                if (target != null && errorSuggestion == null)
                {
                    string exceptionMessage = RS.Format(System.Json.Properties.Resources.DynamicPropertyNotDefined, target.LimitType, Name);
                    Expression throwExpression = Expression.Throw(Expression.Constant(new InvalidOperationException(exceptionMessage)), typeof(object));

                    errorSuggestion = new DynamicMetaObject(throwExpression, target.Restrictions);
                }

                return errorSuggestion;
            }
        }
    }
}
#endif
