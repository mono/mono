//------------------------------------------------------------------------------
// <copyright file="SqlConnectionPoolGroupProviderInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;

    sealed internal class SqlConnectionPoolGroupProviderInfo : DbConnectionPoolGroupProviderInfo {
        private string _alias;
        private System.Security.PermissionSet _failoverPermissionSet;
        private string _failoverPartner;        
        private bool _useFailoverPartner;
        
        internal SqlConnectionPoolGroupProviderInfo(SqlConnectionString connectionOptions) {
            // This is for the case where the user specified the failover partner
            // in the connection string and we have not yet connected to get the 
            // env change.
            _failoverPartner = connectionOptions.FailoverPartner;

            if (ADP.IsEmpty(_failoverPartner)) {
                _failoverPartner = null;
            }
        }

        internal string FailoverPartner {
            get {
                return _failoverPartner;
            }
        }

        internal bool UseFailoverPartner {
            get {
                return _useFailoverPartner;
            }
        }

        internal void AliasCheck(string server) {
            if (_alias != server) {
                lock(this) {
                    if (null == _alias) {
                        _alias = server;
                    }
                    else if (_alias != server) {
                        Bid.Trace("<sc.SqlConnectionPoolGroupProviderInfo|INFO> alias change detected. Clearing PoolGroup\n");
                        base.PoolGroup.Clear();
                        _alias = server;
                    }
                }
            }
        }

        private System.Security.PermissionSet CreateFailoverPermission(SqlConnectionString userConnectionOptions, string actualFailoverPartner) {
            string keywordToReplace;

            // RULES FOR CONSTRUCTING THE CONNECTION STRING TO DEMAND ON:
            //
            // 1) If no Failover Partner was specified in the original string:
            //
            //          Server=actualFailoverPartner
            //
            // 2) If Failover Partner was specified in the original string:
            //
            //          Server=originalValue; Failover Partner=actualFailoverPartner
            //
            // NOTE: in all cases, when we get a failover partner name from 
            //       the server, we will use that name over what was specified  
            //       in the original connection string.
            
            if (null == userConnectionOptions[SqlConnectionString.KEY.FailoverPartner]) {
                keywordToReplace = SqlConnectionString.KEY.Data_Source;
            }
            else {
                keywordToReplace = SqlConnectionString.KEY.FailoverPartner;
            }
            
            string failoverConnectionString = userConnectionOptions.ExpandKeyword(keywordToReplace, actualFailoverPartner);
            return (new SqlConnectionString(failoverConnectionString)).CreatePermissionSet();
        }

        internal void FailoverCheck(SqlInternalConnection connection, bool actualUseFailoverPartner, SqlConnectionString userConnectionOptions, string actualFailoverPartner) {
            if (UseFailoverPartner != actualUseFailoverPartner) {
                // 
                Bid.Trace("<sc.SqlConnectionPoolGroupProviderInfo|INFO> Failover detected. failover partner='%ls'. Clearing PoolGroup\n", actualFailoverPartner);
                base.PoolGroup.Clear();
                _useFailoverPartner = actualUseFailoverPartner;
            }
            // Only construct a new permission set when we're connecting to the
            // primary data source, not the failover partner.
            if (!_useFailoverPartner && _failoverPartner != actualFailoverPartner) {
                // NOTE: we optimisitically generate the permission set to keep 
                //       lock short, but we only do this when we get a new
                //       failover partner.
                // 

                System.Security.PermissionSet failoverPermissionSet = CreateFailoverPermission(userConnectionOptions, actualFailoverPartner);

                lock (this) {
                    if (_failoverPartner != actualFailoverPartner) {
                        _failoverPartner = actualFailoverPartner;
                        _failoverPermissionSet = failoverPermissionSet;
                    }
                }
            }
        }

        internal void FailoverPermissionDemand() {
            if (_useFailoverPartner) {
                // Note that we only demand when there is a permission set, which only
                // happens once we've identified a failover situation in FailoverCheck
                System.Security.PermissionSet failoverPermissionSet = _failoverPermissionSet;
                if (null != failoverPermissionSet) {
                    // demand on pooled failover connections
                    failoverPermissionSet.Demand();
                }
            }        
        }
    }
}
