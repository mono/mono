//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using System.Security.Principal;
    

    abstract class SecureConversationDriver
    {
        public virtual XmlDictionaryString CloseAction
        {
            get
            {
                // PreSharp Bug: Property get methods should not throw exceptions.
                #pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationDriverVersionDoesNotSupportSession)));
            }
        }

        public virtual XmlDictionaryString CloseResponseAction
        {
            get
            {
                // PreSharp Bug: Property get methods should not throw exceptions.
                #pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationDriverVersionDoesNotSupportSession)));
            }
        }

        public virtual bool IsSessionSupported
        {
            get
            {
                return false;
            }
        }
        
        public abstract XmlDictionaryString IssueAction { get; }

        public abstract XmlDictionaryString IssueResponseAction { get; }

        public virtual XmlDictionaryString RenewAction
        {
            get
            {
                // PreSharp Bug: Property get methods should not throw exceptions.
                #pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationDriverVersionDoesNotSupportSession)));
            }
        }

        public virtual XmlDictionaryString RenewResponseAction
        {
            get
            {
                // PreSharp Bug: Property get methods should not throw exceptions.
                #pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationDriverVersionDoesNotSupportSession)));
            }
        }

        public abstract XmlDictionaryString Namespace { get; }

        public abstract XmlDictionaryString RenewNeededFaultCode { get; }

        public abstract XmlDictionaryString BadContextTokenFaultCode { get; }

        public abstract string TokenTypeUri { get; }

        public abstract UniqueId GetSecurityContextTokenId(XmlDictionaryReader reader);
        public abstract bool IsAtSecurityContextToken(XmlDictionaryReader reader);
    }
}
