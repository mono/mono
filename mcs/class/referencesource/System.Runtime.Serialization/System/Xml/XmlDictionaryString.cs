//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.Xml;
    using System.Text;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    public class XmlDictionaryString
    {
        internal const int MinKey = 0;
        internal const int MaxKey = int.MaxValue / 4;

        IXmlDictionary dictionary;
        string value;
        int key;
        byte[] buffer;
        static EmptyStringDictionary emptyStringDictionary = new EmptyStringDictionary();

        public XmlDictionaryString(IXmlDictionary dictionary, string value, int key)
        {
            if (dictionary == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dictionary"));
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            if (key < MinKey || key > MaxKey)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("key", SR.GetString(SR.ValueMustBeInRange, MinKey, MaxKey)));
            this.dictionary = dictionary;
            this.value = value;
            this.key = key;
        }

        static internal string GetString(XmlDictionaryString s)
        {
            if (s == null)
                return null;
            return s.Value;
        }

        static public XmlDictionaryString Empty
        {
            get
            {
                return emptyStringDictionary.EmptyString;
            }
        }

        public IXmlDictionary Dictionary
        {
            get
            {
                return dictionary;
            }
        }

        public int Key
        {
            get
            {
                return key;
            }
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        internal byte[] ToUTF8()
        {
            if (buffer == null)
                buffer = System.Text.Encoding.UTF8.GetBytes(value);
            return buffer;
        }

        public override string ToString()
        {
            return value;
        }

        class EmptyStringDictionary : IXmlDictionary
        {
            XmlDictionaryString empty;

            public EmptyStringDictionary()
            {
                empty = new XmlDictionaryString(this, string.Empty, 0);
            }

            public XmlDictionaryString EmptyString
            {
                get
                {
                    return empty;
                }
            }

            public bool TryLookup(string value, out XmlDictionaryString result)
            {
                if (value == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                if (value.Length == 0)
                {
                    result = empty;
                    return true;
                }
                result = null;
                return false;
            }

            public bool TryLookup(int key, out XmlDictionaryString result)
            {
                if (key == 0)
                {
                    result = empty;
                    return true;
                }
                result = null;
                return false;
            }

            public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
            {
                if (value == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                if (value.Dictionary != this)
                {
                    result = null;
                    return false;
                }
                result = value;
                return true;
            }
        }
    }
}
