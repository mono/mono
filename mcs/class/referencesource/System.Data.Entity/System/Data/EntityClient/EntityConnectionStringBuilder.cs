//---------------------------------------------------------------------
// <copyright file="EntityConnectionStringBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityClient
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a connection string builder for the entity client provider
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "EntityConnectionStringBuilder follows the naming convention of DbConnectionStringBuilder.")]
    [SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers", Justification = "There is no applicable strongly-typed implementation of CopyTo.")]
    public sealed class EntityConnectionStringBuilder : DbConnectionStringBuilder
    {
        // Names of parameters to look for in the connection string
        internal const string NameParameterName = "name";
        internal const string MetadataParameterName = "metadata";
        internal const string ProviderParameterName = "provider";
        internal const string ProviderConnectionStringParameterName = "provider connection string";

        // An array to hold the keywords
        private static readonly string[] s_validKeywords = new string[]
        {
            NameParameterName,
            MetadataParameterName,
            ProviderParameterName,
            ProviderConnectionStringParameterName
        };

        private static Hashtable s_synonyms;

        // Information and data used by the connection
        private string _namedConnectionName;
        private string _providerName;
        private string _metadataLocations;
        private string _storeProviderConnectionString;

        /// <summary>
        /// Constructs the EntityConnectionStringBuilder object
        /// </summary>
        public EntityConnectionStringBuilder()
        {
            // Everything just defaults to null
        }

        /// <summary>
        /// Constructs the EntityConnectionStringBuilder object with a connection string
        /// </summary>
        /// <param name="connectionString">The connection string to initialize this builder</param>
        public EntityConnectionStringBuilder(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the named connection name in the connection string
        /// </summary>
        [DisplayName("Name")]
        [EntityResCategoryAttribute(EntityRes.EntityDataCategory_NamedConnectionString)]
        [EntityResDescriptionAttribute(EntityRes.EntityConnectionString_Name)]
        [RefreshProperties(RefreshProperties.All)]
        public string Name
        {
            get
            {
                return this._namedConnectionName ?? "";
            }
            set
            {
                this._namedConnectionName = value;
                base[NameParameterName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the underlying .NET Framework data provider in the connection string
        /// </summary>
        [DisplayName("Provider")]
        [EntityResCategoryAttribute(EntityRes.EntityDataCategory_Source)]
        [EntityResDescriptionAttribute(EntityRes.EntityConnectionString_Provider)]
        [RefreshProperties(RefreshProperties.All)]
        public string Provider
        {
            get
            {
                return this._providerName ?? "";
            }
            set
            {
                this._providerName = value;
                base[ProviderParameterName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the metadata locations in the connection string, which is a pipe-separated sequence
        /// of paths to folders and individual files
        /// </summary>
        [DisplayName("Metadata")]
        [EntityResCategoryAttribute(EntityRes.EntityDataCategory_Context)]
        [EntityResDescriptionAttribute(EntityRes.EntityConnectionString_Metadata)]
        [RefreshProperties(RefreshProperties.All)]
        public string Metadata
        {
            get
            {
                return this._metadataLocations ?? "";
            }
            set
            {
                this._metadataLocations = value;
                base[MetadataParameterName] = value;                
            }
        }

        /// <summary>
        /// Gets or sets the inner connection string in the connection string
        /// </summary>
        [DisplayName("Provider Connection String")]
        [EntityResCategoryAttribute(EntityRes.EntityDataCategory_Source)]
        [EntityResDescriptionAttribute(EntityRes.EntityConnectionString_ProviderConnectionString)]
        [RefreshProperties(RefreshProperties.All)]
        public string ProviderConnectionString
        {
            get
            {
                return this._storeProviderConnectionString ?? "";
            }
            set
            {
                this._storeProviderConnectionString = value;
                base[ProviderConnectionStringParameterName] = value;
            }
        }

        /// <summary>
        /// Gets whether the EntityConnectionStringBuilder has a fixed size
        /// </summary>
        public override bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a collection of all keywords used by EntityConnectionStringBuilder
        /// </summary>
        public override ICollection Keys
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<string>(s_validKeywords);
            }
        }

        /// <summary>
        /// Returns a hash table object containing all the valid keywords. This is really the same as the Keys
        /// property, it's just that the returned object is a hash table.
        /// </summary>
        internal static Hashtable Synonyms
        {
            get
            {
                // Build the synonyms table if we don't have one
                if (s_synonyms == null)
                {
                    Hashtable table = new Hashtable(s_validKeywords.Length);
                    foreach (string keyword in s_validKeywords)
                    {
                        table.Add(keyword, keyword);
                    }
                    s_synonyms = table;
                }
                return s_synonyms;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the keyword
        /// </summary>
        public override object this[string keyword]
        {
            get
            {
                EntityUtil.CheckArgumentNull(keyword, "keyword");

                // Just access the properties to get the value since the fields, which the properties will be accessing, will
                // have already been set when the connection string is set
                if (string.Compare(keyword, MetadataParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.Metadata;
                }
                else if (string.Compare(keyword, ProviderConnectionStringParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.ProviderConnectionString;
                }
                else if (string.Compare(keyword, NameParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.Name;
                }
                else if (string.Compare(keyword, ProviderParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.Provider;
                }

                throw EntityUtil.KeywordNotSupported(keyword);
            }
            set
            {
                EntityUtil.CheckArgumentNull(keyword, "keyword");

                // If a null value is set, just remove the parameter and return
                if (value == null)
                {
                    this.Remove(keyword);
                    return;
                }

                // Since all of our parameters must be string value, perform the cast here and check
                string stringValue = value as string;
                if (stringValue == null)
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ValueNotString, "value");
                }

                // Just access the properties to get the value since the fields, which the properties will be accessing, will
                // have already been set when the connection string is set
                if (string.Compare(keyword, MetadataParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Metadata = stringValue;
                }
                else if (string.Compare(keyword, ProviderConnectionStringParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.ProviderConnectionString = stringValue;
                }
                else if (string.Compare(keyword, NameParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Name = stringValue;
                }
                else if (string.Compare(keyword, ProviderParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Provider = stringValue;
                }
                else
                {
                    throw EntityUtil.KeywordNotSupported(keyword);
                }
            }
        }

        /// <summary>
        /// Clear all the parameters in the connection string
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            this._namedConnectionName = null;
            this._providerName = null;
            this._metadataLocations = null;
            this._storeProviderConnectionString = null;
        }

        /// <summary>
        /// Determine if this connection string builder contains a specific key
        /// </summary>
        /// <param name="keyword">The keyword to find in this connection string builder</param>
        /// <returns>True if this connections string builder contains the specific key</returns>
        public override bool ContainsKey(string keyword)
        {
            EntityUtil.CheckArgumentNull(keyword, "keyword");

            foreach (string validKeyword in s_validKeywords)
            {
                if (validKeyword.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the value of the given keyword, returns false if there isn't a value with the given keyword
        /// </summary>
        /// <param name="keyword">The keyword specifying the name of the parameter to retrieve</param>
        /// <param name="value">The value retrieved</param>
        /// <returns>True if the value is retrieved</returns>
        public override bool TryGetValue(string keyword, out object value)
        {
            EntityUtil.CheckArgumentNull(keyword, "keyword");

            if (ContainsKey(keyword))
            {
                value = this[keyword];
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Removes a parameter from the builder
        /// </summary>
        /// <param name="keyword">The keyword specifying the name of the parameter to remove</param>
        /// <returns>True if the parameter is removed</returns>
        public override bool Remove(string keyword)
        {
            EntityUtil.CheckArgumentNull(keyword, "keyword");

            // Convert the given object into a string
            if (string.Compare(keyword, MetadataParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this._metadataLocations = null;
            }
            else if (string.Compare(keyword, ProviderConnectionStringParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this._storeProviderConnectionString = null;
            }
            else if (string.Compare(keyword, NameParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this._namedConnectionName = null;
            }
            else if (string.Compare(keyword, ProviderParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this._providerName = null;
            }

            return base.Remove(keyword);
        }
    }
}
