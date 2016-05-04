//------------------------------------------------------------------------------
// <copyright file="SmiEventSink_DeferedProcessing.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System.Data.Sql;
    using System.Data.SqlClient;
    using System.Diagnostics;

    // This class exists purely to defer processing of messages until a later time.
    //  It is designed to allow calling common code that interacts with the SMI layers
    //  without throwing or otherwise processing messages in the sink until later on.
    //
    //  Main example:
    //      SqlCommand.ExecuteNonQuerySmi calls EventStream.ProcessEvent with it's command event sink (CES)
    //          ProcessEvent calls OnParametersAvailable on the CES
    //              OnParametersAvailable sets up a deferedprocessing event sink (DPES) with the CES as its parent
    //              OnParametersAvailable calls ValueUtils to extract param values passing the DPES
    //                  ValueUtils calls Smi passing DPES
    //                      Smi may call MessagePosted, which will send a message up the sink parent chain and save it.
    //                  ValueUtils calls ProcessMessagesAndThrow on DPES, which skips handling
    //      ... return up the stack ...
    //      SqlCommand.ExecuteNonQuerySmi calls CES.ProcessMessagesAndThrow, which handles the messages
    //              sent from the Smi value extraction code.
    //
    //  IMPORTANT: Code that uses the DeferedProccess event sink is responsible for ensuring that
    //  these messages ARE processed at some point.
    internal class SmiEventSink_DeferedProcessing : SmiEventSink_Default {
        internal SmiEventSink_DeferedProcessing ( SmiEventSink parent ) : base(parent) {
        }

        protected override void DispatchMessages(bool ignoreNonFatalMessages) {
            // Skip processing messages.  Since messages are sent to parent and calling code will call 
            //  ProcessMessages against parent, messages ARE NOT LOST!
        }
    }
}

