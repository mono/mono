// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface: SerializationBinder
**
**
** Purpose: The base class of serialization binders.
**
**
===========================================================*/
namespace System.Runtime.Serialization {
    using System;

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class SerializationBinder {

        public virtual void BindToName(Type serializedType, out String assemblyName, out String typeName)
        {
            assemblyName = null;
            typeName = null;
        }

        public abstract Type BindToType(String assemblyName, String typeName);
    }
}
