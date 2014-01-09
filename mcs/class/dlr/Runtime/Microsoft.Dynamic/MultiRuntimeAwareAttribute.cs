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

using System;
using System.Diagnostics;

namespace Microsoft.Scripting {
    /// <summary>
    /// marks a field, class, or struct as being safe to have statics which can be accessed
    /// from multiple runtimes.
    /// 
    /// Static fields which are not read-only or marked with this attribute will be flagged 
    /// by a test which looks for state being shared between runtimes.  Before applying this
    /// attribute you should ensure that it is safe to share the state.  This is typically
    /// state which is lazy initialized or state which is caching values which are identical
    /// in all runtimes and are immutable.
    /// </summary>
    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.Field)]   
    public sealed class MultiRuntimeAwareAttribute : Attribute {
    }
}
