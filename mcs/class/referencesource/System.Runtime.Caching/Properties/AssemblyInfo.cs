//------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security;

// We can't make it SecurityTransparent due to performance implications
//[assembly: SecurityTransparent]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#pragma warning restore 618

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "Assembly is delay-signed.")]

