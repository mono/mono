//------------------------------------------------------------------------------
// <copyright file="DoNotResetAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;

    // If this attribute is applied to a field, it will not be reset by a call to ReflectionUtil.Reset().
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class DoNotResetAttribute : Attribute {
    }

}
