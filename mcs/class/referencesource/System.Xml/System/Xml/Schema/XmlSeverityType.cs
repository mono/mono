//------------------------------------------------------------------------------
// <copyright file="XmlSeverityType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">priyal</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {
    //UE Atention
    /// <include file='doc\XmlSeverityType.uex' path='docs/doc[@for="XmlSeverityType"]/*' />
    public enum XmlSeverityType {
        /// Errors that can be recovered from. 
        /// <include file='doc\XmlSeverityType.uex' path='docs/doc[@for="XmlSeverityType.Error"]/*' />
        Error,
        /// Errors that can be ignored
        /// <include file='doc\XmlSeverityType.uex' path='docs/doc[@for="XmlSeverityType.Warning"]/*' />
        Warning
    }
}

