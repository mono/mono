// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

namespace System.Security {
    using System;
    using System.Runtime.Serialization;
    using System.Globalization;

    [System.Runtime.InteropServices.ComVisible(true)]
    [Serializable] sealed public class XmlSyntaxException : SystemException
    {
        public
        XmlSyntaxException ()
            : base (Environment.GetResourceString( "XMLSyntax_InvalidSyntax" ))
        {
            SetErrorCode(__HResults.CORSEC_E_XMLSYNTAX);
        }

        public
        XmlSyntaxException (String message)
            : base (message)
        {
            SetErrorCode(__HResults.CORSEC_E_XMLSYNTAX);
        }

        public
        XmlSyntaxException (String message, Exception inner)
            : base (message, inner)
        {
            SetErrorCode(__HResults.CORSEC_E_XMLSYNTAX);
        }

        public
        XmlSyntaxException (int lineNumber)
            : base (String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "XMLSyntax_SyntaxError" ), lineNumber ) )
        {
            SetErrorCode(__HResults.CORSEC_E_XMLSYNTAX);
        }

        public
        XmlSyntaxException( int lineNumber, String message )
            : base( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "XMLSyntax_SyntaxErrorEx" ), lineNumber, message ) )
        {
            SetErrorCode(__HResults.CORSEC_E_XMLSYNTAX);
        }

        internal XmlSyntaxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
