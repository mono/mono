//------------------------------------------------------------------------------
// <copyright file="OleDbConnection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb
{
    using System.Data.ProviderBase;

    internal sealed class OleDbConnectionPoolGroupProviderInfo : DbConnectionPoolGroupProviderInfo {
        private bool _hasQuoteFix;
        private string _quotePrefix, _quoteSuffix;

        internal OleDbConnectionPoolGroupProviderInfo() {
        }
    
        internal bool HasQuoteFix {
            get { return _hasQuoteFix; }
        }
        internal string QuotePrefix {
            get { return _quotePrefix; }
        }
        internal string QuoteSuffix {
            get { return _quoteSuffix; }
        }

        internal void SetQuoteFix(string prefix, string suffix) {
            _quotePrefix = prefix;
            _quoteSuffix = suffix;
            _hasQuoteFix = true;
        }
    }
}
