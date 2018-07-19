//------------------------------------------------------------------------------
// <copyright file="IDataRecord.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    // This interface is already shipped. So no more changes!
    
    public interface IDataRecord {

        int FieldCount { get;}

        object this [ int i ] { get;}

        object this [ String name ] { get;}

        String GetName(int i);

        String GetDataTypeName(int i);

        Type GetFieldType(int i);

        Object GetValue(int i);

        int GetValues(object[] values);

        int GetOrdinal(string name);

        bool GetBoolean(int i);

        byte GetByte(int i);

        long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);

        char GetChar(int i);

        long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length);

        Guid GetGuid(int i);

        Int16 GetInt16(int i);

        Int32 GetInt32(int i);

        Int64 GetInt64(int i);

        float GetFloat(int i);

        double GetDouble(int i);

        String GetString(int i);

        Decimal GetDecimal(int i);

        DateTime GetDateTime(int i);

        IDataReader GetData(int i);

        bool IsDBNull(int i);
    }
}
