//------------------------------------------------------------------------------
// <copyright file="SettingsPropertyWrongTypeException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;
    using  System.Collections;

   ////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////
   [Serializable]
   public class SettingsPropertyWrongTypeException : Exception {
       public SettingsPropertyWrongTypeException(String message)
           : base(message)
       {
       }

       public SettingsPropertyWrongTypeException(String message, Exception innerException)
            : base(message, innerException)
       {
       }

       protected SettingsPropertyWrongTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
       {
       }

       public SettingsPropertyWrongTypeException()
       { }
   }
}
