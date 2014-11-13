//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Linq;

    using ReadOnlyStringDictionary = System.Runtime.ReadOnlyDictionaryInternal<string, string>;

    public sealed class CorrelationKey : InstanceKey
    {
        static readonly XNamespace CorrelationNamespace = XNamespace.Get("urn:microsoft-com:correlation");
        static readonly ReadOnlyStringDictionary emptyDictionary = new ReadOnlyStringDictionary(new Dictionary<string, string>(0));

        string name;

        CorrelationKey(string keyString, XNamespace provider)
            : base(GenerateKey(keyString), new Dictionary<XName, InstanceValue>(2)
            {
                { provider.GetName("KeyString"), new InstanceValue(keyString, InstanceValueOptions.Optional) },
                { WorkflowNamespace.KeyProvider, new InstanceValue(provider.NamespaceName, InstanceValueOptions.Optional) },
            })
        {
            KeyString = keyString;
        }

        // The public constructor normalizes the parameters and calls this constructor, which creates the key string and adds it to the data avaliable to the "real" constructor.
        CorrelationKey(ReadOnlyStringDictionary keyData, string scopeName, XNamespace provider)
            : this(GenerateKeyString(keyData, scopeName, provider.NamespaceName), provider)
        {
            KeyData = keyData;
            Provider = provider;
        }

        public CorrelationKey(IDictionary<string, string> keyData, XName scopeName, XNamespace provider)
            : this(keyData == null ? CorrelationKey.emptyDictionary : MakeReadonlyCopy(keyData), scopeName != null ? scopeName.ToString() : null, provider ?? CorrelationNamespace)
        {
            ScopeName = scopeName;
        }

        private static ReadOnlyStringDictionary MakeReadonlyCopy(IDictionary<string, string> dictionary)
        {
            IDictionary<string, string> copy;
            if (dictionary.IsReadOnly)
                copy = dictionary;
            else
                copy = new Dictionary<string, string>(dictionary);
            return new ReadOnlyStringDictionary(copy);
        }

        public IDictionary<string, string> KeyData { get; private set; }

        public XName ScopeName { get; private set; }

        public XNamespace Provider { get; private set; }

        public string KeyString { get; private set; }


        // This name is not an aspect of the key itself, it exists to allow keys to be locally disambiguated.
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (!IsValid)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.CannotSetNameOnTheInvalidKey)));
                }
                this.name = value;
            }
        }

        static Guid GenerateKey(string keyString)
        {
            byte[] keyBytes = Encoding.Unicode.GetBytes(keyString);
            byte[] hashBytes = HashHelper.ComputeHash(keyBytes);

            return new Guid(hashBytes);
        }


        // The checksum ends up describing the structure of the key data, so we don't need to worry about
        // key collisions between maliciously-crafted key data even though we don't do any escaping.
        static string GenerateKeyString(ReadOnlyStringDictionary keyData, string scopeName, string provider)
        {
            if (string.IsNullOrEmpty(scopeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "scopeName", SR.GetString(SR.ScopeNameMustBeSpecified));
            }

            if (provider.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "provider", SR.GetString(SR.ProviderCannotBeEmptyString));
            }

            StringBuilder key = new StringBuilder();
            StringBuilder checksum = new StringBuilder();
            SortedList<string, string> sortedKeyData = new SortedList<string, string>(keyData, StringComparer.Ordinal);

            checksum.Append(sortedKeyData.Count.ToString(NumberFormatInfo.InvariantInfo));
            checksum.Append('.');

            for (int i = 0; i < sortedKeyData.Count; i++)
            {
                if (i > 0)
                {
                    key.Append('&');
                }
                key.Append(sortedKeyData.Keys[i]);
                key.Append('=');
                key.Append(sortedKeyData.Values[i]);

                checksum.Append(sortedKeyData.Keys[i].Length.ToString(NumberFormatInfo.InvariantInfo));
                checksum.Append('.');
                checksum.Append(sortedKeyData.Values[i].Length.ToString(NumberFormatInfo.InvariantInfo));
                checksum.Append('.');
            }

            if (sortedKeyData.Count > 0)
            {
                key.Append(',');
            }

            key.Append(scopeName);
            key.Append(',');
            key.Append(provider);

            checksum.Append(scopeName.Length.ToString(NumberFormatInfo.InvariantInfo));
            checksum.Append('.');
            checksum.Append(provider.Length.ToString(NumberFormatInfo.InvariantInfo));

            key.Append('|');
            key.Append(checksum);

            return key.ToString();
        }
    }
}

