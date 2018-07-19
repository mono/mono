//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.ServiceModel.Channels;

    // The only purpose in life for these classes is so that, on standard bindings, you can say
    //     binding.ReliableSession.Ordered
    //     binding.ReliableSession.InactivityTimeout
    //     binding.ReliableSession.Enabled
    // where these properties are "bucketized" all under .ReliableSession, which makes them easier to 
    // discover/Intellisense
    public class ReliableSession
    {
        ReliableSessionBindingElement element;

        public ReliableSession()
        {
            this.element = new ReliableSessionBindingElement();
        }

        public ReliableSession(ReliableSessionBindingElement reliableSessionBindingElement)
        {
            if (reliableSessionBindingElement == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reliableSessionBindingElement");
            this.element = reliableSessionBindingElement;
        }

        [DefaultValue(ReliableSessionDefaults.Ordered)]
        public bool Ordered
        {
            get { return this.element.Ordered; }
            set { this.element.Ordered = value; }
        }

        public TimeSpan InactivityTimeout
        {
            get { return this.element.InactivityTimeout; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBePositive)));

                this.element.InactivityTimeout = value;
            }
        }

        internal void CopySettings(ReliableSession copyFrom)
        {
            this.Ordered = copyFrom.Ordered;
            this.InactivityTimeout = copyFrom.InactivityTimeout;
        }
    }

    public class OptionalReliableSession : ReliableSession
    {
        bool enabled;

        public OptionalReliableSession() : base() { }

        public OptionalReliableSession(ReliableSessionBindingElement reliableSessionBindingElement) : base(reliableSessionBindingElement) { }

        // We don't include DefaultValue here because this defaults to false, so omitting it would make the XAML somewhat misleading
        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = value;
            }
        }

        internal void CopySettings(OptionalReliableSession copyFrom)
        {
            base.CopySettings(copyFrom);
            this.Enabled = copyFrom.Enabled;
        }
    }
}
