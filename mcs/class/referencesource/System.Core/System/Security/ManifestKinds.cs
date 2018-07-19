// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;

namespace System.Security {
    [Flags]
    public enum ManifestKinds {
        None                        = 0x00000000,
        Deployment                  = 0x00000001,
        Application                 = 0x00000002,
        ApplicationAndDeployment    = Deployment | Application
    }
}