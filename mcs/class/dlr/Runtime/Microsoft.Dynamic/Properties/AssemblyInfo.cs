/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CODEPLEX_40
using System;
#else
using System; using Microsoft;
#endif
using System.Reflection;
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif

using System.Runtime.InteropServices;
using System.Security;

#if MICROSOFT_DYNAMIC

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Microsoft.Dynamic")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Microsoft.Dynamic")]
[assembly: AssemblyCopyright("� Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: CLSCompliant(true)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: System.Resources.NeutralResourcesLanguage("en-US")]

[assembly: AssemblyVersion("0.9.6.20")]
[assembly: AssemblyFileVersion("1.0.0.00")]
[assembly: AssemblyInformationalVersion("1.0")]
[assembly: AllowPartiallyTrustedCallers]
#if CODEPLEX_40
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif

#else

[assembly: InternalsVisibleTo("Microsoft.CSharp, PublicKey =" +
    "002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9" +
    "f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad2361321" +
    "02900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93" +
    "c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc09334" +
    "4d5ad293")]

#endif
