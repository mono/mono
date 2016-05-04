//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xml
{
    using System.Runtime;

    enum StringHandleConstStringType
    {
        Type = 0,
        Root = 1,
        Item = 2
    }

    class StringHandle
    {
        XmlBufferReader bufferReader;
        StringHandleType type;
        int key;
        int offset;
        int length;
        static string[] constStrings = {
                                            "type",
                                            "root",
                                            "item"
                                       };

        public StringHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
            SetValue(0, 0);
        }

        public void SetValue(int offset, int length)
        {
            this.type = StringHandleType.UTF8;
            this.offset = offset;
            this.length = length;
        }

        public void SetConstantValue(StringHandleConstStringType constStringType)
        {
            type = StringHandleType.ConstString;
            key = (int)constStringType;
        }

        public void SetValue(int offset, int length, bool escaped)
        {
            this.type = (escaped ? StringHandleType.EscapedUTF8 : StringHandleType.UTF8);
            this.offset = offset;
            this.length = length;
        }

        public void SetValue(int key)
        {
            this.type = StringHandleType.Dictionary;
            this.key = key;
        }

        public void SetValue(StringHandle value)
        {
            this.type = value.type;
            this.key = value.key;
            this.offset = value.offset;
            this.length = value.length;
        }

        public bool IsEmpty
        {
            get
            {
                if (type == StringHandleType.UTF8)
                    return length == 0;
                return Equals2(string.Empty);
            }
        }

        public bool IsXmlns
        {
            get
            {
                if (type == StringHandleType.UTF8)
                {
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
                return Equals2("xmlns");
            }
        }

        public void ToPrefixHandle(PrefixHandle prefix)
        {
            Fx.Assert(type == StringHandleType.UTF8, "");
            prefix.SetValue(offset, length);
        }

        public string GetString(XmlNameTable nameTable)
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.UTF8)
                return bufferReader.GetString(offset, length, nameTable);
            if (type == StringHandleType.Dictionary)
                return nameTable.Add(bufferReader.GetDictionaryString(key).Value);
            if (type == StringHandleType.ConstString)
                return nameTable.Add(constStrings[key]);
            Fx.Assert(type == StringHandleType.EscapedUTF8, "");
            return bufferReader.GetEscapedString(offset, length, nameTable);
        }

        public string GetString()
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.UTF8)
                return bufferReader.GetString(offset, length);
            if (type == StringHandleType.Dictionary)
                return bufferReader.GetDictionaryString(key).Value;
            if (type == StringHandleType.ConstString)
                return constStrings[key];
            Fx.Assert(type == StringHandleType.EscapedUTF8, "");
            return bufferReader.GetEscapedString(offset, length);
        }

        public byte[] GetString(out int offset, out int length)
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.UTF8)
            {
                offset = this.offset;
                length = this.length;
                return bufferReader.Buffer;
            }
            if (type == StringHandleType.Dictionary)
            {
                byte[] buffer = bufferReader.GetDictionaryString(this.key).ToUTF8();
                offset = 0;
                length = buffer.Length;
                return buffer;
            }
            if (type == StringHandleType.ConstString)
            {
                byte[] buffer = XmlConverter.ToBytes(constStrings[key]);
                offset = 0;
                length = buffer.Length;
                return buffer;
            }
            else
            {
                Fx.Assert(type == StringHandleType.EscapedUTF8, "");
                byte[] buffer = XmlConverter.ToBytes(bufferReader.GetEscapedString(this.offset, this.length));
                offset = 0;
                length = buffer.Length;
                return buffer;
            }
        }

        public bool TryGetDictionaryString(out XmlDictionaryString value)
        {
            if (type == StringHandleType.Dictionary)
            {
                value = bufferReader.GetDictionaryString(key);
                return true;
            }
            else if (IsEmpty)
            {
                value = XmlDictionaryString.Empty;
                return true;
            }

            value = null;
            return false;
        }

        public override string ToString()
        {
            return GetString();
        }

        bool Equals2(int key2, XmlBufferReader bufferReader2)
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.Dictionary)
                return bufferReader.Equals2(this.key, key2, bufferReader2);
            if (type == StringHandleType.UTF8)
                return bufferReader.Equals2(this.offset, this.length, bufferReader2.GetDictionaryString(key2).Value);
            Fx.Assert(type == StringHandleType.EscapedUTF8 || type == StringHandleType.ConstString, "");
            return GetString() == bufferReader.GetDictionaryString(key2).Value;
        }

        bool Equals2(XmlDictionaryString xmlString2)
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.Dictionary)
                return bufferReader.Equals2(this.key, xmlString2);
            if (type == StringHandleType.UTF8)
                return bufferReader.Equals2(this.offset, this.length, xmlString2.ToUTF8());
            Fx.Assert(type == StringHandleType.EscapedUTF8 || type == StringHandleType.ConstString, "");
            return GetString() == xmlString2.Value;
        }

        bool Equals2(string s2)
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.Dictionary)
                return bufferReader.GetDictionaryString(this.key).Value == s2;
            if (type == StringHandleType.UTF8)
                return bufferReader.Equals2(this.offset, this.length, s2);
            Fx.Assert(type == StringHandleType.EscapedUTF8 || type == StringHandleType.ConstString, "");
            return GetString() == s2;
        }

        bool Equals2(int offset2, int length2, XmlBufferReader bufferReader2)
        {
            StringHandleType type = this.type;
            if (type == StringHandleType.Dictionary)
                return bufferReader2.Equals2(offset2, length2, bufferReader.GetDictionaryString(this.key).Value);
            if (type == StringHandleType.UTF8)
                return bufferReader.Equals2(this.offset, this.length, bufferReader2, offset2, length2);
            Fx.Assert(type == StringHandleType.EscapedUTF8 || type == StringHandleType.ConstString, "");
            return GetString() == bufferReader.GetString(offset2, length2);
        }

        bool Equals2(StringHandle s2)
        {
            StringHandleType type = s2.type;
            if (type == StringHandleType.Dictionary)
                return Equals2(s2.key, s2.bufferReader);
            if (type == StringHandleType.UTF8)
                return Equals2(s2.offset, s2.length, s2.bufferReader);
            Fx.Assert(type == StringHandleType.EscapedUTF8 || type == StringHandleType.ConstString, "");
            return Equals2(s2.GetString());
        }

        static public bool operator ==(StringHandle s1, XmlDictionaryString xmlString2)
        {
            return s1.Equals2(xmlString2);
        }

        static public bool operator !=(StringHandle s1, XmlDictionaryString xmlString2)
        {
            return !s1.Equals2(xmlString2);
        }

        static public bool operator ==(StringHandle s1, string s2)
        {
            return s1.Equals2(s2);
        }

        static public bool operator !=(StringHandle s1, string s2)
        {
            return !s1.Equals2(s2);
        }

        static public bool operator ==(StringHandle s1, StringHandle s2)
        {
            return s1.Equals2(s2);
        }

        static public bool operator !=(StringHandle s1, StringHandle s2)
        {
            return !s1.Equals2(s2);
        }

        public int CompareTo(StringHandle that)
        {
            if (this.type == StringHandleType.UTF8 && that.type == StringHandleType.UTF8)
                return bufferReader.Compare(this.offset, this.length, that.offset, that.length);
            else
                return string.Compare(this.GetString(), that.GetString(), StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            StringHandle that = obj as StringHandle;
            if (object.ReferenceEquals(that, null))
                return false;
            return this == that;
        }

        public override int GetHashCode()
        {
            return GetString().GetHashCode();
        }

        enum StringHandleType
        {
            Dictionary,
            UTF8,
            EscapedUTF8,
            ConstString
        }
    }
}
