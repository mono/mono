// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface: IDeserializationEventListener
**
**
** Purpose: Implemented by any class that wants to indicate that
**          it wishes to receive deserialization events.
**
**
===========================================================*/
namespace System.Runtime.Serialization {
    using System;

    // Interface does not need to be marked with the serializable attribute    
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IDeserializationCallback {
#if FEATURE_SERIALIZATION
        void OnDeserialization(Object sender);
#endif
    
    }
}
