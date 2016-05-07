// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System {
    
    using System;
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void EventHandler(Object sender, EventArgs e);

    [Serializable]
    public delegate void EventHandler<TEventArgs>(Object sender, TEventArgs e); // Removed TEventArgs constraint post-.NET 4
}
