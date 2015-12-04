//------------------------------------------------------------------------------
// <copyright file="SqlPipe.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">daltodov</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Sql;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics;

    // SqlPipe
    //    Abstraction of TDS data/message channel exposed to user.
    public sealed class SqlPipe {

        SmiContext              _smiContext;
        SmiRecordBuffer         _recordBufferSent;          // Last recordBuffer sent to pipe (for push model SendEnd).
        SqlMetaData[]           _metaDataSent;              // Metadata of last resultset started (for push model). Overloaded to indicate if push started or not (non-null/null)
        SmiEventSink_Default    _eventSink;                 // Eventsink to use when calling SmiContext entrypoints
        bool                    _isBusy;                    // Is this pipe currently handling an operation?
        bool                    _hadErrorInResultSet;       // true if an exception was thrown from within various bodies; used to control cleanup during SendResultsEnd


        internal SqlPipe( SmiContext smiContext ) {
            _smiContext = smiContext;
            _eventSink = new SmiEventSink_Default();
        }


        //
        // Public methods
        //
        public void ExecuteAndSend( SqlCommand command ) {
            SetPipeBusy( );
            try {
                EnsureNormalSendValid( "ExecuteAndSend" );

                if ( null == command ) {
                    throw ADP.ArgumentNull( "command" );
                }

                SqlConnection connection = command.Connection;

                // if the command doesn't have a connection set up, try to set one up on it's behalf
                if ( null == connection ) {
                    using ( SqlConnection newConnection = new SqlConnection( "Context Connection=true" ) ) {
                        newConnection.Open( );

                        // use try-finally to restore command's connection property to it's original state
                        try {
                            command.Connection = newConnection;
                            command.ExecuteToPipe( _smiContext );
                        }
                        finally {
                            command.Connection = null;
                        }
                    }
                }
                else {
                    // validate connection state
                    if ( ConnectionState.Open != connection.State ) {
                        throw ADP.ClosedConnectionError();
                    }

                    // validate connection is current scope's connection
                    SqlInternalConnectionSmi internalConnection = connection.InnerConnection as SqlInternalConnectionSmi;

                    if ( null == internalConnection ) {
                        throw SQL.SqlPipeCommandHookedUpToNonContextConnection( );
                    }

                    command.ExecuteToPipe( _smiContext );
                }
            }
            finally {
                ClearPipeBusy( );
            }
        }
       
        // Equivalent to TSQL PRINT statement -- sends an info-only message.
        public void Send( string message ) {
            ADP.CheckArgumentNull(message, "message");

            if ( SmiMetaData.MaxUnicodeCharacters < message.Length ) {
                throw SQL.SqlPipeMessageTooLong( message.Length );
            }

            SetPipeBusy( );
            try {
                EnsureNormalSendValid( "Send" );

                _smiContext.SendMessageToPipe( message, _eventSink );

                // Handle any errors that are reported.
                _eventSink.ProcessMessagesAndThrow();
            }
            catch {
                _eventSink.CleanMessages();
                throw;
            }
            finally {
                ClearPipeBusy( );
                Debug.Assert(_eventSink.HasMessages == false, "There should be no messages left in _eventsink at the end of the Send message!");
            }
        }

        // Send results from SqlDataReader
        public void Send( SqlDataReader reader ) {
            ADP.CheckArgumentNull(reader, "reader");

            SetPipeBusy( );
            try {
                EnsureNormalSendValid( "Send" );
                do {
                    SmiExtendedMetaData[] columnMetaData = reader.GetInternalSmiMetaData();

                    if (null != columnMetaData && 0 != columnMetaData.Length) { // SQLBUDT #340528 -- don't send empty results.
                        using ( SmiRecordBuffer recordBuffer = _smiContext.CreateRecordBuffer(columnMetaData, _eventSink) ) {
                            _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.


                            _smiContext.SendResultsStartToPipe( recordBuffer, _eventSink );
                            _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.

                            try {
                                while( reader.Read( ) ) {
                                    if (SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion) {
                                        ValueUtilsSmi.FillCompatibleSettersFromReader(_eventSink, recordBuffer, new List<SmiExtendedMetaData>(columnMetaData), reader);
                                    }
                                    else {
                                        ValueUtilsSmi.FillCompatibleITypedSettersFromReader(_eventSink, recordBuffer, columnMetaData, reader);
                                    }
                                    
                                    _smiContext.SendResultsRowToPipe( recordBuffer, _eventSink );
                                    _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.
                                }
                            }
                            finally {
                                _smiContext.SendResultsEndToPipe( recordBuffer, _eventSink );
                                _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.
                            }
                        }
                    }
                }
                while ( reader.NextResult( ) );
            }
            catch { 
                _eventSink.CleanMessages();
                throw;
            }
            finally {
                ClearPipeBusy( );
                Debug.Assert(_eventSink.HasMessages == false, "There should be no messages left in _eventsink at the end of the Send reader!");
            }
        }

        public void Send( SqlDataRecord record ) {
            ADP.CheckArgumentNull(record, "record");

            SetPipeBusy( );
            try {
                EnsureNormalSendValid( "Send" );

                if (0 != record.FieldCount) { // SQLBUDT #340564 -- don't send empty records.
                
                    SmiRecordBuffer recordBuffer;
                    if (record.RecordContext == _smiContext) {
                        recordBuffer = record.RecordBuffer;
                    } else {    // SendResultsRowToPipe() only takes a RecordBuffer created by an SmiContext
                        SmiExtendedMetaData[] columnMetaData = record.InternalGetSmiMetaData();
                        recordBuffer = _smiContext.CreateRecordBuffer(columnMetaData, _eventSink);
                        if (SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion) {
                            ValueUtilsSmi.FillCompatibleSettersFromRecord(_eventSink, recordBuffer, columnMetaData, record, null /* no default values */);
                        }
                        else {
                            ValueUtilsSmi.FillCompatibleITypedSettersFromRecord(_eventSink, recordBuffer, columnMetaData, record);
                        }
                    }

                    _smiContext.SendResultsStartToPipe( recordBuffer, _eventSink );
                    _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.

                    //  If SendResultsStartToPipe succeeded, then SendResultsEndToPipe must be called.
                    try {
                        _smiContext.SendResultsRowToPipe( recordBuffer, _eventSink );
                        _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.
                    }
                    finally {
                        _smiContext.SendResultsEndToPipe( recordBuffer, _eventSink );
                        _eventSink.ProcessMessagesAndThrow(); // Handle any errors that are reported.
                    }
                }
            }
            catch {
                // VSDD 479525: if exception happens (e.g. SendResultsStartToPipe throw OutOfMemory), _eventSink may not be empty,
                // which will affect server's behavior if the next call successes (previous exception is still in the eventSink, 
                // will be throwed). So we need to clean _eventSink.
                _eventSink.CleanMessages();
                throw;
            }
            finally {
                ClearPipeBusy( );
                Debug.Assert(_eventSink.HasMessages == false, "There should be no messages left in _eventsink at the end of the Send record!");
            }
        }

        public void SendResultsStart( SqlDataRecord record ) {
            ADP.CheckArgumentNull(record, "record");

            SetPipeBusy( );
            try {
                EnsureNormalSendValid( "SendResultsStart" );

                SmiRecordBuffer recordBuffer = record.RecordBuffer;
                if (record.RecordContext == _smiContext) {
                    recordBuffer = record.RecordBuffer;
                } else {
                    recordBuffer = _smiContext.CreateRecordBuffer(record.InternalGetSmiMetaData(), _eventSink);    // Only MetaData needed for sending start
                }
                _smiContext.SendResultsStartToPipe( recordBuffer, _eventSink );

                // Handle any errors that are reported.
                _eventSink.ProcessMessagesAndThrow();

                // remember sent buffer info so it can be used in send row/end.
                _recordBufferSent = recordBuffer;
                _metaDataSent = record.InternalGetMetaData();
            }
            catch {
                _eventSink.CleanMessages();
                throw;
            }
            finally {
                ClearPipeBusy( );
                Debug.Assert(_eventSink.HasMessages == false, "There should be no messages left in _eventsink at the end of the SendResultsStart!");
            }
        }

        public void SendResultsRow( SqlDataRecord record ) {
            ADP.CheckArgumentNull(record, "record");

            SetPipeBusy( );
            try {
                EnsureResultStarted( "SendResultsRow" );

                if ( _hadErrorInResultSet ) {
                    throw SQL.SqlPipeErrorRequiresSendEnd();
                }

                // Assume error state unless cleared below
                _hadErrorInResultSet = true;

                SmiRecordBuffer recordBuffer;
                if (record.RecordContext == _smiContext) {
                    recordBuffer = record.RecordBuffer;
                } else {
                    SmiExtendedMetaData[] columnMetaData = record.InternalGetSmiMetaData();
                    recordBuffer = _smiContext.CreateRecordBuffer(columnMetaData, _eventSink);
                    if (SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion) {
                        ValueUtilsSmi.FillCompatibleSettersFromRecord(_eventSink, recordBuffer, columnMetaData, record, null /* no default values */);
                    }
                    else {
                        ValueUtilsSmi.FillCompatibleITypedSettersFromRecord(_eventSink, recordBuffer, columnMetaData, record);
                    }
                }
                _smiContext.SendResultsRowToPipe( recordBuffer, _eventSink );

                // Handle any errors that are reported.
                _eventSink.ProcessMessagesAndThrow();

                // We successfully traversed the send, clear error state
                _hadErrorInResultSet = false;
            }
            catch {
                _eventSink.CleanMessages();
                throw;
            }
            finally {
                ClearPipeBusy( );
                Debug.Assert(_eventSink.HasMessages == false, "There should be no messages left in _eventsink at the end of the SendResultsRow!");
            }
        }

        public void SendResultsEnd( ) {
            SetPipeBusy( );
            try {
                EnsureResultStarted( "SendResultsEnd" );

                _smiContext.SendResultsEndToPipe( _recordBufferSent, _eventSink );

                // Once end called down to native code, assume end of resultset
                _metaDataSent = null;
                _recordBufferSent = null;
                _hadErrorInResultSet = false;

                // Handle any errors that are reported.
                _eventSink.ProcessMessagesAndThrow();
            }
            catch {
                _eventSink.CleanMessages();
                throw;
            }
            finally {
                ClearPipeBusy( );
                Debug.Assert(_eventSink.HasMessages == false, "There should be no messages left in _eventsink at the end of the SendResultsEnd!");
            }
        }

        // This isn't speced, but it may not be a bad idea to implement...
        public bool IsSendingResults {
            get {
                return null != _metaDataSent;
            }
        }

        internal void OnOutOfScope( ) {
            _metaDataSent = null;
            _recordBufferSent = null;
            _hadErrorInResultSet = false;
            _isBusy = false;
        }

        // Pipe busy status.
        //    Ensures user code cannot call any APIs while a send is in progress.
        //
        //    Public methods must call this method before sending anything to the unmanaged pipe.
        //    Once busy status is set, it must clear before returning from the calling method
        //        ( i.e. clear should be in a finally block).
        private void SetPipeBusy( ) {
            if ( _isBusy ) {
                throw SQL.SqlPipeIsBusy( );
            }
            _isBusy = true;
        }

        // Clear the pipe's busy status.
        private void ClearPipeBusy( ) {
            _isBusy = false;
        }

        //
        // State validation
        //    One of the Ensure* validation methods should appear at the top of every public method
        //

        // Default validation method
        //    Ensures Pipe is not currently transmitting a push-model resultset
        private void EnsureNormalSendValid( string methodName ) {
            if ( IsSendingResults ) {
                throw SQL.SqlPipeAlreadyHasAnOpenResultSet( methodName );
            }
        }

        private void EnsureResultStarted( string methodName ) {
            if ( !IsSendingResults ) {
                throw SQL.SqlPipeDoesNotHaveAnOpenResultSet( methodName );
            }
        }
    }
}


