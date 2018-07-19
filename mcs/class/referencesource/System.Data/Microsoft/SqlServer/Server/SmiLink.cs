//------------------------------------------------------------------------------
// <copyright file="SmiLink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">alazela</owner>
// <owner current="true" primary="false">billin</owner>
//------------------------------------------------------------------------------

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SqlAccess, PublicKey=0024000004800000940000000602000000240000525341310004000001000100272736ad6e5f9586bac2d531eabc3acc666c2f8ec879fa94f8f7b0327d2ff2ed523448f83c3d5c5dd2dfc7bc99c5286b2c125117bf5cbe242b9d41750732b2bdffe649c6efb8e5526d526fdd130095ecdb7bf210809c6cdad8824faa9ac0310ac3cba2aa0523567b2dfa7fe250b30facbd62d4ec99b94ac47c7d3b28f1f6e4c8")] // SQLBU 437687
        
namespace Microsoft.SqlServer.Server {

    using System;

    internal abstract class SmiLink {

        // NOTE: Except for changing the constant value below, adding new members
        //       to this class may create versioning issues.  We would prefer not
        //       to add additional items to this if possible.

        internal const ulong InterfaceVersion = 210;

        // Version negotiation (appdomain-wide negotiation)
        //    This needs to be the first method called when negotiating with a
        //    driver back end.  Once called, the rest of the back end interface 
        //    needs to conform to the returned version number's interface.
        internal abstract ulong NegotiateVersion( ulong requestedVersion );

        // Get utility class valid in current thread execution environment
        //    This returns an object, to allow us to version the context without
        //    having to version this class.  (eg. SmiContext1 vs SmiContext2)
        internal abstract object GetCurrentContext( SmiEventSink eventSink );
    }
}
