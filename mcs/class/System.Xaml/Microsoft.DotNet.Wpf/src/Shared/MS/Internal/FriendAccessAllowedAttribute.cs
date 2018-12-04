// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Description: Implementation of a FriendAccessAllowedAttribute attribute that is used to mark internal metadata
//              that is allowed to be accessed from friend assemblies.


using System;

#if WINDOWS_BASE
namespace MS.Internal.WindowsBase
#elif PRESENTATION_CORE
namespace MS.Internal.PresentationCore 
#elif PRESENTATIONFRAMEWORK
namespace MS.Internal.PresentationFramework
#elif PRESENTATION_CFF_RASTERIZER
namespace MS.Internal.PresentationCffRasterizer
#elif PRESENTATIONUI
namespace MS.Internal.PresentationUI
#elif UIAUTOMATIONTYPES
namespace MS.Internal.UIAutomationTypes
#elif DRT
namespace MS.Internal.Drt
#elif SYSTEM_XAML
namespace MS.Internal.WindowsBase  //current copy of XmlMarkupCompatibilityReader uses this ns for FAAA.
#else
#error Attempt to define FriendAccessAllowedAttribute in an unknown assembly.
namespace MS.Internal.YourAssemblyName
#endif
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.Struct |
        AttributeTargets.Enum |
        AttributeTargets.Interface |
        AttributeTargets.Delegate |
        AttributeTargets.Constructor,
        AllowMultiple = false,
        Inherited = true)
    ]
    internal sealed class FriendAccessAllowedAttribute : Attribute
    {
    }
}

