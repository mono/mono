//---------------------------------------------------------------------
// <copyright file="CurrentValueRecord.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Reflection;

namespace System.Data.Objects
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbUpdatableDataRecord : DbDataRecord, IExtendedDataRecord
    {
        internal readonly StateManagerTypeMetadata _metadata;
        internal readonly ObjectStateEntry _cacheEntry;
        internal readonly object _userObject;
        internal DataRecordInfo _recordInfo;

        internal DbUpdatableDataRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
        {
            _cacheEntry = cacheEntry;
            _userObject = userObject;
            _metadata = metadata;
        }

        internal DbUpdatableDataRecord(ObjectStateEntry cacheEntry) :
            this(cacheEntry, null, null)
        {
        }

        /// <summary>
        /// Returns the number of fields in the record.
        /// </summary>
        override public int FieldCount
        {
            get
            {
                Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                return _cacheEntry.GetFieldCount(_metadata);
            }
        }

        /// <summary>
        /// Retrieves a value with the given field ordinal
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value</returns>
        override public object this[int ordinal]
        {
            get
            {
                return GetValue(ordinal);
            }
        }

        /// <summary>
        /// Retrieves a value with the given field name
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The field value</returns>
        override public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        /// <summary>
        /// Retrieves the field value as a boolean
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a boolean</returns>
        override public bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        /// <summary>
        /// Retrieves the field value as a byte
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a byte</returns>
        override public byte GetByte(int ordinal)
        {
            return (byte)GetValue(ordinal);
        }

        /// <summary>
        /// Retrieves the field value as a byte array
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="dataIndex">The index at which to start copying data</param>
        /// <param name="buffer">The destination buffer where data is copied to</param>
        /// <param name="bufferIndex">The index in the destination buffer where copying will begin</param>
        /// <param name="length">The number of bytes to copy</param>
        /// <returns>The number of bytes copied</returns>
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

        /// <summary>
        /// Retrieves the field value as a char
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a char</returns>
        override public char GetChar(int ordinal)
        {
            return (char)GetValue(ordinal);
        }

        /// <summary>
        /// Retrieves the field value as a char array
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="dataIndex">The index at which to start copying data</param>
        /// <param name="buffer">The destination buffer where data is copied to</param>
        /// <param name="bufferIndex">The index in the destination buffer where copying will begin</param>
        /// <param name="length">The number of chars to copy</param>
        /// <returns>The number of chars copied</returns>
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
                throw EntityUtil.InvalidSourceBufferIndex(tempBuffer.Length, srcIndex, "dataIndex");
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
        IDataReader IDataRecord.GetData(int ordinal)
        {
            return GetDbDataReader(ordinal);
        }

        /// <summary>
        /// Retrieves the field value as a DbDataReader
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns></returns>
        override protected DbDataReader GetDbDataReader(int ordinal)
        {
            throw EntityUtil.NotSupported();
        }
        /// <summary>
        /// Retrieves the name of the field data type
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The name of the field data type</returns>
        override public string GetDataTypeName(int ordinal)
        {
            return ((Type)GetFieldType(ordinal)).Name;
        }
        /// <summary>
        /// Retrieves the field value as a DateTime
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a DateTime</returns>
        override public DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the field value as a decimal
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a decimal</returns>
        override public Decimal GetDecimal(int ordinal)
        {
            return (Decimal)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the field value as a double
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a double</returns>
        override public double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the type of a field
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field type</returns>
        public override Type GetFieldType(int ordinal)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            return _cacheEntry.GetFieldType(ordinal, _metadata);
        }
        /// <summary>
        /// Retrieves the field value as a float
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a float</returns>
        override public float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the field value as a Guid
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a Guid</returns>
        override public Guid GetGuid(int ordinal)
        {
            return (Guid)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the field value as an Int16
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as an Int16</returns>
        override public Int16 GetInt16(int ordinal)
        {
            return (Int16)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the field value as an Int32
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as an Int32</returns>
        override public Int32 GetInt32(int ordinal)
        {
            return (Int32)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the field value as an Int64
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as an Int64</returns>
        override public Int64 GetInt64(int ordinal)
        {
            return (Int64)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the name of a field
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The name of the field</returns>
        override public string GetName(int ordinal)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            return _cacheEntry.GetCLayerName(ordinal, _metadata);
        }
        /// <summary>
        /// Retrieves the ordinal of a field by name
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The ordinal of the field</returns>
        override public int GetOrdinal(string name)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            int ordinal = _cacheEntry.GetOrdinalforCLayerName(name, _metadata);
            if (ordinal == -1)
            {
                throw EntityUtil.ArgumentOutOfRange("name");
            }
            return  ordinal;
        }
        /// <summary>
        /// Retrieves the field value as a string
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a string</returns>
        override public string GetString(int ordinal)
        {
            return (string)GetValue(ordinal);
        }
        /// <summary>
        /// Retrieves the value of a field
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value</returns>
        override public object GetValue(int ordinal)
        {
            return GetRecordValue(ordinal);
        }
        /// <summary>
        /// In derived classes, retrieves the record value for a field
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value</returns>
        protected abstract object GetRecordValue(int ordinal);

        /// <summary>
        /// Retrieves all field values in the record into an object array
        /// </summary>
        /// <param name="values">An array of objects to store the field values</param>
        /// <returns>The number of field values returned</returns>
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
        /// <summary>
        /// Determines if a field has a DBNull value
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>True if the field has a DBNull value</returns>
        override public bool IsDBNull(int ordinal)
        {
            return (GetValue(ordinal) == DBNull.Value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value"></param>
        public void SetBoolean(int ordinal, bool value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value"></param>
        public void SetByte(int ordinal, byte value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetChar(int ordinal, char value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDataRecord(int ordinal, IDataRecord value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDateTime(int ordinal, DateTime value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDecimal(int ordinal, Decimal value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDouble(int ordinal, Double value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetFloat(int ordinal, float value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetGuid(int ordinal, Guid value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetInt16(int ordinal, Int16 value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetInt32(int ordinal, Int32 value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetInt64(int ordinal, Int64 value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetString(int ordinal, string value)
        {
            SetValue(ordinal, value);
        }
        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetValue(int ordinal, object value)
        {
            SetRecordValue(ordinal, value);
        }
        /// <summary>
        /// Sets field values in a record
        /// </summary>
        /// <param name="values"></param>
        /// <returns>The number of fields that were set</returns>
        public int SetValues(params Object[] values)
        {
            int minValue = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < minValue; i++)
            {
                SetRecordValue(i, values[i]);
            }
            return minValue;
        }
        /// <summary>
        /// Sets a field to the DBNull value
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        public void SetDBNull(int ordinal)
        {
            SetRecordValue(ordinal, DBNull.Value);
        }
        /// <summary>
        /// Retrieve data record information
        /// </summary>
        public virtual DataRecordInfo DataRecordInfo
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
        /// <summary>
        /// Retrieves a field value as a DbDataRecord
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value as a DbDataRecord</returns>
        public DbDataRecord GetDataRecord(int ordinal)
        {
            return (DbDataRecord)GetValue(ordinal);
        }

        /// <summary>
        /// Used to return a nested result
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DbDataReader GetDataReader(int i)
        {
            return this.GetDbDataReader(i);
        }

        /// <summary>
        /// Sets the field value for a given ordinal
        /// </summary>
        /// <param name="ordinal">in the cspace mapping</param>
        /// <param name="value">in CSpace</param>
        protected abstract void SetRecordValue(int ordinal, object value);
    }

    public abstract class CurrentValueRecord : DbUpdatableDataRecord
    {
        internal CurrentValueRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject) :
            base(cacheEntry, metadata, userObject)
        {
        }

        internal CurrentValueRecord(ObjectStateEntry cacheEntry) :
            base(cacheEntry)
        {
        }
    }

    public abstract class OriginalValueRecord : DbUpdatableDataRecord
    {
        internal OriginalValueRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject) :
            base(cacheEntry, metadata, userObject)
        {
        }
    }
}
