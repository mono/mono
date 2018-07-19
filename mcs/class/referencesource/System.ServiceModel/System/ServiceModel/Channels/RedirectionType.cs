//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    [Serializable]
    public sealed class RedirectionType
    {
        static RedirectionType cache = new RedirectionType(InternalRedirectionType.Cache);
        static RedirectionType resource = new RedirectionType(InternalRedirectionType.Resource);
        static RedirectionType useIntermediary = new RedirectionType(InternalRedirectionType.UseIntermediary);
        InternalRedirectionType internalType;
        string toString;
        Nullable<int> hashCode = null;

        // For Serialization
        private RedirectionType() { }

        //should be used for known types only
        RedirectionType(InternalRedirectionType type)
        {
            this.Namespace = RedirectionConstants.Namespace;
            this.internalType = type;

            switch (type)
            {
                case InternalRedirectionType.Cache:
                    this.Value = RedirectionConstants.Type.Cache;
                    break;
                case InternalRedirectionType.Resource:
                    this.Value = RedirectionConstants.Type.Resource;
                    break;
                case InternalRedirectionType.UseIntermediary:
                    this.Value = RedirectionConstants.Type.UseIntermediary;
                    break;
                default:
                    Fx.Assert("This constructor doesn't support the following enum value: " + type);
                    break;
            }
        }

        RedirectionType(string value, string ns)
        {
            this.Value = value;
            this.Namespace = ns;

            //delay comparing strings until needed...
            this.internalType = InternalRedirectionType.Unknown;
        }

        internal enum InternalRedirectionType
        {
            Unknown, //we haven't yet checked it against the known types
            Custom,
            Cache,
            UseIntermediary,
            Resource,
        }
        public static RedirectionType Cache { get { return cache; } }

        public static RedirectionType Resource { get { return resource; } }
        public static RedirectionType UseIntermediary { get { return useIntermediary; } }
        public string Namespace { get; private set; }

        public string Value { get; private set; }

        internal InternalRedirectionType InternalType
        {
            get
            {
                if (this.internalType == InternalRedirectionType.Unknown)
                {
                    DetectType();
                }

                return this.internalType;
            }
        }

        public static bool operator !=(RedirectionType left, RedirectionType right)
        {
            return !(left == right);
        }

        public static bool operator ==(RedirectionType left, RedirectionType right)
        {
            bool result = false;

            if ((object)left == (object)null && (object)right == (object)null)
            {
                result = true;
            }
            else if ((object)left != (object)null && (object)right != (object)null)
            {
                if (left.InternalType == right.InternalType)
                {
                    result = true;
                }
                else
                {
                    result = RedirectionUtility.IsNamespaceAndValueMatch(left.Value,
                        left.Namespace, right.Value, right.Namespace);
                }
            }

            return result;
        }

        public static RedirectionType Create(string type, string ns)
        {
            if (type == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }
            else if (type.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("type",
                    SR.GetString(SR.ParameterCannotBeEmpty));
            }

            return new RedirectionType(type, ns);
        }

        public override bool Equals(object obj)
        {
            bool result = base.Equals(obj);

            if (!result)
            {
                result = ((obj as RedirectionType) == this);
            }

            return result;
        }

        public override int GetHashCode()
        {
            if (this.hashCode == null)
            {
                this.hashCode = RedirectionUtility.ComputeHashCode(this.Value, this.Namespace);
            }

            return this.hashCode.Value;
        }

        public override string ToString()
        {
            if (this.toString == null)
            {
                if (this.Namespace != null)
                {
                    this.toString = SR.GetString(SR.RedirectionInfoStringFormatWithNamespace, this.Value, this.Namespace);
                }
                else
                {
                    this.toString = SR.GetString(SR.RedirectionInfoStringFormatNoNamespace, this.Value);
                }
            }
            return this.toString;
        }

        //When Create(...) is used, we delay finding the enum value
        //until the enum value is needed, avoiding the string comparisons if possible 
        void DetectType()
        {
            if (RedirectionUtility.IsNamespaceMatch(this.Namespace, RedirectionConstants.Namespace))
            {
                if (string.Equals(this.Value, RedirectionConstants.Type.Cache, StringComparison.Ordinal))
                {
                    this.internalType = InternalRedirectionType.Cache;
                }
                else if (string.Equals(this.Value, RedirectionConstants.Type.Resource, StringComparison.Ordinal))
                {
                    this.internalType = InternalRedirectionType.Resource;
                }
                else if (string.Equals(this.Value, RedirectionConstants.Type.UseIntermediary, StringComparison.Ordinal))
                {
                    this.internalType = InternalRedirectionType.UseIntermediary;
                }
                else
                {
                    this.internalType = InternalRedirectionType.Custom;
                }
            }
            else
            {
                this.internalType = InternalRedirectionType.Custom;
            }

            Fx.Assert(this.internalType != InternalRedirectionType.Unknown, "Failed to correctly detect internal redirection type");
        }
    }
}
