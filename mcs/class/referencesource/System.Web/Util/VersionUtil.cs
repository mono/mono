//------------------------------------------------------------------------------
// <copyright file="VersionUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Helper class for performing common operations on Version objects
 * 
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web.Util {
    using System;

    internal static class VersionUtil {

        public static readonly Version Framework00 = new Version(0, 0);
        public static readonly Version Framework20 = new Version(2, 0);
        public static readonly Version Framework35 = new Version(3, 5);
        public static readonly Version Framework40 = new Version(4, 0);
        public static readonly Version Framework45 = new Version(4, 5);
        public static readonly Version Framework451 = new Version(4, 5, 1);
        public static readonly Version Framework452 = new Version(4, 5, 2);
        public static readonly Version Framework46 = new Version(4, 6);
        public static readonly Version Framework461 = new Version(4, 6, 1);

        // Convenience accessor for the "default" framework version; various configuration
        // switches can use this as a default value. This value must only be bumped during
        // SxS releases of the .NET Framework.
        public static readonly Version FrameworkDefault = Framework40;
        public const string FrameworkDefaultString = "4.0";

    }
}
