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

using System.Diagnostics;

namespace Microsoft.Scripting.Utils {

}

#if !FEATURE_SERIALIZATION

namespace System {
    /// <summary>
    /// The serializable attribute.
    /// </summary>
    [Conditional("STUB")]
    internal class SerializableAttribute : Attribute {
    }

    /// <summary>
    /// Non serializable attribute.
    /// </summary>
    [Conditional("STUB")]
    internal class NonSerializedAttribute : Attribute {
    }

    namespace Runtime.Serialization {
        /// <summary>
        /// ISerializable interface.
        /// </summary>
        internal interface ISerializable {
        }
    }
}

#endif