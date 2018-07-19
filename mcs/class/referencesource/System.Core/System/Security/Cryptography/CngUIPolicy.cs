// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Configuration parameters for the UI displayed by CNG when accessing a protected key
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class CngUIPolicy {
        private string m_creationTitle;
        private string m_description;
        private string m_friendlyName;
        private CngUIProtectionLevels m_protectionLevel;
        private string m_useContext;

        public CngUIPolicy(CngUIProtectionLevels protectionLevel) :
            this(protectionLevel, null) {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName) :
            this(protectionLevel, friendlyName, null) {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description) :
            this(protectionLevel, friendlyName, description, null) {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel,
                           string friendlyName,
                           string description,
                           string useContext) :
            this(protectionLevel, friendlyName, description, useContext, null) {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel,
                           string friendlyName,
                           string description,
                           string useContext,
                           string creationTitle) {
            m_creationTitle = creationTitle;
            m_description = description;
            m_friendlyName = friendlyName;
            m_protectionLevel = protectionLevel;
            m_useContext = useContext;
        }

        /// <summary>
        ///     Title of the dialog box displaed when a newly created key is finalized, null for the default title
        /// </summary>
        public string CreationTitle {
            get { return m_creationTitle; }
        }

        /// <summary>
        ///     Description text displayed in the dialog box when the key is accessed, null for the default text
        /// </summary>
        public string Description {
            get { return m_description; }
        }

        /// <summary>
        ///     Friendly name to describe the key with in the dialog box that appears when the key is accessed,
        ///     null for default name
        /// </summary>
        public string FriendlyName {
            get { return m_friendlyName; }
        }

        /// <summary>
        ///     Level of UI protection to apply to the key
        /// </summary>
        public CngUIProtectionLevels ProtectionLevel {
            get { return m_protectionLevel; }
        }

        /// <summary>
        ///     Description of how the key will be used
        /// </summary>
        public string UseContext {
            get { return m_useContext; }
        }
    }
}
