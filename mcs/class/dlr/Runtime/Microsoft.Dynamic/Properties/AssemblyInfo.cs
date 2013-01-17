/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Microsoft.Scripting")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Microsoft.Scripting")]
[assembly: AssemblyCopyright("© Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1bbee69c-30c5-41df-8912-b81da6d658c2")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
#if !CLR2 && !SILVERLIGHT && !WIN8 && !ANDROID && !WP75
[assembly: System.Security.SecurityRules(SecurityRuleSet.Level1)]
#endif
[assembly: SecurityTransparent]

[assembly: System.Resources.NeutralResourcesLanguage("en-US")]
[assembly: InternalsVisibleTo("TestInternalDLR, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c10ce00dd2e0ce5046d68183d3ad035b47e92bf0ce7bcf8a03a217ca5d0b0c7db973fdf97579b52b502a23d4069dbf043389e1ab65a1d6c508a9837f3e2350f15e05cc63c0fc4b0410867a51919090e4c33f80203e9b0035b21c32bae20f98b068f90d99a50133a5336480d94039b176519f5fd8524765f33be43da65c4b68ba")]

#if !SILVERLIGHT
[assembly: AssemblyVersion("1.1.0.30")]
[assembly: AssemblyFileVersion("1.1.0.30")]
[assembly: AssemblyInformationalVersion("1.1")]
#endif

#if !SILVERLIGHT && !WP75
[assembly: AllowPartiallyTrustedCallers]
[assembly: TypeForwardedTo(typeof(Microsoft.Scripting.Runtime.DynamicStackFrame))]
#endif

