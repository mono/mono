//------------------------------------------------------------------------------
// <copyright file="assemblycache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
//ThisAssembly class keeps a map of the following:
//AssemblyName to AssemblyID
//TypeName to TypeID
//AssemblyID to AssemblyRef and State
//TypeID to TypeRef and AssemblyId
//
//Adding an assembly to this class will NOT enable users to create types from that assembly. Users should explicitely add type details and link types to assemblies.
// This class also registers for assembly resolve events so that dependent assemblies can be resolved if they are registered.
//This class does NOT know anything about assembly dependencies. It simply loads assemblies as handed over to it.
//Users can take advantage of connection pooling by tying this instance to a pooling-aware component.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Data.Common;

using Microsoft.SqlServer.Server;

namespace System.Data.SqlClient {

    internal sealed class AssemblyCache {
        private AssemblyCache() { /* prevent utility class from being insantiated*/
        }

        internal static int GetLength(Object inst){
            //caller should have allocated enough, based on MaxByteSize
            return SerializationHelperSql9.SizeInBytes(inst);
        }

        //The attribute we are looking for is now moved to an external dll that server provides. If the name is changed.
        //then we we have to make corresponding changes here.
        //please also change sqludcdatetime.cs, sqltime.cs and sqldate.cs

        internal static SqlUdtInfo GetInfoFromType(Type t) {
            Debug.Assert(t != null, "Type object cant be NULL");

            Type orig = t;
            do {
                SqlUdtInfo attr = SqlUdtInfo.TryGetFromType(t);

                if (attr != null ) {
                    return attr;
                }
                else {
                    t = t.BaseType;
                }
            }
            while (t != null);

            throw SQL.UDTInvalidSqlType(orig.AssemblyQualifiedName);
        }
    }
}
