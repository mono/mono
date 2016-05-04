//------------------------------------------------------------------------------
// <copyright file="XmlDataImplementation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------
#pragma warning disable 618 // ignore obsolete warning about XmlDataDocument
namespace System.Xml {

    internal sealed class XmlDataImplementation : XmlImplementation {
        
        public XmlDataImplementation() : base() {
        }
        
        public override XmlDocument CreateDocument() {
            return new XmlDataDocument( this );
        }
    }
}

