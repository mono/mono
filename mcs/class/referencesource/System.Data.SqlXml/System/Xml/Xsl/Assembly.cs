//------------------------------------------------------------------------------
// <copyright file="Assembly.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.Runtime.CompilerServices;

// Hard bind to System.Xml
[assembly: Dependency("System.Xml,", LoadHint.Always)]
[assembly: InternalsVisibleTo("System.Xml, PublicKey=00000000000000000400000000000000")]
