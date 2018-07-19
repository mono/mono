//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en")]
[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: SecurityCritical]

[assembly: XmlnsPrefixAttribute ("http://schemas.microsoft.com/xps/2005/06", "metro")]
[assembly: XmlnsPrefixAttribute ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "wpf")]
[assembly: XmlnsPrefixAttribute ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "av")]

[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Media")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Input")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/xps/2005/06", "System.Windows")]

[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/winfx/2006/xaml", "System.Windows.Markup")]

[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Media")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Diagnostics")]

[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/winfx/2006/xaml/composite-font", "System.Windows.Media")]

[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Media")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinitionAttribute ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Diagnostics")]

#if !MOBILE
[assembly: TypeForwardedTo (typeof (ValueSerializerAttribute))]
#endif

