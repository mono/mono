// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;

namespace System.Runtime.Serialization
{
    /// <summary>
    ///     Represents an <see cref="IFormatterConverter"/> where no conversion is actually performed.
    /// </summary>
    public class StrictFormatterConverter : IFormatterConverter
    {
        public object Convert(object value, TypeCode typeCode)
        {
            return value;
        }

        public object Convert(object value, Type type)
        {
            return value;
        }

        public bool ToBoolean(object value)
        {
            return (bool)value;
        }

        public byte ToByte(object value)
        {
            return (byte)value;
        }

        public char ToChar(object value)
        {
            return (char)value;
        }

        public DateTime ToDateTime(object value)
        {
            return (DateTime)value;
        }

        public decimal ToDecimal(object value)
        {
            return (decimal)value;
        }

        public double ToDouble(object value)
        {
            return (double)value;
        }

        public short ToInt16(object value)
        {
            return (short)value;
        }

        public int ToInt32(object value)
        {
            return (int)value;
        }

        public long ToInt64(object value)
        {
            return (long)value;
        }

        [CLSCompliant(false)]
        public sbyte ToSByte(object value)
        {
            return (sbyte)value;
        }

        public float ToSingle(object value)
        {
            return (float)value;
        }

        public string ToString(object value)
        {
            return (string)value;
        }

        [CLSCompliant(false)]
        public ushort ToUInt16(object value)
        {
            return (ushort)value;
        }

        [CLSCompliant(false)]
        public uint ToUInt32(object value)
        {
            return (uint)value;
        }

        [CLSCompliant(false)]
        public ulong ToUInt64(object value)
        {
            return (ulong)value;
        }
    }
}

#endif // !SILVERLIGHT