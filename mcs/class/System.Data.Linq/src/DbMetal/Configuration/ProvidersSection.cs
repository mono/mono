#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace DbMetal.Configuration
{
    /// <summary>
    /// Handles the providers section.
    /// Each provider is defined as follows:
    ///  &lt;provider name="MySQL"      dbLinqSchemaLoader="DbLinq.MySql.MySqlSchemaLoader, DbLinq.MySql"
    ///                             databaseConnection="MySql.Data.MySqlClient.MySqlConnection, MySql.Data" />
    /// </summary>
    public class ProvidersSection : ConfigurationSection
    {
        public class ProviderElement : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true)]
            public string Name
            {
                get { return (string)this["name"]; }
            }

            [ConfigurationProperty("dbLinqSchemaLoader", IsRequired = true)]
            public string DbLinqSchemaLoader
            {
                get { return (string)this["dbLinqSchemaLoader"]; }
            }

            [ConfigurationProperty("databaseConnection", IsRequired = true)]
            public string DatabaseConnection
            {
                get { return (string)this["databaseConnection"]; }
            }

            [ConfigurationProperty("sqlDialectType", IsRequired = false)]
            public string SqlDialectType
            {
                get { return (string)this["sqlDialectType"]; }
            }
        }

        public class ProvidersCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ProviderElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                var provider = (ProviderElement)element;
                return provider.Name.ToLower();
            }

            public ProviderElement GetProvider(string name)
            {
                return (ProviderElement)BaseGet(name.ToLower());
            }
            public bool TryGetProvider(string name, out ProviderElement element, out string error)
            {
                //use Configuration namespace to get our config
                object[] allKeys = base.BaseGetAllKeys();
                if (Array.IndexOf(allKeys, name.ToLower())<0)
                {
                    string[] allKeyStrings = allKeys.OfType<string>().ToArray();
                    
                    element = null;
                    error = allKeys.Length == 0 
                        ? "There are no <provider> entries in your app.config"
                        : "Key " + name + " not found among " + allKeys.Length + " config entries {" + string.Join(",",allKeyStrings) + "}";
                    return false;
                }
                element = (ProviderElement)BaseGet(name.ToLower());
                error = null;
                return true;
            }
        }

        [ConfigurationProperty("providers", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(ProviderElement), AddItemName = "provider")]
        public ProvidersCollection Providers
        {
            get { return (ProvidersCollection)this["providers"]; }
        }
    }
}