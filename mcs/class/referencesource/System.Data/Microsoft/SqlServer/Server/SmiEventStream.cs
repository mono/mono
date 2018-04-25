//------------------------------------------------------------------------------
// <copyright file="SmiEventStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;

    internal abstract class SmiEventStream : IDisposable {

        internal abstract bool HasEvents { get; }

        internal abstract void Close( SmiEventSink sink );

        public virtual void Dispose( ) {
            // Obsoleting from SMI -- use Close instead.
            //  Intended to be removed (along with inheriting IDisposable) prior to RTM.

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V3+ and dropped support for V2-.
            //  2) Server didn't implement V2- on some interface and negotiated V2-.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
        }

        internal abstract void ProcessEvent( SmiEventSink sink );
    }
}
