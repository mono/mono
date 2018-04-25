//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    

    sealed class MessageSecurityTokenVersion : SecurityTokenVersion
    {
        SecurityVersion securityVersion;
        TrustVersion trustVersion;
        SecureConversationVersion secureConversationVersion;
        bool emitBspRequiredAttributes;
        string toString;
        ReadOnlyCollection<string> supportedSpecs;

        const string bsp10ns = @"http://ws-i.org/profiles/basic-security/core/1.0";
        static MessageSecurityTokenVersion wss11 = new MessageSecurityTokenVersion(
            SecurityVersion.WSSecurity11,
            TrustVersion.WSTrustFeb2005,
            SecureConversationVersion.WSSecureConversationFeb2005,
            "WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005",
            false,
            XD.SecurityXXX2005Dictionary.Namespace.Value,
            XD.TrustFeb2005Dictionary.Namespace.Value,
            XD.SecureConversationFeb2005Dictionary.Namespace.Value);
        static MessageSecurityTokenVersion wss10bsp10 = new MessageSecurityTokenVersion(
            SecurityVersion.WSSecurity10,
            TrustVersion.WSTrustFeb2005,
            SecureConversationVersion.WSSecureConversationFeb2005,
            "WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10",
            true,
            XD.SecurityJan2004Dictionary.Namespace.Value,
            XD.TrustFeb2005Dictionary.Namespace.Value,
            XD.SecureConversationFeb2005Dictionary.Namespace.Value,
            bsp10ns);
        static MessageSecurityTokenVersion wss11bsp10 = new MessageSecurityTokenVersion(
            SecurityVersion.WSSecurity11,
            TrustVersion.WSTrustFeb2005,
            SecureConversationVersion.WSSecureConversationFeb2005,
            "WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10",
            true,
            XD.SecurityXXX2005Dictionary.Namespace.Value,
            XD.TrustFeb2005Dictionary.Namespace.Value,
            XD.SecureConversationFeb2005Dictionary.Namespace.Value,
            bsp10ns);
        static MessageSecurityTokenVersion wss10oasisdec2005bsp10 = new MessageSecurityTokenVersion(
            SecurityVersion.WSSecurity10,
            TrustVersion.WSTrust13,
            SecureConversationVersion.WSSecureConversation13,
            "WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10",
            true,
            XD.SecurityXXX2005Dictionary.Namespace.Value,
            DXD.TrustDec2005Dictionary.Namespace.Value,
            DXD.SecureConversationDec2005Dictionary.Namespace.Value
            );
        static MessageSecurityTokenVersion wss11oasisdec2005 = new MessageSecurityTokenVersion(
            SecurityVersion.WSSecurity11,
            TrustVersion.WSTrust13,
            SecureConversationVersion.WSSecureConversation13,
            "WSSecurity11WSTrust13WSSecureConversation13",
            false,
            XD.SecurityJan2004Dictionary.Namespace.Value,
            DXD.TrustDec2005Dictionary.Namespace.Value,
            DXD.SecureConversationDec2005Dictionary.Namespace.Value
            );
        static MessageSecurityTokenVersion wss11oasisdec2005bsp10 = new MessageSecurityTokenVersion(
            SecurityVersion.WSSecurity11,
            TrustVersion.WSTrust13,
            SecureConversationVersion.WSSecureConversation13,
            "WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10",
            true,
            XD.SecurityXXX2005Dictionary.Namespace.Value,
            DXD.TrustDec2005Dictionary.Namespace.Value,
            DXD.SecureConversationDec2005Dictionary.Namespace.Value
            );

        public static MessageSecurityTokenVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005
        {
            get
            {
                return wss11;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10
        {
            get
            {
                return wss11bsp10;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10
        {
            get
            {
                return wss10bsp10;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10
        {
            get
            {
                return wss10oasisdec2005bsp10;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrust13WSSecureConversation13
        {
            get
            {
                return wss11oasisdec2005;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10
        {
            get
            {
                return wss11oasisdec2005bsp10;
            }
        }

        public static MessageSecurityTokenVersion GetSecurityTokenVersion(SecurityVersion version, bool emitBspAttributes)
        {
            if (version == SecurityVersion.WSSecurity10)
            {
                if (emitBspAttributes)
                    return MessageSecurityTokenVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10;
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            else if (version == SecurityVersion.WSSecurity11)
            {
                if (emitBspAttributes)
                    return MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10;
                else
                    return MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        MessageSecurityTokenVersion(SecurityVersion securityVersion, TrustVersion trustVersion, SecureConversationVersion secureConversationVersion, string toString, bool emitBspRequiredAttributes, params string[] supportedSpecs)
            : base()
        {
            this.emitBspRequiredAttributes = emitBspRequiredAttributes;
            this.supportedSpecs = new ReadOnlyCollection<string>(supportedSpecs);
            this.toString = toString;
            this.securityVersion = securityVersion;
            this.trustVersion = trustVersion;
            this.secureConversationVersion = secureConversationVersion;
        }

        public bool EmitBspRequiredAttributes
        {
            get
            {
                return this.emitBspRequiredAttributes;
            }
        }

        public SecurityVersion SecurityVersion
        {
            get
            {
                return this.securityVersion;
            }
        }

        public TrustVersion TrustVersion
        {
            get
            {
                return this.trustVersion;
            }
        }

        public SecureConversationVersion SecureConversationVersion
        {
            get
            {
                return this.secureConversationVersion;
            }
        }

        public override ReadOnlyCollection<string> GetSecuritySpecifications()
        {
            return supportedSpecs;
        }

        public override string ToString()
        {
            return this.toString;
        }
    }
}
