//------------------------------------------------------------------------------
// <copyright file="MachineKeyValidation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Security.Cryptography;
    using System.Web.Util;
    using System.Globalization;
    using System.Web.Hosting;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public enum MachineKeyValidation
    {
        MD5,
        SHA1,
        TripleDES,
        AES,
        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", Justification = "Matches spec and previous shipped values")]
        HMACSHA256,
        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", Justification = "Matches spec and previous shipped values")]
        HMACSHA384,
        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", Justification = "Matches spec and previous shipped values")]
        HMACSHA512,
        Custom
    }
}
