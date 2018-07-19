//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    [Serializable]
    public class RedirectionScope
    {
        static RedirectionScope endpoint = new RedirectionScope(InternalRedirectionScope.Endpoint);
        static RedirectionScope message = new RedirectionScope(InternalRedirectionScope.Message);
        static RedirectionScope session = new RedirectionScope(InternalRedirectionScope.Session);

        InternalRedirectionScope internalScope;
        string toString;
        Nullable<int> hashCode;

        // For Serialization
        private RedirectionScope() { }

        //should be used for known scopes only
        RedirectionScope(InternalRedirectionScope scope)
        {
            this.Namespace = RedirectionConstants.Namespace;
            this.internalScope = scope;

            switch (scope)
            {
                case InternalRedirectionScope.Message:
                    this.Value = RedirectionConstants.Scope.Message;
                    break;
                case InternalRedirectionScope.Session:
                    this.Value = RedirectionConstants.Scope.Session;
                    break;
                case InternalRedirectionScope.Endpoint:
                    this.Value = RedirectionConstants.Scope.Endpoint;
                    break;
                default:
                    Fx.Assert("This constructor doesn't support the following enum value: " + scope);
                    break;
            }
        }

        RedirectionScope(string value, string ns)
        {
            this.Value = value;
            this.Namespace = ns;
            this.internalScope = InternalRedirectionScope.Unknown;
        }

        internal enum InternalRedirectionScope
        {
            Unknown, //we haven't yet checked it against the known types
            Custom,
            Message,
            Session,
            Endpoint,
        }
        public static RedirectionScope Endpoint { get { return endpoint; } }

        public static RedirectionScope Message { get { return message; } }
        public static RedirectionScope Session { get { return session; } }
        public string Namespace { get; private set; }
        public string Value { get; private set; }

        internal InternalRedirectionScope InternalScope
        {
            get
            {
                if (this.internalScope == InternalRedirectionScope.Unknown)
                {
                    DetectScope();
                }

                return this.internalScope;
            }
        }

        public static bool operator !=(RedirectionScope left, RedirectionScope right)
        {
            return !(left == right);
        }

        public static bool operator ==(RedirectionScope left, RedirectionScope right)
        {
            bool result = false;

            if ((object)left == (object)null && (object)right == (object)null)
            {
                result = true;
            }
            else if ((object)left != (object)null && (object)right != (object)null)
            {
                if (left.InternalScope == right.InternalScope)
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

        public static RedirectionScope Create(string scope, string ns)
        {
            if (scope == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("scope");
            }
            else if (scope.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("scope",
                    SR.GetString(SR.ParameterCannotBeEmpty));
            }

            return new RedirectionScope(scope, ns);
        }

        public override bool Equals(object obj)
        {
            bool result = base.Equals(obj);

            if (!result)
            {
                result = ((obj as RedirectionScope) == this);
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
        void DetectScope()
        {
            if (RedirectionUtility.IsNamespaceMatch(this.Namespace, RedirectionConstants.Namespace))
            {
                if (string.Equals(this.Value, RedirectionConstants.Scope.Message, StringComparison.Ordinal))
                {
                    this.internalScope = InternalRedirectionScope.Message;
                }
                else if (string.Equals(this.Value, RedirectionConstants.Scope.Session, StringComparison.Ordinal))
                {
                    this.internalScope = InternalRedirectionScope.Session;
                }
                else if (string.Equals(this.Value, RedirectionConstants.Scope.Endpoint, StringComparison.Ordinal))
                {
                    this.internalScope = InternalRedirectionScope.Endpoint;
                }
                else
                {
                    this.internalScope = InternalRedirectionScope.Custom;
                }
            }
            else
            {
                this.internalScope = InternalRedirectionScope.Custom;
            }

            Fx.Assert(this.internalScope != InternalRedirectionScope.Unknown, "Failed to correctly detect internal redirection scope");
        }
    }
}
