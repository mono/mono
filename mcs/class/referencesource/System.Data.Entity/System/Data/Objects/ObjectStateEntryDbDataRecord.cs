//---------------------------------------------------------------------
// <copyright file="ObjectStateEntryDbDataRecord.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.ComponentModel;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Objects
{

    internal sealed class ObjectStateEntryDbDataRecord : DbDataRecord, IExtendedDataRecord
    {
        private readonly StateManagerTypeMetadata _metadata;
        private readonly ObjectStateEntry _cacheEntry;
        private readonly object _userObject;
        private DataRecordInfo _recordInfo;

        internal ObjectStateEntryDbDataRecord(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            EntityUtil.CheckArgumentNull(userObject, "userObject");
            EntityUtil.CheckArgumentNull(metadata, "metadata");
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryDbDataRecord for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                    _cacheEntry = cacheEntry;
                    _userObject = userObject;
                    _metadata = metadata;
                    break;
                default:
                    Debug.Assert(false, "A DbDataRecord cannot be created for an entity object that is in an added or detached state.");
                    break;
            }
        }
        internal ObjectStateEntryDbDataRecord(RelationshipEntry cacheEntry)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryDbDataRecord for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                    _cacheEntry = cacheEntry;
                    break;
                default:
                    Debug.Assert(false, "A DbDataRecord cannot be created for an entity object that is in an added or detached state.");
                    break;
            }
        }
        override public int FieldCount
        {
            get
            {
                Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                return _cacheEntry.GetFieldCount(_metadata);
            }
        }
        override public object this[int ordinal]
        {
            get
            {
                return GetValue(ordinal);
            }
        }
        override public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }
        override public bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }
        override public byte GetByte(int ordinal)
        {
            return (byte)GetValue(ordinal);
        }
        override public long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            byte[] tempBuffer;
            tempBuffer = (byte[])GetValue(ordinal);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }
            int srcIndex = (int)dataIndex;
            int byteCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw EntityUtil.InvalidSourceBufferIndex(tempBuffer.Length, srcIndex, "dataIndex");
            }
            else if ((bufferIndex < 0) || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw EntityUtil.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }

            if (0 < byteCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, byteCount);
            }
            else if (length < 0)
            {
                throw EntityUtil.InvalidDataLength(length);
            }
            else
            {
                byteCount = 0;
            }
            return byteCount;
        }
        override public char GetChar(int ordinal)
        {
            return (char)GetValue(ordinal);
        }
        override public long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            char[] tempBuffer;
            tempBuffer = (char[])GetValue(ordinal);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }

            int srcIndex = (int)dataIndex;
            int charCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw EntityUtil.InvalidSourceBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }
            else if ((bufferIndex < 0) || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw EntityUtil.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }

            if (0 < charCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, charCount);
            }
            else if (length < 0)
            {
                throw EntityUtil.InvalidDataLength(length);
            }
            else
            {
                charCount = 0;
            }
            return charCount;
        }
        override protected DbDataReader GetDbDataReader(int ordinal)
        {
            throw EntityUtil.NotSupported();
        }
        override public string GetDataTypeName(int ordinal)
        {
            return (GetFieldType(ordinal)).Name;
        }
        override public DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }
        override public Decimal GetDecimal(int ordinal)
        {
            return (Decimal)GetValue(ordinal);
        }
        override public double GetDouble(int ordinal)
        {
            return (Double)GetValue(ordinal);
        }
        public override Type GetFieldType(int ordinal)
        {
            return _cacheEntry.GetFieldType(ordinal, _metadata);
        }
        override public float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }
        override public Guid GetGuid(int ordinal)
        {
            return (Guid)GetValue(ordinal);
        }
        override public Int16 GetInt16(int ordinal)
        {
            return (Int16)GetValue(ordinal);
        }
        override public Int32 GetInt32(int ordinal)
        {
            return (Int32)GetValue(ordinal);
        }
        override public Int64 GetInt64(int ordinal)
        {
            return (Int64)GetValue(ordinal);
        }
        override public string GetName(int ordinal)
        {
            return _cacheEntry.GetCLayerName(ordinal, _metadata);
        }
        override public int GetOrdinal(string name)
        {
            int ordinal = _cacheEntry.GetOrdinalforCLayerName(name, _metadata);
            if (ordinal == -1)
            {
                throw EntityUtil.ArgumentOutOfRange("name");
            }
            return  ordinal;
        }
        override public string GetString(int ordinal)
        {
            return (string)GetValue(ordinal);
        }
        override public object GetValue(int ordinal)
        {
            if (_cacheEntry.IsRelationship)
            {
                return (_cacheEntry as RelationshipEntry).GetOriginalRelationValue(ordinal);
            }
            else
            {
                return (_cacheEntry as EntityEntry).GetOriginalEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalReadonly);
            }
        }
        override public int GetValues(object[] values)
        {
            if (values == null)
            {
                throw EntityUtil.ArgumentNull("values");
            }
            int minValue = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < minValue; i++)
            {
                values[i] = GetValue(i);
            }
            return minValue;
        }
        override public bool IsDBNull(int ordinal)
        {
            return (GetValue(ordinal) == DBNull.Value);
        }

        public DataRecordInfo DataRecordInfo
        {
            get
            {
                if (null == _recordInfo)
                {
                    Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                    _recordInfo = _cacheEntry.GetDataRecordInfo(_metadata, _userObject);
                }
                return _recordInfo;
            }
        }
        public DbDataRecord GetDataRecord(int ordinal)
        {
            return (DbDataRecord)GetValue(ordinal);
        }
        public DbDataReader GetDataReader(int i)
        {
            return this.GetDbDataReader(i);
        }
    }
}
