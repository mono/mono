//------------------------------------------------------------------------------
// 
// System.Security.Permissions.EnvironmentPermissionAccess.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Thu 07/18/2001 
//
//------------------------------------------------------------------------------

namespace System.Security.Permissions
{
[Flags]
public enum EnvironmentPermissionAccess
{
    AllAccess,
    NoAccess,
    Read,
    Write,
}

} // Namespace
