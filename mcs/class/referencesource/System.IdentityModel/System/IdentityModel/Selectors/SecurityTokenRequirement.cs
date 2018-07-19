//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Globalization;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;

    public class SecurityTokenRequirement
    {
        const string Namespace = "http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement";
        const string tokenTypeProperty = Namespace + "/TokenType";
        const string keyUsageProperty = Namespace + "/KeyUsage";
        const string keyTypeProperty = Namespace + "/KeyType";
        const string keySizeProperty = Namespace + "/KeySize";
        const string requireCryptographicTokenProperty = Namespace + "/RequireCryptographicToken";
        const string peerAuthenticationMode = Namespace + "/PeerAuthenticationMode";
        const string isOptionalTokenProperty = Namespace + "/IsOptionalTokenProperty";
        
        const bool defaultRequireCryptographicToken = false;
        const SecurityKeyUsage defaultKeyUsage = SecurityKeyUsage.Signature;
        const SecurityKeyType defaultKeyType = SecurityKeyType.SymmetricKey;
        const int defaultKeySize = 0;
        const bool defaultIsOptionalToken = false;

        Dictionary<string, object> properties;

        public SecurityTokenRequirement()
        {
            properties = new Dictionary<string, object>();
            this.Initialize();
        }

        static public string TokenTypeProperty { get { return tokenTypeProperty; } }
        static public string KeyUsageProperty { get { return keyUsageProperty; } }
        static public string KeyTypeProperty { get { return keyTypeProperty; } }
        static public string KeySizeProperty { get { return keySizeProperty; } }
        static public string RequireCryptographicTokenProperty { get { return requireCryptographicTokenProperty; } }
        static public string PeerAuthenticationMode { get { return peerAuthenticationMode; } }
        static public string IsOptionalTokenProperty { get { return isOptionalTokenProperty; } }
        
        public string TokenType
        {
            get
            {
                string result;
                return (this.TryGetProperty<string>(TokenTypeProperty, out result)) ? result : null;
            }
            set
            {
                this.properties[TokenTypeProperty] = value;
            }
        }

        internal bool IsOptionalToken
        {
            get
            {
                bool result;
                return (this.TryGetProperty<bool>(IsOptionalTokenProperty, out result)) ? result : defaultIsOptionalToken;
            }
            set
            {
                this.properties[IsOptionalTokenProperty] = value;
            }
        }

        public bool RequireCryptographicToken
        {
            get
            {
                bool result;
                return (this.TryGetProperty<bool>(RequireCryptographicTokenProperty, out result)) ? result : defaultRequireCryptographicToken;
            }
            set
            {
                this.properties[RequireCryptographicTokenProperty] = (object)value;
            }
        }

        public SecurityKeyUsage KeyUsage
        {
            get
            {
                SecurityKeyUsage result;
                return (this.TryGetProperty<SecurityKeyUsage>(KeyUsageProperty, out result)) ? result : defaultKeyUsage;
            }
            set
            {
                SecurityKeyUsageHelper.Validate(value);
                this.properties[KeyUsageProperty] = (object)value;
            }
        }

        public SecurityKeyType KeyType
        {
            get
            {
                SecurityKeyType result;
                return (this.TryGetProperty<SecurityKeyType>(KeyTypeProperty, out result)) ? result : defaultKeyType;
            }
            set
            {
                SecurityKeyTypeHelper.Validate(value);
                this.properties[KeyTypeProperty] = (object)value;
            }
        }

        public int KeySize
        {
            get
            {
                int result;
                return (this.TryGetProperty<int>(KeySizeProperty, out result)) ? result : defaultKeySize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.Properties[KeySizeProperty] = value;
            }
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                return this.properties;
            }
        }

        void Initialize()
        {
            this.KeyType = defaultKeyType;
            this.KeyUsage = defaultKeyUsage;
            this.RequireCryptographicToken = defaultRequireCryptographicToken;
            this.KeySize = defaultKeySize;
            this.IsOptionalToken = defaultIsOptionalToken;
        }

        public TValue GetProperty<TValue>(string propertyName)
        {
            TValue result;
            if (!TryGetProperty<TValue>(propertyName, out result))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SecurityTokenRequirementDoesNotContainProperty, propertyName)));
            }
            return result;
        }

        public bool TryGetProperty<TValue>(string propertyName, out TValue result)
        {
            object dictionaryValue;
            if (!Properties.TryGetValue(propertyName, out dictionaryValue))
            {
                result = default(TValue);
                return false;
            }
            if (dictionaryValue != null && !typeof(TValue).IsAssignableFrom(dictionaryValue.GetType()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SecurityTokenRequirementHasInvalidTypeForProperty, propertyName, dictionaryValue.GetType(), typeof(TValue))));
            }
            result = (TValue)dictionaryValue;
            return true;
        }
    }
}
