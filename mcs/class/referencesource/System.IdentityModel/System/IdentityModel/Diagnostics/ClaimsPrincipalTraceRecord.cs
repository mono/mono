//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Diagnostics
{
    using System.Runtime.Diagnostics;
    using System.Security.Claims;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    /// <summary>
    /// This trace is used to trace a claims principal
    /// Traces:
    ///     Name
    ///     For each Identity:
    ///         Name
    ///         NameClaimType
    ///         RoleClaimType
    ///         Label
    ///         Actor (if present)
    ///         Details about each claim
    /// </summary>
    internal class ClaimsPrincipalTraceRecord : TraceRecord
    {
        internal const string ElementName = "ClaimsPrincipalTraceRecord";
        internal const string _eventId = TraceRecord.EventIdBase + ElementName;

        ClaimsPrincipal _claimsPrincipal;

        public ClaimsPrincipalTraceRecord( ClaimsPrincipal claimsPrincipal )
        {
            _claimsPrincipal = claimsPrincipal;
        }

        internal override string EventId
        {
            get { return ClaimsPrincipalTraceRecord._eventId; }
        }

        internal override void WriteTo( XmlWriter writer ) 
        {
            writer.WriteStartElement( ElementName );
            writer.WriteAttributeString( DiagnosticStrings.NamespaceTag, EventId );

            writer.WriteStartElement( "ClaimsPrincipal");
            writer.WriteAttributeString( "Identity.Name", _claimsPrincipal.Identity.Name );

            foreach ( ClaimsIdentity ci in _claimsPrincipal.Identities )
            {
                WriteClaimsIdentity( ci, writer );
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteClaimsIdentity( ClaimsIdentity ci, XmlWriter writer )
        {
            writer.WriteStartElement( "ClaimsIdentity" );
            writer.WriteAttributeString( "Name", ci.Name );
            writer.WriteAttributeString( "NameClaimType", ci.NameClaimType );
            writer.WriteAttributeString( "RoleClaimType", ci.RoleClaimType );
            writer.WriteAttributeString( "Label", ci.Label );

            if ( ci.Actor != null )
            {
                writer.WriteStartElement( "Actor" );
                WriteClaimsIdentity( ci.Actor, writer );
                writer.WriteEndElement();
            }

            foreach ( Claim c in ci.Claims )
            {
                writer.WriteStartElement( "Claim" );
                writer.WriteAttributeString( "Value", c.Value );
                writer.WriteAttributeString( "Type", c.Type );
                writer.WriteAttributeString( "ValueType", c.ValueType );
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }

}
