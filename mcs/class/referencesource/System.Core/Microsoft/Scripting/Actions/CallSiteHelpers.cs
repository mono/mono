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

using System.Reflection;
namespace System.Runtime.CompilerServices {
    /// <summary>
    /// Class that contains helper methods for DLR CallSites.
    /// </summary>
    public static class CallSiteHelpers {
        private static Type _knownNonDynamicMethodType = typeof(object).GetMethod("ToString").GetType();

        /// <summary>
        /// Checks if a <see cref="MethodBase"/> is internally used by DLR and should not
        /// be displayed on the language code's stack.
        /// </summary>
        /// <param name="mb">The input <see cref="MethodBase"/></param>
        /// <returns>
        /// True if the input <see cref="MethodBase"/> is internally used by DLR and should not
        /// be displayed on the language code's stack. Otherwise, false.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool IsInternalFrame(MethodBase mb) {
            //All the dynamic methods created for DLR rules have a special name.
            //We also check if the method has a different type than the known
            //non-static method. If it does, it is a dynamic method.
            //This could be improved if the CLR provides a way to attach some information
            //to the dynamic method we create, like CustomAttributes.
            if (mb.Name == "CallSite.Target" && mb.GetType() != _knownNonDynamicMethodType) {
                return true;
            }

            //Filter out the helper methods.
            if (mb.DeclaringType == typeof(System.Dynamic.UpdateDelegates)) {
                return true;
            }

            return false;
        }
    }
}
