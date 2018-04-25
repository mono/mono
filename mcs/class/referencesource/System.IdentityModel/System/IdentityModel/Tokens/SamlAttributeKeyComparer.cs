//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Comparison class supporting multi-part keys for a dicitionary
    /// </summary>
    internal class SamlAttributeKeyComparer : IEqualityComparer<SamlAttributeKeyComparer.AttributeKey>
    {
        public class AttributeKey
        {
            string _friendlyName;
            int _hashCode;
            string _name;
            string _nameFormat;
            string _namespace;
            string _valueType;
            string _originalIssuer;

            internal string FriendlyName { get { return _friendlyName; } }
            internal string Name { get { return _name; } }
            internal string NameFormat { get { return _nameFormat; } }
            internal string Namespace { get { return _namespace; } }
            internal string ValueType { get { return _valueType; } }
            internal string OriginalIssuer { get { return _originalIssuer; } }

            public AttributeKey( SamlAttribute attribute )
            {
                if ( attribute == null )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "attribute" );
                }

                _friendlyName = String.Empty;
                _name = attribute.Name;
                _nameFormat = String.Empty;
                _namespace = attribute.Namespace ?? String.Empty;
                _valueType = attribute.AttributeValueXsiType ?? String.Empty;
                _originalIssuer = attribute.OriginalIssuer ?? String.Empty;

                ComputeHashCode();
            }

            public AttributeKey( Saml2Attribute attribute )
            {
                if ( attribute == null )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "attribute" );
                }

                _friendlyName = attribute.FriendlyName ?? String.Empty;
                _name = attribute.Name;
                _nameFormat = attribute.NameFormat == null ? String.Empty : attribute.NameFormat.AbsoluteUri;
                _namespace = String.Empty;
                _valueType = attribute.AttributeValueXsiType ?? String.Empty;
                _originalIssuer = attribute.OriginalIssuer ?? String.Empty;

                ComputeHashCode();
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            void ComputeHashCode()
            {
                _hashCode = _name.GetHashCode();
                _hashCode ^= _friendlyName.GetHashCode();
                _hashCode ^= _nameFormat.GetHashCode();
                _hashCode ^= _namespace.GetHashCode();
                _hashCode ^= _valueType.GetHashCode();
                _hashCode ^= _originalIssuer.GetHashCode();
            }
        }

        #region IEqualityComparer<AttributeKey> Members

        public bool Equals( AttributeKey x, AttributeKey y )
        {
            return x.Name.Equals( y.Name, StringComparison.Ordinal )
                && x.FriendlyName.Equals( y.FriendlyName, StringComparison.Ordinal )
                && x.ValueType.Equals( y.ValueType, StringComparison.Ordinal )
                && x.OriginalIssuer.Equals( y.OriginalIssuer, StringComparison.Ordinal )
                && x.NameFormat.Equals( y.NameFormat, StringComparison.Ordinal )
                && x.Namespace.Equals( y.Namespace, StringComparison.Ordinal );
        }

        public int GetHashCode( AttributeKey obj )
        {
            return obj.GetHashCode();
        }

        #endregion
    };
}
