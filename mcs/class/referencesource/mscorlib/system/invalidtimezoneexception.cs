//------------------------------------------------------------------------------
// <copyright file="InvalidTimeZoneException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System {
   using  System.Runtime.Serialization;
   using  System.Runtime.CompilerServices;

   [Serializable]
   [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
#if MOBILE
   [TypeForwardedFrom("System.Core, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e")]
#else
   [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
#endif
   public class InvalidTimeZoneException : Exception {
       public InvalidTimeZoneException(String message)
           : base(message) { }

       public InvalidTimeZoneException(String message, Exception innerException)
           : base(message, innerException) { }

       protected InvalidTimeZoneException(SerializationInfo info, StreamingContext context)
           : base(info, context) { }

       public InvalidTimeZoneException() { }
   }
}
