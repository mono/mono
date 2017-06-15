// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Data.SqlClient 
{
    /// <summary>
    /// Column Encryption Setting to be used for the SqlConnection.
    /// </summary>
    public enum SqlConnectionColumnEncryptionSetting {
        /// <summary>
        /// Disables column encryption by default on all commands on this connection.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Enables column encryption by default on all commands on this connection.
        /// </summary>
        Enabled,
    }

    /// <summary>
    /// Column Encryption Setting to be used for the SqlCommand.
    /// </summary>
    public enum SqlCommandColumnEncryptionSetting {
        /// <summary>
        /// if �Column Encryption Setting=Enabled� in the connection string, use Enabled. Otherwise, maps to Disabled.
        /// </summary>
        UseConnectionSetting = 0,

        /// <summary>
        /// Enables TCE for the command. Overrides the connection level setting for this command.
        /// </summary>
        Enabled,

        /// <summary>
        /// Parameters will not be encrypted, only the ResultSet will be decrypted. This is an optimization for queries that do not pass any encrypted input parameters.
        /// Overrides the connection level setting for this command.
        /// </summary>
        ResultSetOnly,

        /// <summary>
        /// Disables TCE for the command.Overrides the connection level setting for this command.
        /// </summary>
        Disabled,
    }

    public enum SqlAuthenticationMethod {
        NotSpecified = 0,
        SqlPassword,
        ActiveDirectoryPassword,
        ActiveDirectoryIntegrated,
    }
}
