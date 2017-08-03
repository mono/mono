// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb
{
    [MonoTODO] // OLEDB is not implemented
    public sealed class OleDbFactory : DbProviderFactory { }

    [MonoTODO] // OLEDB is not implemented
    [Serializable] 
    public class OleDbPermission : DBDataPermission 
    {
        [Obsolete("OleDbPermission() has been deprecated.  Use the OleDbPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true) ] // MDAC 86034
        public OleDbPermission() : this(PermissionState.None) { }

        public OleDbPermission(PermissionState state) : base(state) { }
    }
}