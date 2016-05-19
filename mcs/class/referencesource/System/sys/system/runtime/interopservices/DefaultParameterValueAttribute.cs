// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Runtime.InteropServices {

    using System;

    //
    // The DefaultParameterValueAttribute is used in C# to set 
    // the default value for parameters when calling methods
    // from other languages. This is particularly useful for 
    // methods defined in COM interop interfaces.
    //
    [AttributeUsageAttribute(AttributeTargets.Parameter)]
    public sealed class DefaultParameterValueAttribute : System.Attribute
    {
         public DefaultParameterValueAttribute(object value)
         {
             this.value = value;
         }

         public object Value { get { return this.value; } }

         private object value;
    }
}
