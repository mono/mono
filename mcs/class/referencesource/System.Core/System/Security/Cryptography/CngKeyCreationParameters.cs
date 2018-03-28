// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Settings to be applied to a CNG key before it is finalized.
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class CngKeyCreationParameters {
        private CngExportPolicies? m_exportPolicy;
        private CngKeyCreationOptions m_keyCreationOptions;
        private CngKeyUsages? m_keyUsage;
        private CngPropertyCollection m_parameters = new CngPropertyCollection();
        private IntPtr m_parentWindowHandle;
        private CngProvider m_provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
        private CngUIPolicy m_uiPolicy;
        
        /// <summary>
        ///     How many times can this key be exported from the KSP
        /// </summary>
        public CngExportPolicies? ExportPolicy {
            get { return m_exportPolicy; }
            set { m_exportPolicy = value; }
        }

        /// <summary>
        ///     Flags controlling how to create the key
        /// </summary>
        public CngKeyCreationOptions KeyCreationOptions {
            get { return m_keyCreationOptions; }
            set { m_keyCreationOptions = value; }
        }

        /// <summary>
        ///     Which cryptographic operations are valid for use with this key
        /// </summary>
        public CngKeyUsages? KeyUsage {
            get { return m_keyUsage; }
            set { m_keyUsage = value; }
        }

        /// <summary>
        ///     Window handle to use as the parent for the dialog shown when the key is created
        /// </summary>
        public IntPtr ParentWindowHandle {
            get { return m_parentWindowHandle; }

            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            [SecuritySafeCritical]
            set { m_parentWindowHandle = value; }
        }

        /// <summary>
        ///     Extra parameter values to set before the key is finalized
        /// </summary>
        public CngPropertyCollection Parameters {
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            [SecuritySafeCritical]
            get {
                Contract.Ensures(Contract.Result<CngPropertyCollection>() != null);
                return m_parameters;
            }
        }

        /// <summary>
        ///     Internal access to the parameters method without a demand
        /// </summary>
        internal CngPropertyCollection ParametersNoDemand {
            get {
                Contract.Ensures(Contract.Result<CngPropertyCollection>() != null);
                return m_parameters;
            }
        }

        /// <summary>
        ///     KSP to create the key in
        /// </summary>
        public CngProvider Provider {
            get {
                Contract.Ensures(Contract.Result<CngProvider>() != null);
                return m_provider;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                m_provider = value;
            }
        }

        /// <summary>
        ///     Settings for UI shown on access to the key
        /// </summary>
        public CngUIPolicy UIPolicy {
            get { return m_uiPolicy; }
            
            [HostProtection(UI = true)]
            [UIPermission(SecurityAction.Demand, Window = UIPermissionWindow.SafeSubWindows)]
            [SecuritySafeCritical]
            set { m_uiPolicy = value; }
        }
    }
}
