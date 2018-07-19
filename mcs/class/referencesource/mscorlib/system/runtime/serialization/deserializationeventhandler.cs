// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface: DeserializationEventHandler
**
**
** Purpose: The multicast delegate called when the DeserializationEvent is thrown.
**
**
===========================================================*/
namespace System.Runtime.Serialization {

    [Serializable]
    internal delegate void DeserializationEventHandler(Object sender);

    [Serializable]
    internal delegate void SerializationEventHandler(StreamingContext context);
    
}
