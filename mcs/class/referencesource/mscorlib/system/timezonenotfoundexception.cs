//------------------------------------------------------------------------------
// <copyright file="TimeZoneNotFoundException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System {
   using  System.Runtime.Serialization;
   using  System.Runtime.CompilerServices;

   [Serializable]
#if MOBILE
   [TypeForwardedFrom("System.Core, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e")]
#else
   [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
#endif
   [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
   public class TimeZoneNotFoundException : Exception {
       public TimeZoneNotFoundException(String message)
           : base(message) { }

       public TimeZoneNotFoundException(String message, Exception innerException)
           : base(message, innerException) { }

       protected TimeZoneNotFoundException(SerializationInfo info, StreamingContext context)
           : base(info, context) { }

       public TimeZoneNotFoundException() { }
   }
}
