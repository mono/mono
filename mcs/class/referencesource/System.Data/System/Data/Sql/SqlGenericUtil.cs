//------------------------------------------------------------------------------
// <copyright file="SqlGenericUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Sql {
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;

    sealed internal class SqlGenericUtil {

        private SqlGenericUtil() { /* prevent utility class from being insantiated*/ }

        //
        // Sql generic exceptions
        //

        //
        // Sql.Definition
        //

        static internal Exception NullCommandText() {
            return ADP.Argument(Res.GetString(Res.Sql_NullCommandText));
        }
        static internal Exception MismatchedMetaDataDirectionArrayLengths() {
            return ADP.Argument(Res.GetString(Res.Sql_MismatchedMetaDataDirectionArrayLengths));
        }
    }

 }//namespace

