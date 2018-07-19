//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.InfoCards.Diagnostics
{
    using System;
    using System.Xml;
    using System.Diagnostics;
    
    //
    // Summary
    // An InfoCardTraceRecord represents an ETW tracerecord plus some infocard specific
    // schema information. The class is called back by the diagnostics infrastructure through
    // its WriteTo() method in order to serialize the infocard specific contents into the traceRecord structure.
    // as part of a tracing request. the TraceRecord base class is repsonsible for embedding the correct headers etc.
    //
    // Trace records look like this:

    //
    //<TraceRecord xmlns="http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord" Severity="Critical">
    //  <TraceIdentifier>StoreSignatureCollision</TraceIdentifier>
    //  <Description>rabbits</Description>
    //  <ResourceReference>http://schemas.microsoft.com/2004/03/System/AppDomain/{2bd64add-212d-4385-9f8e-6d9ab976c182}</ResourceReference>
    //  <ExtendedData xmlns="http://schemas.microsoft.com/2004/11/InfoCard/StoreSignatureCollisionTraceRecord">
    //      <message>rabbit%s</message>
    //  </ExtendedData>
    //</TraceRecord>
    // 
    class InfoCardTraceRecord : System.Runtime.Diagnostics.TraceRecord
    {
        //
        // The eventID, a string representation of the traceCode. Normally something like
        // 'StoreSignatureCollision' - used to derive the trace uri.
        //
        private string m_eventID;
        
        //
        // A descriptive message about the error schematized as xmlAny
        //
        private string m_message;
        
        
        const string InfoCardEventIdBase = "http://schemas.microsoft.com/2004/11/InfoCard/";
        
        public InfoCardTraceRecord( string eventID, string message )
        {
            InfoCardTrace.Assert( !String.IsNullOrEmpty( eventID ), "null eventid" );
            InfoCardTrace.Assert( !String.IsNullOrEmpty( message ), "null message" );
            m_eventID = eventID;
            m_message = message;
        }

        
        //
        // Summary:
        // Returns the unique identifier for this event. Represented as a uri under the stanard e2e logging 
        // schema - configured as <uriPath> + <infocard event code> + <standard suffix>
        // for example
        // "http://schemas.microsoft.com/2004/11/InfoCard/" + "StoreSignatureCollision" + TraceRecord
        //
        internal override string EventId
        {
            get
            {
                return InfoCardEventIdBase + m_eventID + System.Runtime.Diagnostics.TraceRecord.NamespaceSuffix;
            }
        }
    
        //
        // Summary:
        // Called back by the indigo diagnostic trace infrastructure during etw trace logging. 
        // Writes the extendedData section out to the TraceRecord.
        //
        //
        internal override void WriteTo( XmlWriter writer )
        {
            InfoCardTrace.Assert( null != writer, "null writer" );
            writer.WriteElementString( "message", m_message );
        }
        
        //
        // Override tostring to give a better event logging experience.
        //
        public override string ToString()
        {
            return SR.GetString( SR.EventLogMessage, m_eventID, m_message );
        }
    }
}
