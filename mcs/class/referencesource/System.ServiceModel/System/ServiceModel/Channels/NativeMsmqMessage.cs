//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;
    using Microsoft.Win32.SafeHandles;

    abstract class NativeMsmqMessage : IDisposable
    {
        UnsafeNativeMethods.MQPROPVARIANT[] variants;
        UnsafeNativeMethods.MQMSGPROPS nativeProperties;
        int[] ids;
        GCHandle nativePropertiesHandle;
        GCHandle variantsHandle;
        GCHandle idsHandle;
        MsmqProperty[] properties;
        bool disposed;
        object[] buffersForAsync;

        protected NativeMsmqMessage(int propertyCount)
        {
            this.properties = new MsmqProperty[propertyCount];
            this.nativeProperties = new UnsafeNativeMethods.MQMSGPROPS();
            this.ids = new int[propertyCount];
            this.variants = new UnsafeNativeMethods.MQPROPVARIANT[propertyCount];

            this.nativePropertiesHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            this.idsHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            this.variantsHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
        }

        ~NativeMsmqMessage()
        {
            Dispose(false);
        }

        public virtual void GrowBuffers()
        {
        }

        public object[] GetBuffersForAsync()
        {
            if (null == this.buffersForAsync)
            {
                int propertyBuffersToPin = 0;
                for (int i = 0; i < this.nativeProperties.count; ++i)
                {
                    if (this.properties[i].MaintainsBuffer)
                        ++propertyBuffersToPin;
                }
                this.buffersForAsync = new object[propertyBuffersToPin + 3];
            }

            int bufferCount = 0;
            for (int i = 0; i < this.nativeProperties.count; ++i)
            {
                if (this.properties[i].MaintainsBuffer)
                {
                    this.buffersForAsync[bufferCount++] = this.properties[i].MaintainedBuffer;
                }
            }
            this.buffersForAsync[bufferCount++] = this.ids;
            this.buffersForAsync[bufferCount++] = this.variants;
            this.buffersForAsync[bufferCount] = this.nativeProperties;

            return this.buffersForAsync;
        }

        public IntPtr Pin()
        {
            for (int i = 0; i < this.nativeProperties.count; i++)
                properties[i].Pin();

            this.idsHandle.Target = this.ids;
            this.variantsHandle.Target = this.variants;

            this.nativeProperties.status = IntPtr.Zero;
            this.nativeProperties.variants = this.variantsHandle.AddrOfPinnedObject();
            this.nativeProperties.ids = this.idsHandle.AddrOfPinnedObject();
            this.nativePropertiesHandle.Target = this.nativeProperties;

            return nativePropertiesHandle.AddrOfPinnedObject();
        }

        public void Unpin()
        {
            this.nativePropertiesHandle.Target = null;
            this.idsHandle.Target = null;
            this.variantsHandle.Target = null;

            for (int i = 0; i < this.nativeProperties.count; i++)
                properties[i].Unpin();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                for (int i = 0; i < this.nativeProperties.count; i++)
                {
                    this.properties[i].Dispose();
                }

                this.disposed = true;
            }
            if (this.nativePropertiesHandle.IsAllocated)
                this.nativePropertiesHandle.Free();
            if (this.idsHandle.IsAllocated)
                this.idsHandle.Free();
            if (this.variantsHandle.IsAllocated)
                this.variantsHandle.Free();
        }

        public abstract class MsmqProperty : IDisposable
        {
            UnsafeNativeMethods.MQPROPVARIANT[] variants;
            int index;

            protected MsmqProperty(NativeMsmqMessage message, int id, ushort vt)
            {
                this.variants = message.variants;
                this.index = message.nativeProperties.count++;
                message.variants[this.index].vt = vt;
                message.ids[this.index] = id;
                message.properties[this.index] = this;
            }

            protected int Index
            {
                get { return this.index; }
            }

            public virtual bool MaintainsBuffer
            {
                get { return false; }
            }

            public virtual object MaintainedBuffer
            {
                get { return null; }
            }

            public virtual void Pin()
            {
            }

            public virtual void Unpin()
            {
            }

            public virtual void Dispose()
            {
            }

            protected UnsafeNativeMethods.MQPROPVARIANT[] Variants
            {
                get { return this.variants; }
            }
        }

        public class ByteProperty : MsmqProperty
        {
            public ByteProperty(NativeMsmqMessage message, int id)
                : base(message, id, UnsafeNativeMethods.VT_UI1)
            {
            }

            public ByteProperty(NativeMsmqMessage message, int id, byte value)
                : this(message, id)
            {
                this.Value = value;
            }

            public byte Value
            {
                get
                {
                    return this.Variants[this.Index].byteValue;
                }
                set
                {
                    this.Variants[this.Index].byteValue = value;
                }
            }
        }

        public class ShortProperty : MsmqProperty
        {
            public ShortProperty(NativeMsmqMessage message, int id)
                : base(message, id, UnsafeNativeMethods.VT_UI2)
            {
            }

            public ShortProperty(NativeMsmqMessage message, int id, short value)
                : this(message, id)
            {
                this.Value = value;
            }

            public short Value
            {
                get
                {
                    return this.Variants[this.Index].shortValue;
                }
                set
                {
                    this.Variants[this.Index].shortValue = value;
                }
            }
        }


        public class BooleanProperty : MsmqProperty
        {
            public BooleanProperty(NativeMsmqMessage message, int id)
                : base(message, id, UnsafeNativeMethods.VT_BOOL)
            {
            }

            public BooleanProperty(NativeMsmqMessage message, int id, bool value)
                : this(message, id)
            {
                this.Value = value;
            }

            public bool Value
            {
                get
                {
                    return this.Variants[this.Index].shortValue != 0;
                }
                set
                {
                    this.Variants[this.Index].shortValue = value ? (short)-1 : (short)0;
                }
            }
        }

        public class IntProperty : MsmqProperty
        {
            public IntProperty(NativeMsmqMessage message, int id)
                : base(message, id, UnsafeNativeMethods.VT_UI4)
            {
            }

            public IntProperty(NativeMsmqMessage message, int id, int value)
                : this(message, id)
            {
                this.Value = value;
            }

            public int Value
            {
                get
                {
                    return this.Variants[this.Index].intValue;
                }
                set
                {
                    this.Variants[this.Index].intValue = value;
                }
            }
        }

        public class LongProperty : MsmqProperty
        {
            public LongProperty(NativeMsmqMessage message, int id)
                : base(message, id, UnsafeNativeMethods.VT_UI8)
            {
            }

            public LongProperty(NativeMsmqMessage message, int id, long value)
                : this(message, id)
            {
                this.Value = value;
            }

            public long Value
            {
                get
                {
                    return this.Variants[this.Index].longValue;
                }
                set
                {
                    this.Variants[this.Index].longValue = value;
                }
            }
        }

        public class BufferProperty : MsmqProperty
        {
            byte[] buffer;
            GCHandle bufferHandle;

            public BufferProperty(NativeMsmqMessage message, int id)
                : base(message, id, UnsafeNativeMethods.VT_UI1 | UnsafeNativeMethods.VT_VECTOR)
            {
                bufferHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            }

            public BufferProperty(NativeMsmqMessage message, int id, byte[] buffer)
                : this(message, id, buffer.Length)
            {
                System.Buffer.BlockCopy(buffer, 0, this.Buffer, 0, buffer.Length);
            }

            public BufferProperty(NativeMsmqMessage message, int id, int length)
                : this(message, id)
            {
                SetBufferReference(DiagnosticUtility.Utility.AllocateByteArray(length));
            }

            ~BufferProperty()
            {
                Dispose(false);
            }

            public override void Dispose()
            {
                base.Dispose();
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (bufferHandle.IsAllocated)
                    bufferHandle.Free();
            }

            public void SetBufferReference(byte[] buffer)
            {
                SetBufferReference(buffer, buffer.Length);
            }

            public void SetBufferReference(byte[] buffer, int length)
            {
                this.buffer = buffer;
                this.BufferLength = length;
            }

            public override bool MaintainsBuffer
            {
                get { return true; }
            }

            public override object MaintainedBuffer
            {
                get { return this.buffer; }
            }

            public override void Pin()
            {
                bufferHandle.Target = buffer;
                this.Variants[this.Index].byteArrayValue.intPtr = bufferHandle.AddrOfPinnedObject();
            }

            public override void Unpin()
            {
                this.Variants[this.Index].byteArrayValue.intPtr = IntPtr.Zero;
                bufferHandle.Target = null;
            }

            public byte[] GetBufferCopy(int length)
            {
                byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(length);
                System.Buffer.BlockCopy(this.buffer, 0, buffer, 0, length);
                return buffer;
            }

            public void EnsureBufferLength(int length)
            {
                if (this.buffer.Length < length)
                {
                    SetBufferReference(DiagnosticUtility.Utility.AllocateByteArray(length));
                }
            }

            public int BufferLength
            {
                get
                {
                    return this.Variants[this.Index].byteArrayValue.size;
                }
                set
                {
                    if (value > this.buffer.Length)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                    this.Variants[this.Index].byteArrayValue.size = value;
                }
            }

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }
        }

        public class StringProperty : MsmqProperty
        {
            char[] buffer;
            GCHandle bufferHandle;

            internal StringProperty(NativeMsmqMessage message, int id, string value)
                : this(message, id, value.Length + 1)
            {
                CopyValueToBuffer(value);
            }

            internal StringProperty(NativeMsmqMessage message, int id, int length)
                : base(message, id, UnsafeNativeMethods.VT_LPWSTR)
            {
                this.buffer = DiagnosticUtility.Utility.AllocateCharArray(length);
                this.bufferHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            }

            ~StringProperty()
            {
                Dispose(false);
            }

            public override bool MaintainsBuffer
            {
                get { return true; }
            }

            public override object MaintainedBuffer
            {
                get { return this.buffer; }
            }

            public override void Pin()
            {
                this.bufferHandle.Target = buffer;
                this.Variants[this.Index].intPtr = bufferHandle.AddrOfPinnedObject();
            }

            public override void Unpin()
            {
                this.Variants[this.Index].intPtr = IntPtr.Zero;
                this.bufferHandle.Target = null;
            }

            public override void Dispose()
            {
                base.Dispose();
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (bufferHandle.IsAllocated)
                    bufferHandle.Free();
            }

            public void EnsureValueLength(int length)
            {
                if (length > this.buffer.Length)
                {
                    this.buffer = DiagnosticUtility.Utility.AllocateCharArray(length);
                }
            }

            public void SetValue(string value)
            {
                if (null == value)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                EnsureValueLength(value.Length + 1);
                CopyValueToBuffer(value);
            }

            void CopyValueToBuffer(string value)
            {
                value.CopyTo(0, this.buffer, 0, value.Length);
                this.buffer[value.Length] = '\0';
            }

            public string GetValue(int length)
            {
                if (length == 0)
                {
                    return null;
                }
                else
                {
                    return new string(this.buffer, 0, length - 1);
                }
            }
        }
    }

    static class MsmqMessageId
    {
        const int guidSize = 16;

        public static string ToString(byte[] messageId)
        {
            StringBuilder result = new StringBuilder();
            byte[] guid = new byte[guidSize];
            Array.Copy(messageId, guid, guidSize);
            int id = BitConverter.ToInt32(messageId, guidSize);
            result.Append((new Guid(guid)).ToString());
            result.Append("\\");
            result.Append(id);
            return result.ToString();
        }

        public static byte[] FromString(string messageId)
        {
            string[] pieces = messageId.Split(new char[] { '\\' });
            if (pieces.Length != 2)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidMessageId, messageId), "messageId"));

            Guid guid;
            if (!DiagnosticUtility.Utility.TryCreateGuid(pieces[0], out guid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidMessageId, messageId), "messageId"));
            }

            int integerId;
            try
            {
                integerId = Convert.ToInt32(pieces[1], CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidMessageId, messageId), "messageId"));
            }

            byte[] bytes = new byte[UnsafeNativeMethods.PROPID_M_MSGID_SIZE];
            Array.Copy(guid.ToByteArray(), bytes, guidSize);
            Array.Copy(BitConverter.GetBytes(integerId), 0, bytes, guidSize, 4);
            return bytes;
        }
    }

    class MsmqEmptyMessage : NativeMsmqMessage
    {
        public MsmqEmptyMessage()
            : base(0)
        { }
    }

    static class MsmqDuration
    {
        public static int FromTimeSpan(TimeSpan timeSpan)
        {
            long totalSeconds = (long)timeSpan.TotalSeconds;
            if (totalSeconds > int.MaxValue)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                                                              SR.GetString(SR.MsmqTimeSpanTooLarge)));
            return (int)totalSeconds;
        }

        public static TimeSpan ToTimeSpan(int seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }
    }

    static class MsmqDateTime
    {
        public static DateTime ToDateTime(int seconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(seconds);
        }
    }
}
