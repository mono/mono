//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ComponentModel;

    public sealed class WSDualHttpSecurity
    {
        static readonly MessageSecurityVersion WSDualMessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

        internal const WSDualHttpSecurityMode DefaultMode = WSDualHttpSecurityMode.Message;

        WSDualHttpSecurityMode mode;
        MessageSecurityOverHttp messageSecurity;

        public WSDualHttpSecurity()
            : this(DefaultMode, new MessageSecurityOverHttp())
        {
        }

        WSDualHttpSecurity(WSDualHttpSecurityMode mode, MessageSecurityOverHttp messageSecurity)
        {
            Fx.Assert(WSDualHttpSecurityModeHelper.IsDefined(mode), string.Format("Invalid WSDualHttpSecurityMode value: {0}", mode.ToString()));

            this.mode = mode;
            this.messageSecurity = messageSecurity == null ? new MessageSecurityOverHttp() : messageSecurity;
        }

        public WSDualHttpSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!WSDualHttpSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public MessageSecurityOverHttp Message
        {
            get { return this.messageSecurity; }
            set
            {
                this.messageSecurity = (value == null) ? new MessageSecurityOverHttp() : value;
            }
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            if (this.mode == WSDualHttpSecurityMode.Message)
            {
                return this.messageSecurity.CreateSecurityBindingElement(false, true, WSDualMessageSecurityVersion);
            }
            else
            {
                return null;
            }
        }

        internal static bool TryCreate(SecurityBindingElement sbe, out WSDualHttpSecurity security)
        {
            security = null;
            if (sbe == null)
                security = new WSDualHttpSecurity(WSDualHttpSecurityMode.None, null);
            else
            {
                MessageSecurityOverHttp messageSecurity;
                if (!MessageSecurityOverHttp.TryCreate(sbe, false, true, out messageSecurity))
                    return false;
                security = new WSDualHttpSecurity(WSDualHttpSecurityMode.Message, messageSecurity);
            }
            // the last check: make sure that security binding element match the incoming security
            return SecurityElement.AreBindingsMatching(security.CreateMessageSecurity(), sbe);
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeMode()
                || this.ShouldSerializeMessage();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return this.Mode != DefaultMode;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.Message.InternalShouldSerialize();
        }
    }
}
