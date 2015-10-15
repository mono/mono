// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.CompilerServices
{
    // C++ recognizes three char types: signed char, unsigned char, and char.
    // When a char is neither signed nor unsigned, it is a naked char.
    // This modopt indicates that the modified instance is a naked char.
    //
    // Any compiler could use this to indicate that the user has not specified
    // Sign behavior for the given byte.
    public static class IsSignUnspecifiedByte 
    {
    }
}
