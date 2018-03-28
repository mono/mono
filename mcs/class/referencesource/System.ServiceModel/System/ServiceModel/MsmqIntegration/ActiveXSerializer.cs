//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System;
    using System.IO;
    using System.Globalization;

    class ActiveXSerializer
    {

        byte[] byteBuffer;
        char[] charBuffer;
        object bufferLock = new object();

        TKind[] TakeLockedBuffer<TKind>(out bool lockHeld, int size)
        {
            lockHeld = false;
            Monitor.Enter(this.bufferLock, ref lockHeld);
            if (typeof(byte) == typeof(TKind))
            {
                if (null == this.byteBuffer || size > this.byteBuffer.Length)
                    this.byteBuffer = new byte[size];
                return this.byteBuffer as TKind[];
            }
            else if (typeof(char) == typeof(TKind))
            {
                if (null == this.charBuffer || size > this.charBuffer.Length)
                    this.charBuffer = new char[size];
                return this.charBuffer as TKind[];
            }
            else
                return null;
        }

        void ReleaseLockedBuffer()
        {
            Monitor.Exit(this.bufferLock);
        }

        public object Deserialize(MemoryStream stream, int bodyType)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));

            VarEnum variantType = (VarEnum)bodyType;

            byte[] bytes;
            byte[] newBytes;
            int size;
            int count;
            bool lockHeld;

            switch (variantType)
            {
                case VarEnum.VT_LPSTR:
                    bytes = stream.ToArray();
                    size = bytes.Length;

                    lockHeld = false;
                    try
                    {
                        char[] buffer = TakeLockedBuffer<char>(out lockHeld, size);
                        System.Text.Encoding.ASCII.GetChars(bytes, 0, size, buffer, 0);
                        return new String(buffer, 0, size);
                    }
                    finally
                    {
                        if (lockHeld)
                        {
                            ReleaseLockedBuffer();
                        }
                    }
                case VarEnum.VT_BSTR:
                case VarEnum.VT_LPWSTR:
                    bytes = stream.ToArray();
                    size = bytes.Length / 2;

                    lockHeld = false;
                    try
                    {
                        char[] buffer = TakeLockedBuffer<char>(out lockHeld, size);
                        System.Text.Encoding.Unicode.GetChars(bytes, 0, size * 2, buffer, 0);
                        return new String(buffer, 0, size);
                    }
                    finally
                    {
                        if (lockHeld)
                        {
                            ReleaseLockedBuffer();
                        }
                    }
                case VarEnum.VT_VECTOR | VarEnum.VT_UI1:
                    bytes = stream.ToArray();
                    newBytes = new byte[bytes.Length];
                    Array.Copy(bytes, newBytes, bytes.Length);

                    return newBytes;

                case VarEnum.VT_BOOL:
                    bytes = new byte[1];
                    count = stream.Read(bytes, 0, 1);

                    if (count != 1)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return (bytes[0] != 0);

                case VarEnum.VT_CLSID:
                    bytes = new byte[16];
                    count = stream.Read(bytes, 0, 16);

                    if (count != 16)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return new Guid(bytes);

                case VarEnum.VT_CY:

                    bytes = new byte[8];
                    count = stream.Read(bytes, 0, 8);

                    if (count != 8)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return Decimal.FromOACurrency(BitConverter.ToInt64(bytes, 0));

                case VarEnum.VT_DATE:

                    bytes = new byte[8];
                    count = stream.Read(bytes, 0, 8);

                    if (count != 8)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return new DateTime(BitConverter.ToInt64(bytes, 0));

                case VarEnum.VT_I1:
                case VarEnum.VT_UI1:

                    bytes = new byte[1];
                    count = stream.Read(bytes, 0, 1);

                    if (count != 1)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return bytes[0];

                case VarEnum.VT_I2:
                    bytes = new byte[2];
                    count = stream.Read(bytes, 0, 2);

                    if (count != 2)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToInt16(bytes, 0);

                case VarEnum.VT_UI2:
                    bytes = new byte[2];
                    count = stream.Read(bytes, 0, 2);

                    if (count != 2)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToUInt16(bytes, 0);

                case VarEnum.VT_I4:
                    bytes = new byte[4];
                    count = stream.Read(bytes, 0, 4);

                    if (count != 4)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToInt32(bytes, 0);

                case VarEnum.VT_UI4:
                    bytes = new byte[4];
                    count = stream.Read(bytes, 0, 4);

                    if (count != 4)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToUInt32(bytes, 0);

                case VarEnum.VT_I8:
                    bytes = new byte[8];
                    count = stream.Read(bytes, 0, 8);

                    if (count != 8)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToInt64(bytes, 0);

                case VarEnum.VT_UI8:
                    bytes = new byte[8];
                    count = stream.Read(bytes, 0, 8);

                    if (count != 8)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToUInt64(bytes, 0);

                case VarEnum.VT_R4:
                    bytes = new byte[4];
                    count = stream.Read(bytes, 0, 4);

                    if (count != 4)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToSingle(bytes, 0);

                case VarEnum.VT_R8:
                    bytes = new byte[8];
                    count = stream.Read(bytes, 0, 8);

                    if (count != 8)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqCannotDeserializeActiveXMessage)));

                    return BitConverter.ToDouble(bytes, 0);

                case VarEnum.VT_NULL:
                    return null;


                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqInvalidTypeDeserialization)));
            }
        }


        public void Serialize(Stream stream, object obj, ref int bodyType)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));

            VarEnum variantType;
            if (obj is string)
            {
                int size = ((string)obj).Length * 2;

                bool lockHeld = false;
                try
                {
                    byte[] buffer = TakeLockedBuffer<byte>(out lockHeld, size);
                    System.Text.Encoding.Unicode.GetBytes(((string)obj).ToCharArray(), 0, size / 2, buffer, 0);
                    stream.Write(buffer, 0, size);
                }
                finally
                {
                    if (lockHeld)
                    {
                        ReleaseLockedBuffer();
                    }
                }
                variantType = VarEnum.VT_LPWSTR;
            }
            else if (obj is byte[])
            {
                byte[] bytes = (byte[])obj;
                stream.Write(bytes, 0, bytes.Length);
                variantType = VarEnum.VT_UI1 | VarEnum.VT_VECTOR;
            }
            else if (obj is char[])
            {
                char[] chars = (char[])obj;
                int size = chars.Length * 2;

                bool lockHeld = false;
                try
                {
                    byte[] buffer = TakeLockedBuffer<byte>(out lockHeld, size);
                    System.Text.Encoding.Unicode.GetBytes(chars, 0, size / 2, buffer, 0);
                    stream.Write(buffer, 0, size);
                }
                finally
                {
                    if (lockHeld)
                    {
                        ReleaseLockedBuffer();
                    }
                }
                variantType = VarEnum.VT_LPWSTR;
            }
            else if (obj is byte)
            {
                stream.Write(new byte[] { (byte)obj }, 0, 1);
                variantType = VarEnum.VT_UI1;
            }
            else if (obj is bool)
            {
                if ((bool)obj)
                    stream.Write(new byte[] { 0xff }, 0, 1);
                else
                    stream.Write(new byte[] { 0x00 }, 0, 1);
                variantType = VarEnum.VT_BOOL;
            }
            else if (obj is char)
            {
                byte[] bytes = BitConverter.GetBytes((Char)obj);
                stream.Write(bytes, 0, 2);
                variantType = VarEnum.VT_UI2;
            }
            else if (obj is Decimal)
            {
                byte[] bytes = BitConverter.GetBytes(Decimal.ToOACurrency((Decimal)obj));
                stream.Write(bytes, 0, 8);
                variantType = VarEnum.VT_CY;
            }
            else if (obj is DateTime)
            {
                byte[] bytes = BitConverter.GetBytes(((DateTime)obj).Ticks);
                stream.Write(bytes, 0, 8);
                variantType = VarEnum.VT_DATE;
            }
            else if (obj is Double)
            {
                byte[] bytes = BitConverter.GetBytes((Double)obj);
                stream.Write(bytes, 0, 8);
                variantType = VarEnum.VT_R8;
            }
            else if (obj is Guid)
            {
                byte[] bytes = ((Guid)obj).ToByteArray();
                stream.Write(bytes, 0, 16);
                variantType = VarEnum.VT_CLSID;
            }
            else if (obj is Int16)
            {
                byte[] bytes = BitConverter.GetBytes((short)obj);
                stream.Write(bytes, 0, 2);
                variantType = VarEnum.VT_I2;
            }
            else if (obj is UInt16)
            {
                byte[] bytes = BitConverter.GetBytes((UInt16)obj);
                stream.Write(bytes, 0, 2);
                variantType = VarEnum.VT_UI2;
            }
            else if (obj is Int32)
            {
                byte[] bytes = BitConverter.GetBytes((int)obj);
                stream.Write(bytes, 0, 4);
                variantType = VarEnum.VT_I4;
            }
            else if (obj is UInt32)
            {

                byte[] bytes = BitConverter.GetBytes((UInt32)obj);
                stream.Write(bytes, 0, 4);
                variantType = VarEnum.VT_UI4;
            }
            else if (obj is Int64)
            {

                byte[] bytes = BitConverter.GetBytes((Int64)obj);
                stream.Write(bytes, 0, 8);
                variantType = VarEnum.VT_I8;
            }
            else if (obj is UInt64)
            {

                byte[] bytes = BitConverter.GetBytes((UInt64)obj);
                stream.Write(bytes, 0, 8);
                variantType = VarEnum.VT_UI8;
            }
            else if (obj is Single)
            {

                byte[] bytes = BitConverter.GetBytes((float)obj);
                stream.Write(bytes, 0, 4);
                variantType = VarEnum.VT_R4;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqInvalidTypeSerialization)));
            }

            bodyType = (int)variantType;
        }

    }
}
