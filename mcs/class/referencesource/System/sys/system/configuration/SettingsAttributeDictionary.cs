//------------------------------------------------------------------------------
// <copyright file="SettingsAttributeDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;
    using  System.Globalization;
    using  System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;
    using System.ComponentModel;

    [Serializable()]
    public class SettingsAttributeDictionary : Hashtable
    {
        public SettingsAttributeDictionary() : base() { }
        public SettingsAttributeDictionary(SettingsAttributeDictionary attributes) : base((Hashtable) attributes) { }
    }
}
