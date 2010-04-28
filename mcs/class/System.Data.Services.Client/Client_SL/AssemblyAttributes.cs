//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Reflection;
using System.Security;
using System.Resources;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[assembly:ComVisible(false)]
[assembly:CLSCompliant(false)]                                                        
[assembly:AssemblyTitle("System.Data.Services.Client.dll")]          
[assembly:AssemblyDescription("System.Data.Services.Client.dll")] 
[assembly:AssemblyDefaultAlias("System.Data.Services.Client.dll")]   
[assembly:AssemblyCompany("Microsoft Corporation")]         
[assembly:AssemblyProduct("Microsoft\u00AE .NET Framework")]         
[assembly:AssemblyCopyright("\u00A9 Microsoft Corporation.  All rights reserved.")]    

[assembly:NeutralResourcesLanguageAttribute("en-US")]

[assembly: System.Security.SecurityCritical]

#if ASTORIA_LIGHT

[assembly: System.Reflection.AssemblyVersion(ThisAssembly.Version)]
[assembly: System.Reflection.AssemblyFileVersion(ThisAssembly.InformationalVersion)]
[assembly: System.Reflection.AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]
[assembly: System.Resources.SatelliteContractVersion(ThisAssembly.Version)]

internal static class ThisAssembly
{
    internal const string Version = "2.0.5.0";
    internal const string InformationalVersion = "2.0.40216.0";
}

internal static class AssemblyRef
{
    internal const string MicrosoftSilverlightPublicKeyToken = "b03f5f7f11d50a3a";
}

#endif