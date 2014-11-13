//------------------------------------------------------------------------------
// <copyright file="SmiEventSink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;
    using System.Data;
    using System.Data.Sql;

    // SqlEventSink is implemented by calling code.  In all methods that accept
    // a SqlEventSink directly the sink must be able to handle multiple callbacks 
    // without control returning from the original call.

    // Methods that do not accept SmiEventSync are (generally) ProcessEvent on
    // the SmiEventStream methods returning a SmiEventStream and methods that 
    // are certain to never call to the server (most will, for in-proc back end). 

    // Methods are commented with their corresponding TDS token

    // NOTE: Throwing from these methods will not usually produce the desired
    //       effect -- the managed to native boundary will eat any exceptions,
    //       and will cause a simple "Something bad happened" exception to be
    //       thrown in the native to managed boundary...
    internal abstract class SmiEventSink {

        #region Active methods

        // Called at end of stream whether errors or no
        internal abstract void BatchCompleted( );

        // Called zero or one time when output parameters are available (errors could prevent event from occuring)
        internal virtual void ParameterAvailable(SmiParameterMetaData metaData, SmiTypedGetterSetter paramValue, int ordinal) {
            // Adding as of V200

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V3- and hasn't implemented V200 yet.
            //  2) Server didn't implement V200 on some interface, but negotiated V200+.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
        }

        // Called when the server database context changes (ENVCHANGE token)
        internal abstract void DefaultDatabaseChanged( string databaseName );

        // Called for messages and errors (ERROR and INFO tokens)
        internal abstract void MessagePosted ( int number, byte state, byte errorClass, string server, string message, string procedure, int lineNumber );

        // Called for new resultset starting (COLMETADATA token)
        internal abstract void MetaDataAvailable( SmiQueryMetaData[] metaData, bool nextEventIsRow );


        internal virtual void RowAvailable(SmiTypedGetterSetter rowData) {
            // Adding as of V200

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V3- and hasn't implemented V200 yet.
            //  2) Server didn't implement V200 on some interface, but negotiated V200+.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
        }

        // Called when any statement completes on server (DONE token)
        internal abstract void StatementCompleted( int rowsAffected );

        // Called when a transaction is commited (ENVCHANGE token)
        internal abstract void TransactionCommitted( long transactionId );

        // Called when a transaction is commited (ENVCHANGE token)
        internal abstract void TransactionDefected( long transactionId );

        // Called when a transaction is commited (ENVCHANGE token)
        internal abstract void TransactionEnlisted( long transactionId );

        // Called when a transaction is forcibly ended in the server, not requested
        // by the provider's batch (ENVCHANGE token)
        internal abstract void TransactionEnded( long transactionId );

        // Called when a transaction is rolled back (ENVCHANGE token)
        internal abstract void TransactionRolledBack( long transactionId );

        // Called when a transaction is started (ENVCHANGE token)
        internal abstract void TransactionStarted( long transactionId );

        #endregion

        #region OBSOLETE METHODS
        #region OBSOLETED as of V200 but active in previous version
        // Called zero or one time when output parameters are available (errors could prevent event from occuring)
        internal virtual void ParametersAvailable( SmiParameterMetaData[] metaData, ITypedGettersV3 paramValues ) {
            // Adding as of V3
            // Obsoleting as of V200

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V200+ and dropped support for V200-.
            //  2) Server didn't implement V3- on some interface and negotiated V3-.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
        }

        // Called when a new row arrives (ROW token)
        internal virtual void RowAvailable( ITypedGettersV3 rowData ) {
            // Adding as of V3
            // Obsoleting as of V200

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V200+ and dropped support for V200-.
            //  2) Server didn't implement V3- on some interface and negotiated V3-.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
        }

        #endregion

        #region OBSOLETED and never shipped (without ObsoleteAttribute)
        // Called when a new row arrives (ROW token)
        internal virtual void RowAvailable( ITypedGetters rowData ) {
            // Obsoleting from SMI -- use end of dispose that takes an event sink instead.
            //  Intended to be removed (along with inheriting IDisposable) prior to RTM.

            // Implement body with throw because there are only a couple of ways to get to this code:
            //  1) Client is calling this method even though the server negotiated for V3+ and dropped support for V2-.
            //  2) Server didn't implement V2- on some interface and negotiated V2-.
            System.Data.Common.ADP.InternalError( System.Data.Common.ADP.InternalErrorCode.UnimplementedSMIMethod );
        }

        #endregion
        #endregion
    }
}
    
