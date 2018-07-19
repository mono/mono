//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.InfoCards.Diagnostics;

   [Serializable]
   public class IdentityValidationException : System.Exception
   {
       public IdentityValidationException()
           : base()
       {
       }

       public IdentityValidationException( string message )
           : base( message )
       {
       }

       public IdentityValidationException( string message, Exception innerException )
           : base( message, innerException )
       {
       }

       protected IdentityValidationException( SerializationInfo info, StreamingContext context )
           : base( info, context )
       {
       }
}
}
