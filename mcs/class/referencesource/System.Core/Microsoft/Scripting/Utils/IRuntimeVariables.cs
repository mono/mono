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

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// An interface to represent values of runtime variables.
    /// </summary>
    public interface IRuntimeVariables {
        /// <summary>
        /// Count of the variables.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// An indexer to get/set the values of the runtime variables.
        /// </summary>
        /// <param name="index">An index of the runtime variable.</param>
        /// <returns>The value of the runtime variable.</returns>
        object this[int index] { get; set; }
    }
}
