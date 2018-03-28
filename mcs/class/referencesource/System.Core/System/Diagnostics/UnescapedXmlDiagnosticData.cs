//------------------------------------------------------------------------------
// <copyright file="EventSchemaTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace System.Diagnostics {

// This is just a type to tag XML data; we don't do any welformedness check or any such validation
[System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
public class UnescapedXmlDiagnosticData {
    string _xmlString;
    
    public UnescapedXmlDiagnosticData (String xmlPayload) {
        _xmlString = xmlPayload;
        if (_xmlString == null) {
            _xmlString = String.Empty;
        }
    }

    public String UnescapedXml { 
        get {return _xmlString;} 
        set {_xmlString = value;}
    }

    public override String ToString() {
        return _xmlString;
    }
}

}
