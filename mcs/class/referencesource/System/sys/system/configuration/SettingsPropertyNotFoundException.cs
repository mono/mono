//------------------------------------------------------------------------------
// <copyright file="SettingsPropertyNotFoundException.cs" company="Microsoft">
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
   ////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////
   [Serializable]
   public class SettingsPropertyNotFoundException : Exception
   {
       public SettingsPropertyNotFoundException(String message)
           : base(message)
       {
       }

       public SettingsPropertyNotFoundException(String message, Exception innerException)
           : base(message, innerException)
       {
       }

       protected SettingsPropertyNotFoundException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }

       public SettingsPropertyNotFoundException()
       { }
   }
}
