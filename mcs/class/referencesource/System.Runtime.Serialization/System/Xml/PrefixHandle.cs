//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.Runtime;

    enum PrefixHandleType
    {
        Empty,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Buffer,
        Max,
    }

    class PrefixHandle
    {
        XmlBufferReader bufferReader;
        PrefixHandleType type;
        int offset;
        int length;
        static string[] prefixStrings = { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        static byte[] prefixBuffer = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z' };

        public PrefixHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
        }

        public void SetValue(PrefixHandleType type)
        {
            Fx.Assert(type != PrefixHandleType.Buffer, "");
            this.type = type;
        }

        public void SetValue(PrefixHandle prefix)
        {
            this.type = prefix.type;
            this.offset = prefix.offset;
            this.length = prefix.length;
        }

        public void SetValue(int offset, int length)
        {
            if (length == 0)
            {
                SetValue(PrefixHandleType.Empty);
                return;
            }

            if (length == 1)
            {
                byte ch = bufferReader.GetByte(offset);
                if (ch >= 'a' && ch <= 'z')
                {
                    SetValue(GetAlphaPrefix(ch - 'a'));
                    return;
                }
            }

            this.type = PrefixHandleType.Buffer;
            this.offset = offset;
            this.length = length;
        }

        public bool IsEmpty
        {
            get
            {
                return type == PrefixHandleType.Empty;
            }
        }

        public bool IsXmlns
        {
            get
            {
                if (type != PrefixHandleType.Buffer)
                    return false;
                if (this.length != 5)
                    return false;
                byte[] buffer = bufferReader.Buffer;
                int offset = this.offset;
                return buffer[offset + 0] == 'x' &&
                       buffer[offset + 1] == 'm' &&
                       buffer[offset + 2] == 'l' &&
                       buffer[offset + 3] == 'n' &&
                       buffer[offset + 4] == 's';
            }
        }

        public bool IsXml
        {
            get
            {
                if (type != PrefixHandleType.Buffer)
                    return false;
                if (this.length != 3)
                    return false;
                byte[] buffer = bufferReader.Buffer;
                int offset = this.offset;
                return buffer[offset + 0] == 'x' &&
                       buffer[offset + 1] == 'm' &&
                       buffer[offset + 2] == 'l';
            }
        }

        public bool TryGetShortPrefix(out PrefixHandleType type)
        {
            type = this.type;
            return (type != PrefixHandleType.Buffer);
        }

        public static string GetString(PrefixHandleType type)
        {
            Fx.Assert(type != PrefixHandleType.Buffer, "");
            return prefixStrings[(int)type];
        }

        public static PrefixHandleType GetAlphaPrefix(int index)
        {
            Fx.Assert(index >= 0 && index < 26, "");
            return (PrefixHandleType)(PrefixHandleType.A + index);
        }

        public static byte[] GetString(PrefixHandleType type, out int offset, out int length)
        {
            Fx.Assert(type != PrefixHandleType.Buffer, "");
            if (type == PrefixHandleType.Empty)
            {
                offset = 0;
                length = 0;
            }
            else
            {
                length = 1;
                offset = (int)(type - PrefixHandleType.A);
            }
            return prefixBuffer;
        }

        public string GetString(XmlNameTable nameTable)
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
                return GetString(type);
            else
                return bufferReader.GetString(offset, length, nameTable);
        }

        public string GetString()
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
                return GetString(type);
            else
                return bufferReader.GetString(offset, length);
        }

        public byte[] GetString(out int offset, out int length)
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
                return GetString(type, out offset, out length);
            else
            {
                offset = this.offset;
                length = this.length;
                return bufferReader.Buffer;
            }
        }

        public int CompareTo(PrefixHandle that)
        {
            return GetString().CompareTo(that.GetString());
        }

        bool Equals2(PrefixHandle prefix2)
        {
            PrefixHandleType type1 = this.type;
            PrefixHandleType type2 = prefix2.type;
            if (type1 != type2)
                return false;
            if (type1 != PrefixHandleType.Buffer)
                return true;
            if (this.bufferReader == prefix2.bufferReader)
                return bufferReader.Equals2(this.offset, this.length, prefix2.offset, prefix2.length);
            else
                return bufferReader.Equals2(this.offset, this.length, prefix2.bufferReader, prefix2.offset, prefix2.length);
        }

        bool Equals2(string prefix2)
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
                return GetString(type) == prefix2;
            return bufferReader.Equals2(this.offset, this.length, prefix2);
        }

        bool Equals2(XmlDictionaryString prefix2)
        {
            return Equals2(prefix2.Value);
        }

        static public bool operator ==(PrefixHandle prefix1, string prefix2)
        {
            return prefix1.Equals2(prefix2);
        }

        static public bool operator !=(PrefixHandle prefix1, string prefix2)
        {
            return !prefix1.Equals2(prefix2);
        }

        static public bool operator ==(PrefixHandle prefix1, XmlDictionaryString prefix2)
        {
            return prefix1.Equals2(prefix2);
        }

        static public bool operator !=(PrefixHandle prefix1, XmlDictionaryString prefix2)
        {
            return !prefix1.Equals2(prefix2);
        }

        static public bool operator ==(PrefixHandle prefix1, PrefixHandle prefix2)
        {
            return prefix1.Equals2(prefix2);
        }

        static public bool operator !=(PrefixHandle prefix1, PrefixHandle prefix2)
        {
            return !prefix1.Equals2(prefix2);
        }

        public override bool Equals(object obj)
        {
            PrefixHandle that = obj as PrefixHandle;
            if (object.ReferenceEquals(that, null))
                return false;
            return this == that;
        }

        public override string ToString()
        {
            return GetString();
        }

        public override int GetHashCode()
        {
            return GetString().GetHashCode();
        }
    }
}
