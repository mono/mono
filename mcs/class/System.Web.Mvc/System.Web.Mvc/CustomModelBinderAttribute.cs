/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;

    [AttributeUsage(ValidTargets, AllowMultiple = false, Inherited = false)]
    public abstract class CustomModelBinderAttribute : Attribute {

        internal const AttributeTargets ValidTargets = AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Struct;

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This method can potentially perform a non-trivial amount of work.")]
        public abstract IModelBinder GetBinder();

    }
}
