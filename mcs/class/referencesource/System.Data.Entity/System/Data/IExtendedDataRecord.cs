//------------------------------------------------------------------------------
// <copyright file="IExtendedDataRecord.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">sheetgu</owner>
// <owner current="true" primary="false">simoncav</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    using System.Data.Common;

    /// <summary>
    /// DataRecord interface supporting structured types and rich metadata information.
    /// </summary>
    public interface IExtendedDataRecord : IDataRecord {

        /// <summary>
        /// DataRecordInfo property describing the contents of the record.
        /// </summary>
        DataRecordInfo DataRecordInfo { get;}

        /// <summary>
        /// Used to return a nested DbDataRecord.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        DbDataRecord GetDataRecord(int i);

        /// <summary>
        /// Used to return a nested result
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        DbDataReader GetDataReader(int i);
    }
}