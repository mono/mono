// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: HeaderHandler
**
**
** Purpose: The delegate used to process headers on the stream
** during deserialization.
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System.Runtime.Remoting;
    //Define the required delegate
[System.Runtime.InteropServices.ComVisible(true)]
    public delegate Object HeaderHandler(Header[] headers);
}
