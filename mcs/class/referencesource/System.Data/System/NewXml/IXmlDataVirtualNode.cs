//------------------------------------------------------------------------------
// <copyright file="IXmlDataVirtualNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------
namespace System.Xml {

    using System;
    using System.Data;

    internal interface IXmlDataVirtualNode {
        bool IsOnNode( XmlNode nodeToCheck );
        bool IsOnColumn(DataColumn col );
        bool IsInUse();
        void OnFoliated( XmlNode foliatedNode );
    }
}
