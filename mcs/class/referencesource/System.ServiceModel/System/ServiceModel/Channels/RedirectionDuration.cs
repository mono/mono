//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    [Serializable]
    public class RedirectionDuration
    {
        static RedirectionDuration permanent = new RedirectionDuration(InternalRedirectionDuration.Permanent);

        static RedirectionDuration temporary = new RedirectionDuration(InternalRedirectionDuration.Temporary);
        InternalRedirectionDuration internalDuration;
        string toString = null;
        Nullable<int> hashCode = null;

        // For Serialization
        private RedirectionDuration() { }

        //should be used for known durations only
        RedirectionDuration(InternalRedirectionDuration duration)
        {
            this.Namespace = RedirectionConstants.Namespace;
            this.internalDuration = duration;

            switch (duration)
            {
                case InternalRedirectionDuration.Temporary:
                    this.Value = RedirectionConstants.Duration.Temporary;
                    break;
                case InternalRedirectionDuration.Permanent:
                    this.Value = RedirectionConstants.Duration.Permanent;
                    break;
                default:
                    Fx.Assert("This constructor doesn't support the following enum value: " + duration);
                    break;
            }
        }

        RedirectionDuration(string duration, string ns)
        {
            this.Value = duration;
            this.Namespace = ns;
            this.internalDuration = InternalRedirectionDuration.Unknown;
        }

        internal enum InternalRedirectionDuration
        {
            Unknown, //we haven't yet checked it against the known types
            Custom,
            Temporary,
            Permanent,
        }
        public static RedirectionDuration Permanent { get { return permanent; } }

        public static RedirectionDuration Temporary { get { return temporary; } }
        public string Namespace { get; private set; }
        public string Value { get; private set; }

        internal InternalRedirectionDuration InternalDuration
        {
            get
            {
                if (this.internalDuration == InternalRedirectionDuration.Unknown)
                {
                    DetectDuration();
                }

                return this.internalDuration;
            }
        }

        public static bool operator !=(RedirectionDuration left, RedirectionDuration right)
        {
            return !(left == right);
        }

        public static bool operator ==(RedirectionDuration left, RedirectionDuration right)
        {
            bool result = false;

            if ((object)left == (object)null && (object)right == (object)null)
            {
                result = true;
            }
            else if ((object)left != (object)null && (object)right != (object)null)
            {
                if (left.InternalDuration == right.InternalDuration)
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

        public static RedirectionDuration Create(string duration, string ns)
        {
            if (duration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("duration");
            }
            else if (duration.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("duration",
                    SR.GetString(SR.ParameterCannotBeEmpty));
            }

            return new RedirectionDuration(duration, ns);
        }

        public override bool Equals(object obj)
        {
            bool result = base.Equals(obj);

            if (!result)
            {
                result = ((obj as RedirectionDuration) == this);
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
        void DetectDuration()
        {
            if (RedirectionUtility.IsNamespaceMatch(this.Namespace, RedirectionConstants.Namespace))
            {
                if (string.Equals(this.Value, RedirectionConstants.Duration.Temporary, StringComparison.Ordinal))
                {
                    this.internalDuration = InternalRedirectionDuration.Temporary;
                }
                else if (string.Equals(this.Value, RedirectionConstants.Duration.Permanent, StringComparison.Ordinal))
                {
                    this.internalDuration = InternalRedirectionDuration.Permanent;
                }
                else
                {
                    this.internalDuration = InternalRedirectionDuration.Custom;
                }
            }
            else
            {
                this.internalDuration = InternalRedirectionDuration.Custom;
            }

            Fx.Assert(this.internalDuration != InternalRedirectionDuration.Unknown, "Failed to correctly detect internal redirection duration");
        }
    }
}
