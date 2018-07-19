//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Xml;
    using System.Collections.Generic;

    class IdentityModelDictionary : IXmlDictionary
    {
        static public readonly IdentityModelDictionary Version1 = new IdentityModelDictionary(new IdentityModelStringsVersion1());
        IdentityModelStrings strings;
        int count;
        XmlDictionaryString[] dictionaryStrings;
        Dictionary<string, int> dictionary;
        XmlDictionaryString[] versionedDictionaryStrings;

        public IdentityModelDictionary(IdentityModelStrings strings)
        {
            this.strings = strings;
            this.count = strings.Count;
        }

        static public IdentityModelDictionary CurrentVersion
        {
            get
            {
                return Version1;
            }
        }

        public XmlDictionaryString CreateString(string value, int key)
        {
            return new XmlDictionaryString(this, value, key);
        }

        public bool TryLookup(string key, out XmlDictionaryString value)
        {
            if (key == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
            if (this.dictionary == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(count);
                for (int i = 0; i < count; i++)
                    dictionary.Add(strings[i], i);
                this.dictionary = dictionary;
            }
            int id;
            if (this.dictionary.TryGetValue(key, out id))
                return TryLookup(id, out value);
            value = null;
            return false;
        }

        public bool TryLookup(int key, out XmlDictionaryString value)
        {
            if (key < 0 || key >= count)
            {
                value = null;
                return false;
            }
            if (dictionaryStrings == null)
                dictionaryStrings = new XmlDictionaryString[count];
            XmlDictionaryString s = dictionaryStrings[key];
            if (s == null)
            {
                s = CreateString(strings[key], key);
                dictionaryStrings[key] = s;
            }
            value = s;
            return true;
        }

        public bool TryLookup(XmlDictionaryString key, out XmlDictionaryString value)
        {
            if (key == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
            if (key.Dictionary == this)
            {
                value = key;
                return true;
            }
            if (key.Dictionary == CurrentVersion)
            {
                if (versionedDictionaryStrings == null)
                    versionedDictionaryStrings = new XmlDictionaryString[CurrentVersion.count];
                XmlDictionaryString s = versionedDictionaryStrings[key.Key];
                if (s == null)
                {
                    if (!TryLookup(key.Value, out s))
                    {
                        value = null;
                        return false;
                    }
                    versionedDictionaryStrings[key.Key] = s;
                }
                value = s;
                return true;
            }
            value = null;
            return false;
        }
    }
}
