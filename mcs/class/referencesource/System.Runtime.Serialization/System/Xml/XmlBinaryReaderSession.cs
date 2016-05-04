//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using StringHandle = System.Int64;

namespace System.Xml
{
    using System.Xml;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class XmlBinaryReaderSession : IXmlDictionary
    {
        const int MaxArrayEntries = 2048;

        XmlDictionaryString[] strings;
        Dictionary<int, XmlDictionaryString> stringDict;

        public XmlBinaryReaderSession()
        {
        }

        public XmlDictionaryString Add(int id, string value)
        {
            if (id < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(SR.GetString(SR.XmlInvalidID)));
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            XmlDictionaryString xmlString;
            if (TryLookup(id, out xmlString))
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlIDDefined)));

            xmlString = new XmlDictionaryString(this, value, id);
            if (id >= MaxArrayEntries)
            {
                if (stringDict == null)
                    this.stringDict = new Dictionary<int, XmlDictionaryString>();

                this.stringDict.Add(id, xmlString);
            }
            else
            {
                if (strings == null)
                {
                    strings = new XmlDictionaryString[Math.Max(id + 1, 16)];
                }
                else if (id >= strings.Length)
                {
                    XmlDictionaryString[] newStrings = new XmlDictionaryString[Math.Min(Math.Max(id + 1, strings.Length * 2), MaxArrayEntries)];
                    Array.Copy(strings, newStrings, strings.Length);
                    strings = newStrings;
                }
                strings[id] = xmlString;
            }
            return xmlString;
        }

        public bool TryLookup(int key, out XmlDictionaryString result)
        {
            if (strings != null && key >= 0 && key < strings.Length)
            {
                result = strings[key];
                return result != null;
            }
            else if (key >= MaxArrayEntries)
            {
                if (this.stringDict != null)
                    return this.stringDict.TryGetValue(key, out result);
            }
            result = null;
            return false;
        }

        public bool TryLookup(string value, out XmlDictionaryString result)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            if (strings != null)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    XmlDictionaryString s = strings[i];
                    if (s != null && s.Value == value)
                    {
                        result = s;
                        return true;
                    }
                }
            }

            if (this.stringDict != null)
            {
                foreach (XmlDictionaryString s in this.stringDict.Values)
                {
                    if (s.Value == value)
                    {
                        result = s;
                        return true;
                    }
                }
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

        public void Clear()
        {
            if (strings != null)
                Array.Clear(strings, 0, strings.Length);

            if (this.stringDict != null)
                this.stringDict.Clear();
        }
    }
}
