//------------------------------------------------------------------------------
// 
// System.Runtime.CompilerServices.MethodCodeType.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Thu 07/18/2001 
//
//------------------------------------------------------------------------------

namespace System.Runtime.CompilerServices
{
[Flags] 
public enum MethodCodeType
{
    IL,
    Native,
    OPTIL,
    Runtime,
}

} // Namespace
