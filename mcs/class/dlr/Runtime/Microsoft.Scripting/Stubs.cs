/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if WIN8

// When compiled with Dev10 VS CSC reports errors if this is not defined
// error CS0656: Missing compiler required member 'System.Threading.Thread.get_ManagedThreadId'
// error CS0656: Missing compiler required member 'System.Threading.Thread.get_CurrentThread'
namespace System.Threading {
    internal class Thread {
        public int ManagedThreadId { get { throw new NotImplementedException(); } }
        public static Thread CurrentThread { get { throw new NotImplementedException(); } }
    }
}

namespace System.IO {
    [Serializable]
    public enum FileMode {
        CreateNew = 1,
        Create,
        Open,
        OpenOrCreate,
        Truncate,
        Append
    }

    [Serializable]
    public enum FileAccess {
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }

    [Serializable]
    public enum FileShare {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = 3,
        Delete = 4,
        Inheritable = 16
    }
}
#else
namespace System {
    public static class TypeExtensions {
#if !FEATURE_GET_TYPE_INFO
        public static Type GetTypeInfo(this Type type) {
            return type;
        }
#endif

        public static Type AsType(this Type type) {
            return type;
        }
    }
}
#endif

#if !FEATURE_SERIALIZATION

namespace System {
    using System.Diagnostics;

    [Conditional("STUB")]
    public class SerializableAttribute : Attribute {
    }

    [Conditional("STUB")]
    public class NonSerializedAttribute : Attribute {
    }

    namespace Runtime.Serialization {
        public interface ISerializable {
        }

        public interface IDeserializationCallback {
        }
    }

    public class SerializationException : Exception {
    }
}

#endif

#if SILVERLIGHT && !CLR2

namespace System {
    namespace Runtime.InteropServices {
        /// <summary>
        /// The Default Parameter Value Attribute.
        /// </summary>
        public sealed class DefaultParameterValueAttribute : Attribute {
            /// <summary>
            /// The constructor
            /// </summary>
            /// <param name="value">The value.</param>
            public DefaultParameterValueAttribute(object value) { }
        }
    }
}

#endif