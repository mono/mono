//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceEnumerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Sql {

    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    public sealed class SqlDataSourceEnumerator : DbDataSourceEnumerator {

        private static readonly SqlDataSourceEnumerator SingletonInstance = new SqlDataSourceEnumerator();
        internal const string ServerName     = "ServerName";
        internal const string InstanceName   = "InstanceName";
        internal const string IsClustered    = "IsClustered";
        internal const string Version        = "Version";
        private  const int    timeoutSeconds = ADP.DefaultCommandTimeout;
        private long timeoutTime;                                // variable used for timeout computations, holds the value of the hi-res performance counter at which this request should expire

        private SqlDataSourceEnumerator() : base() {
        }

        public static SqlDataSourceEnumerator Instance {
            get { 
                return SqlDataSourceEnumerator.SingletonInstance;
            }
        }

        override public DataTable GetDataSources() {
            (new NamedPermissionSet("FullTrust")).Demand(); // SQLBUDT 244304
            char[] buffer = null;
            StringBuilder strbldr = new StringBuilder();

            Int32  bufferSize = 1024;
            Int32  readLength = 0;
            buffer            = new char[bufferSize];
			bool   more       = true;
            bool   failure    = false;
            IntPtr handle     = ADP.PtrZero;

			RuntimeHelpers.PrepareConstrainedRegions();
            try {
                timeoutTime = TdsParserStaticMethods.GetTimeoutSeconds(timeoutSeconds);
                RuntimeHelpers.PrepareConstrainedRegions();
                try {} finally {
                    handle = SNINativeMethodWrapper.SNIServerEnumOpen();
                }

                if (ADP.PtrZero != handle) {
                    while (more && !TdsParserStaticMethods.TimeoutHasExpired(timeoutTime)) {
			            readLength = SNINativeMethodWrapper.SNIServerEnumRead(handle, buffer, bufferSize, ref more);
                        if (readLength > bufferSize) {
                            failure = true;
                            more = false;
                        }
                        else if (0 < readLength) {
							strbldr.Append(buffer, 0, readLength);
						}    
                    }
                }
            }
            finally {
                if (ADP.PtrZero != handle) {
                    SNINativeMethodWrapper.SNIServerEnumClose(handle);
                }
            }

            if (failure) {
                Debug.Assert(false, "GetDataSources:SNIServerEnumRead returned bad length");
                Bid.Trace("<sc.SqlDataSourceEnumerator.GetDataSources|ERR> GetDataSources:SNIServerEnumRead returned bad length, requested %d, received %d", bufferSize, readLength);
                throw ADP.ArgumentOutOfRange("readLength");
            }

            return ParseServerEnumString(strbldr.ToString());
        }
        
        private static string _Version = "Version:";
        private static string _Cluster = "Clustered:";
        private static int _clusterLength = _Cluster.Length;
        private static int _versionLength =_Version.Length;

        static private DataTable ParseServerEnumString(string serverInstances) {
            DataTable dataTable = new DataTable("SqlDataSources");
            dataTable.Locale = CultureInfo.InvariantCulture;
            dataTable.Columns.Add(ServerName, typeof(string));
            dataTable.Columns.Add(InstanceName, typeof(string));
            dataTable.Columns.Add(IsClustered, typeof(string));
            dataTable.Columns.Add(Version, typeof(string));
            DataRow dataRow = null;
            string serverName = null;
            string instanceName = null;
            string isClustered = null;
            string version = null;
            
            // Every row comes in the format "serverName\instanceName;Clustered:[Yes|No];Version:.." 
            // Every row is terminated by a null character.
            // Process one row at a time
            foreach (string instance in serverInstances.Split('\0')) {
                string value = instance.Trim('\0'); // MDAC 91934
                if (0 == value.Length) {
                    continue;
                }
				foreach (string instance2 in value.Split(';')) {					
					if (serverName == null) {
                        foreach(string instance3 in instance2.Split('\\')) {							
							if (serverName == null) {
                                serverName = instance3;
                                continue;
                            }
                            Debug.Assert(instanceName == null);
                            instanceName = instance3;
                        }
                        continue;
                    }
                    if (isClustered == null) {
                        Debug.Assert(String.Compare(_Cluster, 0, instance2, 0, _clusterLength, StringComparison.OrdinalIgnoreCase) == 0);
                        isClustered = instance2.Substring(_clusterLength);
                        continue;
                    }
                    Debug.Assert(version == null);
                    Debug.Assert(String.Compare(_Version, 0, instance2, 0, _versionLength, StringComparison.OrdinalIgnoreCase) == 0);
                    version =  instance2.Substring(_versionLength);
                }

                string query = "ServerName='"+serverName+"'";

                if (!ADP.IsEmpty(instanceName)) { // SQL BU DT 20006584: only append instanceName if present.
                    query += " AND InstanceName='"+instanceName+"'";
                }

                // SNI returns dupes - do not add them.  SQL BU DT 290323
                if (dataTable.Select(query).Length == 0) {
                    dataRow = dataTable.NewRow();
                    dataRow[0] = serverName;
                    dataRow[1] = instanceName;
                    dataRow[2] = isClustered;
                    dataRow[3] = version;
                    dataTable.Rows.Add(dataRow);
                }
                serverName = null;
                instanceName = null;
                isClustered = null;
                version = null;
            }
            foreach(DataColumn column in dataTable.Columns) {
                column.ReadOnly = true;
            }
            return dataTable;
        }
    }
}
