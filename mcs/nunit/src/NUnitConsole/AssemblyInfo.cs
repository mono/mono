using System;
using System.Reflection;
using System.Runtime.CompilerServices;

// Mark the framework assembly as CLS compliant
[assembly:CLSCompliant(true)]

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly:AssemblyTitle("NUnit Testing Framework")]
[assembly:AssemblyDescription("A unit testing framework for the .Net platform, ported from junit by Kent Beck and Erich Gamma.")]
[assembly:AssemblyConfiguration("")]
[assembly:AssemblyCompany("")]
[assembly:AssemblyProduct("NUnit")]
[assembly:AssemblyCopyright("")]
[assembly:AssemblyTrademark("")]
[assembly:AssemblyCulture("")]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Revision
//      Build Number
//
// You can specify all the value or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly:AssemblyVersion("1.10.*")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified - the assembly cannot be signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. 
//   (*) If the key file and a key name attributes are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP - that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the file is installed into the CSP and used.
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile(@"..\..\..\..\NUnit.key")]
//[assembly: AssemblyKeyName("")]
