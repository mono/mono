//------------------------------------------------------------------------------
// <copyright file="supportedJoinOperators.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">rickfe</owner>
// <owner current="true" primary="false">stevesta</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
	[Flags]
    public enum  SupportedJoinOperators { 
        None 		= 0x00000000,
        Inner 		= 0x00000001,
        LeftOuter 	= 0x00000002,
        RightOuter 	= 0x00000004,
        FullOuter 	= 0x00000008    
    }
}


