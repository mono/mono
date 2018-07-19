//------------------------------------------------------------------------------
//  <copyright file="SqlUdtInfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
//     Information Contained Herein is Proprietary and Confidential.
//  </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Reflection.Emit;
    using System.Security.Permissions;

    using Microsoft.SqlServer.Server;

    internal class SqlUdtInfo {
        internal readonly Microsoft.SqlServer.Server.Format SerializationFormat;
        internal readonly bool IsByteOrdered;
        internal readonly bool IsFixedLength;
        internal readonly int MaxByteSize;
        internal readonly string Name;
        internal readonly string ValidationMethodName;

        private SqlUdtInfo(Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute attr) {
            SerializationFormat = (Microsoft.SqlServer.Server.Format)attr.Format;
            IsByteOrdered       = attr.IsByteOrdered;
            IsFixedLength       = attr.IsFixedLength;
            MaxByteSize         = attr.MaxByteSize;
            Name                = attr.Name;
            ValidationMethodName= attr.ValidationMethodName;
        }
        internal static SqlUdtInfo GetFromType(Type target) {
            SqlUdtInfo udtAttr = TryGetFromType(target);
            if (udtAttr == null) {
                throw InvalidUdtException.Create(target, Res.SqlUdtReason_NoUdtAttribute);
            }
            return udtAttr;
        }

        // VSTFDEVDIV 479671: Type.GetCustomAttributes is an time-expensive call.
        // Improve UDT serialization performance by caching the resulted UDT type information using type-safe dictionary.
        // Use a per-thread cache, so we do not need to synchronize access to it
        [ThreadStatic]
        private static Dictionary<Type, SqlUdtInfo> m_types2UdtInfo;

        internal static SqlUdtInfo TryGetFromType(Type target) {
            if (m_types2UdtInfo == null)
                m_types2UdtInfo = new Dictionary<Type, SqlUdtInfo>();

            SqlUdtInfo udtAttr = null;
            if (!m_types2UdtInfo.TryGetValue(target, out udtAttr)) {
                // query SqlUserDefinedTypeAttribute first time and cache the result
                object[] attr = target.GetCustomAttributes(typeof(Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute), false);
                if (attr != null && attr.Length == 1) {
                    udtAttr = new SqlUdtInfo((Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute)attr[0]);
                }
                m_types2UdtInfo.Add(target, udtAttr);
            }
            return udtAttr;
        }
    }
}
