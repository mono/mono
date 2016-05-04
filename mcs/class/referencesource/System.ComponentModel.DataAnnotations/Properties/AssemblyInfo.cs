//------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;

#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#pragma warning restore 618

// System.ComponentModel.DataAnnotations.Test is our managed code unit test assembly, which needs
// to have access to internal members in this assembly.
[assembly: InternalsVisibleTo("System.ComponentModel.DataAnnotations.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "Assembly is delay-signed.")]

#if ATLAS_DEV
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: SecurityTransparent]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AssemblyVersion("99.0.0.0")]
[assembly: NeutralResourcesLanguage("en-US")]
#endif