// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

internal static class NameSpaces
{
    public const string Mc = "http://schemas.openxmlformats.org/markup-compatibility/2006";
    public const string Design = "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation";
    public const string Design2010 = "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation";
    public const string Toolbox = "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation/toolbox";
    public const string Activities = "http://schemas.microsoft.com/netfx/2009/xaml/activities";
    public const string DebugSymbol = "http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger";
    public const string DesignPrefix = "sap";
    public const string Design2010Prefix = "sap2010";
    public const string McPrefix = "mc";
    public const string DebugSymbolPrefix = "sads";

    public static bool ShouldIgnore(string ns)
    {
        return ns == Design2010 || ns == DebugSymbol || ns == Design;
    }
}
