// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Data.SqlClient;
using System.Reflection;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace System.Data.Common
{
    public static class DbProviderFactories 
    {
        static public DbProviderFactory GetFactory(string providerInvariantName) => null;
        static public DataTable GetFactoryClasses() => null;
    }
}

namespace System.Data.OleDb
{
}