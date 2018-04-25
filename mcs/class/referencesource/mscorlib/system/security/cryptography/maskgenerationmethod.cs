// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>ShawnFa</OWNER>
// 

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public abstract class MaskGenerationMethod {
        [System.Runtime.InteropServices.ComVisible(true)]
        abstract public byte[] GenerateMask(byte[] rgbSeed, int cbReturn);
    }
}
