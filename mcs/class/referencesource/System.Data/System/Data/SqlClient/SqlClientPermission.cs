//------------------------------------------------------------------------------
// <copyright file="SqlClientPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System.Collections;
    using System.Data.Common;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable] 
    public sealed class SqlClientPermission :  DBDataPermission {

        [ Obsolete("SqlClientPermission() has been deprecated.  Use the SqlClientPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true) ] // MDAC 86034
        public SqlClientPermission() : this(PermissionState.None) {
        }

        public SqlClientPermission(PermissionState state) : base(state) {
        }

        [ Obsolete("SqlClientPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the SqlClientPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true) ] // MDAC 86034
        public SqlClientPermission(PermissionState state, bool allowBlankPassword) : this(state) {
            AllowBlankPassword = allowBlankPassword;
        }

        private SqlClientPermission(SqlClientPermission permission) : base(permission) { // for Copy
        }

        internal SqlClientPermission(SqlClientPermissionAttribute permissionAttribute) : base(permissionAttribute) { // for CreatePermission
        }

        internal SqlClientPermission(SqlConnectionString constr) : base(constr) { // for Open
            if ((null == constr) || constr.IsEmpty) {
                base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
            }
        }

        public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior) {
            DBConnectionString constr = new DBConnectionString(connectionString, restrictions, behavior, SqlConnectionString.GetParseSynonyms(), false);
            AddPermissionEntry(constr);
        }

        override public IPermission Copy () {
            return new SqlClientPermission(this);
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
    [Serializable] 
    public sealed class SqlClientPermissionAttribute : DBDataPermissionAttribute {

        public SqlClientPermissionAttribute(SecurityAction action) : base(action) {
        }

        override public IPermission CreatePermission() {
            return new SqlClientPermission(this);
        }
    }
}
