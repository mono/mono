//------------------------------------------------------------------------------
// <copyright file="IValidationEventHandling.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">[....]</owner>                                                               
//------------------------------------------------------------------------------

using System;
using System.Xml.Schema;

namespace System.Xml {
    internal interface IValidationEventHandling {

        // This is a ValidationEventHandler, but it is not strongly typed due to dependencies on System.Xml.Schema
        object EventHandler { get; }

        // The exception is XmlSchemaException, but it is not strongly typed due to dependencies on System.Xml.Schema
        void SendEvent(Exception exception, XmlSeverityType severity);
    }
}
