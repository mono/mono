// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
//  StorePermissionFlags.cs
//

namespace System.Security.Permissions {
    [Flags, Serializable()]
    public enum StorePermissionFlags {
        NoFlags                     = 0x00,

        CreateStore                 = 0x01,
        DeleteStore                 = 0x02,
        EnumerateStores             = 0x04,

        OpenStore                   = 0x10,
        AddToStore                  = 0x20,
        RemoveFromStore             = 0x40,
        EnumerateCertificates       = 0x80,
        
        AllFlags                    = 0xF7
    }
}
