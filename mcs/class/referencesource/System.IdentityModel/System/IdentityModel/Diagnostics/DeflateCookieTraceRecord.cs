//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Diagnostics
{
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    /// <summary>
    /// This trace is used to when DeflateCookieTransform.Encode() is called.
    /// Traces:
    ///     original size
    ///     deflated size
    /// Helps users to determine if the compression algorithim is of value.
    /// </summary>
    internal class DeflateCookieTraceRecord : TraceRecord
    {
        const string ElementName = "DeflateCookieTraceRecord";
        const string _eventId = TraceRecord.EventIdBase + ElementName;

        int _originalSize;
        int _deflatedSize;

        public DeflateCookieTraceRecord( int originalSize, int deflatedSize )
        {
            _originalSize = originalSize;
            _deflatedSize = deflatedSize;
        }

        internal override string EventId
        {
            get { return _eventId; }
        }

        internal override void WriteTo( XmlWriter writer ) 
        {
            writer.WriteStartElement( ElementName );
            writer.WriteAttributeString( DiagnosticStrings.NamespaceTag, EventId );

            writer.WriteElementString( DiagnosticStrings.DeflateCookieOriginalSizeTag, _originalSize.ToString( CultureInfo.InvariantCulture ) );
            writer.WriteElementString( DiagnosticStrings.DeflateCookieAfterDeflatingTag, _deflatedSize.ToString( CultureInfo.InvariantCulture ) );

            writer.WriteEndElement();
        }                     
    }

}
