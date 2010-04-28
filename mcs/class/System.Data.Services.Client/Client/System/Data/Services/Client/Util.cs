//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System.Collections;
    using System.Diagnostics;
    using System.Xml;
    using System.Reflection;
    using System.Linq.Expressions;

    #endregion Namespaces.

    internal static class Util
    {
        internal const string VersionSuffix = ";NetFx";

        internal const string CodeGeneratorToolName = "System.Data.Services.Design";

        internal static readonly Version DataServiceVersionEmpty = new Version(0, 0);

        internal static readonly Version DataServiceVersion1 = new Version(1, 0);

        internal static readonly Version DataServiceVersion2 = new Version(2, 0);

        internal static readonly Version MaxResponseVersion = DataServiceVersion2;

        internal static readonly Version[] SupportedResponseVersions = 
        { 
            DataServiceVersion1,
            DataServiceVersion2
        };

        internal static readonly char[] ForwardSlash = new char[1] { '/' };

        private static char[] whitespaceForTracing = new char[] { '\r', '\n', ' ', ' ', ' ', ' ', ' ' };

#if DEBUG
        private static Action<string> DebugFaultInjector = new Action<string>((s) => { });

        private static Func<String, String> referenceIdentity = delegate(String identity)
        {
            return identity;
        };

        private static Func<String, String> dereferenceIdentity = delegate(String identity)
        {
            return identity;
        };
#endif

        [Conditional("DEBUG")]
        internal static void DebugInjectFault(string state)
        {
#if DEBUG
            DebugFaultInjector(state);
#endif
        }

        internal static String ReferenceIdentity(String uri)
        {
#if DEBUG
            return referenceIdentity(uri);
#else
            return uri;
#endif
        }

        internal static String DereferenceIdentity(String uri)
        {
#if DEBUG
            return dereferenceIdentity(uri);
#else
            return uri;
#endif
        }

        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw Error.ArgumentNull(parameterName);
            }

            return value;
        }

        internal static void CheckArgumentNotEmpty(string value, string parameterName)
        {
            CheckArgumentNull(value, parameterName);
            if (0 == value.Length)
            {
                throw Error.Argument(Strings.Util_EmptyString, parameterName);
            }
        }

        internal static void CheckArgumentNotEmpty<T>(T[] value, string parameterName) where T : class
        {
            CheckArgumentNull(value, parameterName);
            if (0 == value.Length)
            {
                throw Error.Argument(Strings.Util_EmptyArray, parameterName);
            }

            for (int i = 0; i < value.Length; ++i)
            {
                if (Object.ReferenceEquals(value[i], null))
                {
                    throw Error.Argument(Strings.Util_NullArrayElement, parameterName);
                }
            }
        }

        internal static MergeOption CheckEnumerationValue(MergeOption value, string parameterName)
        {
            switch (value)
            {
                case MergeOption.AppendOnly:
                case MergeOption.OverwriteChanges:
                case MergeOption.PreserveChanges:
                case MergeOption.NoTracking:
                    return value;
                default:
                    throw Error.ArgumentOutOfRange(parameterName);
            }
        }

#if ASTORIA_LIGHT        
        internal static HttpStack CheckEnumerationValue(HttpStack value, string parameterName)
        {
            switch (value)
            {
                case HttpStack.Auto:
                case HttpStack.ClientHttp:
                case HttpStack.XmlHttp:
                    return value;
                default:
                    throw Error.ArgumentOutOfRange(parameterName);
            }
        }
#endif

        internal static char[] GetWhitespaceForTracing(int depth)
        {
            char[] whitespace = Util.whitespaceForTracing;
            while (whitespace.Length <= depth)
            {
                char[] tmp = new char[2 * whitespace.Length];
                tmp[0] = '\r';
                tmp[1] = '\n';
                for (int i = 2; i < tmp.Length; ++i)
                {
                    tmp[i] = ' ';
                }

                System.Threading.Interlocked.CompareExchange(ref Util.whitespaceForTracing, tmp, whitespace);
                whitespace = tmp;
            }

            return whitespace;
        }

        internal static Uri CreateUri(string value, UriKind kind)
        {
            return value == null ? null : new Uri(value, kind);
        }

        internal static Uri CreateUri(Uri baseUri, Uri requestUri)
        {
            Debug.Assert((null != baseUri) && baseUri.IsAbsoluteUri, "baseUri !IsAbsoluteUri");
            Debug.Assert(String.IsNullOrEmpty(baseUri.Query) && String.IsNullOrEmpty(baseUri.Fragment), "baseUri has query or fragment");
            Util.CheckArgumentNull(requestUri, "requestUri");

            if (!requestUri.IsAbsoluteUri)
            {
                if (baseUri.OriginalString.EndsWith("/", StringComparison.Ordinal))
                {
                    if (requestUri.OriginalString.StartsWith("/", StringComparison.Ordinal))
                    {
                        requestUri = new Uri(baseUri, Util.CreateUri(requestUri.OriginalString.TrimStart(Util.ForwardSlash), UriKind.Relative));
                    }
                    else
                    {
                        requestUri = new Uri(baseUri, requestUri);
                    }
                }
                else
                {
                    requestUri = Util.CreateUri(baseUri.OriginalString + "/" + requestUri.OriginalString.TrimStart(Util.ForwardSlash), UriKind.Absolute);
                }
            }

            return requestUri;
        }

        internal static bool ContainsReference<T>(T[] array, T value) where T : class
        {
            return (0 <= IndexOfReference<T>(array, value));
        }

        internal static void Dispose<T>(ref T disposable) where T : class, IDisposable
        {
            Dispose(disposable);
            disposable = null;
        }

        internal static void Dispose<T>(T disposable) where T : class, IDisposable
        {
            if (null != disposable)
            {
                disposable.Dispose();
            }
        }

        internal static int IndexOfReference<T>(T[] array, T value) where T : class
        {
            Debug.Assert(null != array, "null array");
            for (int i = 0; i < array.Length; ++i)
            {
                if (object.ReferenceEquals(array[i], value))
                {
                    return i;
                }
            }

            return -1;
        }

        internal static bool DoNotHandleException(Exception ex)
        {
            return ((null != ex) &&
                    ((ex is System.StackOverflowException) ||
                     (ex is System.OutOfMemoryException) ||
                     (ex is System.Threading.ThreadAbortException)));
        }

        internal static bool IsKnownClientExcption(Exception ex)
        {
            return (ex is DataServiceClientException) || (ex is DataServiceQueryException) || (ex is DataServiceRequestException);
        }

        internal static T NullCheck<T>(T value, InternalError errorcode) where T : class
        {
            if (Object.ReferenceEquals(value, null))
            {
                Error.ThrowInternalError(errorcode);
            }

            return value;
        }

        internal static bool AreSame(string value1, string value2)
        {
            bool result = (value1 == value2);
            return result;
        }

        internal static bool AreSame(XmlReader reader, string localName, string namespaceUri)
        {
            Debug.Assert((null != reader) && (null != localName) && (null != namespaceUri), "null");
            return ((XmlNodeType.Element == reader.NodeType) || (XmlNodeType.EndElement == reader.NodeType)) &&
                    AreSame(reader.LocalName, localName) && AreSame(reader.NamespaceURI, namespaceUri);
        }

        internal static bool DoesNullAttributeSayTrue(XmlReader reader)
        {
            string attributeValue = reader.GetAttribute(XmlConstants.AtomNullAttributeName, XmlConstants.DataWebMetadataNamespace);
            return ((null != attributeValue) && XmlConvert.ToBoolean(attributeValue));
        }

        internal static bool TypeAllowsNull(Type type)
        {
            Debug.Assert(type != null, "type != null");
            return !type.IsValueType || IsNullableType(type);
        }

        internal static Type GetTypeAllowingNull(Type type)
        {
            Debug.Assert(type != null, "type != null");
            return TypeAllowsNull(type) ? type : typeof(Nullable<>).MakeGenericType(type);
        }

        internal static void SetNextLinkForCollection(object collection, DataServiceQueryContinuation continuation)
        {
            Debug.Assert(collection != null, "collection != null");

            foreach (var property in collection.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (property.Name != "Continuation" || !property.CanWrite)
                {
                    continue;
                }

                if (typeof(DataServiceQueryContinuation).IsAssignableFrom(property.PropertyType))
                {
                    property.SetValue(collection, continuation, null);
                }
            }
        }

        internal static object ActivatorCreateInstance(Type type, params object[] arguments)
        {
            Debug.Assert(type != null, "type != null");
#if ASTORIA_LIGHT
            int argumentCount = (arguments == null) ? 0 : arguments.Length;
            ConstructorInfo[] constructors = type.GetConstructors();
            ConstructorInfo constructor = null;
            for (int i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].GetParameters().Length == argumentCount)
                {
                    Debug.Assert( constructor == null, "Make sure that the specific type has only one constructor with specified argument count");
                    constructor = constructors[i];
#if !DEBUG
                    break;
#endif
                }
            }

            if (constructor == null)
            {
                throw new MissingMethodException();
            }

            return ConstructorInvoke(constructor, arguments);
#else            
            return Activator.CreateInstance(type, arguments);
#endif
        }

        internal static object ConstructorInvoke(ConstructorInfo constructor, object[] arguments)
        {
            if (constructor == null)
            {
                throw new MissingMethodException();
            }
#if ASTORIA_LIGHT
            int argumentCount = (arguments == null) ? 0 : arguments.Length;
            ParameterExpression argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
            Expression[] argumentExpressions = new Expression[argumentCount];
            ParameterInfo[] parameters = constructor.GetParameters();
            for (int i = 0; i < argumentExpressions.Length; i++)
            {
                argumentExpressions[i] = Expression.Constant(arguments[i], parameters[i].ParameterType);
            }

            Expression newExpression = Expression.New(constructor, argumentExpressions);
            Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(newExpression, typeof(object)),
                argumentsExpression);
            object result = lambda.Compile()(arguments);
            return result;
#else
            return constructor.Invoke(arguments);
#endif
        }

        #region Tracing

        [Conditional("TRACE")]
        internal static void TraceElement(XmlReader reader, System.IO.TextWriter writer)
        {
            Debug.Assert(XmlNodeType.Element == reader.NodeType, "not positioned on Element");

            if (null != writer)
            {
                writer.Write(Util.GetWhitespaceForTracing(2 + reader.Depth), 0, 2 + reader.Depth);
                writer.Write("<{0}", reader.Name);

                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        writer.Write(" {0}=\"{1}\"", reader.Name, reader.Value);
                    }
                    while (reader.MoveToNextAttribute());

                    reader.MoveToElement();
                }

                writer.Write(reader.IsEmptyElement ? " />" : ">");
            }
        }

        [Conditional("TRACE")]
        internal static void TraceEndElement(XmlReader reader, System.IO.TextWriter writer, bool indent)
        {
            if (null != writer)
            {
                if (indent)
                {
                    writer.Write(Util.GetWhitespaceForTracing(2 + reader.Depth), 0, 2 + reader.Depth);
                }

                writer.Write("</{0}>", reader.Name);
            }
        }

        [Conditional("TRACE")]
        internal static void TraceText(System.IO.TextWriter writer, string value)
        {
            if (null != writer)
            {
                writer.Write(value);
            }
        }
        
        #endregion        

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
