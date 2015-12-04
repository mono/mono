//------------------------------------------------------------------------------
// <copyright file="SmiContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Security.Principal;

    // NOTE: connection, transaction and context pipe operations could be 
    //       encapsulated in their own classes, and should if they get complex 
    //       (transaction is borderline at this point).
    internal abstract class SmiContext {

        internal abstract event EventHandler OutOfScope;

        internal abstract SmiConnection ContextConnection { get; }

        internal abstract long ContextTransactionId { get; }

        internal abstract System.Transactions.Transaction ContextTransaction { get; }

        internal abstract bool HasContextPipe { get; }

        internal abstract WindowsIdentity WindowsIdentity { get; }
        
        internal abstract SmiRecordBuffer CreateRecordBuffer (
            SmiExtendedMetaData[]   columnMetaData,     // Extended metadata because it requires names, udttypename and xmlschemaname ignored
            SmiEventSink            eventSink
        );

        internal abstract SmiRequestExecutor CreateRequestExecutor (
            string                  commandText,
            CommandType             commandType,
            SmiParameterMetaData[]  parameterMetaData,
            SmiEventSink            eventSink
        );

        // 

        internal abstract object GetContextValue ( int key );

        internal abstract void GetTriggerInfo (
            SmiEventSink            eventSink,
            out bool[]              columnsUpdated,
            out TriggerAction       action,
            out SqlXml              eventInstanceData
        );

        internal abstract void SendMessageToPipe( string message, SmiEventSink eventSink );

        internal abstract void SendResultsStartToPipe( SmiRecordBuffer recordBuffer, SmiEventSink eventSink );

        internal abstract void SendResultsRowToPipe( SmiRecordBuffer recordBuffer, SmiEventSink eventSink );

        internal abstract void SendResultsEndToPipe( SmiRecordBuffer recordBuffer, SmiEventSink eventSink );

        internal abstract void SetContextValue ( int key, object value );

        // Scratch LOB storage region
        internal virtual SmiStream GetScratchStream( SmiEventSink sink ) {
            // Adding as of V3

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V2- and hasn't implemented V3 yet.
            //  2) Server didn't implement V3, but negotiated V3+.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
            return null;
        }
    }
}

