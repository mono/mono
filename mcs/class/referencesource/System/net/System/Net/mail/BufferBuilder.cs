//-----------------------------------------------------------------------------
// <copyright file="BufferBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Text;

    internal class BufferBuilder
    {
        byte[] buffer;
        int offset;

        internal BufferBuilder() : this(256)
        {
        }

        internal BufferBuilder(int initialSize)
        {
            this.buffer = new byte[initialSize];
        }

        void EnsureBuffer(int count)
        {
            if (count > this.buffer.Length - this.offset)
            {
                byte[] newBuffer = new byte[((buffer.Length * 2)>(buffer.Length + count))?(buffer.Length*2):(buffer.Length + count)];
                Buffer.BlockCopy(this.buffer, 0, newBuffer, 0, this.offset);
                this.buffer = newBuffer;
            }
        }

        internal void Append(byte value)
        {
            EnsureBuffer(1);
            this.buffer[this.offset++] = value;
        }

        internal void Append(byte[] value)
        {
            Append(value, 0, value.Length);
        }

        internal void Append(byte[] value, int offset, int count)
        {
            EnsureBuffer(count);
            Buffer.BlockCopy(value, offset, this.buffer, this.offset, count);
            this.offset += count;
        }

        internal void Append(string value)
        {
            Append(value, false);
        }

        internal void Append(string value, bool allowUnicode)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }
            Append(value, 0, value.Length, allowUnicode);
        }

        internal void Append(string value, int offset, int count, bool allowUnicode)
        {
            if (allowUnicode)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value.ToCharArray(), offset, count);
                Append(bytes);
            }
            else
            {
                Append(value, offset, count);
            }
        }

        // Does not allow unicode, only ANSI
        internal void Append(string value, int offset, int count)
        {
            EnsureBuffer(count);
            for (int i = 0; i < count; i++)
            {
                char c = value[offset+i];
                if ((ushort)c > 0xFF)
                    throw new FormatException(SR.GetString(SR.MailHeaderFieldInvalidCharacter, c));
                this.buffer[this.offset + i] = (byte)c;
            }
            this.offset += count;
        }

        internal int Length
        {
            get
            {
                return this.offset;
            }
        }

        internal byte[] GetBuffer()
        {
            return this.buffer;
        }

        internal void Reset()
        {
            this.offset = 0;
        }
    }
}
