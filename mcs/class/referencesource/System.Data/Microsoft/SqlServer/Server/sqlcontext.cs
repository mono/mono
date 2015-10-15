//------------------------------------------------------------------------------
// <copyright file="SqlContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="true" primary="false">daltodov</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Security.Principal;

    public sealed class SqlContext {

        // There are no publicly visible instance methods/properties on SqlContext.
        //  With the current design, the user should never get an actual instance of
        //  this class.  Instances are only used internally to hold owned objects
        //  such as SqlPipe and SqlTriggerContext.
        
        private SmiContext        _smiContext;
        private SqlPipe           _pipe;
        private SqlTriggerContext _triggerContext;

        private SqlContext( SmiContext smiContext ) {
            _smiContext = smiContext;
            _smiContext.OutOfScope += new EventHandler(OnOutOfScope);
        }

        //
        //  Public API
        //

        public static bool IsAvailable { 
            get {
                bool result = InOutOfProcHelper.InProc;
                return result;
            }
        }
        
        // Get the SqlPipe (if any) for the current scope.
        public static SqlPipe Pipe {
            get {
                return CurrentContext.InstancePipe;
            }
        }

        // Get the SqlTriggerContext (if any) for the current scope.
        public static SqlTriggerContext TriggerContext {
            get {
                return CurrentContext.InstanceTriggerContext;
            }
        }

        public static WindowsIdentity WindowsIdentity{
            get {
                return CurrentContext.InstanceWindowsIdentity;
            }
        }

        //
        // Internal class methods
        //

        // CurrentContext should be the *only* way to get to an instance of SqlContext.
        private static SqlContext CurrentContext {
            get {
                SmiContext smiContext = SmiContextFactory.Instance.GetCurrentContext();

                SqlContext result = (SqlContext)smiContext.GetContextValue( (int)SmiContextFactory.ContextKey.SqlContext );

                if ( null == result ) {
                    result = new SqlContext( smiContext );
                    smiContext.SetContextValue( (int)SmiContextFactory.ContextKey.SqlContext, result );
                }

                return result;
            }
        }

        //
        //  Internal instance methods
        //
        private SqlPipe InstancePipe {
            get {
                if ( null == _pipe && _smiContext.HasContextPipe ) {
                    _pipe = new SqlPipe( _smiContext );
                }

                Debug.Assert( null == _pipe || _smiContext.HasContextPipe, "Caching logic error for contained pipe!" );

                return _pipe;
            }
        }

        private SqlTriggerContext InstanceTriggerContext {
            get {
                if ( null == _triggerContext ) {
                    bool[]               columnsUpdated;
                    TriggerAction        triggerAction;
                    SqlXml               eventInstanceData;
                    SmiEventSink_Default eventSink = new SmiEventSink_Default();
                
                    _smiContext.GetTriggerInfo(eventSink, out columnsUpdated, out triggerAction, out eventInstanceData);

                    eventSink.ProcessMessagesAndThrow();

                    if (TriggerAction.Invalid != triggerAction) {
                        _triggerContext = new SqlTriggerContext( triggerAction, columnsUpdated, eventInstanceData );
                    }
                }

                return _triggerContext;
            }
        }

        private WindowsIdentity InstanceWindowsIdentity {
            get {
                return _smiContext.WindowsIdentity;
            }
        }

        // Called whenever the context goes out of scope, we need to make
        // sure that we release internal state, such as the pipe's record buffer
        private void OnOutOfScope( object s, EventArgs e ) {
            if (Bid.AdvancedOn) {
                Bid.Trace( "<sc.SqlContext.OutOfScope|ADV> SqlContext is out of scope\n" );
            }

            if ( null != _pipe ) {
                _pipe.OnOutOfScope();
            }

            _triggerContext = null;
        }
    }
}
