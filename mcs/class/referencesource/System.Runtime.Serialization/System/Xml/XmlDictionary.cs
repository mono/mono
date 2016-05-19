//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Runtime.Serialization;

    public class XmlDictionary : IXmlDictionary
    {
        static IXmlDictionary empty;
        Dictionary<string, XmlDictionaryString> lookup;
        XmlDictionaryString[] strings;
        int nextId;

        static public IXmlDictionary Empty
        {
            get
            {
                if (empty == null)
                    empty = new EmptyDictionary();
                return empty;
            }
        }

        public XmlDictionary()
        {
            this.lookup = new Dictionary<string, XmlDictionaryString>();
            this.strings = null;
            this.nextId = 0;
        }

        public XmlDictionary(int capacity)
        {
            this.lookup = new Dictionary<string, XmlDictionaryString>(capacity);
            this.strings = new XmlDictionaryString[capacity];
            this.nextId = 0;
        }

        public virtual XmlDictionaryString Add(string value)
        {
            XmlDictionaryString str;
            if (!this.lookup.TryGetValue(value, out str))
            {
                if (this.strings == null)
                {
                    this.strings = new XmlDictionaryString[4];
                }
                else if (this.nextId == this.strings.Length)
                {
                    int newSize = this.nextId * 2;
                    if (newSize == 0)
                        newSize = 4;
                    Array.Resize(ref this.strings, newSize);
                }
                str = new XmlDictionaryString(this, value, this.nextId);
                this.strings[this.nextId] = str;
                this.lookup.Add(value, str);
                this.nextId++;
            }
            return str;
        }

        public virtual bool TryLookup(string value, out XmlDictionaryString result)
        {
            return this.lookup.TryGetValue(value, out result);
        }

        public virtual bool TryLookup(int key, out XmlDictionaryString result)
        {
            if (key < 0 || key >= this.nextId)
            {
                result = null;
                return false;
            }
            result = this.strings[key];
            return true;
        }

        public virtual bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
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

        class EmptyDictionary : IXmlDictionary
        {
            public bool TryLookup(string value, out XmlDictionaryString result)
            {
                result = null;
                return false;
            }

            public bool TryLookup(int key, out XmlDictionaryString result)
            {
                result = null;
                return false;
            }

            public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
            {
                result = null;
                return false;
            }
        }
    }
}


