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

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// This attribute marks a parameter that is not allowed to be null.
    /// It is used by the method binding infrastructure to generate better error 
    /// messages and method selection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class NotNullAttribute : Attribute {
        public NotNullAttribute() {
        }
    }

    /// <summary>
    /// This attribute marks a parameter whose type is an array that is not allowed to have null items.
    /// It is used by the method binding infrastructure to generate better error 
    /// messages and method selection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class NotNullItemsAttribute : Attribute {
        public NotNullItemsAttribute() {
        }
    }
}
