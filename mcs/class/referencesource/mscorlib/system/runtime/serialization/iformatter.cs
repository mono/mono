// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface: IFormatter;
**
**
** Purpose: The interface for all formatters.
**
**
===========================================================*/
namespace System.Runtime.Serialization {
    using System.Runtime.Remoting;
    using System;
    using System.IO;

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IFormatter {
        Object Deserialize(Stream serializationStream);

        void Serialize(Stream serializationStream, Object graph);


        ISurrogateSelector SurrogateSelector {
            get;
            set;
        }

        SerializationBinder Binder {
            get;
            set;
        }

        StreamingContext Context {
            get;
            set;
        }
    }
}
