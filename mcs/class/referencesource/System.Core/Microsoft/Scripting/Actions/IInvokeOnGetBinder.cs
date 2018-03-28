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

namespace System.Dynamic {
    /// <summary>
    /// Represents information about a dynamic get member operation, indicating
    /// if the get member should invoke properties when performing the get.
    /// </summary>
    public interface IInvokeOnGetBinder {
        /// <summary>
        /// Gets the value indicating if this GetMember should invoke properties
        /// when performing the get. The default value when this interface is not present
        /// is true.
        /// </summary>
        /// <remarks>
        /// This property is used by some languages to get a better COM interop experience.
        /// When the value is set to false, the dynamic COM object won't invoke the object
        /// but will instead bind to the name, and return an object that can be invoked or
        /// indexed later. This is useful for indexed properties and languages that don't
        /// produce InvokeMember call sites.
        /// </remarks>
        bool InvokeOnGet { get; }
    }
}
